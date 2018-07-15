using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class ScoringSystem : TypeSafeEnum<ScoringSystem>
    {
        // Define values here.
        public static readonly ScoringSystem Manual = new ScoringSystem("Manual");

        // Constructor is private: values are defined within this class only!
        private ScoringSystem(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
