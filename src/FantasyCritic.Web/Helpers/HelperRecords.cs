using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Web.Helpers;

public record LeagueRecord(FantasyCriticUser? CurrentUser, League League, IReadOnlyList<FantasyCriticUserRemovable> PlayersInLeague, LeagueUserRelationship Relationship);
public record LeagueYearRecord(FantasyCriticUser? CurrentUser, LeagueYear LeagueYear, IReadOnlyList<FantasyCriticUserRemovable> PlayersInLeague,
    IReadOnlyList<FantasyCriticUser> ActiveUsers, IReadOnlyList<LeagueInvite> InvitedPlayers, LeagueYearUserRelationship Relationship);
public record LeagueYearWithSupplementalDataRecord(FantasyCriticUser? CurrentUser, LeagueYear LeagueYear, LeagueYearSupplementalData SupplementalData, IReadOnlyList<FantasyCriticUserRemovable> PlayersInLeague,
    IReadOnlyList<FantasyCriticUser> ActiveUsers, IReadOnlyList<LeagueInvite> InvitedPlayers, LeagueYearUserRelationship Relationship);
public record LeagueYearPublisherRecord(FantasyCriticUser? CurrentUser, LeagueYear LeagueYear, Publisher Publisher, PublisherUserRelationship Relationship);
public record LeagueYearPublisherGameRecord(FantasyCriticUser? CurrentUser, LeagueYear LeagueYear, Publisher Publisher, PublisherGame PublisherGame, PublisherUserRelationship Relationship);

public record ConferenceRecord(FantasyCriticUser? CurrentUser, Conference Conference, IReadOnlyList<ConferencePlayer> PlayersInConference,
    ConferenceUserRelationship Relationship, IReadOnlyList<ConferenceLeague> ConferenceLeagues);
public record ConferenceYearRecord(FantasyCriticUser? CurrentUser, ConferenceYear ConferenceYear, ConferenceUserRelationship Relationship);

public record ConferenceYearWithSupplementalDataRecord(FantasyCriticUser? CurrentUser, ConferenceYear ConferenceYear, IReadOnlyList<ConferencePlayer> PlayersInConference,
    ConferenceUserRelationship Relationship, IReadOnlyList<ConferenceLeagueYear> ConferenceLeagueYears, IReadOnlyList<ConferenceYearStanding> ConferenceYearStandings,
    IReadOnlyList<ManagerMessage> ManagerMessages);
