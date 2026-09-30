using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses;

public class PendingScoreUpdateViewModel
{
    public PendingScoreUpdateViewModel(GameCriticScoreUpdateMessage domain, LocalDate currentDate)
    {
        MasterGameUpdateID = domain.MasterGameUpdateID;
        MasterGame = new MasterGameViewModel(domain.Game, currentDate);
        OldCriticScore = domain.OldCriticScore;
        NewCriticScore = domain.NewCriticScore;
    }

    public Guid MasterGameUpdateID { get; }
    public MasterGameViewModel MasterGame { get; }
    public decimal? OldCriticScore { get; }
    public decimal? NewCriticScore { get; }
}
