using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Utilities;
using FantasyCritic.Web.Extensions;

namespace FantasyCritic.Web.Models.Requests.MasterGame;

public class MasterGameRequestRequest
{
    public MasterGameRequestRequest(string gameName, string estimatedReleaseDate, string requestNote)
    {
        GameName = gameName;
        EstimatedReleaseDate = estimatedReleaseDate;
        RequestNote = requestNote;
    }

    public string GameName { get; }
    public string EstimatedReleaseDate { get; }
    public string RequestNote { get; }

    public string? SteamLink { get; init; }
    public string? OpenCriticLink { get; init; }
    public string? GGLink { get; init; }
    public LocalDate? ReleaseDate { get; init; }

    public MasterGameRequest ToDomain(FantasyCriticUser user, Instant requestTimestamp)
    {
        int? steamID = null;
        var steamGameIDString = SubstringSearching.GetBetween(SteamLink, "/app/", "/");
        if (steamGameIDString.IsSuccess)
        {
            bool parseResult = int.TryParse(steamGameIDString.Value, out int steamIDResult);
            if (parseResult)
            {
                steamID = steamIDResult;
            }
        }

        int? openCriticID = URLParsingExtensions.GetOpenCriticIDFromURL(OpenCriticLink);
        var ggToken = URLParsingExtensions.GetGGTokenFromURL(GGLink);

        return new MasterGameRequest(Guid.NewGuid(), user, requestTimestamp, RequestNote, GameName, steamID, openCriticID, ggToken,
            ReleaseDate, EstimatedReleaseDate, false, null, null, null, null, false);
    }
}
