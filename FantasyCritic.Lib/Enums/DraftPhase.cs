using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class DraftPhase : TypeSafeEnum<DraftPhase>
    {
        // Define values here.
        public static readonly DraftPhase StandardGames = new DraftPhase("StandardGames");
        public static readonly DraftPhase Counterpicks = new DraftPhase("Counterpicks");
        public static readonly DraftPhase Complete = new DraftPhase("Complete");

        // Constructor is private: values are defined within this class only!
        private DraftPhase(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
