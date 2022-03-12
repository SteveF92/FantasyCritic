using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace FantasyCritic.Lib.Royale
{
    public class YearQuarter : IEquatable<YearQuarter>, IComparable<YearQuarter>
    {
        public YearQuarter(int year, int quarter)
        {
            Year = year;
            Quarter = quarter;
        }

        public int Year { get; }
        public int Quarter { get; }

        public LocalDate FirstDateOfQuarter => new LocalDate(Year, (Quarter - 1) * 3 + 1, 1);

        public LocalDate LastDateOfQuarter => NextQuarter.FirstDateOfQuarter.PlusDays(-1);

        public override string ToString()
        {
            return $"{Year}-Q{Quarter}";
        }

        public YearQuarter LastQuarter
        {
            get
            {
                if (Quarter == 1)
                {
                    return new YearQuarter(Year - 1, 4);
                }
                return new YearQuarter(Year, Quarter - 1);
            }
        }

        public YearQuarter NextQuarter
        {
            get
            {
                if (Quarter == 4)
                {
                    return new YearQuarter(Year + 1, 1);
                }
                return new YearQuarter(Year, Quarter + 1);
            }
        }

        public bool Equals(YearQuarter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Year == other.Year && Quarter == other.Quarter;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((YearQuarter)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Year * 397) ^ Quarter;
            }
        }

        public int CompareTo(YearQuarter other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return FirstDateOfQuarter.CompareTo(other.FirstDateOfQuarter);
        }
    }
}
