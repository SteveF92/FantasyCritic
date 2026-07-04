using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Requests.LeagueManager;

public class CreateLeagueRequest
{
    public CreateLeagueRequest(string leagueName, bool publicLeague, bool testLeague, bool customRulesLeague,
        LeagueYearSettingsViewModel leagueYearSettings, IReadOnlyList<DraftSettingsRequest> drafts)
    {
        LeagueName = leagueName;
        PublicLeague = publicLeague;
        TestLeague = testLeague;
        CustomRulesLeague = customRulesLeague;
        LeagueYearSettings = leagueYearSettings;
        Drafts = drafts;
    }

    public string LeagueName { get; }
    public bool PublicLeague { get; }
    public bool TestLeague { get; }
    public bool CustomRulesLeague { get; }
    public LeagueYearSettingsViewModel LeagueYearSettings { get; }
    public IReadOnlyList<DraftSettingsRequest> Drafts { get; }

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

    public LeagueCreationParameters ToDomain(FantasyCriticUser manager, IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
    {
        LeagueYearParameters leagueYearParameters = LeagueYearSettings.ToDomain(tagDictionary);
        var draftParams = Drafts.Select((d, i) => d.ToDomain(i)).ToList();
        return new LeagueCreationParameters(manager, LeagueName, PublicLeague, TestLeague, CustomRulesLeague,
            leagueYearParameters, draftParams);
    }
}
