using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using TrustchainCore.Extensions;

namespace TrustgraphCore.Model
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ClaimStandardModel
    {
        public ClaimType Types; // What claims has been made
        public ClaimType Flags; // Collection of claims that are boolean
        public byte Rating;     // Used for trust that use rating

        public static ClaimStandardModel Parse(JObject claim)
        {
            var result = new ClaimStandardModel();
            var claimType = typeof(ClaimType);
            var names = Enum.GetNames(claimType);
            foreach (var name in names)
            {
                var token = claim.GetValue(name, StringComparison.OrdinalIgnoreCase);
                if (token == null)
                    continue;

                if (token.Type == JTokenType.Null)
                    continue;

                var ct = (ClaimType)Enum.Parse(claimType, name);
                result.Types |= ct; // The claim has been defined

                if (token.Type == JTokenType.Boolean)
                {
                    var val = token.ToBoolean();
                    if (val)
                        result.Flags |= ct; // Set it to true in flags!
                }

                if (ct == ClaimType.Rating && token.Type == JTokenType.Integer)
                    result.Rating = (byte)token.ToInteger();
            }

            return result;
        }

        public JObject ConvertToJObject()
        {
            var result = new JObject();

            if((Types & ClaimType.Trust) != 0)
                result.Add(new JProperty("trust", (Flags & ClaimType.Trust) != 0));

            if ((Types & ClaimType.Confirm) != 0)
                result.Add(new JProperty("confirm", (Flags & ClaimType.Confirm) != 0));

            if ((Types & ClaimType.Rating) != 0)
                result.Add(new JProperty("rating", Rating));

            return result;
        }
    }
}
