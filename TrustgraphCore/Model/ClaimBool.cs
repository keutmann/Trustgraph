using System;

namespace TrustpathCore.Model
{
    [Flags]
    public enum ClaimBool : byte
    {
        Trust = 1,
        Confirm = 2
    }
}
