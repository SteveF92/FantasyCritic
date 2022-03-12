using FantasyCritic.Lib.Domain.Calculations;

namespace FantasyCritic.MySQL.Entities;

internal class PublisherGameUpdateEntity
{
    public PublisherGameUpdateEntity(KeyValuePair<Guid, PublisherGameCalculatedStats> keyValuePair)
    {
        PublisherGameID = keyValuePair.Key;
        FantasyPoints = keyValuePair.Value.FantasyPoints;
    }

    public Guid PublisherGameID { get; }
    public decimal? FantasyPoints { get; }
}
