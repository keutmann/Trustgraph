using System;
using System.Collections.Generic;
using TrustgraphCore.Data;
using TrustgraphCore.Model;
using TrustchainCore.Extensions;

namespace TrustgraphCore.Service
{
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
            Verify(query);

            var context = new QueryContext(GraphService.Graph.Address.Count); // Do not return this object, its heavy on memory!
            context.IssuerIndex = GraphService.Graph.AddressIndex.ContainsKey(query.Issuer) ? GraphService.Graph.AddressIndex[query.Issuer] : -1;
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

        public void Verify(GraphQuery query)
        {
            if (query.Issuer.Length != 20)
                throw new ApplicationException("Invalid byte length on Issuer");

            if (query.Subject.Length != 20)
                throw new ApplicationException("Invalid byte length on Issuer");
        }

        public ResultContext BuildResultContext(QueryContext context)
        {
            var result = new ResultContext();

            result.TotalNodeCount = context.TotalNodeCount;
            result.TotalEdgeCount = context.TotalEdgeCount;
            result.MatchEdgeCount = context.MatchEdgeCount;
           
            return result;
        }

        public List<SubjectNode> BuildResultNode(QueryContext context)
        {
            var results = new List<SubjectNode>();
            var nodelist = new Dictionary<Int64Container, SubjectNode>();
            var currentNodes = new List<SubjectNode>();
            foreach (var item in context.Results)
            {
                var tn = new SubjectNode();
                tn.NodeIndex = item.NodeIndex;
                tn.ParentIndex = item.ParentIndex;
                var visited = context.Visited[tn.NodeIndex];

                tn.EdgeIndex = new Int64Container(item.NodeIndex, visited.EdgeIndex);
                GraphService.InitSubjectModel(tn, item.Edge);

                //nodelist.Add(new Int64ToInt32(item.NodeIndex, 0), tn); // Needed?
                currentNodes.Add(tn);
            }

            while (results.Count == 0)
            {
                var currentLevelNodes = new List<SubjectNode>();
                foreach (var tn in currentNodes)
                {
                    if (tn.NodeIndex == context.IssuerIndex)
                    {
                        results.Add(tn);
                        continue;
                    }

                    var parentNode = new SubjectNode();
                    parentNode.NodeIndex = tn.ParentIndex;

                    var visited = context.Visited[tn.NodeIndex];
                    parentNode.ParentIndex = context.Visited[tn.ParentIndex].ParentIndex;
                    parentNode.EdgeIndex = new Int64Container(parentNode.NodeIndex, visited.EdgeIndex);

                    if (nodelist.ContainsKey(parentNode.EdgeIndex))
                    {
                        // A previouse node in the collection has already created this
                        nodelist[parentNode.EdgeIndex].Children.Add(tn);
                        continue;
                    }

                    var address = GraphService.Graph.Address[parentNode.NodeIndex];
                    var edge = address.Edges[visited.EdgeIndex];
                    GraphService.InitSubjectModel(parentNode, edge);
                    parentNode.Children = new List<SubjectNode>();
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

            //context.Visited.Add(item.Index, 
            //    new VisitItem(item.ParentIndex, item.EdgeIndex, item.Cost)); // Makes sure that we do not run this block again.
            context.Visited[item.Index] = new VisitItem(item.ParentIndex, item.EdgeIndex); // Makes sure that we do not run this block again.
            var edges = GraphService.Graph.Address[item.Index].Edges;

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
            var node = GraphService.Graph.Address[item.Index];

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

                var visited = context.Visited[edges[i].SubjectId];
                if(visited.ParentIndex > -1) // If parentIndex is -1 then it has not been used yet!
                {
                    var parentAddress = GraphService.Graph.Address[visited.ParentIndex];
                    var visitedEdge = parentAddress.Edges[visited.EdgeIndex];
                    if (visitedEdge.Cost > edges[i].Cost) // If the current cost is lower then its a better route.
                        context.Visited[edges[i].SubjectId] = new VisitItem(item.Index, i); // Overwrite the old visit with the new because of lower cost

                    continue; // We have already done this node, so no need to reprocess.
                }

                list.Add(new QueueItem(edges[i].SubjectId, item.Index, i, edges[i].Cost));
            }
            return list;
        }


        private EdgeModel CreateEgdeQuery(GraphQuery query)
        {
            var edge = new EdgeModel();

            edge.SubjectId = GraphService.Graph.AddressIndex.ContainsKey(query.Subject) ? GraphService.Graph.AddressIndex[query.Subject] : -1;
            edge.SubjectType = GraphService.Graph.SubjectTypesIndex.ContainsKey(query.SubjectType) ? GraphService.Graph.SubjectTypesIndex[query.SubjectType] : -1;
            edge.Scope = (GraphService.Graph.ScopeIndex.ContainsKey(query.Scope)) ? GraphService.Graph.ScopeIndex[query.Scope] : -1;
            
            edge.Activate = query.Activate;
            edge.Expire = query.Expire;
            edge.Claim = ClaimStandardModel.Parse(query.Claim);

            return edge;
        }
    }
}
