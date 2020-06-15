using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class PickupSystem : TypeSafeEnum<PickupSystem>
    {
        // Define values here.
        public static readonly PickupSystem Manual = new PickupSystem("Manual");
        public static readonly PickupSystem Budget = new PickupSystem("Budget");

        // Constructor is private: values are defined within this class only!
        private PickupSystem(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
