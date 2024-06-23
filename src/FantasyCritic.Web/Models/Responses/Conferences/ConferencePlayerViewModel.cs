using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferencePlayerViewModel
{
    public ConferencePlayerViewModel(Conference conference, ConferencePlayer conferencePlayer)
    {
        UserID = conferencePlayer.User.UserID;
        DisplayName = conferencePlayer.User.DisplayName;
        IsConferenceManager = conference.ConferenceManager.UserID == conferencePlayer.User.UserID;
        LeaguesIn = conferencePlayer.LeaguesIn;
        LeaguesManaging = conferencePlayer.LeaguesManaging;

    }

    public Guid UserID { get; }
    public string DisplayName { get; }
    public bool IsConferenceManager { get; }
    public IReadOnlySet<Guid> LeaguesIn { get; }
    public IReadOnlySet<Guid> LeaguesManaging { get; }
}
