using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleStandingsViewModel
{
    public RoyaleStandingsViewModel(RoyalePublisher domain, IEnumerable<RoyaleYearQuarter> quartersWon, LocalDate currentDate)
    {
        PublisherID = domain.PublisherID;
        PlayerName = domain.User.DisplayName;
        UserID = domain.User.UserID;
        PublisherName = domain.PublisherName;
        TotalFantasyPoints = domain.GetTotalFantasyPoints();
        Ranking = domain.Ranking;
        Budget = domain.Budget;
        GamesPurchased = domain.PublisherGames.Count;
        GamesReleased = domain.PublisherGames.Count(x => x.MasterGame.IsReleasedAndReleasedInQuarter(currentDate, domain.YearQuarter.YearQuarter));
        PreviousQuarterWinner = quartersWon.Select(x => x.YearQuarter).Contains(domain.YearQuarter.YearQuarter.LastQuarter);
        OneTimeWinner = quartersWon.Any();
    }

    public Guid PublisherID { get; }
    public Guid UserID { get; }
    public string PlayerName { get; }
    public string PublisherName { get; }
    public decimal TotalFantasyPoints { get; }
    public int? Ranking { get; }
    public decimal Budget { get; }
    public int GamesPurchased { get; }
    public int GamesReleased { get; }
    public bool PreviousQuarterWinner { get; }
    public bool OneTimeWinner { get; }
}
