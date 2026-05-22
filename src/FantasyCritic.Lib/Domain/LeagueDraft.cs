using FantasyCritic.Lib.Extensions;
using NodaTime;

namespace FantasyCritic.Lib.Domain;

public class LeagueDraft
{
    public LeagueDraft(Guid draftID, LeagueYearKey leagueYearKey, int draftNumber, int gamesToDraft, int counterPicksToDraft, PlayStatus playStatus,
        IEnumerable<PublisherDraftInfo> publisherDraftInfos, Instant? draftStartedTimestamp)
    {
        DraftID = draftID;
        LeagueYearKey = leagueYearKey;
        DraftNumber = draftNumber;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
        PlayStatus = playStatus;
        PublisherDraftInfos = publisherDraftInfos.ToList();
        DraftStartedTimestamp = draftStartedTimestamp;
    }

    public Guid DraftID { get; }
    public LeagueYearKey LeagueYearKey { get; }
    public int DraftNumber { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public PlayStatus PlayStatus { get; }
    public IReadOnlyList<PublisherDraftInfo> PublisherDraftInfos { get; }
    public Instant? DraftStartedTimestamp { get; }
    public bool DraftOrderSet => PublisherDraftInfos.Any();

    public static Result ValidateDraftCounts(int gamesToDraft, int counterPicksToDraft, int standardGames, int counterPicks)
    {
        if (gamesToDraft > standardGames)
        {
            return Result.Failure("Can't draft more than the total number of standard games.");
        }

        if (counterPicksToDraft > counterPicks)
        {
            return Result.Failure("Can't draft more counter picks than the total number of counter picks.");
        }

        if (counterPicksToDraft > gamesToDraft)
        {
            return Result.Failure("Can't have more drafted counter picks than drafted games.");
        }

        return Result.Success();
    }
}
