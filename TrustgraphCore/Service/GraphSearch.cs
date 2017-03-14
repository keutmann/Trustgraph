using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TrustgraphCore.Data;
using TrustgraphCore.Model;
using TrustchainCore.Extensions;


namespace TrustgraphCore.Service
{

    public class GraphQuery
    {
        public byte[] Issuer;
        public byte[] Subject;
        public string SubjectType;
        public string Scope;
        public int Activate;
        public int Expire;
        public JObject Claim;
    }

    public class ResultNode
    {
        public EdgeModel Edge { get; set; }
        public List<ResultNode> Children = new List<ResultNode>();
    }

    public class GraphQueryContext
    {
        //public List<EdgeModel> Edges;
        //public string Result;

        //public List<int> NodeIndex = new List<int>();
        public EdgeModel Query { get; set; }
        public HashSet<int> Visited { get; set; } 
        public int MaxCost { get; set; }
        public int Level { get; set; }

        public GraphQueryContext()
        {
            Visited = new HashSet<int>();
            MaxCost = 600; // About 6 levels down
        }
    }

    public class GraphSearch : IGraphSearch
    {
        public IGraphContext Data { get; set; }
        public long UnixTime { get; set; } 

        public GraphSearch(IGraphContext data)
        {
            this.Data = data;
            UnixTime = DateTime.Now.ToUnixTime();
        }

        public GraphQueryContext Query(GraphQuery query)
        {
            var issuerIndex = Data.Graph.NodeIndex.ContainsKey(query.Issuer) ? Data.Graph.NodeIndex[query.Issuer]: -1;
            if (issuerIndex == -1)
                throw new ApplicationException("Unknown issuer id");

            var context = new GraphQueryContext();

            context.Query = CreateEgdeQuery(query);
            if(context.Query.SubjectId == -1)
                throw new ApplicationException("Unknown subject id");

            List<int> nodeIndex = new List<int>();
            nodeIndex.Add(issuerIndex); // Starting point!
            var found = false;
            while(nodeIndex.Count > 0 || found)
            {
                foreach (var index in nodeIndex)
                {
                    Query(index, context);
                }

                var list = new List<int>();
                foreach (var index in nodeIndex)
                {
                    list.AddRange(Enqueue(index, context));
                }
                nodeIndex = list;
                context.Level++;
            }

            return context;
        }

        public ResultNode Query(int nodeIndex, GraphQueryContext context)
        {
            ResultNode result = null;

            context.Visited.Add(nodeIndex); // Makes sure that we do not run this block again.

            var node = Data.Graph.Nodes[nodeIndex];

            var index = PeekNode(node, context);
            if (index >= 0)
            {
                // found!!
                result = new ResultNode();
                result.Edge = node.Edges[index];
            }
            return result;
        }

        public List<int> Enqueue(int nodeIndex, GraphQueryContext context)
        {
            var list = new List<int>();
            var node = Data.Graph.Nodes[nodeIndex];

            var edges = node.Edges;
            for (var i = 0; i < edges.Length; i++)
            {
                if (context.Visited.Contains(edges[i].SubjectId))
                    continue; // We have already done this one

                if (edges[i].SubjectType != context.Query.SubjectType ||
                    edges[i].Scope != context.Query.Scope ||
                    (edges[i].Claim.Types & context.Query.Claim.Types) == 0)
                    continue;

                if (edges[i].Activate > UnixTime ||
                    (edges[i].Expire > 0 && edges[i].Expire < UnixTime)) 
                    continue;

                if ((edges[i].Claim.Flags & context.Query.Claim.Types) == 0) // The value is false, do not follow!
                    continue;

                //var childCost = cost + edges[i].Cost; // 
                //if (childCost > context.MaxCost)
                //    continue;

                list.Add(edges[i].SubjectId);

                //var childResult = Query(edges[i].SubjectId, childCost, context);
                //if(childResult != null) // A result has been found 
                //{
                //    if (result == null)
                //        result = new ResultNode();

                //    result.Children.Add(childResult);
                //}
            }
            return list;
        }



        public int PeekNode(NodeModel node, GraphQueryContext context)
        {
            var edges = node.Edges;

            for (var i = 0; i < edges.Length; i++)
            {
                if (edges[i].SubjectType != context.Query.SubjectType ||
                    edges[i].Scope != context.Query.Scope ||
                    (edges[i].Claim.Types & context.Query.Claim.Types) == 0)
                    continue;

                if (edges[i].Activate > UnixTime ||
                   (edges[i].Expire > 0 && edges[i].Expire < UnixTime))

                    continue;
                if (edges[i].SubjectId == context.Query.SubjectId)
                {
                    // Found the subject
                    return i;
                }
            }
            return -1;
        }



        private EdgeModel CreateEgdeQuery(GraphQuery query)
        {
            var edge = new EdgeModel();

            edge.SubjectId = Data.Graph.NodeIndex.ContainsKey(query.Subject) ? Data.Graph.NodeIndex[query.Subject] : -1;
            edge.SubjectType = Data.Graph.SubjectTypesIndex.ContainsKey(query.SubjectType) ? Data.Graph.SubjectTypesIndex[query.SubjectType] : -1;
            edge.Scope = (Data.Graph.ScopeIndex.ContainsKey(query.Scope)) ? Data.Graph.ScopeIndex[query.Scope] : -1;
            
            edge.Activate = query.Activate;
            edge.Expire = query.Expire;
            edge.Claim = ClaimStandardModel.Parse(query.Claim);

            return edge;
        }
    }
}
