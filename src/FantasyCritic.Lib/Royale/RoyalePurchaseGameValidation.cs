namespace FantasyCritic.Lib.Royale;

public sealed class RoyalePurchaseGameValidation
{
    private RoyalePurchaseGameValidation(string? blockingReason)
    {
        BlockingReason = blockingReason;
    }

    public static RoyalePurchaseGameValidation Valid() => new(null);

    public static RoyalePurchaseGameValidation Invalid(string blockingReason) => new(blockingReason);

    public string? BlockingReason { get; }

    public bool CanPurchase => BlockingReason is null;
}
