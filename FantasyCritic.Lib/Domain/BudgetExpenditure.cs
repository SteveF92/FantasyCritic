using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class BudgetExpenditure
    {
        public BudgetExpenditure(Publisher publisher, uint value)
        {
            Publisher = publisher;
            Value = value;
        }

        public Publisher Publisher { get; }
        public uint Value { get; }
    }
}
