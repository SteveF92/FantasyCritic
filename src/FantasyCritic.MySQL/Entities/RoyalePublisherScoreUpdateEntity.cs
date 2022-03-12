namespace FantasyCritic.MySQL.Entities;

internal class RoyalePublisherScoreUpdateEntity
{
    public RoyalePublisherScoreUpdateEntity(KeyValuePair<(Guid, Guid), decimal?> keyValuePair)
    {
        PublisherID = keyValuePair.Key.Item1;
        MasterGameID = keyValuePair.Key.Item2;
        FantasyPoints = keyValuePair.Value;
    }

    public Guid PublisherID { get; }
    public Guid MasterGameID { get; }
    public decimal? FantasyPoints { get; }
}