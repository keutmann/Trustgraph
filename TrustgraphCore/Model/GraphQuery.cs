using Newtonsoft.Json.Linq;

namespace TrustgraphCore.Model
{
    public class GraphQuery
    {
        public byte[] Issuer;
        public byte[] Subject;
        public string SubjectType;
        public string Scope;
        public uint Activate;
        public uint Expire;
        public JObject Claim;
    }
}
