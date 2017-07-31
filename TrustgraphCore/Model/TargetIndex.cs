using System.Runtime.InteropServices;

namespace TrustgraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TargetIndex
    {
        public int Id; // The type of the subject
        public int Type; // Use lookup table to handle subject type
    }
}
