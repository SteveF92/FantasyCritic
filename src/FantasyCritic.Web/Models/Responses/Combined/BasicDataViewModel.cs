using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses.Combined;

public record BasicDataViewModel(
    BidTimesViewModel BidTimes,
    List<MasterGameTagViewModel> MasterGameTags,
    LeagueOptionsViewModel LeagueOptions,
    List<SupportedYearViewModel> SupportedYears);
