using System.Runtime.InteropServices;

namespace TrustgraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct QueueItem
    {
        public int Index;
        public int ParentIndex;
        public int EdgeIndex;
        public int Cost;

        public QueueItem(int index, int parentIndex, int edgeIndex, int cost)
        {
            Index = index;
            ParentIndex = parentIndex;
            EdgeIndex = edgeIndex;
            Cost = cost;
        }
    }
}
