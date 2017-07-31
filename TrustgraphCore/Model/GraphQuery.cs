using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TrustgraphCore.Model
{
    public class GraphQuery
    {
        public List<byte[]> Issuers;
        public List<SubjectQuery> Subjects;
        //public byte[] Subject;
        //public string SubjectType;
        public string Scope;
        public uint Activate;
        public uint Expire;
        public JObject Claim;
    }
}
