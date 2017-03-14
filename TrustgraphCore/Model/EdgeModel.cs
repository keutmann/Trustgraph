using System;
using System.Runtime.InteropServices;

namespace TrustpathCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EdgeModel
    {
        public short SubjectType; // Use lookup table to handle subject type
        public Int32 SubjectId;
        public ClaimType Types;
        public ClaimBool Claim;
        public byte Rating;
        public Int32 Activate;
        public Int32 Expire;
        public short Cost;
        public short Scope;
    }
}
