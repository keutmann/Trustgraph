﻿using System;

namespace TrustgraphCore.Model
{
    [Flags]
    public enum ClaimType : byte
    {
        Clear = 1,
        Trust = 2,
        Confirm = 4,
        Rating = 8,
    }
}
