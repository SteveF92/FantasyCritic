using FantasyCritic.Lib.Domain.AllTimeStats;

namespace FantasyCritic.Web.Models.Responses.AllTimeStats;

public class HallOfFameGameResponse
{
    public HallOfFameGameResponse(HallOfFameGame domain, LocalDate currentDate)
    {
        MasterGame = new MasterGameYearViewModel(domain.Game, currentDate);
        PickedBy = new MinimalPublisherViewModel(domain.PickedBy);
        Stats = domain.Stats.ToDictionary();
    }

    public MasterGameYearViewModel MasterGame { get; }
    public MinimalPublisherViewModel PickedBy { get; }
    public Dictionary<string, object> Stats { get; }
}
