using System.Collections.Generic;

namespace TrustgraphCore.Model
{
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
}
