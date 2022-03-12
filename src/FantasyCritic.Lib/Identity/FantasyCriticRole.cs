using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Identity;

public class FantasyCriticRole : IdentityRole<Guid>
{
    public FantasyCriticRole()
    {

    }

    public FantasyCriticRole(Guid roleID, string name, string normalizedName)
    {
        Id = roleID;
        Name = name;
        NormalizedName = normalizedName;
    }
}