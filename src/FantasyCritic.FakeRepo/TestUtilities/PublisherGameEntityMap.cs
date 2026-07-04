using System.Globalization;
using CsvHelper.Configuration;
using FantasyCritic.Lib.SharedSerialization.Database;
using NodaTime.Text;

namespace FantasyCritic.FakeRepo.TestUtilities;

/// <summary>
/// Test PublisherGames.csv does not include DraftID yet; ignore until LeagueDraft.csv / column split.
/// </summary>
public sealed class PublisherGameEntityMap : ClassMap<PublisherGameEntity>
{
    private static readonly InstantPattern TimestampPattern = InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss");

    public PublisherGameEntityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.DraftID).Ignore();
        Map(m => m.Timestamp).Convert(args => TimestampPattern.Parse(args.Row.GetField("Timestamp")!).GetValueOrThrow());
    }
}
