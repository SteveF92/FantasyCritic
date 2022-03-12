using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Enums
{
    public class TradingSystem : TypeSafeEnum<TradingSystem>
    {

        // Define values here.
        public static readonly TradingSystem NoTrades = new TradingSystem("NoTrades", "No Trades");
        public static readonly TradingSystem Standard = new TradingSystem("Standard", "Standard");

        // Constructor is private: values are defined within this class only!
        private TradingSystem(string value, string readableName)
            : base(value)
        {
            ReadableName = readableName;
        }

        public string ReadableName { get; }
        public override string ToString() => Value;
    }
}
