namespace FantasyCritic.Lib.Domain.Requests;

public class ClaimGameDomainRequest
{
    public ClaimGameDomainRequest(LeagueYear leagueYear, Publisher publisher, string gameName, bool counterPick, bool counterPickedGameIsManualWillNotRelease,
        bool managerOverride, bool autoDraft, MasterGame? masterGame, int? draftPosition, int? overallDraftPosition)
    {
        LeagueYear = leagueYear;
        Publisher = publisher;
        GameName = gameName;
        CounterPick = counterPick;
        CounterPickedGameIsManualWillNotRelease = counterPickedGameIsManualWillNotRelease;
        ManagerOverride = managerOverride;
        AutoDraft = autoDraft;
        MasterGame = masterGame;
        DraftPosition = draftPosition;
        OverallDraftPosition = overallDraftPosition;
    }

    public LeagueYear LeagueYear { get; }
    public Publisher Publisher { get; }
    public string GameName { get; }
    public bool CounterPick { get; }
    public bool CounterPickedGameIsManualWillNotRelease { get; }
    public bool ManagerOverride { get; }
    public bool AutoDraft { get; }
    public MasterGame? MasterGame { get; }
    public int? DraftPosition { get; }
    public int? OverallDraftPosition { get; }
}
