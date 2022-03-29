using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities;

internal class MasterGameRequestEntity
{
    public MasterGameRequestEntity()
    {

    }

    public MasterGameRequestEntity(MasterGameRequest domain)
    {
        RequestID = domain.RequestID;
        UserID = domain.User.Id;
        RequestTimestamp = domain.RequestTimestamp;
        RequestNote = domain.RequestNote;

        GameName = domain.GameName;
        SteamID = domain.SteamID;
        OpenCriticID = domain.OpenCriticID;
        GGToken = domain.GGToken;
        ReleaseDate = domain.ReleaseDate;
        EstimatedReleaseDate = domain.EstimatedReleaseDate;

        Answered = domain.Answered;
        ResponseTimestamp = domain.ResponseTimestamp;
        ResponseNote = domain.ResponseNote;

        if (domain.MasterGame is not null)
        {
            MasterGameID = domain.MasterGame.MasterGameID;
        }

        Hidden = domain.Hidden;
    }

    //Request
    public Guid RequestID { get; set; }
    public Guid UserID { get; set; }
    public Instant RequestTimestamp { get; set; }
    public string RequestNote { get; set; }

    //Game Details
    public string GameName { get; set; }
    public int? SteamID { get; set; }
    public int? OpenCriticID { get; set; }
    public string? GGToken { get; set; }
    public LocalDate? ReleaseDate { get; set; }
    public string EstimatedReleaseDate { get; set; }

    //Response
    public bool Answered { get; set; }
    public Instant? ResponseTimestamp { get; set; }
    public string ResponseNote { get; set; }
    public Guid? MasterGameID { get; set; }

    public bool Hidden { get; set; }

    public MasterGameRequest ToDomain(FantasyCriticUser user, MasterGame? masterGame)
    {
        return new MasterGameRequest(RequestID, user, RequestTimestamp, RequestNote, GameName, SteamID, OpenCriticID, GGToken, ReleaseDate, EstimatedReleaseDate,
            Answered, ResponseTimestamp, ResponseNote, masterGame, Hidden);
    }
}
