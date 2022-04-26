using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueYearViewModel
{
    public LeagueYearViewModel(LeagueViewModel leagueViewModel, LeagueYear leagueYear,
        LocalDate currentDate,  IEnumerable<FantasyCriticUser> activeUsers,
        CompletePlayStatus completePlayStatus, SystemWideValues systemWideValues,
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
        
        HasSpecialSlots = leagueYear.Options.HasSpecialSlots;
        OneShotMode = leagueYear.Options.OneShotMode;
        CounterPickDeadline = leagueYear.CounterPickDeadline;
        Publishers = leagueYear.Publishers
            .OrderBy(x => x.DraftPosition)
            .Select(x => new PublisherViewModel(leagueYear, x, currentDate, completePlayStatus.DraftStatus?.NextDraftPublisher, userIsInLeague, userIsInvitedToLeague, systemWideValues, counterPickedPublisherGameIDs))
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

        Players = allPublishersMade ? playerVMs.OrderBy(x => x.Publisher!.DraftPosition).ToList() : playerVMs;
        PlayStatus = new PlayStatusViewModel(completePlayStatus);
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
    public int CounterPicks { get; }
    public string DraftSystem { get; }
    public string PickupSystem { get; }
    public string TiebreakSystem { get; }
    public string ScoringSystem { get; }
    public string TradingSystem { get; }
    public bool UnlinkedGameExists { get; }
    public bool UserIsActive { get; }
    public bool HasSpecialSlots { get; }
    public bool OneShotMode { get; }
    public LocalDate CounterPickDeadline { get; }
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
