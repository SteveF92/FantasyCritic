using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities.Identity
{
    internal class FantasyCriticRoleEntity
    {
        public FantasyCriticRoleEntity()
        {

        }

        public FantasyCriticRoleEntity(FantasyCriticRole role)
        {
            RoleID = role.Id;
            Name = role.Name;
            NormalizedName = role.NormalizedName;
        }

        public int RoleID { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }


        public FantasyCriticRole ToDomain()
        {
            FantasyCriticRole domain = new FantasyCriticRole(RoleID, Name, NormalizedName);
            return domain;
        }
    }
}
