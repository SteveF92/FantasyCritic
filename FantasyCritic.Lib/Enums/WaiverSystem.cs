using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class AcquisitionSystem : TypeSafeEnum<AcquisitionSystem>
    {
        // Define values here.
        public static readonly AcquisitionSystem Manual = new AcquisitionSystem("Manual");

        // Constructor is private: values are defined within this class only!
        private AcquisitionSystem(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
