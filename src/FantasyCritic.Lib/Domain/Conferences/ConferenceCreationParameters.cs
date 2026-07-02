using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Conferences;
public class ConferenceCreationParameters
{
    public ConferenceCreationParameters(MinimalFantasyCriticUser manager, string conferenceName, string primaryLeagueName, bool customRulesConference, LeagueYearParameters leagueYearParameters, IEnumerable<DraftParameters> drafts)
    {
        Manager = manager;
        ConferenceName = conferenceName;
        PrimaryLeagueName = primaryLeagueName;
        CustomRulesConference = customRulesConference;
        LeagueYearParameters = leagueYearParameters;
        Drafts = drafts.ToList();
    }

    public MinimalFantasyCriticUser Manager { get; }
    public string ConferenceName { get; }
    public string PrimaryLeagueName { get; }
    public bool CustomRulesConference { get; }
    public LeagueYearParameters LeagueYearParameters { get; }
    public IReadOnlyList<DraftParameters> Drafts { get; }
}
