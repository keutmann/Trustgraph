using System;
using System.Collections.Generic;
using System.Linq;
using TrustchainCore.Collections.Generic;

namespace TrustgraphCore.Model
{
    public class GraphModel
    {
        public List<NodeModel> Nodes = new List<NodeModel>();
        public Dictionary<byte[], int> NodeIndex = new Dictionary<byte[], int>(new ByteArrayComparer());

        public Dictionary<string, int> SubjectTypesIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, int> ScopeIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public GraphModel()
        {
            SubjectTypesIndex.Add("", 0);
            ScopeIndex.Add("", 0);
        }
    }
}
