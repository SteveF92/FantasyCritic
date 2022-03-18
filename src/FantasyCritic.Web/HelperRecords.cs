using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web;

public record LeagueYearRecord(FantasyCriticUser CurrentUser, LeagueYear LeagueYear);
public record LeagueYearPublisherRecord(FantasyCriticUser CurrentUser, LeagueYear LeagueYear, Publisher Publisher);
public record LeagueYearPublisherGameRecord(FantasyCriticUser CurrentUser, LeagueYear LeagueYear, Publisher Publisher, PublisherGame PublisherGame);
