using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyaleStandingsViewModel
{
    public RoyaleStandingsViewModel(RoyalePublisher domain, int? ranking, IEnumerable<RoyaleYearQuarter> quartersWon)
    {
        PublisherID = domain.PublisherID;
        PlayerName = domain.User.DisplayName;
        UserID = domain.User.UserID;
        PublisherName = domain.PublisherName;
        TotalFantasyPoints = domain.GetTotalFantasyPoints();
        if (TotalFantasyPoints > 0)
        {
            Ranking = ranking;
        }

        PreviousQuarterWinner = quartersWon.Select(x => x.YearQuarter).Contains(domain.YearQuarter.YearQuarter.LastQuarter);
        OneTimeWinner = quartersWon.Any();
    }

    public Guid PublisherID { get; }
    public Guid UserID { get; }
    public string PlayerName { get; }
    public string PublisherName { get; }
    public decimal TotalFantasyPoints { get; }
    public int? Ranking { get; }
    public bool PreviousQuarterWinner { get; }
    public bool OneTimeWinner { get; }
}
