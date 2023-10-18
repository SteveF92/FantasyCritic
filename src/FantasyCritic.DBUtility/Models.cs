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
    public int TotalBidCount { get; set; }
    public int SuccessfulBids { get; set; }
    public int FailedBids { get; set; }
    public int TotalBidLeagues { get; set; }
    public int TotalBidAmount { get; set; }
    public int TotalDropCount { get; set; }
    public int SuccessfulDrops { get; set; }
    public int FailedDrops { get; set; }
}
