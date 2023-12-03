using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.Conferences;

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

        if (string.IsNullOrWhiteSpace(PrimaryLeagueName))
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

    public ConferenceCreationParameters ToDomain(FantasyCriticUser manager, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        LeagueYearParameters leagueYearParameters = LeagueYearSettings.ToDomain(tagDictionary);
        ConferenceCreationParameters parameters = new ConferenceCreationParameters(manager, ConferenceName, PrimaryLeagueName, CustomRulesConference, leagueYearParameters);
        return parameters;
    }
}

public record EditConferenceRequest(Guid ConferenceID, string ConferenceName, bool CustomRulesConference)
{
    public Result IsValid()
    {
        if (string.IsNullOrWhiteSpace(ConferenceName))
        {
            return Result.Failure("You cannot have a blank conference name.");
        }

        return Result.Success();
    }
}
