using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.Lib.Domain.Requests;

public record NewDraftLeagueSettingsChanges(
    LeagueOptions NewLeagueOptions,
    LeagueManagerAction LeagueManagerAction);
