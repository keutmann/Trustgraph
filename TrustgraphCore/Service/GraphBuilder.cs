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
        protected GraphModel Graph { get; set; }

        public struct NodeIndex
        {
            public int Index;
            public NodeModel Node;
        }

        public GraphBuilder(IGraphContext context)
        {
            Context = context;
            Graph = Context.Graph;
        }

        public void Build(IEnumerable<TrustModel> trusts)
        {
            long unixTime = DateTime.Now.ToUnixTime();
            foreach (var trust in trusts)
            {
                var nodeIndex = EnsureNode(trust.Issuer.Id);
                var nodeEdges = new List<EdgeModel>(nodeIndex.Node.Edges);

                // Remove old edges!
                nodeEdges.RemoveAll(e => e.Expire < unixTime);

                foreach (var subject in trust.Issuer.Subjects)
                {
                    // Verify that the subject has not expired!
                    if (subject.Expire < unixTime)
                        continue;

                    var subjectEdge = new EdgeModel();
                    var subjectNode = EnsureNode(subject.Id);
                    subjectEdge.SubjectId = subjectNode.Index;
                    subjectEdge.SubjectType = EnsureSubjectType(subject.IdType);
                    subjectEdge.Scope = EnsureScopeIndex(subject.Scope);
                    subjectEdge.Activate = (int)subject.Activate;
                    subjectEdge.Expire = (int)subject.Expire;
                    subjectEdge.Cost = (short)subject.Cost;
                    subjectEdge.Timestamp = (int)trust.Issuer.Timestamp;
                    subjectEdge.Claim = ParseClaim(subject);

                    if(subjectEdge.Claim.Flags == ClaimType.Clear)
                    {
                        nodeEdges.RemoveAll(e => MatchEdge(subjectEdge, e));
                        continue;
                    }


                    var found = false;
                    for(var i = 0; i < nodeEdges.Count; i++)
                    {
                        var nodeEdge = nodeEdges[i];
                        if (!MatchEdge(nodeEdge, subjectEdge))
                            continue;

                        if (nodeEdge.Timestamp > subjectEdge.Timestamp)
                            continue; // Ignore old trusts!

                        // Edge to be updated!
                        found = true;

                        var oldFlags = nodeEdge.Claim.Flags - subjectEdge.Claim.Types; // Clear out new from old
                        subjectEdge.Claim.Types |= nodeEdge.Claim.Types; // Merge from old
                        subjectEdge.Claim.Flags |= (ClaimType)oldFlags; // Merge old flags into new
                        subjectEdge.Claim.Rating = nodeEdge.Claim.Rating;

                        nodeEdges[i] = subjectEdge;
                        break;
                    }

                    if(!found)
                        nodeEdges.Add(subjectEdge);
                }
                nodeIndex.Node.Edges = nodeEdges.ToArray();
            }
        }

        private bool MatchEdge(EdgeModel edge, EdgeModel e)
        {
            if (edge.SubjectId != e.SubjectId)
                return false;

            if (edge.SubjectType != e.SubjectType)
                return false;

            if (edge.Scope != e.Scope)
                return false;

            return true;
        }

        private ClaimStandardModel ParseClaim(SubjectModel subject)
        {
            var result = new ClaimStandardModel();
            var claimType = typeof(ClaimType);
            var names = Enum.GetNames(claimType);
            foreach (var name in names)
            {
                var token = subject.Claim.GetValue(name, StringComparison.OrdinalIgnoreCase);
                if (token.Type == JTokenType.Null)
                    continue;

                var ct = (ClaimType)Enum.Parse(claimType, name);
                result.Types |= ct; // The claim has been defined

                if (token.Type == JTokenType.Boolean) {
                    var val = token.ToBoolean();
                    if (val)
                        result.Flags |= ct; // Set it to true in flags!
                }

                if (ct == ClaimType.Rating && token.Type == JTokenType.Integer)
                    result.Rating = (byte)token.ToInteger();
            }

            return result;
        }


        public NodeIndex EnsureNode(byte[] id)
        {
            var result = new NodeIndex();

            if (!Graph.IssuerIdIndex.ContainsKey(id))
            {
                result.Index = Graph.Nodes.Count;
                Graph.IssuerIdIndex.Add(id, result.Index);

                result.Node = new NodeModel();
                Graph.Nodes.Add(result.Node);
            }
            else
            {
                result.Index = Graph.IssuerIdIndex[id];
                result.Node = Graph.Nodes[result.Index];
            }
            return result;
        }

        public short EnsureSubjectType(string subjectType)
        {

            if (!Graph.SubjectTypesIndex.ContainsKey(subjectType))
            {
                var index = (short)Graph.SubjectTypesIndex.Count;
                Graph.SubjectTypesIndex.Add(subjectType, index);

                return index;
            }

            return (short)Graph.SubjectTypesIndex[subjectType];
        }

        public short EnsureScopeIndex(string scope)
        {
            if (!Graph.ScopeIndex.ContainsKey(scope))
            {
                var index = (short)Graph.ScopeIndex.Count;
                Graph.ScopeIndex.Add(scope, index);

                return index;
            }

            return (short)Graph.ScopeIndex[scope];
        }

    }
}
