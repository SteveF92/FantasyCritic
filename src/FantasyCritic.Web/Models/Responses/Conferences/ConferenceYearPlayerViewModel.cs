using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.Web.Models.Responses.Conferences;

public class ConferenceYearPlayerViewModel
{
    public ConferenceYearPlayerViewModel(ConferenceYear conferenceYear, ConferencePlayer conferencePlayer)
    {
        UserID = conferencePlayer.User.Id;
        DisplayName = conferencePlayer.User.UserName;
        LeaguesActiveIn = conferencePlayer.YearsActiveIn.Where(x => x.Year == conferenceYear.Year).Select(x => x.LeagueID).ToHashSet();
    }

    public Guid UserID { get; }
    public string DisplayName { get; }
    public IReadOnlySet<Guid> LeaguesActiveIn { get; }
}
