using System.Runtime.InteropServices;

namespace TrustgraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NodeModel
    {
        public EdgeModel[] Edges;
    }
}
