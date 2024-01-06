using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class MinimalConferenceViewModel
{
    public MinimalConferenceViewModel(Conference domain, bool isManager)
    {
        ConferenceID = domain.ConferenceID;
        ConferenceName = domain.ConferenceName;
        IsManager = isManager;
        Years = domain.Years;
        ActiveYear = domain.Years.Max();
        CustomRulesConference = domain.CustomRulesConference;
        ConferenceManagerDisplayName = domain.ConferenceManager.UserName;
    }

    public Guid ConferenceID { get; }
    public string ConferenceName { get; }
    public bool IsManager { get; }
    public IReadOnlyList<int> Years { get; }
    public int ActiveYear { get; }
    public bool CustomRulesConference { get; }
    public string ConferenceManagerDisplayName { get; }
}
