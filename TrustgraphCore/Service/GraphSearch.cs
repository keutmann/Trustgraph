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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ResultNode
    {
        public int NodeIndex { get; set; }
        public int ParentIndex { get; set; }
        public EdgeModel Edge { get; set; }
    }

    public class TreeNode
    {
        public EdgeModel? Edge;
        public List<TreeNode> Children = new List<TreeNode>();

        public TreeNode(EdgeModel? edge)
        {
            Edge = edge;
        }

        public TreeNode(EdgeModel? edge, TreeNode child) : this(edge)
        {
            Children.Add(child);
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

        public TreeNode Result { get; set; }
    }


    public class QueryContext 
    {
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
            var issuerIndex = GraphService.Graph.NodeIndex.ContainsKey(query.Issuer) ? GraphService.Graph.NodeIndex[query.Issuer] : -1;
            if (issuerIndex == -1)
                throw new ApplicationException("Unknown issuer id");

            var context = new QueryContext(); // Do not return this object, its heavy on memory!
            context.Query = CreateEgdeQuery(query);
            if (context.Query.SubjectId == -1)
                throw new ApplicationException("Unknown subject id");

            Query(issuerIndex, context);

            // Create a stripdown version of QueryContext in order to release Query memory when exiting this function
            var result = BuildResultContext(context);

            if (context.Results.Count > 0) 
                result.Result = BuildResultTree(context);

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

        
        public TreeNode BuildResultTree(QueryContext context)
        {
            var treeList = new Dictionary<int, TreeNode>();
            TreeNode rootNode = new TreeNode(null);

            foreach (var item in context.Results)
            {
                var index = item.NodeIndex;
                var visited = context.Visited[index];
                
                var level = 0;
                var node = new TreeNode(item.Edge);
                treeList.Add(index, node);
                if (visited.ParentIndex < 0)
                {
                    rootNode.Children.Add(node); // Set start index, so we know where to begin
                    continue;
                }

                
                while (visited.ParentIndex >= 0 && level < context.Level) // Level functuions as a dead man switch
                {
                    var parentNode = GraphService.Graph.Nodes[visited.ParentIndex];
                    var edge = parentNode.Edges[visited.EdgeIndex];

                    if (treeList.ContainsKey(visited.ParentIndex))
                    {
                        treeList[visited.ParentIndex].Children.Add(node);
                        break; // Stop here, parents have been build!
                    }

                    var parent = new TreeNode(edge, node);
                    treeList.Add(visited.ParentIndex, parent);
                    node = parent;

                    // Now go one level up!
                    visited = context.Visited[visited.ParentIndex];

                    if (visited.ParentIndex < 0)
                        rootNode.Children.Add(node); // Set start node, so we know where to begin

                    level++;
                }
            }
            return rootNode;
        }

        public void Query(int issuerIndex, QueryContext context)
        {
            List<QueueItem> queue = new List<QueueItem>();
            queue.Add(new QueueItem(issuerIndex, -1, -1, 0)); // Starting point!

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
                    edges[i].Scope != context.Query.Scope ||
                    (edges[i].Claim.Types & context.Query.Claim.Types) == 0)
                    continue;

                if (edges[i].Activate > UnixTime ||
                   (edges[i].Expire > 0 && edges[i].Expire < UnixTime))
                    continue;

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
