namespace FantasyCritic.Lib.Identity;

public record MinimalFantasyCriticUser(Guid UserID, string DisplayName, string EmailAddress) : IMinimalFantasyCriticUser;

public interface IMinimalFantasyCriticUser
{
    Guid UserID { get; }
    string DisplayName { get; }
    string EmailAddress { get; }
}
