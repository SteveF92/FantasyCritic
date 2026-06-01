namespace FantasyCritic.Web.Models.Responses.Royale;

public record RoyaleQuarterDataViewModel(
    IEnumerable<RoyaleYearQuarterViewModel> RoyaleYearQuarters,
    RoyaleYearQuarterViewModel RoyaleYearQuarter,
    Guid? UserRoyalePublisherID,
    List<RoyaleStandingsViewModel> RoyaleStandings,
    List<RoyalePublisherViewModel> TopPublishers);
