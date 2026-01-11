using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Interfaces;

public interface IRoyaleRepo
{
    Task CreatePublisher(RoyalePublisher publisher);
    Task<RoyalePublisher?> GetPublisher(RoyaleYearQuarter yearQuarter, IVeryMinimalFantasyCriticUser user);
    Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters();
    Task<RoyaleYearQuarterData?> GetRoyaleYearQuarterData(int year, int quarter);
    Task<RoyalePublisherData?> GetPublisherData(Guid publisherID);
    Task PurchaseGame(RoyalePublisherGame game, RoyaleAction action);
    Task SellGame(RoyalePublisherGame publisherGame, decimal refund, RoyaleAction action);
    Task SetAdvertisingMoney(RoyalePublisherGame publisherGame, decimal advertisingMoney, RoyaleAction action);
    Task<IReadOnlyList<RoyalePublisher>> GetAllPublishers(int year, int quarter);
    Task UpdateFantasyPoints(Dictionary<(Guid, Guid), decimal?> publisherGameScores);
    Task ChangePublisherName(RoyalePublisher publisher, string publisherName);
    Task CalculateRoyaleWinnerForQuarter(int year, int quarter);
    Task StartNewQuarter(YearQuarter nextQuarter);
    Task FinishQuarter(RoyaleYearQuarter supportedQuarter);
    Task ChangePublisherIcon(RoyalePublisher publisher, string? publisherIcon);
    Task ChangePublisherSlogan(RoyalePublisher publisher, string? publisherSlogan);
}
