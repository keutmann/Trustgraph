using System.Runtime.InteropServices;

namespace TrustpathCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NodeModel
    {
        public EdgeModel[] Edges;
    }
}
