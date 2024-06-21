using FantasyCritic.Lib.Domain.Combinations;

namespace FantasyCritic.Web.Models.Responses;

public class SingleGameNewsPublisherViewModel
{
    //For User Mode
    public SingleGameNewsPublisherViewModel(LeagueYearPublisherPair publisherPair)
    {
        LeagueID = publisherPair.LeagueYear.League.LeagueID;
        Year = publisherPair.LeagueYear.Year;
        LeagueName = publisherPair.LeagueYear.League.LeagueName;
        PublisherID = publisherPair.Publisher.PublisherID;
        PublisherName = publisherPair.Publisher.PublisherName;
    }

    public SingleGameNewsPublisherViewModel(PublisherInfo publisherInfo)
    {
        LeagueID = publisherInfo.LeagueID;
        Year = publisherInfo.Year;
        LeagueName = publisherInfo.LeagueName;
        PublisherID = publisherInfo.PublisherID;
        PublisherName = publisherInfo.PublisherName;
    }

    //For League Mode
    public SingleGameNewsPublisherViewModel(MasterGame masterGame, IReadOnlyList<LeagueYearPublisherPair> publisherPairs)
    {
        if (publisherPairs.Count == 1)
        {
            var publisherPair = publisherPairs.Single();
            LeagueID = publisherPair.LeagueYear.League.LeagueID;
            Year = publisherPair.LeagueYear.Year;
            LeagueName = publisherPair.LeagueYear.League.LeagueName;
            PublisherID = publisherPair.Publisher.PublisherID;
            PublisherName = publisherPair.Publisher.PublisherName;
        }
        else if (publisherPairs.Count == 2)
        {
            var standardPublisherPair = publisherPairs.Single(x => x.Publisher.PublisherGames.Where(y => !y.CounterPick).Where(y => y.MasterGame is not null).Any(y => y.MasterGame!.MasterGame.MasterGameID == masterGame.MasterGameID));
            var counterPickPublisherPair = publisherPairs.Single(x => x.Publisher.PublisherID != standardPublisherPair.Publisher.PublisherID);
            LeagueID = standardPublisherPair.LeagueYear.League.LeagueID;
            Year = standardPublisherPair.LeagueYear.Year;
            LeagueName = standardPublisherPair.LeagueYear.League.LeagueName;
            PublisherID = standardPublisherPair.Publisher.PublisherID;
            PublisherName = standardPublisherPair.Publisher.PublisherName;
            CounterPickPublisherID = counterPickPublisherPair.Publisher.PublisherID;
            CounterPickPublisherName = counterPickPublisherPair.Publisher.PublisherName;
        }
        else
        {
            throw new Exception($"Problem with upcoming games. Happened for Game: {masterGame.GameName} and publisherIDs: {string.Join('|', publisherPairs.Select(x => x.Publisher.PublisherID))}");
        }
    }

    public Guid? LeagueID { get; }
    public int? Year { get; }
    public string LeagueName { get; }
    public Guid? PublisherID { get; }
    public string PublisherName { get; }
    public Guid? CounterPickPublisherID { get; }
    public string? CounterPickPublisherName { get; }
}
