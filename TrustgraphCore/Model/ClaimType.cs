using System;

namespace TrustgraphCore.Model
{
    [Flags]
    public enum ClaimType : byte
    {
        Trust = 1,
        Confirm = 2,
        Rating = 4,
    }
}
