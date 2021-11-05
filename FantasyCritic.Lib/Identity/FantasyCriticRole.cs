namespace FantasyCritic.Lib.Identity
{
    public class FantasyCriticRole
    {
        public FantasyCriticRole(int roleID, string name, string normalizedName)
        {
            RoleID = roleID;
            Name = name;
            NormalizedName = normalizedName;
        }

        public int RoleID { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }
    }
}
