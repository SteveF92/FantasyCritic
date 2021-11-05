using Microsoft.AspNetCore.Identity;

namespace FantasyCritic.Lib.Identity
{
    public class FantasyCriticRole : IdentityRole
    {
        public FantasyCriticRole()
        {

        }

        public FantasyCriticRole(int roleID, string name, string normalizedName)
        {
            base.Id = roleID.ToString();
            Name = name;
            NormalizedName = normalizedName;
        }

        public new int Id => int.Parse(base.Id);
    }
}
