using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class PickupSystem : TypeSafeEnum<PickupSystem>
    {
        // Define values here.
        public static readonly PickupSystem SecretBidding = new PickupSystem("SecretBidding");
        public static readonly PickupSystem SemiPublicBidding = new PickupSystem("SemiPublicBidding");

        // Constructor is private: values are defined within this class only!
        private PickupSystem(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
