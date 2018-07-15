using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class EligibilitySystem : TypeSafeEnum<EligibilitySystem>
    {
        // Define values here.
        public static readonly EligibilitySystem Unlimited = new EligibilitySystem("Unlimited");

        // Constructor is private: values are defined within this class only!
        private EligibilitySystem(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
