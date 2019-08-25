using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Royale
{
    //TODO need to implement the interfaces better. Use resharper
    public class YearQuarter : IEquatable<YearQuarter>, IComparable<YearQuarter>
    {
        public YearQuarter(int year, int quarter)
        {
            Year = year;
            Quarter = quarter;
        }

        public int Year { get; }
        public int Quarter { get; }

        public int CompareTo(YearQuarter other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(YearQuarter other)
        {
            throw new NotImplementedException();
        }
    }
}
