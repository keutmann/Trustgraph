using System.Collections.Generic;

namespace TrustgraphCore.Model
{
    public class QueryContext
    {
        public int IssuerIndex { get; set; }
        public EdgeModel Query { get; set; }
        public VisitItem[] Visited;
        public List<ResultNode> Results { get; set; }
        public int MaxCost { get; set; }
        public int Level { get; set; }
        public int MaxLevel { get; set; }
        public int TotalNodeCount = 0;
        public int TotalEdgeCount = 0;
        public int MatchEdgeCount = 0;

        public QueryContext(int addressCount)
        {
            Visited = new VisitItem[addressCount];
            var template = new VisitItem(-1, -1);
            for (int i = 0; i < addressCount; i++)
                Visited[i] = template;

            MaxCost = 600; // About 6 levels down
            Results = new List<ResultNode>();
            MaxLevel = 7;
        }
    }
}
