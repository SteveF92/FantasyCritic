namespace FantasyCritic.Web.Models.Requests.Conferences;

public record AddLeagueToConferenceRequest(Guid ConferenceID, int Year, string LeagueName, Guid LeagueManager)
{
    public Result IsValid()
    {
        if (string.IsNullOrWhiteSpace(LeagueName))
        {
            return Result.Failure("You cannot have a blank league name.");
        }

        return Result.Success();
    }
}
