using System.Globalization;
using CsvHelper.Configuration;
using FantasyCritic.SharedSerialization.Database;

namespace FantasyCritic.Test.TestUtilities;
public sealed class LeagueYearEntityMap : ClassMap<LeagueYearEntity>
{
    public LeagueYearEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.DraftStartedTimestamp).Constant(null);
        Map(m => m.ReleaseSystem).Constant("MustBeReleased");
        Map(m => m.MightReleaseDroppableMonth).Constant(null);
        Map(m => m.MightReleaseDroppableDay).Constant(null);
    }
}
