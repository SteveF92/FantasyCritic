using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Helpers;

public record LeagueRecord(FantasyCriticUser CurrentUser, League League);
public record LeagueYearRecord(FantasyCriticUser CurrentUser, LeagueYear LeagueYear);
public record LeagueYearPublisherRecord(FantasyCriticUser CurrentUser, LeagueYear LeagueYear, Publisher Publisher);
public record LeagueYearPublisherGameRecord(FantasyCriticUser CurrentUser, LeagueYear LeagueYear, Publisher Publisher, PublisherGame PublisherGame);
