using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TrustgraphCore.Data;
using TrustgraphCore.Model;
using TrustchainCore.Extensions;
using System.Runtime.InteropServices;

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
        public int NodeIndex { get; set; }
        public int ParentIndex { get; set; }
        public EdgeModel Edge { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VisitItem
    {
        public int ParentIndex;
        public int EdgeIndex;
        public int Cost;

        public VisitItem(int parentIndex, int edgeIndex, int cost)
        {
            ParentIndex = parentIndex;
            EdgeIndex = edgeIndex;
            Cost = cost;
        }
    }



    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct QueueItem 
    {
        public int Index;
        public int ParentIndex;
        public int EdgeIndex;
        public int Cost;

        public QueueItem(int index, int parentIndex, int edgeIndex, int cost)
        {
            Index = index;
            ParentIndex = parentIndex;
            EdgeIndex = edgeIndex;
            Cost = cost;
        }
    }

    public class GraphQueryContext
    {
        //public List<EdgeModel> Edges;
        //public string Result;

        //public List<int> NodeIndex = new List<int>();
        public EdgeModel Query { get; set; }
        public Dictionary<int, VisitItem> Visited { get; set; } 
        public int MaxCost { get; set; }
        public int Level { get; set; }

        public GraphQueryContext()
        {
            Visited = new Dictionary<int, VisitItem>();
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

            List<QueueItem> queue = new List<QueueItem>();
            queue.Add(new QueueItem(issuerIndex, -1, -1, 0)); // Starting point!

            while (queue.Count > 0 || context.Level > 6)
            {
                // Check current level for trust
                var results = new List<ResultNode>();
                foreach (var item in queue)
                {
                    var result = Query(item, context);
                    if(result != null)
                    {
                        results.Add(result);
                    }
                }
                // Stop here if trust found
                if(results.Count > 0)
                {

                    break; // Stop processing the query!
                }


                // Continue to next level
                var subQueue = new List<QueueItem>();
                foreach (var item in queue)
                {
                    subQueue.AddRange(Enqueue(item, context));
                }
                queue = subQueue;

                context.Level++; 
            }

            return context;
        }

        public ResultNode Query(QueueItem item, GraphQueryContext context)
        {
            ResultNode result = null;

            context.Visited.Add(item.Index, new VisitItem(item.ParentIndex, item.EdgeIndex, item.Cost)); // Makes sure that we do not run this block again.

            var node = Data.Graph.Nodes[item.Index];

            var edgeIndex = PeekNode(node, context);
            if (edgeIndex >= 0)
            {
                // found!!
                result = new ResultNode();
                result.NodeIndex = item.Index;
                result.ParentIndex = item.ParentIndex;
                result.Edge = node.Edges[edgeIndex];
            }
            return result;
        }

        public List<QueueItem> Enqueue(QueueItem item, GraphQueryContext context)
        {
            var list = new List<QueueItem>();
            var node = Data.Graph.Nodes[item.Index];

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

                if ((edges[i].Claim.Flags & context.Query.Claim.Types) == 0) // The value is false, do not follow!
                    continue;

                if (context.Visited.ContainsKey(edges[i].SubjectId))
                {
                    var visited = context.Visited[edges[i].SubjectId];
                    if(visited.Cost > edges[i].Cost) // If the current cost is lower then its a better route.
                        context.Visited[edges[i].SubjectId] = new VisitItem(item.Index, i, edges[i].Cost); // Overwrite the old visit with the new because of lower cost

                    continue; // We have already done this node, so no need to reprocess.
                }

                list.Add(new QueueItem(edges[i].SubjectId, item.Index, i, edges[i].Cost));
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
