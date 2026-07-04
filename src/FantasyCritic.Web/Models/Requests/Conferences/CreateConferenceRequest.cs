using FantasyCritic.Lib.Domain.Conferences;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.Requests.LeagueManager;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.Conferences;

public class CreateConferenceRequest
{
    public CreateConferenceRequest(string conferenceName, string primaryLeagueName, bool customRulesConference,
        LeagueYearSettingsViewModel leagueYearSettings, IReadOnlyList<DraftSettingsRequest> drafts)
    {
        ConferenceName = conferenceName;
        PrimaryLeagueName = primaryLeagueName;
        CustomRulesConference = customRulesConference;
        LeagueYearSettings = leagueYearSettings;
        Drafts = drafts;
    }

    public string ConferenceName { get; }
    public string PrimaryLeagueName { get; }
    public bool CustomRulesConference { get; }
    public LeagueYearSettingsViewModel LeagueYearSettings { get; }
    public IReadOnlyList<DraftSettingsRequest> Drafts { get; }

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

        if (Drafts.Count < 1)
        {
            return Result.Failure("At least one draft is required.");
        }

        for (int i = 0; i < Drafts.Count; i++)
        {
            if (Drafts[i].GamesToDraft < 1)
            {
                return Result.Failure($"Draft {i + 1}: games to draft must be at least 1.");
            }

            if (Drafts[i].CounterPicksToDraft < 0)
            {
                return Result.Failure($"Draft {i + 1}: counter picks to draft cannot be negative.");
            }
        }

        int totalGamesToDraft = Drafts.Sum(d => d.GamesToDraft);
        if (totalGamesToDraft > LeagueYearSettings.StandardGames)
        {
            return Result.Failure($"Total games to draft across all drafts ({totalGamesToDraft}) cannot exceed standard games ({LeagueYearSettings.StandardGames}).");
        }

        return Result.Success();
    }

    public ConferenceCreationParameters ToDomain(MinimalFantasyCriticUser manager, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        LeagueYearParameters leagueYearParameters = LeagueYearSettings.ToDomain(tagDictionary);
        var draftParams = Drafts.Select((d, i) => d.ToDomain(i)).ToList();
        return new ConferenceCreationParameters(manager, ConferenceName, PrimaryLeagueName, CustomRulesConference,
            leagueYearParameters, draftParams);
    }
}

public record EditConferenceRequest(Guid ConferenceID, string ConferenceName, bool CustomRulesConference);
