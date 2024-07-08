namespace FantasyCritic.Lib.Identity;

public record MinimalFantasyCriticUser(Guid UserID, string DisplayName, string EmailAddress) : IMinimalFantasyCriticUser;
public record VeryMinimalFantasyCriticUser(Guid UserID, string DisplayName) : IVeryMinimalFantasyCriticUser;

public interface IMinimalFantasyCriticUser : IVeryMinimalFantasyCriticUser
{
    string EmailAddress { get; }
}

public interface IVeryMinimalFantasyCriticUser
{
    Guid UserID { get; }
    string DisplayName { get; }
    VeryMinimalFantasyCriticUser ToConcrete() => new VeryMinimalFantasyCriticUser(UserID, DisplayName);
}

