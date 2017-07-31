using System;
using System.Collections.Generic;
using TrustgraphCore.Data;
using TrustchainCore.Extensions;

namespace TrustgraphCore.Model
{
    public class QueryContext
    {
        public IGraphContext GraphService { get; set; }

        public List<int> IssuerIndex { get; set; }
        public EdgeModel Query { get; set; }
        public VisitItem[] Visited = null;
        public List<ResultNode> Results { get; set; }
        public int MaxCost { get; set; }
        public int Level { get; set; }
        public int MaxLevel { get; set; }
        public int TotalNodeCount = 0;
        public int TotalEdgeCount = 0;
        public int MatchEdgeCount = 0;

        public QueryContext(int addressCount)
        {
            IssuerIndex = new List<int>();

            InitializeVisited(addressCount);

            MaxCost = 600; // About 6 levels down
            Results = new List<ResultNode>();
            MaxLevel = 7;
        }

        public QueryContext(IGraphContext graphService, GraphQuery query) : this(graphService.Graph.Address.Count)
        {
            GraphService = graphService;
            if(query.Issuers == null || query.Issuers.Count == 0)
                throw new ApplicationException("Missing issuers");

            foreach (var issuer in query.Issuers)
            {
                var index = GraphService.Graph.AddressIndex.ContainsKey(issuer) ? GraphService.Graph.AddressIndex[issuer] : -1;
                if (index == -1)
                    throw new ApplicationException("Unknown issuer id " + issuer.ConvertToHex());

                IssuerIndex.Add(index);
            }

            Query = CreateEgdeQuery(query);
            if (Query.SubjectId == -1)
                throw new ApplicationException("Unknown subject id");
        }


        /// <summary>
        /// Get a Visit item 
        /// If a index is larger than the array then rebuild the array to fit the index size.
        /// This may happen during the Query and new Trusts are added.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VisitItem GetVisitItemSafely(int index)
        {
            if (index >= Visited.Length)
                InitializeVisited(index + 1);

            return Visited[index]; 
        }

        public void SetVisitItemSafely(int index, VisitItem item)
        {
            if (index >= Visited.Length)
                InitializeVisited(index + 1);

            Visited[index] = item;
        }


        private void InitializeVisited(int count)
        {
            var template = new VisitItem(-1, -1);
            var index = 0;
            if (Visited != null) // Make sure to copy the old data to the new array
            {
                var tempArray = new VisitItem[count];
                Visited.CopyTo(tempArray, 0);
                index = Visited.Length;
                Visited = tempArray;
            }
            else
                Visited = new VisitItem[count];

            for (int i = index; i < count; i++)
                Visited[i] = template;
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
