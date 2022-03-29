namespace FantasyCritic.Web.Models.Responses;

public class SingleGameNewsViewModel
{
    public SingleGameNewsViewModel(MasterGameYear masterGame, IReadOnlyList<LeagueYearPublisherPair> publishersPairsThatHaveGame, bool userMode, LocalDate currentDate)
    {
        MasterGame = new MasterGameYearViewModel(masterGame, currentDate);
        MasterGameID = masterGame.MasterGame.MasterGameID;
        GameName = masterGame.MasterGame.GameName;
        EstimatedReleaseDate = masterGame.MasterGame.EstimatedReleaseDate;
        MaximumReleaseDate = masterGame.MasterGame.GetDefiniteMaximumReleaseDate();
        ReleaseDate = masterGame.MasterGame.ReleaseDate;

        if (userMode)
        {
            if (publishersPairsThatHaveGame.Count() == 1)
            {
                var publisherPair = publishersPairsThatHaveGame.Single();
                LeagueID = publisherPair.LeagueYear.League.LeagueID;
                Year = publisherPair.LeagueYear.Year;
                LeagueName = publisherPair.LeagueYear.League.LeagueName;
                PublisherID = publisherPair.Publisher.PublisherID;
                PublisherName = publisherPair.Publisher.PublisherName;
            }
            else
            {
                LeagueName = $"{publishersPairsThatHaveGame.Count()} Leagues";
                PublisherName = "Multiple";
            }
        }
        else
        {
            if (publishersPairsThatHaveGame.Count() == 1)
            {
                var publisherPair = publishersPairsThatHaveGame.Single();
                LeagueID = publisherPair.LeagueYear.League.LeagueID;
                Year = publisherPair.LeagueYear.Year;
                LeagueName = publisherPair.LeagueYear.League.LeagueName;
                PublisherID = publisherPair.Publisher.PublisherID;
                PublisherName = publisherPair.Publisher.PublisherName;
            }
            else if (publishersPairsThatHaveGame.Count() == 2)
            {
                var standardPublisherPair = publishersPairsThatHaveGame.Single(x => x.Publisher.PublisherGames.Where(x => !x.CounterPick).Where(x => x.MasterGame.HasValueTempoTemp).Any(x => x.MasterGame.ValueTempoTemp.MasterGame.MasterGameID == masterGame.MasterGame.MasterGameID));
                var counterPickPublisherPair = publishersPairsThatHaveGame.Single(x => x.Publisher.PublisherID != standardPublisherPair.Publisher.PublisherID);
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
                throw new Exception($"Problem with upcoming games. Happened for Game: {masterGame.MasterGame.GameName} and publisherIDs: {string.Join('|', publishersPairsThatHaveGame.Select(x => x.Publisher.PublisherID))}");
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
