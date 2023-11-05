using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using NodaTime;

namespace FantasyCritic.DBUtility;
public record ActionSetGrouping(LocalDate ProcessDate, IReadOnlyList<ActionProcessingSetMetadata> ProcessingSets)
{
    public override string ToString() => $"{ProcessDate.ToISOString()}_{ProcessingSets.Count} Sets";
}

public class TopBidsAndDropsEntity
{
    public int Year { get; set; }
    public LocalDate ProcessDate { get; set; }
    public Guid MasterGameID { get; set; }

    public int TotalStandardBidCount { get; set; }
    public int SuccessfulStandardBids { get; set; }
    public int FailedStandardBids { get; set; }
    public int TotalStandardBidLeagues { get; set; }
    public int TotalStandardBidAmount { get; set; }

    public int TotalCounterPickBidCount { get; set; }
    public int SuccessfulCounterPickBids { get; set; }
    public int FailedCounterPickBids { get; set; }
    public int TotalCounterPickBidLeagues { get; set; }
    public int TotalCounterPickBidAmount { get; set; }

    public int TotalDropCount { get; set; }
    public int SuccessfulDrops { get; set; }
    public int FailedDrops { get; set; }
}
