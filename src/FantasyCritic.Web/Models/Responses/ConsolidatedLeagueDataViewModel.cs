using FantasyCritic.Lib.Domain.AllTimeStats;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Models.Responses.AllTimeStats;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Responses;

public class ConsolidatedLeagueDataViewModel
{
    public ConsolidatedLeagueDataViewModel(League league, IReadOnlyList<FantasyCriticUserRemovable> playersInLeague, LeagueAllTimeStats allTimeStats,
        SystemWideValues systemWideValues, LocalDate currentDate)
    {
        var typedManager = new VeryMinimalFantasyCriticUserViewModel(league.LeagueManager);
        var typedPlayersInLeague = playersInLeague.Select(x => new VeryMinimalFantasyCriticUserViewModel(x.User)).ToList();

        var latestDraftStartedYear = league.Years.Where(x => x.AnyDraftStarted).MaxBy(x => x.Year);
        var highestNonFinishedYear = league.Years.Where(x => !x.Finished).MaxBy(x => x.Year);
        var years = league.Years.Select(x => x.Year).ToList();
        var activeYear = latestDraftStartedYear?.Year ?? highestNonFinishedYear?.Year ?? years.Max();

        League = new ConsolidatedLeagueViewModel(league.LeagueID, league.LeagueName, typedManager, league.ConferenceID, league.ConferenceName,
            typedPlayersInLeague, years, activeYear, league.PublicLeague, league.TestLeague, league.CustomRulesLeague, league.NumberOfFollowers);

        var leagueAllTimeStatsViewModel = new LeagueAllTimeStatsResponse(new LeagueViewModel(league, false, false, false), allTimeStats, systemWideValues, currentDate);
        AllTimeStats = new ConsolidatedAllTimeStatsViewModel(leagueAllTimeStatsViewModel.PlayerAllTimeStats, leagueAllTimeStatsViewModel.Publishers, leagueAllTimeStatsViewModel.HallOfFameGameLists);
    }

    public ConsolidatedLeagueViewModel League { get; }
    public ConsolidatedAllTimeStatsViewModel AllTimeStats { get; }
}

public record ConsolidatedLeagueViewModel(Guid LeagueID, string LeagueName, VeryMinimalFantasyCriticUserViewModel LeagueManager, Guid? ConferenceID, string? ConferenceName,
    IReadOnlyList<VeryMinimalFantasyCriticUserViewModel> Players, IReadOnlyList<int> Years, int ActiveYear, bool PublicLeague, bool TestLeague, bool CustomRulesLeague, int NumberOfFollowers);
public record ConsolidatedAllTimeStatsViewModel(List<LeaguePlayerAllTimeStatsResponse> PlayerAllTimeStats, List<AllTimeStatsPublisherViewModel> Publishers, List<HallOfFameGameListResponse> HallOfFameGameLists);

public class ConsolidatedLeagueYearViewModel
{
    public ConsolidatedLeagueYearViewModel(ConsolidatedLeagueYearData domain, SystemWideValues systemWideValues, Instant currentInstant, LocalDate currentDate,
        IReadOnlyDictionary<Guid, MasterGameYear> masterGameYearDictionary)
    {
        LeagueYear leagueYear = domain.LeagueYear;
        LeagueID = leagueYear.League.LeagueID;
        Year = leagueYear.Year;
        SupportedYear = new SupportedYearViewModel(leagueYear.SupportedYear);
        Settings = new LeagueYearSettingsViewModel(leagueYear);

        IReadOnlyDictionary<PublisherGame, Publisher> counterPickedByDictionary = GameUtilities.GetCounterPickedByDictionary(leagueYear);
        List<FantasyCriticUser> activePublisherUsers = leagueYear.Publishers.Select(x => x.User).ToList();
        List<MinimalFantasyCriticUser> activeUsersMinimal = activePublisherUsers.Select(x => x.ToMinimal()).ToList();
        bool conferenceDraftsNotEnabled = leagueYear.ConferenceLocked.HasValue && !leagueYear.ConferenceLocked.Value;
        var activeDraftNextPublisher = DraftFunctions.GetDraftStatus(leagueYear)?.NextDraftPublisher;

        Publishers = leagueYear.Publishers
            .OrderBy(x => x.GetDraftPosition(leagueYear.DraftForPublisherDisplayOrder.DraftID))
            .Select(x => new PublisherViewModel(leagueYear, x, currentDate, activeDraftNextPublisher,
                userIsInLeague: false, outstandingInvite: false, systemWideValues, counterPickedByDictionary))
            .ToList();

        Dictionary<Guid, int> publisherRankings = leagueYear.Publishers
            .Select(x => new
            {
                x.PublisherID,
                Ranking = leagueYear.Publishers.Count(y => y.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options) > x.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options)) + 1
            })
            .ToDictionary(x => x.PublisherID, x => x.Ranking);

        Dictionary<Guid, int> publisherProjectedRankings = leagueYear.Publishers
            .Select(x => new
            {
                x.PublisherID,
                Ranking = leagueYear.Publishers.Count(y => y.GetProjectedFantasyPoints(leagueYear, systemWideValues) > x.GetProjectedFantasyPoints(leagueYear, systemWideValues)) + 1
            })
            .ToDictionary(x => x.PublisherID, x => x.Ranking);

        Guid? previousYearWinnerUserID = domain.PreviousSeasonWinnerUserID;

        List<PlayerWithPublisherViewModel> playerVMs = new List<PlayerWithPublisherViewModel>();
        foreach (MinimalFantasyCriticUser user in activeUsersMinimal)
        {
            Publisher? publisher = leagueYear.GetUserPublisher(user);
            if (publisher is null)
            {
                continue;
            }

            int ranking = publisherRankings[publisher.PublisherID];
            int projectedRanking = publisherProjectedRankings[publisher.PublisherID];
            bool isPreviousYearWinner = previousYearWinnerUserID.HasValue && previousYearWinnerUserID.Value == user.UserID;
            playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, publisher, currentDate, systemWideValues,
                userIsInLeague: false, userIsInvitedToLeague: false, removable: false, isPreviousYearWinner, ranking, projectedRanking));
        }

        Players = playerVMs.OrderBy(x => x.Publisher!.DraftPosition).ToList();

        Drafts = leagueYear.Drafts.Select(d => new LeagueDraftViewModel(d, leagueYear, activePublisherUsers, isManager: false, conferenceDraftsNotEnabled)).ToList();
        EligibilityOverrides = leagueYear.EligibilityOverrides.Select(x => new EligibilityOverrideViewModel(x, currentDate)).ToList();
        TagOverrides = leagueYear.TagOverrides.Select(x => new TagOverrideViewModel(x, currentDate)).ToList();
        SlotInfo = new PublisherSlotRequirementsViewModel(leagueYear.Options);

        ManagerMessages = domain.ManagerMessages.Select(x => new ManagerMessageViewModel(x, false)).OrderBy(x => x.Timestamp).ToList();

        Trades = domain.Trades
            .Where(x => x.IsVisibleInConsolidatedExport())
            .Select(x => new TradeViewModel(x, currentDate))
            .OrderByDescending(x => x.ProposedTimestamp)
            .ToList();

        SpecialAuctions = domain.SpecialAuctions.Select(x => new SpecialAuctionViewModel(x, currentInstant)).ToList();

        Actions = domain.LeagueActions.Select(x => new LeagueActionViewModel(leagueYear, x))
            .Concat(domain.LeagueManagerActions.Select(x => new LeagueActionViewModel(leagueYear, x)))
            .OrderByDescending(x => x.Timestamp)
            .ToList();

        ActionProcessingSets = domain.ActionProcessingSets.Where(x => x.HasActions)
            .Select(x => new LeagueActionProcessingSetViewModel(x, currentDate, masterGameYearDictionary))
            .OrderByDescending(x => x.ProcessTime)
            .ToList();

        UnderReview = leagueYear.UnderReview;
    }

    public Guid LeagueID { get; }
    public int Year { get; }
    public SupportedYearViewModel SupportedYear { get; }
    public LeagueYearSettingsViewModel Settings { get; }
    public PublisherSlotRequirementsViewModel SlotInfo { get; }
    public IReadOnlyList<PlayerWithPublisherViewModel> Players { get; }
    public IReadOnlyList<PublisherViewModel> Publishers { get; }
    public IReadOnlyList<EligibilityOverrideViewModel> EligibilityOverrides { get; }
    public IReadOnlyList<TagOverrideViewModel> TagOverrides { get; }
    public IReadOnlyList<LeagueDraftViewModel> Drafts { get; }
    public IReadOnlyList<ManagerMessageViewModel> ManagerMessages { get; }
    public IReadOnlyList<TradeViewModel> Trades { get; }
    public IReadOnlyList<SpecialAuctionViewModel> SpecialAuctions { get; }
    public IReadOnlyList<LeagueActionViewModel> Actions { get; }
    public IReadOnlyList<LeagueActionProcessingSetViewModel> ActionProcessingSets { get; }
    public bool UnderReview { get; }
}
