using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Model;

namespace TrustgraphCore.Model
{
    public class SubjectNode : SubjectModel
    {
        [JsonIgnore]
        public int NodeIndex { get; set; }

        [JsonIgnore]
        public int ParentIndex { get; set; }

        [JsonIgnore]
        public Int64Container EdgeIndex { get; set; }

        [JsonProperty(PropertyName = "nodes", NullValueHandling = NullValueHandling.Ignore, Order = 100)]
        public List<SubjectNode> Nodes { get; set; }

        public SubjectNode() : base()
        {
        }
    }
}
