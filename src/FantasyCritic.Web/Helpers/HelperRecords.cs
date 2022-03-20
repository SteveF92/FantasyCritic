using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Helpers;

public record LeagueRecord(Maybe<FantasyCriticUser> CurrentUser, League League, LeagueUserRelationship Relationship);
public record LeagueYearRecord(Maybe<FantasyCriticUser> CurrentUser, LeagueYear LeagueYear, LeagueYearUserRelationship Relationship);
public record LeagueYearPublisherRecord(Maybe<FantasyCriticUser> CurrentUser, LeagueYear LeagueYear, Publisher Publisher, PublisherUserRelationship Relationship);
public record LeagueYearPublisherGameRecord(Maybe<FantasyCriticUser> CurrentUser, LeagueYear LeagueYear, Publisher Publisher, PublisherGame PublisherGame, PublisherUserRelationship Relationship);
