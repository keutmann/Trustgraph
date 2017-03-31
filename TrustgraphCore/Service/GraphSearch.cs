using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TrustgraphCore.Data;
using TrustgraphCore.Model;
using TrustchainCore.Extensions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Newtonsoft.Json;
using TrustchainCore.Model;

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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ResultNode
    {
        public int NodeIndex { get; set; }
        public int ParentIndex { get; set; }
        public EdgeModel Edge { get; set; }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Int64ToInt32
    {
        [FieldOffset(0)]
        public Int64 Int64Value;
        [FieldOffset(0)]
        public Int32 LeftInt32;
        [FieldOffset(4)]
        public Int32 RightInt32;

        public Int64ToInt32(Int64 value)
        {
            LeftInt32 = 0;
            RightInt32 = 0;
            Int64Value = value;
        }

        public Int64ToInt32(Int32 left, Int32 right)
        {
            Int64Value = 0;
            LeftInt32 = left;
            RightInt32 = right;
        }
    }

    public class TreeNode : SubjectModel
    {
        [JsonIgnore]
        public int NodeIndex { get; set; }

        [JsonIgnore]
        public int ParentIndex { get; set; }

        [JsonIgnore]
        public Int64ToInt32 EdgeIndex { get; set; }

        [JsonProperty(PropertyName = "nodes", NullValueHandling = NullValueHandling.Ignore, Order = 100)]
        public List<TreeNode> Children { get; set; }

        public TreeNode() : base()
        {
        }
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

    /// <summary>
    /// The result of the query
    /// </summary>
    public class ResultContext
    {
        public int TotalNodeCount = 0;
        public int TotalEdgeCount = 0;
        public int MatchEdgeCount = 0;

        public List<TreeNode> Nodes { get; set; }
    }


    public class QueryContext 
    {
        public int IssuerIndex { get; set; }
        public EdgeModel Query { get; set; }
        public Dictionary<int, VisitItem> Visited { get; set; }
        public List<ResultNode> Results { get; set; }
        public int MaxCost { get; set; }
        public int Level { get; set; }
        public int MaxLevel { get; set; }
        public int TotalNodeCount = 0;
        public int TotalEdgeCount = 0;
        public int MatchEdgeCount = 0;

        public QueryContext()
        {
            Visited = new Dictionary<int, VisitItem>();
            MaxCost = 600; // About 6 levels down
            Results = new List<ResultNode>();
            MaxLevel = 7;
        }
    }


    public class GraphSearch : IGraphSearch
    {
        public IGraphContext GraphService { get; set; }
        public long UnixTime { get; set; } 

        public GraphSearch(IGraphContext data)
        {
            this.GraphService = data;
            UnixTime = DateTime.Now.ToUnixTime();
        }

        public ResultContext Query(GraphQuery query)
        {
            var context = new QueryContext(); // Do not return this object, its heavy on memory!
            context.IssuerIndex = GraphService.Graph.NodeIndex.ContainsKey(query.Issuer) ? GraphService.Graph.NodeIndex[query.Issuer] : -1;
            if (context.IssuerIndex == -1)
                throw new ApplicationException("Unknown issuer id");

            context.Query = CreateEgdeQuery(query);
            if (context.Query.SubjectId == -1)
                throw new ApplicationException("Unknown subject id");

            Query(context);

            // Create a stripdown version of QueryContext in order to release Query memory when exiting this function
            var result = BuildResultContext(context);

            if (context.Results.Count > 0)
                result.Nodes = BuildResultNode(context);//result.Node = BuildResultTree(context);

            return result;
        }

        public ResultContext BuildResultContext(QueryContext context)
        {
            var result = new ResultContext();

            result.TotalNodeCount = context.TotalNodeCount;
            result.TotalEdgeCount = context.TotalEdgeCount;
            result.MatchEdgeCount = context.MatchEdgeCount;
           
            return result;
        }

        public List<TreeNode> BuildResultNode(QueryContext context)
        {
            var results = new List<TreeNode>();
            var nodelist = new Dictionary<Int64ToInt32, TreeNode>();
            var currentNodes = new List<TreeNode>();
            foreach (var item in context.Results)
            {
                var tn = new TreeNode();
                tn.NodeIndex = item.NodeIndex;
                tn.ParentIndex = item.ParentIndex;
                var visited = context.Visited[tn.NodeIndex];

                tn.EdgeIndex = new Int64ToInt32(item.NodeIndex, visited.EdgeIndex);
                GraphService.InitSubjectModel(tn, item.Edge);

                //nodelist.Add(new Int64ToInt32(item.NodeIndex, 0), tn); // Needed?
                currentNodes.Add(tn);
            }

            while (results.Count == 0)
            {
                var currentLevelNodes = new List<TreeNode>();
                foreach (var tn in currentNodes)
                {
                    if (tn.NodeIndex == context.IssuerIndex)
                    {
                        results.Add(tn);
                        continue;
                    }

                    var parentNode = new TreeNode();
                    parentNode.NodeIndex = tn.ParentIndex;

                    var visited = context.Visited[tn.NodeIndex];
                    parentNode.ParentIndex = context.Visited[tn.ParentIndex].ParentIndex;
                    parentNode.EdgeIndex = new Int64ToInt32(parentNode.NodeIndex, visited.EdgeIndex);

                    if (nodelist.ContainsKey(parentNode.EdgeIndex))
                    {
                        // A previouse node in the collection has already created this
                        nodelist[parentNode.EdgeIndex].Children.Add(tn);
                        continue;
                    }

                    var graphNode = GraphService.Graph.Nodes[parentNode.NodeIndex];
                    var edge = graphNode.Edges[visited.EdgeIndex];
                    GraphService.InitSubjectModel(parentNode, edge);
                    parentNode.Children = new List<TreeNode>();
                    parentNode.Children.Add(tn);

                    currentLevelNodes.Add(parentNode);
                    nodelist.Add(parentNode.EdgeIndex, parentNode);
                }
                currentNodes = currentLevelNodes;
            }

            return results;
        }



        public void Query(QueryContext context)
        {
            List<QueueItem> queue = new List<QueueItem>();
            queue.Add(new QueueItem(context.IssuerIndex, -1, -1, 0)); // Starting point!

            while (queue.Count > 0 || context.Level > 6)
            {
                context.TotalNodeCount += queue.Count;

                // Check current level for trust
                foreach (var item in queue)
                    PeekNode(item, context);

                // Stop here if trust found at current level
                if (context.Results.Count > 0)
                    break; // Stop processing the query!

                // Continue to next level
                var subQueue = new List<QueueItem>();
                foreach (var item in queue)
                    subQueue.AddRange(Enqueue(item, context));

                queue = subQueue;

                context.Level++;
            }
        }

        private bool PeekNode(QueueItem item, QueryContext context)
        {

            context.Visited.Add(item.Index, 
                new VisitItem(item.ParentIndex, item.EdgeIndex, item.Cost)); // Makes sure that we do not run this block again.

            var edges = GraphService.Graph.Nodes[item.Index].Edges;

            for (var i = 0; i < edges.Length; i++)
            {
                context.TotalEdgeCount++;

                if (edges[i].SubjectType != context.Query.SubjectType ||
                    edges[i].Scope != context.Query.Scope)
                    continue;

                if (edges[i].Activate > UnixTime ||
                   (edges[i].Expire > 0 && edges[i].Expire < UnixTime))
                    continue;

                if ((edges[i].Claim.Types & context.Query.Claim.Types) == 0)
                    continue; // No claims match query

                context.MatchEdgeCount++;

                if (edges[i].SubjectId == context.Query.SubjectId)
                {
                    var result = new ResultNode();
                    result.NodeIndex = item.Index;
                    result.ParentIndex = item.ParentIndex;
                    result.Edge = edges[i];
                    context.Results.Add(result);
                    return true;
                }
            }

            return false;
        }

        public List<QueueItem> Enqueue(QueueItem item, QueryContext context)
        {
            var list = new List<QueueItem>();
            var node = GraphService.Graph.Nodes[item.Index];

            var edges = node.Edges;
            for (var i = 0; i < edges.Length; i++)
            {
                if (edges[i].SubjectType != context.Query.SubjectType ||
                    edges[i].Scope != context.Query.Scope)
                    continue; // Do not follow when Trust do not match scope or SubjectType

                if (edges[i].Activate > UnixTime ||
                    (edges[i].Expire > 0 && edges[i].Expire < UnixTime)) 
                    continue; // Do not follow when Trust has not activated or has expired

                if ((edges[i].Claim.Flags & ClaimType.Trust) == 0)
                    continue; // Do not follow when trust is false or do not exist.

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


        private EdgeModel CreateEgdeQuery(GraphQuery query)
        {
            var edge = new EdgeModel();

            edge.SubjectId = GraphService.Graph.NodeIndex.ContainsKey(query.Subject) ? GraphService.Graph.NodeIndex[query.Subject] : -1;
            edge.SubjectType = GraphService.Graph.SubjectTypesIndex.ContainsKey(query.SubjectType) ? GraphService.Graph.SubjectTypesIndex[query.SubjectType] : -1;
            edge.Scope = (GraphService.Graph.ScopeIndex.ContainsKey(query.Scope)) ? GraphService.Graph.ScopeIndex[query.Scope] : -1;
            
            edge.Activate = query.Activate;
            edge.Expire = query.Expire;
            edge.Claim = ClaimStandardModel.Parse(query.Claim);

            return edge;
        }
    }
}
