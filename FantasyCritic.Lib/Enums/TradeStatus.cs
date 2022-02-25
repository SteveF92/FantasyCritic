using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class TradeStatus : TypeSafeEnum<TradeStatus>
    {
        // Define values here.
        public static readonly TradeStatus Accepted = new TradeStatus("Accepted");
        public static readonly TradeStatus Executed = new TradeStatus("Executed");
        public static readonly TradeStatus Proposed = new TradeStatus("Proposed");
        public static readonly TradeStatus RejectedByCounterParty = new TradeStatus("RejectedByCounterParty");
        public static readonly TradeStatus RejectedByManager = new TradeStatus("RejectedByManager");
        public static readonly TradeStatus Rescinded = new TradeStatus("Rescinded");

        // Constructor is private: values are defined within this class only!
        private TradeStatus(string value)
            : base(value)
        {

        }

        public override string ToString() => Value;
    }
}
