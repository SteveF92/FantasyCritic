using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class CreateLeagueRequest
{
    public CreateLeagueRequest(string leagueName, bool publicLeague, bool testLeague, bool customRulesLeague, LeagueYearSettingsViewModel leagueYearSettings)
    {
        LeagueName = leagueName;
        PublicLeague = publicLeague;
        TestLeague = testLeague;
        CustomRulesLeague = customRulesLeague;
        LeagueYearSettings = leagueYearSettings;
    }

    public string LeagueName { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public bool CustomRulesLeague { get; }
    public LeagueYearSettingsViewModel LeagueYearSettings { get; }

    public Result IsValid()
    {
        if (string.IsNullOrWhiteSpace(LeagueName))
        {
            return Result.Failure("You cannot have a blank league name.");
        }

        var settingsValid = LeagueYearSettings.IsValid();
        if (settingsValid.IsFailure)
        {
            return Result.Failure(settingsValid.Error);
        }

        return Result.Success();
    }

    public LeagueCreationParameters ToDomain(FantasyCriticUser manager, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        LeagueYearParameters leagueYearParameters = LeagueYearSettings.ToDomain(tagDictionary);
        LeagueCreationParameters parameters = new LeagueCreationParameters(manager, LeagueName, PublicLeague, TestLeague, CustomRulesLeague, leagueYearParameters);
        return parameters;
    }
}
