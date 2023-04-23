using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Services;

public class PublisherService
{
    private readonly IFantasyCriticRepo _fantasyCriticRepo;
    private readonly DiscordPushService _discordPushService;
    private readonly InterLeagueService _interLeagueService;
    private readonly IClock _clock;

    public PublisherService(IFantasyCriticRepo fantasyCriticRepo,
        DiscordPushService discordPushService,
        InterLeagueService interLeagueService,
        IClock clock)
    {
        _fantasyCriticRepo = fantasyCriticRepo;
        _discordPushService = discordPushService;
        _interLeagueService = interLeagueService;
        _clock = clock;
    }

    public async Task<Publisher> CreatePublisher(LeagueYear leagueYear, FantasyCriticUser user, string publisherName)
    {
        int draftPosition = 1;
        var existingPublishers = leagueYear.Publishers;
        if (existingPublishers.Any())
        {
            draftPosition = existingPublishers.Max(x => x.DraftPosition) + 1;
        }

        Publisher publisher = new Publisher(Guid.NewGuid(), leagueYear.Key, user, publisherName, null, null, draftPosition, new List<PublisherGame>(), new List<FormerPublisherGame>(), 100, 0, 0, 0, 0, false);
        await _fantasyCriticRepo.CreatePublisher(publisher);
        await _discordPushService.SendNewPublisherMessage(publisher);
        return publisher;
    }

    public async Task ChangePublisherName(Publisher publisher, string newPublisherName)
    {
        await _fantasyCriticRepo.ChangePublisherName(publisher, newPublisherName);
        await _discordPushService.SendPublisherNameUpdateMessage(publisher, publisher.PublisherName, newPublisherName);
    }

    public Task ChangePublisherIcon(Publisher publisher, string? publisherIcon)
    {
        return _fantasyCriticRepo.ChangePublisherIcon(publisher, publisherIcon);
    }

    public Task ChangePublisherSlogan(Publisher publisher, string? publisherSlogan)
    {
        return _fantasyCriticRepo.ChangePublisherSlogan(publisher, publisherSlogan);
    }

    public Task SetAutoDraft(Publisher publisher, bool autoDraft)
    {
        return _fantasyCriticRepo.SetAutoDraft(publisher, autoDraft);
    }

    public Task RemovePublisherGame(LeagueYear leagueYear, Publisher publisher, PublisherGame publisherGame)
    {
        var now = _clock.GetCurrentInstant();
        var formerPublisherGame = publisherGame.GetFormerPublisherGame(now, "Removed by league manager");
        RemoveGameDomainRequest removeGameRequest = new RemoveGameDomainRequest(publisher, publisherGame);
        LeagueAction leagueAction = new LeagueAction(removeGameRequest, now);
        return _fantasyCriticRepo.ManagerRemovePublisherGame(leagueYear, publisher, publisherGame, formerPublisherGame, leagueAction);
    }

    public async Task<Result> EditPublisher(EditPublisherRequest editValues)
    {
        if (!editValues.SomethingChanged())
        {
            return Result.Failure("You need to specify something to change.");
        }

        if (editValues.Budget.HasValue && editValues.Budget.Value > 100)
        {
            return Result.Failure("Budget cannot be set to over $100.");
        }

        if (editValues.Budget.HasValue && editValues.Budget.Value < 0)
        {
            return Result.Failure("Budget cannot be set to under $0.");
        }

        bool allowUnlimitedWillReleaseDrops = editValues.LeagueYear.Options.WillReleaseDroppableGames == -1;
        if (editValues.WillReleaseGamesDropped.HasValue)
        {
            if (allowUnlimitedWillReleaseDrops)
            {
                return Result.Failure("Your league allows unlimited will release drops, so there is no reason to edit this.");
            }
            if (editValues.WillReleaseGamesDropped.Value > editValues.LeagueYear.Options.WillReleaseDroppableGames)
            {
                return Result.Failure("Will release games dropped cannot be set to more than is allowed in the league.");
            }
        }

        bool allowUnlimitedWillNotReleaseDrops = editValues.LeagueYear.Options.WillNotReleaseDroppableGames == -1;
        if (editValues.WillNotReleaseGamesDropped.HasValue)
        {
            if (allowUnlimitedWillNotReleaseDrops)
            {
                return Result.Failure("Your league allows unlimited will not release drops, so there is no reason to edit this.");
            }
            if (editValues.WillNotReleaseGamesDropped.Value > editValues.LeagueYear.Options.WillNotReleaseDroppableGames)
            {
                return Result.Failure("Will not release games dropped cannot be set to more than is allowed in the league.");
            }
        }

        bool allowUnlimitedFreeDrops = editValues.LeagueYear.Options.FreeDroppableGames == -1;
        if (editValues.FreeGamesDropped.HasValue)
        {
            if (allowUnlimitedFreeDrops)
            {
                return Result.Failure("Your league allows unlimited unrestricted drops, so there is no reason to edit this.");
            }
            if (editValues.FreeGamesDropped.Value > editValues.LeagueYear.Options.FreeDroppableGames)
            {
                return Result.Failure("Unrestricted games dropped cannot be set to more than is allowed in the league.");
            }
        }

        LeagueAction leagueAction = new LeagueAction(editValues, _clock.GetCurrentInstant());
        await _fantasyCriticRepo.EditPublisher(editValues, leagueAction);
        await _discordPushService.SendPublisherEditMessage(editValues);
        return Result.Success();
    }

    public Task FullyRemovePublisher(LeagueYear leagueYear, Publisher publisher)
    {
        return _fantasyCriticRepo.FullyRemovePublisher(leagueYear, publisher);
    }

    public async Task<Result> SetBidPriorityOrder(IReadOnlyList<KeyValuePair<PickupBid, int>> bidPriorities)
    {
        var requiredNumbers = Enumerable.Range(1, bidPriorities.Count).ToList();
        var requestedBidNumbers = bidPriorities.Select(x => x.Value);
        bool allRequiredPresent = new HashSet<int>(requiredNumbers).SetEquals(requestedBidNumbers);
        if (!allRequiredPresent)
        {
            return Result.Failure("Some of the positions are not valid.");
        }

        await _fantasyCriticRepo.SetBidPriorityOrder(bidPriorities);
        return Result.Success();
    }

    public async Task<Result> SetQueueRankings(IReadOnlyList<KeyValuePair<QueuedGame, int>> queueRanks)
    {
        var requiredNumbers = Enumerable.Range(1, queueRanks.Count).ToList();
        var requestedQueueNumbers = queueRanks.Select(x => x.Value);
        bool allRequiredPresent = new HashSet<int>(requiredNumbers).SetEquals(requestedQueueNumbers);
        if (!allRequiredPresent)
        {
            return Result.Failure("Some of the positions are not valid.");
        }

        await _fantasyCriticRepo.SetQueueRankings(queueRanks);
        return Result.Success();
    }

    public async Task<IReadOnlyList<QueuedGame>> GetQueuedGames(Publisher publisher)
    {
        var queuedGames = await _fantasyCriticRepo.GetQueuedGames(publisher);
        return queuedGames.OrderBy(x => x.Rank).ToList();
    }

    public async Task<Result> ReorderPublisherGames(LeagueYear leagueYear, Publisher publisher, Dictionary<int, Guid?> slotStates)
    {
        var requestedPositionsWithGames = slotStates.Where(x => x.Value.HasValue).ToList();
        var gameDictionary = publisher.PublisherGames.Where(x => !x.CounterPick).ToDictionary(x => x.PublisherGameID);
        var slotDictionary = publisher.GetPublisherSlots(leagueYear.Options).Where(x => !x.CounterPick).ToDictionary(x => x.SlotNumber);
        foreach (var requestedSlotState in requestedPositionsWithGames)
        {
            var moveIntoSlot = slotDictionary.GetValueOrDefault(requestedSlotState.Key);
            if (moveIntoSlot is null)
            {
                return Result.Failure("Invalid movement.");
            }

            var game = gameDictionary[requestedSlotState.Value!.Value];
            var currentSlot = slotDictionary[game.SlotNumber];
            var currentSlotIsValid = currentSlot.SlotIsValid(leagueYear);
            var potentialNewSlot = moveIntoSlot.GetWithReplacedGame(game);
            var potentialNewSlotIsValid = potentialNewSlot.SlotIsValid(leagueYear);
            if (currentSlotIsValid && !potentialNewSlotIsValid && !leagueYear.Options.AllowMoveIntoIneligible)
            {
                return Result.Failure("You cannot move a game into a slot that it is not eligible for.");
            }
        }

        await _fantasyCriticRepo.ReorderPublisherGames(publisher, slotStates);
        return Result.Success();
    }

    public async Task<IReadOnlyList<LeagueYearPublisherPair>> GetPublishersWithLeagueYears(FantasyCriticUser user)
    {
        var supportedYears = await _interLeagueService.GetSupportedYears();
        var activeYears = supportedYears.Where(x => x.OpenForPlay && !x.Finished);

        var leagueYearPublishers = new List<LeagueYearPublisherPair>();
        foreach (var year in activeYears)
        {
            var activeLeagueYears = await _fantasyCriticRepo.GetLeagueYearsForUser(user, year.Year);

            foreach (var leagueYear in activeLeagueYears)
            {
                if (leagueYear.League.TestLeague)
                {
                    continue;
                }

                var userPublisher = leagueYear.GetUserPublisher(user);
                if (userPublisher is null)
                {
                    continue;
                }

                leagueYearPublishers.Add(new LeagueYearPublisherPair(leagueYear, userPublisher));
            }
        }

        return leagueYearPublishers;
    }

    public async Task<Result> ReassignPublisher(LeagueYear leagueYear, IReadOnlyList<FantasyCriticUser> allUsersInLeague, Guid publisherID, Guid newUserID)
    {
        var userAlreadyHasPublisher = leagueYear.Publishers.Any(x => x.User.Id == newUserID);
        if (userAlreadyHasPublisher)
        {
            return Result.Failure("That user already has a publisher in this league year.");
        }

        var newUser = allUsersInLeague.SingleOrDefault(x => x.Id == newUserID);
        if (newUser is null)
        {
            return Result.Failure("User must be in the league to assign a publisher to them.");
        }

        var publisherToReassign = leagueYear.GetPublisherByID(publisherID);
        if (publisherToReassign is null)
        {
            return Result.Failure("Publisher not found.");
        }

        if (publisherToReassign.User.Id == leagueYear.League.LeagueManager.Id)
        {
            return Result.Failure("You cannot reassign your publisher to someone else.");
        }

        await _fantasyCriticRepo.ReassignPublisher(leagueYear, publisherToReassign, newUser);
        return Result.Success();
    }
}
