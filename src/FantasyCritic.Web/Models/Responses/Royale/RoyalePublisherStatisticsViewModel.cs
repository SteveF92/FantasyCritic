using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class RoyalePublisherStatisticsViewModel
{
    public RoyalePublisherStatisticsViewModel(RoyalePublisherStatistics domain)
    {
        Date = domain.Date;
        FantasyPoints = domain.FantasyPoints;
    }

    public LocalDate Date { get; }
    public decimal FantasyPoints { get; }
}
