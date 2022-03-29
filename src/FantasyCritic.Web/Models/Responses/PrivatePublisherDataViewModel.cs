using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;
public class PrivatePublisherDataViewModel
{
    public PrivatePublisherDataViewModel(LeagueYear leagueYear, Publisher userPublisher,
        IEnumerable<PickupBid> myActiveBids, IEnumerable<DropRequest> myActiveDrops, IEnumerable<QueuedGame> queuedGames,
        LocalDate currentDate)
    {
        MyActiveBids = myActiveBids.Select(x => new PickupBidViewModel(x, currentDate)).OrderBy(x => x.Priority).ToList();
        MyActiveDrops = myActiveDrops.Select(x => new DropGameRequestViewModel(x, currentDate)).OrderBy(x => x.Timestamp).ToList();

        HashSet<MasterGame> publisherMasterGames = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => !x.CounterPick && x.MasterGame.HasValueTempoTemp)
            .Select(x => x.MasterGame.ValueTempoTemp.MasterGame)
            .ToHashSet();

        HashSet<MasterGame> myPublisherMasterGames = userPublisher.MyMasterGames;

        QueuedGames = queuedGames.Select(x =>
            new QueuedGameViewModel(x, currentDate, publisherMasterGames.Contains(x.MasterGame),
                myPublisherMasterGames.Contains(x.MasterGame)
            )).OrderBy(x => x.Rank).ToList();
    }

    public IReadOnlyList<PickupBidViewModel> MyActiveBids { get; }
    public IReadOnlyList<DropGameRequestViewModel> MyActiveDrops { get; }
    public IReadOnlyList<QueuedGameViewModel> QueuedGames { get; }
}
