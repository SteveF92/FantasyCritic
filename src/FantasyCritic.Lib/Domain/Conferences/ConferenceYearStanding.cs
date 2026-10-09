namespace FantasyCritic.Lib.Domain.Conferences;

public record ConferenceYearStanding(Guid LeagueID, string LeagueName, int Year, Guid PublisherID, string DisplayName, string PublisherName, bool HasUser,
    decimal TotalFantasyPoints, decimal ProjectedFantasyPoints);
