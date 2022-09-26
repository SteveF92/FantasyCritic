using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities;

internal class MasterGameChangeRequestEntity
{
    public MasterGameChangeRequestEntity()
    {

    }

    public MasterGameChangeRequestEntity(MasterGameChangeRequest domain)
    {
        RequestID = domain.RequestID;
        UserID = domain.User.Id;
        RequestTimestamp = domain.RequestTimestamp;
        RequestNote = domain.RequestNote;
        MasterGameID = domain.MasterGame.MasterGameID;
        OpenCriticID = domain.OpenCriticID;
        GGToken = domain.GGToken;

        Answered = domain.Answered;
        ResponseTimestamp = domain.ResponseTimestamp;
        ResponseNote = domain.ResponseNote;
        ResponseUserID = domain.ResponseUser?.Id;

        Hidden = domain.Hidden;
    }

    //Request
    public Guid RequestID { get; set; }
    public Guid UserID { get; set; }
    public Instant RequestTimestamp { get; set; }
    public string RequestNote { get; set; } = null!;
    public Guid MasterGameID { get; set; }
    public int? OpenCriticID { get; set; }
    public string? GGToken { get; set; }

    //Response
    public bool Answered { get; set; }
    public Instant? ResponseTimestamp { get; set; }
    public string? ResponseNote { get; set; }
    public Guid? ResponseUserID { get; set; }

    public bool Hidden { get; set; }

    public MasterGameChangeRequest ToDomain(FantasyCriticUser user, MasterGame masterGame, FantasyCriticUser? responseUser)
    {
        return new MasterGameChangeRequest(RequestID, user, RequestTimestamp, RequestNote, masterGame, OpenCriticID, GGToken, Answered, ResponseTimestamp, ResponseNote, responseUser, Hidden);
    }
}
