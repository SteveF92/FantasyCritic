using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Utilities;

namespace FantasyCritic.MySQL.Entities.Identity
{
    internal class FantasyCriticRoleEntity
    {
        public FantasyCriticRoleEntity()
        {

        }

        public FantasyCriticRoleEntity(FantasyCriticRole role)
        {
            RoleID = role.Id.ToInt();
            Name = role.Name;
            NormalizedName = role.NormalizedName;
        }

        public int RoleID { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }


        public FantasyCriticRole ToDomain()
        {
            FantasyCriticRole domain = new FantasyCriticRole(RoleID.ToGuid(), Name, NormalizedName);
            return domain;
        }
    }
}
