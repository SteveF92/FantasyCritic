using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Extensions;

namespace FantasyCritic.Web.Models.Requests.MasterGame;

public class MasterGameChangeRequestRequest
{
    [Required]
    public Guid MasterGameID { get; set; }
    [Required]
    public string RequestNote { get; set; }
    public string OpenCriticLink { get; set; }
    public string GGLink { get; set; }

    public MasterGameChangeRequest ToDomain(FantasyCriticUser user, Instant requestTimestamp, Lib.Domain.MasterGame masterGame)
    {
        int? openCriticID = URLParsingExtensions.GetOpenCriticIDFromURL(OpenCriticLink);
        var ggToken = URLParsingExtensions.GetGGTokenFromURL(GGLink);
        return new MasterGameChangeRequest(Guid.NewGuid(), user, requestTimestamp, RequestNote, masterGame, openCriticID, ggToken, false, null, null, false);
    }
}
