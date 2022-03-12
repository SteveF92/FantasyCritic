namespace FantasyCritic.MySQL.Entities.Identity
{
    public class ExternalLoginEntity
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public Guid UserID { get; set; }
        public string ProviderDisplayName { get; set; }
    }
}
