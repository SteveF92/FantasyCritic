using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Extensions;

namespace FantasyCritic.Web.Models.Requests.MasterGame;

public class MasterGameChangeRequestRequest
{
    public MasterGameChangeRequestRequest(Guid masterGameID, string requestNote)
    {
        MasterGameID = masterGameID;
        RequestNote = requestNote;
    }

    public Guid MasterGameID { get; }
    public string RequestNote { get; }

    public string? OpenCriticLink { get; init; }
    public string? GGLink { get; init; }

    public MasterGameChangeRequest ToDomain(FantasyCriticUser user, Instant requestTimestamp, Lib.Domain.MasterGame masterGame)
    {
        int? openCriticID = URLParsingExtensions.GetOpenCriticIDFromURL(OpenCriticLink);
        var ggToken = URLParsingExtensions.GetGGTokenFromURL(GGLink);
        return new MasterGameChangeRequest(Guid.NewGuid(), user, requestTimestamp, RequestNote, masterGame, openCriticID, ggToken, false, null, null, null, false);
    }
}
