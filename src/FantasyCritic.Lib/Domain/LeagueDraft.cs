using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Domain;

public class LeagueDraft
{
    public LeagueDraft(Guid draftID, LeagueYearKey leagueYearKey, int draftNumber, string name, LocalDate? scheduledDate,
        int gamesToDraft, int counterPicksToDraft, bool counterPicksMustBeFromThisDraft, bool draftOrderSet, PlayStatus playStatus,
        IEnumerable<PublisherDraftInfo> publisherDraftInfo, Instant? draftStartedTimestamp)
    {
        DraftID = draftID;
        LeagueYearKey = leagueYearKey;
        DraftNumber = draftNumber;
        Name = name;
        ScheduledDate = scheduledDate;
        GamesToDraft = gamesToDraft;
        CounterPicksToDraft = counterPicksToDraft;
        CounterPicksMustBeFromThisDraft = counterPicksMustBeFromThisDraft;
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
    public bool CounterPicksMustBeFromThisDraft { get; }
    public bool DraftOrderSet { get; }
    public PlayStatus PlayStatus { get; }
    public IReadOnlyList<PublisherDraftInfo> PublisherDraftInfo { get; }
    public Instant? DraftStartedTimestamp { get; }


    public LeagueDraft UpdateDraft(string name, LocalDate? scheduledDate, int gamesToDraft, int counterPicksToDraft,
        bool counterPicksMustBeFromThisDraft)
    {
        return new LeagueDraft(DraftID, LeagueYearKey, DraftNumber, name, scheduledDate, gamesToDraft, counterPicksToDraft,
            counterPicksMustBeFromThisDraft, DraftOrderSet, PlayStatus, PublisherDraftInfo, DraftStartedTimestamp);
    }

    public Result<(int StandardGameStartingPoint, int CounterPickStartingPoint)> GetStartingOverallDraftPosition(LeagueYear leagueYear)
    {
        if (DraftNumber == 1)
        {
            return Result.Success((0, 0));
        }

        if (!DraftStartedTimestamp.HasValue)
        {
            return Result.Failure<(int StandardGameStartingPoint, int CounterPickStartingPoint)>("Draft has not started.");
        }

        var draftStartedTimestamp = DraftStartedTimestamp.Value;
        var gamesTakenBeforeThisDraft = leagueYear.Publishers
            .SelectMany(x => x.GetCurrentAndFormerPublisherGames())
            .Where(x => !x.AcquiredInTradeID.HasValue)
            .GroupBy(x => x.CounterPick)
            .ToDictionary(x => x.Key, y => y.Count(z => z.Timestamp < draftStartedTimestamp));
        
        var tuple = (gamesTakenBeforeThisDraft.GetValueOrDefault(false, 0), gamesTakenBeforeThisDraft.GetValueOrDefault(true, 0));
        return Result.Success(tuple);
    }

    public LeagueOptionsDifferences GetDifferences(LeagueDraft existingDraft)
    {
        List<string> differences = new List<string>();

        if (GamesToDraft != existingDraft.GamesToDraft)
        {
            differences.Add($"Games to draft changed from {existingDraft.GamesToDraft} to {GamesToDraft}.");
        }

        if (CounterPicksToDraft != existingDraft.CounterPicksToDraft)
        {
            differences.Add($"Counter picks to draft changed from {existingDraft.CounterPicksToDraft} to {CounterPicksToDraft}.");
        }

        if (CounterPicksMustBeFromThisDraft != existingDraft.CounterPicksMustBeFromThisDraft)
        {
            differences.Add($"Counter picks must be from this draft changed from {existingDraft.CounterPicksMustBeFromThisDraft} to {CounterPicksMustBeFromThisDraft}.");
        }

        if (Name != existingDraft.Name)
        {
            differences.Add($"Draft name changed from '{existingDraft.Name}' to '{Name}'.");
        }

        if (ScheduledDate != existingDraft.ScheduledDate)
        {
            differences.Add(GetScheduledDateDifferenceString(existingDraft.ScheduledDate, ScheduledDate));
        }

        return new LeagueOptionsDifferences(differences);
    }

    private static string GetScheduledDateDifferenceString(LocalDate? existingScheduledDate, LocalDate? newScheduledDate)
    {
        if (existingScheduledDate is null && newScheduledDate is not null)
        {
            return $"Draft Scheduled for {newScheduledDate.Value.ToLongDate()}.";
        }

        if (existingScheduledDate is not null && newScheduledDate is null)
        {
            return $"Scheduled Date of {existingScheduledDate.Value.ToLongDate()} removed.";
        }

        return $"Draft scheduled date changed from {existingScheduledDate!.Value.ToLongDate()} to {newScheduledDate!.Value.ToLongDate()}.";
    }
}
