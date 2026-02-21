namespace FantasyCritic.Lib.Domain;

public class PossibleMasterGameYear
{
    public PossibleMasterGameYear(MasterGameYear masterGame, bool taken, bool alreadyOwned, bool isEligible, bool isEligibleInOpenSlot,
        bool isReleased, WillReleaseStatus willReleaseStatus, bool hasScore)
    {
        MasterGame = masterGame;
        Taken = taken;
        AlreadyOwned = alreadyOwned;
        IsEligible = isEligible;
        IsEligibleInOpenSlot = isEligibleInOpenSlot;
        IsReleased = isReleased;
        WillReleaseStatus = willReleaseStatus;
        HasScore = hasScore;
    }

    public MasterGameYear MasterGame { get; }
    public bool Taken { get; }
    public bool AlreadyOwned { get; }
    public bool IsEligible { get; }
    public bool IsEligibleInOpenSlot { get; }
    public bool IsReleased { get; }
    public WillReleaseStatus WillReleaseStatus { get; }
    public bool HasScore { get; }

    public bool IsAvailable => !Taken && !AlreadyOwned && IsEligible && !IsReleased && WillReleaseStatus.CountAsWillRelease && !HasScore;

    public string GetStatus(LocalDate currentDate)
    {
        if (Taken)
        {
            return "Taken";
        }
        if (AlreadyOwned)
        {
            return "Already Owned";
        }
        if (MasterGame.IsReleasingToday(currentDate))
        {
            return "Releasing Today";
        }
        if (IsReleased)
        {
            return "Released";
        }
        if (HasScore)
        {
            return "Has Score";
        }
        if (!IsEligible)
        {
            return "Ineligible";
        }
        if (!WillReleaseStatus.CountAsWillRelease)
        {
            return "Will Not Release";
        }

        return "Available";
    }
}
