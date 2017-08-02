using System;
using System.Linq;
using System.Collections.Generic;
using TrustchainCore.Model;
using TrustgraphCore.Model;
using TrustchainCore.Extensions;
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

        public IGraphBuilder Append(PackageModel package)
        {
            Build(package.Trust);

            return this;
        }

        public IGraphBuilder Build(IEnumerable<TrustModel> trusts)
        {
            long unixTime = DateTime.Now.ToUnixTime();
            foreach (var trust in trusts)
            {
                var issuerIndex = Context.EnsureNode(trust.Issuer.Id);
                var issuerNode = Context.Graph.Address[issuerIndex];
                var issuerEdges = new List<EdgeModel>(issuerNode.Edges != null ? issuerNode.Edges : new EdgeModel[0]);

                foreach (var subject in trust.Issuer.Subjects)
                {
                    BuildSubject(trust, issuerEdges, subject);
                }

                // Remove old edges!
                issuerEdges.RemoveAll(e => e.Expire > 0 && e.Expire < unixTime);

                issuerNode.Edges = issuerEdges.ToArray();
                Context.Graph.Address[issuerIndex] = issuerNode;
            }
            return this;
        }

        private void BuildSubject(TrustModel trust, List<EdgeModel> issuerEdges, SubjectModel subject)
        {
            var subjectEdge = Context.CreateEdgeModel(subject, (int)trust.Issuer.Timestamp);
            var ids = new List<int>();
            // Find all edges that matchs
            for (var i = 0 ; i < issuerEdges.Count; i++)
            {
                if (issuerEdges[i].SubjectId != subjectEdge.SubjectId)
                    continue;

                if (issuerEdges[i].SubjectType != subjectEdge.SubjectType)
                    continue;

                if (issuerEdges[i].Scope != subjectEdge.Scope)
                    continue;

                if ((issuerEdges[i].Claim.Types & subjectEdge.Claim.Types) == 0)
                    continue;

                // Edge to be updated!
                ids.Add(i);
            }

            var flagTypes = subjectEdge.Claim.Types.GetFlags();
            foreach (ClaimType flagtype in flagTypes)
            {
                var i = -1;
                if (ids.Count > 0)
                {
                    i = ids.FirstOrDefault(p => (issuerEdges[p].Claim.Types & flagtype) > 0);
                    if (issuerEdges[i].Timestamp > subjectEdge.Timestamp) // Make sure that we cannot overwrite with old data
                        continue;
                }

                var nodeEdge = subjectEdge; // Copy the subjectEdge object
                nodeEdge.Claim.Types = flagtype; // overwrite the flags
                nodeEdge.Claim.Flags = subjectEdge.Claim.Flags & flagtype; // overwrite the flags
                nodeEdge.Claim.Rating = (flagtype == ClaimType.Rating) ? subjectEdge.Claim.Rating : (byte)0;

                if (i >= 0 && i < issuerEdges.Count)
                    issuerEdges[i] = nodeEdge;
                else
                    issuerEdges.Add(nodeEdge);
            }
        }
    }
}
