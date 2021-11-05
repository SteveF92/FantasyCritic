using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Identity
{
    public class FantasyCriticRole : IdentityRole<int>
    {
        public FantasyCriticRole()
        {

        }

        public FantasyCriticRole(int roleID, string name, string normalizedName)
        {
            Id = roleID;
            Name = name;
            NormalizedName = normalizedName;
        }
    }
}
