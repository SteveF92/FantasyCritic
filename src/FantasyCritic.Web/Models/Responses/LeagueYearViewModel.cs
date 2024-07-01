using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Responses;

public class LeagueYearViewModel
{
    public LeagueYearViewModel(LeagueViewModel leagueViewModel, LeagueYear leagueYear, Instant currentInstant, IReadOnlyList<MinimalFantasyCriticUser> activeUsers,
        CompletePlayStatus completePlayStatus, IEnumerable<LeagueInvite> invitedPlayers, bool userIsInLeague, bool userIsInvitedToLeague, bool userIsManager,
        FantasyCriticUser? accessingUser, LeagueYearSupplementalData supplementalData, IReadOnlyDictionary<PublisherGame, Publisher> counterPickedByDictionary,
        GameNewsViewModel gameNews)
    {
        var currentDate = currentInstant.ToEasternDate();
        League = leagueViewModel;
        LeagueID = leagueYear.League.LeagueID;
        Year = leagueYear.Year;
        SupportedYear = new SupportedYearViewModel(leagueYear.SupportedYear);
        Settings = new LeagueYearSettingsViewModel(leagueYear);
        UnlinkedGameExists = leagueYear.Publishers.SelectMany(x => x.PublisherGames).Any(x => x.MasterGame is null);

        if (accessingUser is not null)
        {
            UserIsActive = activeUsers.Any(x => x.UserID == accessingUser.Id);
        }

        Publishers = leagueYear.Publishers
            .OrderBy(x => x.DraftPosition)
            .Select(x => new PublisherViewModel(leagueYear, x, currentDate, completePlayStatus.DraftStatus?.NextDraftPublisher, userIsInLeague, userIsInvitedToLeague, supplementalData.SystemWideValues, counterPickedByDictionary))
            .ToList();

        var publisherRankings = leagueYear.Publishers
            .Select(x => new
                {
                    x.PublisherID,
                    Ranking = leagueYear.Publishers.Count(y => y.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options) > x.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options)) + 1
                }
            )
            .ToDictionary(x => x.PublisherID, x => x.Ranking);

        var publisherProjectedRankings = leagueYear.Publishers
            .Select(x => new
                {
                    x.PublisherID,
                    Ranking = leagueYear.Publishers.Count(y => y.GetProjectedFantasyPoints(leagueYear, supplementalData.SystemWideValues, currentDate) > x.GetProjectedFantasyPoints(leagueYear, supplementalData.SystemWideValues, currentDate)) + 1
                }
            )
            .ToDictionary(x => x.PublisherID, x => x.Ranking);

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
                int ranking = publisherRankings[publisher.PublisherID];
                int projectedRanking = publisherProjectedRankings[publisher.PublisherID];
                bool isPreviousYearWinner = supplementalData.PreviousYearWinner?.Id == user.UserID;
                playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, publisher, currentDate, supplementalData.SystemWideValues,
                    userIsInLeague, userIsInvitedToLeague, false, isPreviousYearWinner, ranking, projectedRanking));
            }
        }

        if (Year == leagueYear.League.Years.Max())
        {
            foreach (var invitedPlayer in invitedPlayers)
            {
                allPublishersMade = false;

                if (invitedPlayer.InviteUser is not null)
                {
                    playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.InviteID, invitedPlayer.InviteUser.DisplayName));
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
        }
        
        Players = allPublishersMade ? playerVMs.OrderBy(x => x.Publisher!.DraftPosition).ToList() : playerVMs;
        PlayStatus = new PlayStatusViewModel(completePlayStatus);
        EligibilityOverrides = leagueYear.EligibilityOverrides.Select(x => new EligibilityOverrideViewModel(x, currentDate)).ToList();
        TagOverrides = leagueYear.TagOverrides.Select(x => new TagOverrideViewModel(x, currentDate)).ToList();
        SlotInfo = new PublisherSlotRequirementsViewModel(leagueYear.Options);

        ManagerMessages = supplementalData.ManagerMessages.Select(x => new ManagerMessageViewModel(x, x.IsDismissed(accessingUser))).OrderBy(x => x.Timestamp).ToList();
        if (!userIsInLeague)
        {
            ManagerMessages = ManagerMessages.Where(x => x.IsPublic).ToList();
        }

        if (supplementalData.PublicBiddingGames is not null)
        {
            PublicBiddingGames = new PublicBiddingSetViewModel(supplementalData.PublicBiddingGames, currentDate);
        }

        ActiveTrades = supplementalData.ActiveTrades.Select(x => new TradeViewModel(x, currentDate)).ToList();
        ActiveSpecialAuctions = supplementalData.ActiveSpecialAuctions.Select(x => new SpecialAuctionViewModel(x, currentInstant)).ToList();
        GameNews = gameNews;
        AllPublishersForUserViewModels = supplementalData.AllPublishersForUser.Select(p => new LeaguePublisherViewModel(p.PublisherID, p.PublisherName, p.LeagueID, p.LeagueName, p.Year)).ToList();

        if (supplementalData.PrivatePublisherData is not null)
        {
            Publisher? userPublisher = leagueYear.GetUserPublisher(accessingUser);
            if (userPublisher is null)
            {
                throw new Exception($"User publisher for LeagueID: {leagueYear.League.LeagueID}, UserID: {accessingUser?.UserID} cannot be null");
            }

            PrivatePublisherData = new PrivatePublisherDataViewModel(leagueYear, userPublisher, supplementalData.PrivatePublisherData, currentDate);
        }
    }

    public LeagueViewModel League { get; }
    public Guid LeagueID { get; }
    public int Year { get; }
    public SupportedYearViewModel SupportedYear { get; }
    public LeagueYearSettingsViewModel Settings { get; }
    public PublisherSlotRequirementsViewModel SlotInfo { get; }
    public bool UnlinkedGameExists { get; }
    public bool UserIsActive { get; }
    public IReadOnlyList<PlayerWithPublisherViewModel> Players { get; }
    public IReadOnlyList<PublisherViewModel> Publishers { get; }
    public IReadOnlyList<EligibilityOverrideViewModel> EligibilityOverrides { get; }
    public IReadOnlyList<TagOverrideViewModel> TagOverrides { get; }
    public PlayStatusViewModel PlayStatus { get; }
    public IReadOnlyList<ManagerMessageViewModel> ManagerMessages { get; }
    public PublicBiddingSetViewModel? PublicBiddingGames { get; }
    public IReadOnlyList<TradeViewModel> ActiveTrades { get; }
    public IReadOnlyList<SpecialAuctionViewModel> ActiveSpecialAuctions { get; }
    public GameNewsViewModel GameNews { get; }
    public IReadOnlyList<LeaguePublisherViewModel> AllPublishersForUserViewModels { get; }
    public PrivatePublisherDataViewModel? PrivatePublisherData { get; }
}
