using FantasyCritic.Lib.Domain.Calculations;

namespace FantasyCritic.MySQL.Entities;

public class PublisherStatisticsEntity
{
    public PublisherStatisticsEntity()
    {

    }

    public PublisherStatisticsEntity(PublisherStatistics domain)
    {
        PublisherID = domain.PublisherID;
        Date = domain.Date;
        FantasyPoints = domain.FantasyPoints;
        ProjectedPoints = domain.ProjectedPoints;
        RemainingBudget = domain.RemainingBudget;
        NumberOfStandardGames = domain.NumberOfStandardGames;
        NumberOfStandardGamesReleased = domain.NumberOfStandardGamesReleased;
        NumberOfStandardGamesExpectedToRelease = domain.NumberOfStandardGamesExpectedToRelease;
        NumberOfStandardGamesNotExpectedToRelease = domain.NumberOfStandardGamesNotExpectedToRelease;
        NumberOfCounterPicks = domain.NumberOfCounterPicks;
        NumberOfCounterPicksReleased = domain.NumberOfCounterPicksReleased;
        NumberOfCounterPicksExpectedToRelease = domain.NumberOfCounterPicksExpectedToRelease;
        NumberOfCounterPicksNotExpectedToRelease = domain.NumberOfCounterPicksNotExpectedToRelease;
    }

    public Guid PublisherID { get; set; }
    public LocalDate Date { get; set; }
    public decimal FantasyPoints { get; set; }
    public decimal ProjectedPoints { get; set; }
    public ushort RemainingBudget { get; set; }
    public byte NumberOfStandardGames { get; set; }
    public byte NumberOfStandardGamesReleased { get; set; }
    public byte NumberOfStandardGamesExpectedToRelease { get; set; }
    public byte NumberOfStandardGamesNotExpectedToRelease { get; set; }
    public byte NumberOfCounterPicks { get; set; }
    public byte NumberOfCounterPicksReleased { get; set; }
    public byte NumberOfCounterPicksExpectedToRelease { get; set; }
    public byte NumberOfCounterPicksNotExpectedToRelease { get; set; }
}
