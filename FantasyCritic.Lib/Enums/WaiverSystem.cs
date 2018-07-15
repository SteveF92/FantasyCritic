using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class WaiverSystem : TypeSafeEnum<WaiverSystem>
    {
        // Define values here.
        public static readonly WaiverSystem Manual = new WaiverSystem("Manual");

        // Constructor is private: values are defined within this class only!
        private WaiverSystem(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
