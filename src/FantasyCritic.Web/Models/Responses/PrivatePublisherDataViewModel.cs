using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Web.Models.Responses;
public class PrivatePublisherDataViewModel
{
    public PrivatePublisherDataViewModel(LeagueYear leagueYear, Publisher userPublisher,
        IEnumerable<PickupBid> myActiveBids, IEnumerable<DropRequest> myActiveDrops, IEnumerable<QueuedGame> queuedGames,
        LocalDate currentDate, IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        MyActiveBids = myActiveBids.Select(x => new PickupBidViewModel(x, currentDate, masterGameYearDictionary)).OrderBy(x => x.Priority).ToList();
        MyActiveDrops = myActiveDrops.Select(x => new DropGameRequestViewModel(x, currentDate, masterGameYearDictionary)).OrderBy(x => x.Timestamp).ToList();

        HashSet<MasterGame> publisherMasterGames = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => !x.CounterPick && x.MasterGame is not null)
            .Select(x => x.MasterGame!.MasterGame)
            .ToHashSet();

        HashSet<MasterGame> myPublisherMasterGames = userPublisher.MyMasterGames;

        List<QueuedGameViewModel> queuedGameVMs = new List<QueuedGameViewModel>();

        foreach (var queuedGame in queuedGames)
        {
            bool taken = publisherMasterGames.Contains(queuedGame.MasterGame);
            bool alreadyOwned = myPublisherMasterGames.Contains(queuedGame.MasterGame);
            
            if (masterGameYearDictionary.TryGetValue(queuedGame.MasterGame.MasterGameID, out var masterGameYear))
            {
                
                queuedGameVMs.Add(new QueuedGameViewModel(queuedGame, masterGameYear, currentDate, taken, alreadyOwned));
            }
            else
            {
                var defaultMasterGameYear = new MasterGameYear(queuedGame.MasterGame, leagueYear.Year);
                queuedGameVMs.Add(new QueuedGameViewModel(queuedGame, defaultMasterGameYear, currentDate, taken, alreadyOwned));
            }
        }

        QueuedGames = queuedGameVMs;
    }

    public IReadOnlyList<PickupBidViewModel> MyActiveBids { get; }
    public IReadOnlyList<DropGameRequestViewModel> MyActiveDrops { get; }
    public IReadOnlyList<QueuedGameViewModel> QueuedGames { get; }
}
