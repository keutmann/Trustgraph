using System.Collections.Generic;
using TrustgraphCore.Model;

namespace TrustgraphCore.Data
{
    public interface IGraphContext
    {
        HashSet<string> FilesLoaded { get; set; }
        GraphModel Graph { get; set; }
    }
}