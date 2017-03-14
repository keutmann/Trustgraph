using System;

namespace TrustgraphCore.Model
{
    [Flags]
    public enum ClaimFlag : byte
    {
        Clear = 1,
        Trust = 2,
        Confirm = 4
    }
}
