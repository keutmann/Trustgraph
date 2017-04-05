using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrustgraphCore.Model
{
    /// <summary>
    /// The result of the query
    /// </summary>
    public class ResultContext
    {
        public int TotalNodeCount = 0;
        public int TotalEdgeCount = 0;
        public int MatchEdgeCount = 0;

        [JsonProperty(PropertyName = "nodes", NullValueHandling = NullValueHandling.Ignore, Order = 100)]
        public List<SubjectNode> Nodes { get; set; }
    }
}
