using System.Globalization;
using CsvHelper.Configuration;
using NodaTime.Text;

namespace FantasyCritic.FakeRepo.TestUtilities;

/// <summary>
/// Test PublisherGames.csv does not include DraftID yet; ignore until LeagueDraft.csv / column split.
/// It has SlotNumber rather than LeagueSlotID; TestDataService links each game to its slot.
/// </summary>
internal sealed class PublisherGameEntityMap : ClassMap<TestPublisherGameEntity>
{
    private static readonly InstantPattern TimestampPattern = InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss");

    public PublisherGameEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.DraftID).Ignore();
        Map(m => m.LeagueSlotID).Ignore();
        Map(m => m.Timestamp).Convert(args => TimestampPattern.Parse(args.Row.GetField("Timestamp")!).GetValueOrThrow());
    }
}
