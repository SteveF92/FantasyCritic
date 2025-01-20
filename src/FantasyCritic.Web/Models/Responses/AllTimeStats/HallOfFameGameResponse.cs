using FantasyCritic.Lib.Domain.AllTimeStats;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses.AllTimeStats;

public class HallOfFameGameResponse
{
    public HallOfFameGameResponse(HallOfFameGame domain, LocalDate currentDate)
    {
        MasterGame = new MasterGameViewModel(domain.Game, currentDate);
        Stat = domain.Stat;
    }

    public MasterGameViewModel MasterGame { get; }
    public object Stat { get; }
}
