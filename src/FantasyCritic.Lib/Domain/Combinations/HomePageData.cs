using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Domain.Combinations;

public record HomePageData(IReadOnlyList<LeagueWithMostRecentYearStatus> MyLeagues, IReadOnlyList<CompleteLeagueInvite> InvitedLeagues, IReadOnlyList<MinimalConference> MyConferences,
    TopBidsAndDropsData? TopBidsAndDropsData, IReadOnlyList<PublicLeagueYearStats> PublicLeagueYears, IReadOnlyList<SingleGameNews> MyGameDetails,
    RoyaleYearQuarter ActiveRoyaleYearQuarter, Guid? ActiveYearQuarterRoyalePublisherID);

public record TopBidsAndDropsData(LocalDate ProcessDate, IReadOnlyList<TopBidsAndDropsGame> TopBidsAndDrops);
