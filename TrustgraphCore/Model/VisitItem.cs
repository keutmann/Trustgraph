using System.Runtime.InteropServices;

namespace TrustgraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VisitItem
    {
        public int ParentIndex;
        public int EdgeIndex;
        //public int Cost;

        public VisitItem(int parentIndex, int edgeIndex)
        {
            ParentIndex = parentIndex;
            EdgeIndex = edgeIndex;
            //Cost = cost;
        }
    }
}
