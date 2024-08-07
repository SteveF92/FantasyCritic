using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyalePublisherGameViewModel
{
    public RoyalePublisherGameViewModel(RoyalePublisherGame domain, RoyaleYearQuarter yearQuarter, LocalDate currentDate, IEnumerable<MasterGameTag> allMasterGameTags, bool thisPlayerIsViewing)
    {
        PublisherID = domain.PublisherID;
        Locked = domain.IsLocked(currentDate, allMasterGameTags);
        AdvertisingMoney = domain.AdvertisingMoney;
        FantasyPoints = domain.FantasyPoints;
        CurrentlyIneligible = domain.CalculateIsCurrentlyIneligible(allMasterGameTags);
        Timestamp = domain.Timestamp;
        GameHidden = domain.IsHidden(yearQuarter, currentDate);
        if (!domain.IsHidden(yearQuarter, currentDate) || thisPlayerIsViewing)
        {
            MasterGame = new MasterGameYearViewModel(domain.MasterGame, currentDate);
            AmountSpent = domain.AmountSpent;
            RefundAmount = AmountSpent;
            if (!CurrentlyIneligible)
            {
                RefundAmount /= 2;
            }
        }
    }

    public Guid PublisherID { get; }
    public bool GameHidden { get; }
    public MasterGameYearViewModel? MasterGame { get; }
    public bool Locked { get; }
    public Instant Timestamp { get; }
    public decimal? AmountSpent { get; }
    public decimal AdvertisingMoney { get; }
    public decimal? FantasyPoints { get; }
    public bool CurrentlyIneligible { get; }
    public decimal? RefundAmount { get; }
}
