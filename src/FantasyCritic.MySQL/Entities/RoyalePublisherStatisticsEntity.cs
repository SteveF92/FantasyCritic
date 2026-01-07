
namespace FantasyCritic.MySQL.Entities;

public class RoyalePublisherStatisticsEntity
{
    public Guid PublisherID { get; set; }
    public LocalDate Date { get; set; }
    public decimal FantasyPoints { get; set; }
}
