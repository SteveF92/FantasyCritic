using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain;

public class MasterGameChangeRequest
{
    public MasterGameChangeRequest(Guid requestID, FantasyCriticUser user, Instant requestTimestamp, string requestNote,
        MasterGame masterGame, int? openCriticID, string? ggToken, bool answered, Instant? responseTimestamp, string? responseNote, FantasyCriticUser? responseUser, bool hidden)
    {
        RequestID = requestID;
        User = user;
        RequestTimestamp = requestTimestamp;
        RequestNote = requestNote;
        MasterGame = masterGame;
        OpenCriticID = openCriticID;
        GGToken = ggToken;
        Answered = answered;
        ResponseTimestamp = responseTimestamp;
        ResponseNote = responseNote;
        ResponseUser = responseUser;
        Hidden = hidden;
    }

    //Request
    public Guid RequestID { get; }
    public FantasyCriticUser User { get; }
    public Instant RequestTimestamp { get; }
    public string RequestNote { get; }
    public MasterGame MasterGame { get; }
    public int? OpenCriticID { get; }
    public string? GGToken { get; }

    //Answer
    public bool Answered { get; }
    public Instant? ResponseTimestamp { get; }
    public string? ResponseNote { get; }
    public FantasyCriticUser? ResponseUser { get; }

    public bool Hidden { get; }
}
