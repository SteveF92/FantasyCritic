using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class PickupSystem : TypeSafeEnum<PickupSystem>
    {

        // Define values here.
        public static readonly PickupSystem SecretBidding = new PickupSystem("SecretBidding", "Secret Bidding");
        public static readonly PickupSystem SemiPublicBidding = new PickupSystem("SemiPublicBidding", "Public Bidding");

        // Constructor is private: values are defined within this class only!
        private PickupSystem(string value, string readableName)
            : base(value)
        {
            ReadableName = readableName;
        }

        public string ReadableName { get; }


        public override string ToString() => Value;
    }
}
