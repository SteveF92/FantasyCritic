using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Enums
{
    public class TiebreakSystem : TypeSafeEnum<TiebreakSystem>
    {

        // Define values here.
        public static readonly TiebreakSystem LowestProjectedPoints = new TiebreakSystem("LowestProjectedPoints");
        public static readonly TiebreakSystem EarliestBid = new TiebreakSystem("EarliestBid");

        // Constructor is private: values are defined within this class only!
        private TiebreakSystem(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
