namespace FantasyCritic.Lib.Domain.Conferences;

public record ConferenceYearStanding(Guid LeagueID, int Year, Guid PublisherID, string DisplayName, string PublisherName, decimal FantasyPoints, decimal ProjectedPoints);
