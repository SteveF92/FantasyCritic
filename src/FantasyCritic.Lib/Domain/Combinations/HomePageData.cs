using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Domain.Combinations;

public record HomePageData(IReadOnlyList<LeagueWithMostRecentYearStatus> MyLeagues, IReadOnlyList<LeagueInvite> InvitedLeagues, IReadOnlyList<Conference> MyConferences,
    TopBidsAndDropsData? TopBidsAndDropsData, IReadOnlyList<LeagueYearPublisherPair> MyPublishers, IReadOnlyList<LeagueYear> PublicLeagueYears,
    RoyaleYearQuarter ActiveRoyaleYearQuarter, RoyalePublisherData? ActiveRoyalePublisher);

public record TopBidsAndDropsData(LocalDate ProcessDate, IReadOnlyList<TopBidsAndDropsGame> TopBidsAndDrops);
public record RoyalePublisherData(RoyalePublisher Publisher, IReadOnlyList<MasterGameTag> MasterGameTags, IReadOnlyList<RoyaleYearQuarter> QuartersWon);
