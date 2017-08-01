using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrustgraphCore.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SubjectQuery
    {
        [JsonProperty(PropertyName = "id")]
        public byte[] Id;
        [JsonProperty(PropertyName = "type")]
        public string Type;
    }
}
