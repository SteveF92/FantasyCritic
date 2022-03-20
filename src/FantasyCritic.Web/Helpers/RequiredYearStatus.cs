namespace FantasyCritic.Web.Helpers;

public class RequiredYearStatus : TypeSafeEnum<RequiredYearStatus>
{
    // Define values here.
    public static readonly RequiredYearStatus Any = new RequiredYearStatus("Any");
    public static readonly RequiredYearStatus AnyYearNotFinished = new RequiredYearStatus("AnyYearNotFinished");
    public static readonly RequiredYearStatus YearNotFinishedDraftNotStarted = new RequiredYearStatus("YearNotFinishedDraftNotStarted");
    public static readonly RequiredYearStatus PlayOpenDraftNotStarted = new RequiredYearStatus("PlayOpenDraftNotStarted");
    public static readonly RequiredYearStatus DuringDraft = new RequiredYearStatus("DuringDraft");
    public static readonly RequiredYearStatus DraftPaused = new RequiredYearStatus("DraftPaused");
    public static readonly RequiredYearStatus ActiveDraft = new RequiredYearStatus("ActiveDraft");
    public static readonly RequiredYearStatus YearNotFinishedDraftNotFinished = new RequiredYearStatus("YearNotFinishedDraftNotFinished");
    public static readonly RequiredYearStatus YearNotFinishedDraftFinished = new RequiredYearStatus("YearNotFinishedDraftFinished");

    // Constructor is private: values are defined within this class only!
    private RequiredYearStatus(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;

    public Result StateIsValid(LeagueYear leagueYear)
    {
        switch (this)
        {
            case { } status when Any.Equals(status):
                break;
            case { } status when AnyYearNotFinished.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                break;
            case { } status when YearNotFinishedDraftNotStarted.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (leagueYear.PlayStatus.PlayStarted)
                {
                    return Result.Failure("That action can only be taken before the draft starts.");
                }
                break;
            case { } status when YearNotFinishedDraftNotFinished.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (leagueYear.PlayStatus.DraftFinished)
                {
                    return Result.Failure("That action cannot be taken after the draft is finished.");
                }
                break;
            case { } status when DuringDraft.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (leagueYear.PlayStatus.Equals(PlayStatus.NotStartedDraft) || leagueYear.PlayStatus.Equals(PlayStatus.DraftFinal))
                {
                    return Result.Failure("That action can only be taken during the draft is active.");
                }
                break;
            case { } status when ActiveDraft.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (!leagueYear.PlayStatus.DraftIsActive)
                {
                    return Result.Failure("That action can only be taken while the draft is active.");
                }
                break;
            case { } status when ActiveDraft.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (!leagueYear.PlayStatus.DraftIsPaused)
                {
                    return Result.Failure("That action can only be taken while the draft is paused.");
                }
                break;
            case { } status when YearNotFinishedDraftFinished.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (!leagueYear.PlayStatus.DraftFinished)
                {
                    return Result.Failure("That action can't be taken until the draft is finished.");
                }
                break;
            default:
                throw new NotImplementedException("Unknown year status requirement.");
        }

        return Result.Success();
    }
}
