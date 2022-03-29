using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Patreon;

public class PatronInfo
{
    public PatronInfo(FantasyCriticUser user, bool isPlusUser, string? donorName)
    {
        User = user;
        IsPlusUser = isPlusUser;
        DonorName = donorName;
    }

    public FantasyCriticUser User { get; }
    public bool IsPlusUser { get; }
    public string? DonorName { get; }
}
