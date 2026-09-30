using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses;

public class PendingNewMasterGameViewModel
{
    public PendingNewMasterGameViewModel(NewMasterGameMessage domain, LocalDate currentDate)
    {
        MasterGameUpdateID = domain.MasterGameUpdateID;
        MasterGame = new MasterGameViewModel(domain.MasterGame, currentDate);
    }

    public Guid MasterGameUpdateID { get; }
    public MasterGameViewModel MasterGame { get; }
}
