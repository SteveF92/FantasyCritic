using FantasyCritic.Lib.Domain.AllTimeStats;

namespace FantasyCritic.Web.Models.Responses.AllTimeStats;

public class HallOfFameGameResponse
{
    public HallOfFameGameResponse(HallOfFameGame domain, LocalDate currentDate)
    {
        MasterGame = new MasterGameYearViewModel(domain.Game, currentDate);
        PickedBy = new MinimalPublisherViewModel(domain.PickedBy);
        Stats = domain.Stats.ToDictionary(x => x.Key, y => new HallOfFameGameStatResponse(y.Value.Stat, y.Value.StatType));
    }

    public MasterGameYearViewModel MasterGame { get; }
    public MinimalPublisherViewModel PickedBy { get; }
    public Dictionary<string, HallOfFameGameStatResponse> Stats { get; }
}

public record HallOfFameGameStatResponse(object Stat, string StatType);
