using System.Collections.Generic;
using TrustpathCore.Model;

namespace TrustpathCore.Data
{
    public interface IGraphContext
    {
        HashSet<string> FilesLoaded { get; set; }
        GraphModel Graph { get; set; }
    }
}