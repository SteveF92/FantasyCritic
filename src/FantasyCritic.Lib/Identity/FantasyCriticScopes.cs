namespace FantasyCritic.Lib.Identity;
public static class FantasyCriticScopes
{
    public static readonly FantasyCriticScope ReadScope = new FantasyCriticScope("fc_read", "Read Data");
    public static readonly FantasyCriticScope WriteScope = new FantasyCriticScope("fc_write", "Change Data");
}

public record FantasyCriticScope(string Name, string DisplayName);
