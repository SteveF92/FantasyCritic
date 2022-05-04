namespace FantasyCritic.Lib.Domain.LeagueActions;

public record LeagueYearSpecialAuctionSet(LeagueYear LeagueYear, IReadOnlyList<SpecialAuctionWithBids> SpecialAuctionsWithBids);
