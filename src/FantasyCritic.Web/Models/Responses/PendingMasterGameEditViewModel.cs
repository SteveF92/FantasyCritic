using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses;

public class PendingMasterGameEditViewModel
{
    public PendingMasterGameEditViewModel(MasterGameEditMessage domain, LocalDate currentDate)
    {
        MasterGameUpdateID = domain.MasterGameUpdateID;
        MasterGame = new MasterGameViewModel(domain.EditedGame, currentDate);
        Changes = domain.Changes;
    }

    public Guid MasterGameUpdateID { get; }
    public MasterGameViewModel MasterGame { get; }
    public IReadOnlyList<string> Changes { get; }
}
