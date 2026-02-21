namespace FantasyCritic.Web.Models.Responses;

public class PossibleMasterGameYearViewModel
{
    public PossibleMasterGameYearViewModel(PossibleMasterGameYear masterGame, LocalDate currentDate)
    {
        MasterGame = new MasterGameYearViewModel(masterGame.MasterGame, currentDate);
        Taken = masterGame.Taken;
        AlreadyOwned = masterGame.AlreadyOwned;
        IsEligible = masterGame.IsEligible;
        IsEligibleInOpenSlot = masterGame.IsEligibleInOpenSlot;
        IsReleased = masterGame.IsReleased;
        WillRelease = masterGame.WillReleaseStatus.CountAsWillRelease;
        HasScore = masterGame.HasScore;
        IsAvailable = masterGame.IsAvailable;
        Status = masterGame.GetStatus(currentDate);
    }

    public MasterGameYearViewModel MasterGame { get; }
    public bool Taken { get; }
    public bool AlreadyOwned { get; }
    public bool IsEligible { get; }
    public bool IsEligibleInOpenSlot { get; }
    public bool IsReleased { get; }
    public bool WillRelease { get; }
    public bool HasScore { get; }
    public bool IsAvailable { get; }
    public string Status { get; }
}
