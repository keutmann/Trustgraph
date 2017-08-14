using System;
using System.Linq;
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

        public QueryContext Query(RequestQuery query)
        {
            Verify(query);

            var context = new QueryContext(GraphService, query);
            
            if(context.IssuerIndex.Count > 0 && context.TargetIndex.Count > 0)
                ExecuteQuery(context);

            if (context.Results.Count > 0)
                context.Nodes = BuildResultNode(context);//result.Node = BuildResultTree(context);

            return context;
        }

        public void Verify(RequestQuery query)
        {
            //if (query.Issuer.Length != 20)
            //    throw new ApplicationException("Invalid byte length on Issuer");

            // Script definition specifies this
            //if (query.Subjects.Length != 20)
            //    throw new ApplicationException("Invalid byte length on Issuer");
        }

        public List<SubjectNode> BuildResultNode(QueryContext context)
        {
            var results = new List<SubjectNode>();
            var nodelist = new Dictionary<Int64Container, SubjectNode>();
            var subjectNodes = new List<SubjectNode>();
            foreach (var item in context.Results)
            {
                var tn = new SubjectNode();
                tn.NodeIndex = item.Edge.SubjectId;
                tn.ParentIndex = item.NodeIndex;
                var visited = context.Visited[item.NodeIndex];

                tn.EdgeIndex = new Int64Container(item.NodeIndex, visited.EdgeIndex);
                GraphService.InitSubjectModel(tn, item.Edge);

                subjectNodes.Add(tn);
            }

            while (results.Count == 0)
            {
                var currentLevelNodes = new List<SubjectNode>();
                foreach (var subject in subjectNodes)
                {
                    var parentNode = new SubjectNode();
                    parentNode.NodeIndex = subject.ParentIndex;

                    var visited = context.Visited[subject.NodeIndex];
                    parentNode.ParentIndex = context.Visited[subject.ParentIndex].ParentIndex;
                    parentNode.EdgeIndex = new Int64Container(parentNode.NodeIndex, visited.EdgeIndex);

                    if (nodelist.ContainsKey(parentNode.EdgeIndex))
                    {
                        // A previouse node in the collection has already created this
                        nodelist[parentNode.EdgeIndex].Nodes.Add(subject);
                        continue;
                    }

                    var address = GraphService.Graph.Address[parentNode.NodeIndex];
                    parentNode.Id = address.Id;

                    if (visited.EdgeIndex >= 0)
                    {
                        var edge = address.Edges[visited.EdgeIndex];
                        GraphService.InitSubjectModel(parentNode, edge);
                    }
                    parentNode.Nodes = new List<SubjectNode>();
                    parentNode.Nodes.Add(subject);

                    currentLevelNodes.Add(parentNode);
                    nodelist.Add(parentNode.EdgeIndex, parentNode);

                    if (context.IssuerIndex.Contains(parentNode.NodeIndex))
                    {
                        results.Add(parentNode);
                        continue;
                    }

                }
                subjectNodes = currentLevelNodes;
            }

            return results;
        }



        public void ExecuteQuery(QueryContext context)
        {
            List<QueueItem> queue = new List<QueueItem>();
            foreach (var index in context.IssuerIndex)
                queue.Add(new QueueItem(index, -1, -1, 0)); // Starting point!

            while (queue.Count > 0 && context.Level < 4) // Do go more than 4 levels down
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
            int found = 0;
            context.SetVisitItemSafely(item.Index, new VisitItem(item.ParentIndex, item.EdgeIndex)); // Makes sure that we do not run this block again.
            var edges = GraphService.Graph.Address[item.Index].Edges;
            if (edges == null)
                return false;

            for (var i = 0; i < edges.Length; i++)
            {
                context.TotalEdgeCount++;

                if (edges[i].Activate > UnixTime ||
                   (edges[i].Expire > 0 && edges[i].Expire < UnixTime))
                    continue;

                if ((edges[i].Claim.Types & context.Claim.Types) == 0)
                    continue; 

                //if (edges[i].SubjectType != context.Query.SubjectType ||
                if(context.Scope != 0 &&
                    edges[i].Scope != context.Scope)
                    continue; // No claims match query

                context.MatchEdgeCount++;

                for(var t = 0; t < context.TargetIndex.Count; t++) 
                    if (context.TargetIndex[t].Id == edges[i].SubjectId)
                    {
                        var result = new ResultNode();
                        result.NodeIndex = item.Index;
                        result.ParentIndex = item.ParentIndex;
                        result.Edge = edges[i];
                        context.Results.Add(result);
                        found ++;
                        if (found >= context.TargetIndex.Count) // Do not look further, because we found them all.
                            return true;
                    }
            }

            return found != 0;
        }

        public List<QueueItem> Enqueue(QueueItem item, QueryContext context)
        {
            var list = new List<QueueItem>();
            var address = GraphService.Graph.Address[item.Index];

            var edges = address.Edges;
            if (edges == null)
                return list;

            for (var i = 0; i < edges.Length; i++)
            {
                //if (edges[i].SubjectType != context.Query.SubjectType ||
                if(context.Scope != 0 && 
                    edges[i].Scope != context.Scope)
                    continue; // Do not follow when Trust do not match scope or SubjectType

                if (edges[i].Activate > UnixTime ||
                    (edges[i].Expire > 0 && edges[i].Expire < UnixTime)) 
                    continue; // Do not follow when Trust has not activated or has expired

                if ((edges[i].Claim.Flags & ClaimType.Trust) == 0)
                    continue; // Do not follow when trust is false or do not exist.

                var visited = context.GetVisitItemSafely(edges[i].SubjectId);
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
    }
}

