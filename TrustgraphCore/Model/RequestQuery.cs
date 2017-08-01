using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TrustgraphCore.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RequestQuery
    {
        [JsonProperty(PropertyName = "issuers")]
        public List<byte[]> Issuers;

        [JsonProperty(PropertyName = "subjects")]
        public List<SubjectQuery> Subjects;

        //[JsonProperty(PropertyName = "subjecttype")]
        //public string subjecttype;

        [JsonProperty(PropertyName = "scope")]
        public string Scope;

        [JsonProperty(PropertyName = "claim")]
        public JObject Claim;
    }
}
