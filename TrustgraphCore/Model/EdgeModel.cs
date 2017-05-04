using System;
using System.Runtime.InteropServices;

namespace TrustgraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EdgeModel
    {
        public Int32 SubjectType; // Use lookup table to handle subject type
        public Int32 SubjectId; // The type of the subject
        public UInt32 Activate; // When to begin consider the trust
        public UInt32 Expire;    // When the trust expire
        public short Cost;  // cost of following the trust, lower the better
        public Int32 Scope; // scope of the trust
        public Int32 Timestamp; // The timestamp of the trust
        public ClaimStandardModel Claim; // Claims 
    }
}
