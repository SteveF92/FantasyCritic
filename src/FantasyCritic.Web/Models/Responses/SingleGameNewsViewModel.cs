namespace FantasyCritic.Web.Models.Responses;

public class SingleGameNewsViewModel
{
    public SingleGameNewsViewModel(MasterGameYear masterGame, IEnumerable<Publisher> publishers, IEnumerable<Publisher> standardGamePublishers, bool userMode, LocalDate currentDate)
    {
        MasterGame = new MasterGameYearViewModel(masterGame, currentDate);
        MasterGameID = masterGame.MasterGame.MasterGameID;
        GameName = masterGame.MasterGame.GameName;
        EstimatedReleaseDate = masterGame.MasterGame.EstimatedReleaseDate;
        MaximumReleaseDate = masterGame.MasterGame.GetDefiniteMaximumReleaseDate();
        ReleaseDate = masterGame.MasterGame.ReleaseDate;

        if (userMode)
        {
            if (publishers.Count() == 1)
            {
                var publisher = publishers.Single();
                LeagueID = publisher.LeagueYear.League.LeagueID;
                Year = publisher.LeagueYear.Year;
                LeagueName = publisher.LeagueYear.League.LeagueName;
                PublisherID = publisher.PublisherID;
                PublisherName = publisher.PublisherName;
            }
            else
            {
                LeagueName = $"{publishers.Count()} Leagues";
                PublisherName = "Multiple";
            }
        }
        else
        {
            if (publishers.Count() == 1)
            {
                var publisher = publishers.Single();
                LeagueID = publisher.LeagueYear.League.LeagueID;
                Year = publisher.LeagueYear.Year;
                LeagueName = publisher.LeagueYear.League.LeagueName;
                PublisherID = publisher.PublisherID;
                PublisherName = publisher.PublisherName;
            }
            else if (standardGamePublishers.Count() == 1)
            {
                var publisher = standardGamePublishers.Single();
                var counterPickPublisher = publishers.Single(x => x.PublisherID != publisher.PublisherID);
                LeagueID = publisher.LeagueYear.League.LeagueID;
                Year = publisher.LeagueYear.Year;
                LeagueName = publisher.LeagueYear.League.LeagueName;
                PublisherID = publisher.PublisherID;
                PublisherName = publisher.PublisherName;
                CounterPickPublisherID = counterPickPublisher.PublisherID;
                CounterPickPublisherName = counterPickPublisher.PublisherName;
            }
            else
            {
                throw new Exception($"Problem with upcoming games. Happened for Game: {masterGame.MasterGame.GameName} and publisherIDs: {string.Join('|', publishers.Select(x => x.PublisherID))}");
            }
        }
    }

    public MasterGameYearViewModel MasterGame { get; }
    public Guid MasterGameID { get; }
    public string GameName { get; }
    public string EstimatedReleaseDate { get; }
    public LocalDate MaximumReleaseDate { get; }
    public LocalDate? ReleaseDate { get; }
    public Guid? LeagueID { get; }
    public int? Year { get; }
    public string LeagueName { get; }
    public Guid? PublisherID { get; }
    public string PublisherName { get; }
    public Guid? CounterPickPublisherID { get; }
    public string CounterPickPublisherName { get; }
}
