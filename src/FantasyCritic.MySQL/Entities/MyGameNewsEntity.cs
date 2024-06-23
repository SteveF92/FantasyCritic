namespace FantasyCritic.MySQL.Entities;
internal class MyGameNewsEntity
{
    public Guid MasterGameID { get; set; }
    public bool CounterPick { get; set; }
    public Guid LeagueID { get; set; }
    public string LeagueName { get; set; } = null!;
    public int Year { get; set; }
    public Guid PublisherID { get; set; }
    public string PublisherName { get; set; } = null!;

    public static IReadOnlyList<SingleGameNews> BuildMyGameNewsFromEntities(IEnumerable<MyGameNewsEntity> myGameNewsEntities, IReadOnlyDictionary<MasterGameYearKey, MasterGameYear> masterGameYears)
    {
        List<SingleGameNews> domains = new List<SingleGameNews>();

        var groupedByMasterGame = myGameNewsEntities.GroupBy(x => x.MasterGameID);

        foreach (var masterGameGroup in groupedByMasterGame)
        {
            var highestYear = masterGameGroup.Select(x => x.Year).Max();
            var masterGameYear = masterGameYears[new MasterGameYearKey(masterGameGroup.Key, highestYear)];
            var publisherInfos = masterGameGroup.Select(x => new PublisherInfo(x.LeagueID, x.LeagueName, x.Year, x.PublisherID, x.PublisherName, x.CounterPick)).ToList();
            domains.Add(new SingleGameNews(masterGameYear, publisherInfos));
        }

        return domains;
    }
}
