using System;
using System.Runtime.InteropServices;

namespace TrustgraphCore.Model
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Int64Container
    {
        [FieldOffset(0)]
        public Int64 Int64Value;
        [FieldOffset(0)]
        public Int32 LeftInt32;
        [FieldOffset(4)]
        public Int32 RightInt32;

        public Int64Container(Int64 value)
        {
            LeftInt32 = 0;
            RightInt32 = 0;
            Int64Value = value;
        }

        public Int64Container(Int32 left, Int32 right)
        {
            Int64Value = 0;
            LeftInt32 = left;
            RightInt32 = right;
        }
    }
}
