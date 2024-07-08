using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyalePublisherViewModel
{
    public RoyalePublisherViewModel(RoyalePublisher domain, LocalDate currentDate, int? ranking, IEnumerable<RoyaleYearQuarter> quartersWon,
        IEnumerable<MasterGameTag> allMasterGameTags, bool thisPlayerIsViewing)
    {
        PublisherID = domain.PublisherID;
        YearQuarter = new RoyaleYearQuarterViewModel(domain.YearQuarter);
        PlayerName = domain.User.DisplayName;
        UserID = domain.User.UserID;
        PublisherName = domain.PublisherName;
        PublisherIcon = domain.PublisherIcon;
        PublisherSlogan = domain.PublisherSlogan;
        PublisherGames = domain.PublisherGames.Select(x => new RoyalePublisherGameViewModel(x, currentDate, allMasterGameTags, thisPlayerIsViewing)).ToList();
        Budget = domain.Budget;
        TotalFantasyPoints = domain.GetTotalFantasyPoints();
        if (TotalFantasyPoints > 0)
        {
            Ranking = ranking;
        }

        QuartersWon = quartersWon.Select(x => new RoyaleYearQuarterViewModel(x)).ToList();
        PreviousQuarterWinner = quartersWon.Select(x => x.YearQuarter).Contains(domain.YearQuarter.YearQuarter.LastQuarter);
        OneTimeWinner = quartersWon.Any();
    }

    public Guid PublisherID { get; }
    public RoyaleYearQuarterViewModel YearQuarter { get; }
    public Guid UserID { get; }
    public string PlayerName { get; }
    public string PublisherName { get; }
    public string? PublisherIcon { get; }
    public string? PublisherSlogan { get; }
    public IReadOnlyList<RoyalePublisherGameViewModel> PublisherGames { get; }
    public decimal Budget { get; }
    public decimal TotalFantasyPoints { get; }
    public int? Ranking { get; }
    public IReadOnlyList<RoyaleYearQuarterViewModel> QuartersWon { get; }
    public bool PreviousQuarterWinner { get; }
    public bool OneTimeWinner { get; }
}
