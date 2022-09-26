using System.Globalization;
using CsvHelper.Configuration;
using FantasyCritic.SharedSerialization;

namespace FantasyCritic.Test.TestUtilities;
public sealed class LeagueYearEntityMap : ClassMap<LeagueYearEntity>
{
    public LeagueYearEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.DraftStartedTimestamp).Constant(null);
    }
}
