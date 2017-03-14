using System.Runtime.InteropServices;

namespace TrustgraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ClaimStandardModel
    {
        public ClaimType Types; // What claims has been made
        public ClaimType Flags; // Collection of claims that are boolean
        public byte Rating;     // Used for trust that use rating
    }
}
