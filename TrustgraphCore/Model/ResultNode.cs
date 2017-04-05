using System.Runtime.InteropServices;

namespace TrustgraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ResultNode
    {
        public int NodeIndex { get; set; }
        public int ParentIndex { get; set; }
        public EdgeModel Edge { get; set; }
    }
}
