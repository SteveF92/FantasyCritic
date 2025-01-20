using FantasyCritic.Lib.Domain.AllTimeStats;

namespace FantasyCritic.Web.Models.Responses.AllTimeStats;

public class HallOfFameGameListResponse
{
    public HallOfFameGameListResponse(HallOfFameGameList domain, LocalDate currentDate)
    {
        Name = domain.Name;
        StatName = domain.StatName;
        StatType = domain.StatType;
        Games = domain.Games.Select(x => new HallOfFameGameResponse(x, currentDate)).ToList();
    }

    public string Name { get; }
    public string StatName { get; }
    public string StatType { get; }
    public List<HallOfFameGameResponse> Games { get; }
}
