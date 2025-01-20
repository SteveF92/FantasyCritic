using FantasyCritic.Lib.Domain.AllTimeStats;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses.AllTimeStats;

public class HallOfFameGameResponse
{
    public HallOfFameGameResponse(HallOfFameGame domain, LocalDate currentDate)
    {
        MasterGame = new MasterGameViewModel(domain.Game, currentDate);
        PickedBy = new MinimalPublisherViewModel(domain.PickedBy);
        Stat = domain.Stat;
    }

    public MasterGameViewModel MasterGame { get; }
    public MinimalPublisherViewModel PickedBy { get; }
    public object Stat { get; }
}
