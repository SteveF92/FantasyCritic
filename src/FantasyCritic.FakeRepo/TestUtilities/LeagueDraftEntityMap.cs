using System;
using System.Globalization;
using CsvHelper.Configuration;
using FantasyCritic.Lib.SharedSerialization.Database;
using NodaTime.Text;

namespace FantasyCritic.FakeRepo.TestUtilities;

/// <summary>
/// Maps LeagueYears.csv draft columns onto LeagueDraftEntity.
/// Phase 1 reads LeagueYears.csv; later switch the reader path to LeagueDraft.csv only.
/// </summary>
public sealed class LeagueDraftEntityMap : ClassMap<LeagueDraftEntity>
{
    private static readonly InstantPattern TimestampPattern = InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss");

    public LeagueDraftEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.DraftID).Convert(args =>
        {
            var leagueID = Guid.Parse(args.Row.GetField("LeagueID")!);
            var year = int.Parse(args.Row.GetField("Year")!, CultureInfo.InvariantCulture);
            return TestLeagueDraftIds.For(leagueID, year);
        });
        Map(m => m.DraftNumber).Constant(1);
        Map(m => m.Name).Constant("Initial Draft");
        Map(m => m.ScheduledDate).Constant(null);
        Map(m => m.CounterPicksMustBeFromThisDraft).Constant(true);
        Map(m => m.DraftOrderSet).Convert(args => args.Row.GetField("DraftOrderSet") == "1");
        Map(m => m.DraftStartedTimestamp).Convert(args =>
        {
            var value = args.Row.GetField("DraftStartedTimestamp");
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return TimestampPattern.Parse(value).GetValueOrThrow();
        });
    }
}
