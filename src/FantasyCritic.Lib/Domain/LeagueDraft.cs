namespace FantasyCritic.Lib.Domain;

public class LeagueDraft
{
    public LeagueDraft(Guid draftID, LeagueYearKey leagueYearKey, int draftNumber, string name, LocalDate? scheduledDate,
        int gamesToDraft, int counterPicksToDraft, bool draftOrderSet, PlayStatus playStatus,
        IEnumerable<PublisherDraftInfo> publisherDraftInfo, Instant? draftStartedTimestamp)
    {
        DraftID = draftID;
        LeagueYearKey = leagueYearKey;
        DraftNumber = draftNumber;
        Name = name;
        ScheduledDate = scheduledDate;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
        DraftOrderSet = draftOrderSet;
        PlayStatus = playStatus;
        PublisherDraftInfo = publisherDraftInfo.ToList();
        DraftStartedTimestamp = draftStartedTimestamp;
    }

    public Guid DraftID { get; }
    public LeagueYearKey LeagueYearKey { get; }
    public int DraftNumber { get; }
    public string Name { get; }
    public LocalDate? ScheduledDate { get; }
    public int GamesToDraft { get; }
    public int CounterPicksToDraft { get; }
    public bool DraftOrderSet { get; }
    public PlayStatus PlayStatus { get; }
    public IReadOnlyList<PublisherDraftInfo> PublisherDraftInfo { get; }
    public Instant? DraftStartedTimestamp { get; }

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

    public LeagueDraft UpdateDraft(int gamesToDraft, int counterPicksToDraft)
    {
        return new LeagueDraft(DraftID, LeagueYearKey, DraftNumber, Name, ScheduledDate, gamesToDraft, counterPicksToDraft,
            DraftOrderSet, PlayStatus, PublisherDraftInfo, DraftStartedTimestamp);
    }
}
