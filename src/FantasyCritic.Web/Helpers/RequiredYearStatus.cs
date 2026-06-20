namespace FantasyCritic.Web.Helpers;

public class RequiredYearStatus : TypeSafeEnum<RequiredYearStatus>
{
    // Define values here.
    public static readonly RequiredYearStatus Any = new RequiredYearStatus("Any");
    public static readonly RequiredYearStatus AnyYearNotFinished = new RequiredYearStatus("AnyYearNotFinished");
    public static readonly RequiredYearStatus YearNotFinishedNoDraftsStarted = new RequiredYearStatus("YearNotFinishedNoDraftsStarted");
    public static readonly RequiredYearStatus PlayOpenNoDraftsStarted = new RequiredYearStatus("PlayOpenNoDraftsStarted");
    public static readonly RequiredYearStatus DuringDraft = new RequiredYearStatus("DuringDraft");
    public static readonly RequiredYearStatus DraftPaused = new RequiredYearStatus("DraftPaused");
    public static readonly RequiredYearStatus ActiveDraft = new RequiredYearStatus("ActiveDraft");
    public static readonly RequiredYearStatus YearNotFinishedNoDraftsActive = new RequiredYearStatus("YearNotFinishedNoDraftsActive");
    public static readonly RequiredYearStatus YearNotFinishedOrUnderReviewNoDraftsActive = new RequiredYearStatus("YearNotFinishedOrUnderReviewNoDraftsActive");
    public static readonly RequiredYearStatus YearFinishedNoDraftsActive = new RequiredYearStatus("YearFinishedNoDraftsActive");

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
            case { } status when YearNotFinishedNoDraftsStarted.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (leagueYear.IsAnyDraftStarted)
                {
                    return Result.Failure("That action can only be taken before the draft starts.");
                }
                break;
            case { } status when PlayOpenNoDraftsStarted.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (!leagueYear.SupportedYear.OpenForPlay)
                {
                    return Result.Failure("That year is not open for drafting yet.");
                }
                if (leagueYear.IsAnyDraftStarted)
                {
                    return Result.Failure("That action can only be taken before the draft starts.");
                }
                break;
            case { } status when DuringDraft.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (!(leagueYear.ActiveDraft?.PlayStatus.DraftIsActiveOrPaused ?? false))
                {
                    return Result.Failure("That action can only be taken during the draft is active.");
                }
                break;
            case { } status when DraftPaused.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (!(leagueYear.ActiveDraft?.PlayStatus.DraftIsPaused ?? false))
                {
                    return Result.Failure("That action can only be taken while the draft is paused.");
                }
                break;
            case { } status when ActiveDraft.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (!(leagueYear.ActiveDraft?.PlayStatus.DraftIsActive ?? false))
                {
                    return Result.Failure("That action can only be taken while the draft is active.");
                }
                break;
            case { } status when YearNotFinishedNoDraftsActive.Equals(status):
                if (leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is finished.");
                }
                if (leagueYear.IsAnyDraftInProgress)
                {
                    return Result.Failure("That action can't be taken while there is a draft active.");
                }
                break;
            case { } status when YearNotFinishedOrUnderReviewNoDraftsActive.Equals(status):
                if (leagueYear.SupportedYear.Finished && !leagueYear.UnderReview)
                {
                    return Result.Failure("That year is finished.");
                }
                if (leagueYear.IsAnyDraftInProgress)
                {
                    return Result.Failure("That action can't be taken while there is a draft active.");
                }
                break;
            case { } status when YearFinishedNoDraftsActive.Equals(status):
                if (!leagueYear.SupportedYear.Finished)
                {
                    return Result.Failure("That year is not finished.");
                }
                if (leagueYear.IsAnyDraftInProgress)
                {
                    return Result.Failure("That action can't be taken while there is a draft active.");
                }
                break;
            default:
                throw new NotImplementedException("Unknown year status requirement.");
        }

        return Result.Success();
    }
}
