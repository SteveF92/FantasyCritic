using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Interfaces;

public interface IRoyaleRepo
{
    Task CreatePublisher(RoyalePublisher publisher);
    Task<RoyalePublisher?> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user);
    Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters();
    Task<RoyalePublisher?> GetPublisher(Guid publisherID);
    Task PurchaseGame(RoyalePublisherGame game);
    Task SellGame(RoyalePublisherGame publisherGame, bool fullRefund);
    Task SetAdvertisingMoney(RoyalePublisherGame publisherGame, decimal advertisingMoney);
    Task<IReadOnlyList<RoyalePublisher>> GetAllPublishers(int year, int quarter);
    Task UpdateFantasyPoints(Dictionary<(Guid, Guid), decimal?> publisherGameScores);
    Task ChangePublisherName(RoyalePublisher publisher, string publisherName);
    Task<IReadOnlyList<RoyaleYearQuarter>> GetQuartersWonByUser(FantasyCriticUser user);
    Task<IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<RoyaleYearQuarter>>> GetRoyaleWinners();
    Task StartNewQuarter(YearQuarter nextQuarter);
    Task FinishQuarter(RoyaleYearQuarter supportedQuarter);
    Task ChangePublisherIcon(RoyalePublisher publisher, string? publisherIcon);
    Task ChangePublisherSlogan(RoyalePublisher publisher, string? publisherSlogan);
}
