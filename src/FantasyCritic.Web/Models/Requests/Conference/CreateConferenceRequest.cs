using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.Conference;

public class CreateConferenceRequest
{
    public CreateConferenceRequest(string conferenceName, string primaryLeagueName, bool customRulesConference, LeagueYearSettingsViewModel leagueYearSettings)
    {
        ConferenceName = conferenceName;
        PrimaryLeagueName = primaryLeagueName;
        CustomRulesConference = customRulesConference;
        LeagueYearSettings = leagueYearSettings;
    }

    public string ConferenceName { get; }
    public string PrimaryLeagueName { get; }
    public bool CustomRulesConference { get; }
    public LeagueYearSettingsViewModel LeagueYearSettings { get; }

    public Result IsValid()
    {
        if (string.IsNullOrWhiteSpace(ConferenceName))
        {
            return Result.Failure("You cannot have a blank conference name.");
        }

        var settingsValid = LeagueYearSettings.IsValid();
        if (settingsValid.IsFailure)
        {
            return Result.Failure(settingsValid.Error);
        }

        return Result.Success();
    }
}
