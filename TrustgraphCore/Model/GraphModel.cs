using System;
using System.Collections.Generic;

namespace TrustgraphCore.Model
{
    public class GraphModel
    {
        public List<NodeModel> Nodes = new List<NodeModel>();
        public Dictionary<byte[], int> IssuerIdIndex = new Dictionary<byte[], int>();

        public Dictionary<string, int> SubjectTypesIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, short> ScopeIndex = new Dictionary<string, short>(StringComparer.OrdinalIgnoreCase);
    }
}
