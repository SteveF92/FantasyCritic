namespace FantasyCritic.Web.Models.Responses.Royale;

public record UserRoyaleHistoryViewModel(
    Guid UserID,
    string PlayerName,
    List<RoyaleYearQuarterViewModel> QuartersWon,
    List<RoyalePublisherHistoryViewModel> Publishers);
