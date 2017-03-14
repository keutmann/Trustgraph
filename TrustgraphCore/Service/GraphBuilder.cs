using System;
using System.Collections.Generic;
using TrustchainCore.Model;
using TrustgraphCore.Model;
using TrustchainCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TrustgraphCore.Data;

namespace TrustgraphCore.Service
{
    public class GraphBuilder : IGraphBuilder
    {
        public IGraphContext Context { get; set; }

        public GraphBuilder(IGraphContext context)
        {
            Context = context;
        }

        public void Build(IEnumerable<TrustModel> trusts)
        {
            long unixTime = DateTime.Now.ToUnixTime();
            foreach (var trust in trusts)
            {
                var issuerIndex = Context.EnsureNode(trust.Issuer.Id);
                var issuerNode = Context.Graph.Nodes[issuerIndex];
                var issuerEdges = new List<EdgeModel>(issuerNode.Edges);

                foreach (var subject in trust.Issuer.Subjects)
                {
                    BuildSubject(trust, issuerEdges, subject);
                }

                // Remove old edges!
                issuerEdges.RemoveAll(e => e.Expire < unixTime);

                issuerNode.Edges = issuerEdges.ToArray();
            }
        }

        private void BuildSubject(TrustModel trust, List<EdgeModel> issuerEdges, SubjectModel subject)
        {
            var subjectEdge = Context.CreateEdgeModel(subject, (int)trust.Issuer.Timestamp);
            var rating = subjectEdge.Claim.Rating;
            var flagTypes = subjectEdge.Claim.Types;
            foreach (ClaimType flagtype in flagTypes.GetFlags())
            {
                var i = 0;
                for (; i < issuerEdges.Count; i++)
                {
                    if (issuerEdges[i].SubjectId != subjectEdge.SubjectId)
                        continue;

                    if (issuerEdges[i].SubjectType != subjectEdge.SubjectType)
                        continue;

                    if (issuerEdges[i].Scope != subjectEdge.Scope)
                        continue;

                    if ((issuerEdges[i].Claim.Types & flagtype) == 0)
                        continue;

                    if (issuerEdges[i].Timestamp > subjectEdge.Timestamp) // Make sure that we cannot overwrite with old data
                        continue;

                    // Edge to be updated!
                    break;
                }

                var nodeEdge = subjectEdge; // Copy the subjectEdge object
                nodeEdge.Claim.Types = flagtype; // overwrite the flags
                nodeEdge.Claim.Flags = subjectEdge.Claim.Flags & flagtype; // overwrite the flags
                nodeEdge.Claim.Rating = (flagtype == ClaimType.Rating) ? subjectEdge.Claim.Rating : (byte)0;

                if (i < issuerEdges.Count)
                    issuerEdges[i] = nodeEdge;
                else
                    issuerEdges.Add(nodeEdge);
            }
        }
    }
}
