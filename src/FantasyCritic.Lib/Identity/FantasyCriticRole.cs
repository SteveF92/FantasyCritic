using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Identity;

public class FantasyCriticRole : IdentityRole<Guid>
{
    public FantasyCriticRole(Guid roleID, string name, string normalizedName)
    {
        Id = roleID;
        Name = name;
        NormalizedName = normalizedName;
    }

    public new string Name
    {
        get => base.Name!;
        set => base.Name = value;
    }

    public new string NormalizedName
    {
        get => base.NormalizedName!;
        set => base.NormalizedName = value;
    }
}
