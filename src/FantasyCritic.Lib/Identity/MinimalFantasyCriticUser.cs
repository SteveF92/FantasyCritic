namespace FantasyCritic.Lib.Identity;

public record MinimalFantasyCriticUser(Guid UserID, string DisplayName, string EmailAddress) : IMinimalFantasyCriticUser
{
    public virtual bool Equals(IVeryMinimalFantasyCriticUser? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return UserID.Equals(other.UserID);
    }

    public override int GetHashCode()
    {
        return UserID.GetHashCode();
    }
}

public record VeryMinimalFantasyCriticUser(Guid UserID, string DisplayName) : IVeryMinimalFantasyCriticUser
{
    public virtual bool Equals(IVeryMinimalFantasyCriticUser? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return UserID.Equals(other.UserID);
    }

    public override int GetHashCode()
    {
        return UserID.GetHashCode();
    }
}

public interface IMinimalFantasyCriticUser : IVeryMinimalFantasyCriticUser
{
    string EmailAddress { get; }
}

public interface IVeryMinimalFantasyCriticUser : IEquatable<IVeryMinimalFantasyCriticUser>
{
    Guid UserID { get; }
    string DisplayName { get; }
    VeryMinimalFantasyCriticUser ToConcrete() => new VeryMinimalFantasyCriticUser(UserID, DisplayName);
}

