using FantasyCritic.SharedSerialization.API;

namespace FantasyCritic.Web.Models.Responses;

public class QueuedGameViewModel
{
    public QueuedGameViewModel(QueuedGame queuedGame, LocalDate currentDate, bool taken, bool alreadyOwned)
    {
        MasterGame = new MasterGameViewModel(queuedGame.MasterGame, currentDate);
        Rank = queuedGame.Rank;
        Taken = taken;
        AlreadyOwned = alreadyOwned;
    }

    public MasterGameViewModel MasterGame { get; }
    public int Rank { get; }
    public bool Taken { get; }
    public bool AlreadyOwned { get; }
    public bool IsAvailable => !Taken && !AlreadyOwned;

    public string Status
    {
        get
        {
            if (Taken)
            {
                return "Taken";
            }
            if (AlreadyOwned)
            {
                return "Already Owned";
            }

            return "Available";
        }
    }
}
