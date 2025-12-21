namespace FantasyCritic.Lib.Royale;

public record RoyaleAction(RoyalePublisher Publisher, MasterGameYear MasterGame, string ActionType, string Description, Instant Timestamp)
{
    public bool IsHidden(LocalDate currentDate)
    {
        return RoyaleFunctions.GameOrActionIsHidden(Publisher.YearQuarter, MasterGame, currentDate);
    }
}
