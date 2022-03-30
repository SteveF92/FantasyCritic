using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueYearViewModel
{
    public LeagueYearViewModel(LeagueViewModel leagueViewModel, LeagueYear leagueYear,
        LocalDate currentDate, StartDraftResult startDraftResult, IEnumerable<FantasyCriticUser> activeUsers, Publisher? nextDraftPublisher,
        DraftPhase draftPhase, SystemWideValues systemWideValues,
        IEnumerable<LeagueInvite> invitedPlayers, bool userIsInLeague, bool userIsInvitedToLeague, bool userIsManager,
        FantasyCriticUser? accessingUser, IEnumerable<ManagerMessage> managerMessages, FantasyCriticUser? previousYearWinner,
        IReadOnlyList<PublicBiddingMasterGame>? publicBiddingGames, IReadOnlySet<Guid> counterPickedPublisherGameIDs,
        IEnumerable<Trade> activeTrades, PrivatePublisherDataViewModel? privatePublisherData, GameNewsViewModel gameNews)
    {
        League = leagueViewModel;
        LeagueID = leagueYear.League.LeagueID;
        Year = leagueYear.Year;
        SupportedYear = new SupportedYearViewModel(leagueYear.SupportedYear);
        StandardGames = leagueYear.Options.StandardGames;
        GamesToDraft = leagueYear.Options.GamesToDraft;
        CounterPicks = leagueYear.Options.CounterPicks;
        DraftSystem = leagueYear.Options.DraftSystem.Value;
        PickupSystem = leagueYear.Options.PickupSystem.Value;
        TiebreakSystem = leagueYear.Options.TiebreakSystem.Value;
        ScoringSystem = leagueYear.Options.ScoringSystem.Name;
        TradingSystem = leagueYear.Options.TradingSystem.Value;
        UnlinkedGameExists = leagueYear.Publishers.SelectMany(x => x.PublisherGames).Any(x => x.MasterGame is null);

        if (accessingUser is not null)
        {
            UserIsActive = activeUsers.Any(x => x.Id == accessingUser.Id);
        }
        
        HasSpecialSlots = leagueYear.Options.HasSpecialSlots();
        Publishers = leagueYear.Publishers
            .OrderBy(x => x.DraftPosition)
            .Select(x => new PublisherViewModel(leagueYear, x, currentDate, nextDraftPublisher, userIsInLeague, userIsInvitedToLeague, systemWideValues, counterPickedPublisherGameIDs))
            .ToList();

        List<PlayerWithPublisherViewModel> playerVMs = new List<PlayerWithPublisherViewModel>();
        bool allPublishersMade = true;
        foreach (var user in activeUsers)
        {
            var publisher = leagueYear.GetUserPublisher(user);
            if (publisher is null)
            {
                playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, false));
                allPublishersMade = false;
            }
            else
            {
                bool isPreviousYearWinner = previousYearWinner is not null && previousYearWinner.Id == user.Id;
                playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, publisher, currentDate, systemWideValues,
                    userIsInLeague, userIsInvitedToLeague, false, isPreviousYearWinner));
            }
        }

        foreach (var invitedPlayer in invitedPlayers)
        {
            allPublishersMade = false;

            if (invitedPlayer.User is not null)
            {
                playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.InviteID, invitedPlayer.User.UserName));
            }
            else
            {
                if (accessingUser is not null)
                {
                    if (userIsManager || string.Equals(invitedPlayer.EmailAddress, accessingUser.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.InviteID, invitedPlayer.EmailAddress));
                    }
                    else
                    {
                        playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.InviteID, "<Email Address Hidden>"));
                    }
                }
                else
                {
                    playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.InviteID, "<Email Address Hidden>"));
                }
            }
        }

        bool readyToSetDraftOrder = false;
        if (allPublishersMade)
        {
            Players = playerVMs.OrderBy(x => x.Publisher!.DraftPosition).ToList();
            readyToSetDraftOrder = true;
        }
        else
        {
            Players = playerVMs;
        }

        PlayStatus = new PlayStatusViewModel(leagueYear.PlayStatus, readyToSetDraftOrder, startDraftResult.Ready, startDraftResult.Errors, draftPhase);
        EligibilityOverrides = leagueYear.EligibilityOverrides.Select(x => new EligibilityOverrideViewModel(x, currentDate)).ToList();
        TagOverrides = leagueYear.TagOverrides.Select(x => new TagOverrideViewModel(x, currentDate)).ToList();
        SlotInfo = new PublisherSlotRequirementsViewModel(leagueYear.Options);

        ManagerMessages = managerMessages.Select(x => new ManagerMessageViewModel(x, x.IsDismissed(accessingUser))).OrderBy(x => x.Timestamp).ToList();
        if (!userIsInLeague)
        {
            ManagerMessages = ManagerMessages.Where(x => x.IsPublic).ToList();
        }

        if (publicBiddingGames is not null)
        {
            PublicBiddingGames = publicBiddingGames.Select(x => new PublicBiddingMasterGameViewModel(x, currentDate)).ToList();
        }

        ActiveTrades = activeTrades.Select(x => new TradeViewModel(x, currentDate)).ToList();
        PrivatePublisherData = privatePublisherData;
        GameNews = gameNews;
    }

    public LeagueViewModel League { get; }
    public Guid LeagueID { get; }
    public int Year { get; }
    public SupportedYearViewModel SupportedYear { get; }
    public int StandardGames { get; }
    public int GamesToDraft { get; }
    public int CounterPicks { get; }
    public string DraftSystem { get; }
    public string PickupSystem { get; }
    public string TiebreakSystem { get; }
    public string ScoringSystem { get; }
    public string TradingSystem { get; }
    public bool UnlinkedGameExists { get; }
    public bool UserIsActive { get; }
    public bool HasSpecialSlots { get; }
    public IReadOnlyList<PlayerWithPublisherViewModel> Players { get; }
    public IReadOnlyList<PublisherViewModel> Publishers { get; }
    public IReadOnlyList<EligibilityOverrideViewModel> EligibilityOverrides { get; }
    public IReadOnlyList<TagOverrideViewModel> TagOverrides { get; }
    public PublisherSlotRequirementsViewModel SlotInfo { get; }
    public PlayStatusViewModel PlayStatus { get; }
    public IReadOnlyList<ManagerMessageViewModel> ManagerMessages { get; }
    public IReadOnlyList<PublicBiddingMasterGameViewModel>? PublicBiddingGames { get; }
    public IReadOnlyList<TradeViewModel> ActiveTrades { get; }
    public PrivatePublisherDataViewModel? PrivatePublisherData { get; }
    public GameNewsViewModel GameNews { get; }
}
