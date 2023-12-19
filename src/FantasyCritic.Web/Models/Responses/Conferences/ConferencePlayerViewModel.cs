using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferencePlayerViewModel
{
    public ConferencePlayerViewModel(Conference conference, ConferencePlayer conferencePlayer)
    {
        UserID = conferencePlayer.User.Id;
        DisplayName = conferencePlayer.User.UserName;
        IsConferenceManager = conference.ConferenceManager.Equals(conferencePlayer.User);
        NumberOfLeaguesIn = conferencePlayer.LeaguesIn.Count;
        NumberOfLeaguesManaging = conferencePlayer.LeaguesManaging.Count;
    }

    public Guid UserID { get; }
    public string DisplayName { get; }
    public bool IsConferenceManager { get; }
    public int NumberOfLeaguesIn { get; }
    public int NumberOfLeaguesManaging { get; }
}
