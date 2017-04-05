using Newtonsoft.Json.Linq;

namespace TrustgraphCore.Model
{
    public class GraphQuery
    {
        public byte[] Issuer;
        public byte[] Subject;
        public string SubjectType;
        public string Scope;
        public int Activate;
        public int Expire;
        public JObject Claim;
    }
}
