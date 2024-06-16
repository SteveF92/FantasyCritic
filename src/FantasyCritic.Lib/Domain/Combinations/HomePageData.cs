using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Domain.Combinations;

public record HomePageData(IReadOnlyList<LeagueWithMostRecentYearStatus> MyLeagues, IReadOnlyList<CompleteLeagueInvite> InvitedLeagues, IReadOnlyList<Conference> MyConferences,
    TopBidsAndDropsData? TopBidsAndDropsData, IReadOnlyList<LeagueYearPublisherPair> MyPublishers, IReadOnlyList<PublicLeagueYearStats> PublicLeagueYears,
    RoyaleYearQuarter ActiveRoyaleYearQuarter, Guid? ActiveYearQuarterRoyalePublisherID);

public record TopBidsAndDropsData(LocalDate ProcessDate, IReadOnlyList<TopBidsAndDropsGame> TopBidsAndDrops);
