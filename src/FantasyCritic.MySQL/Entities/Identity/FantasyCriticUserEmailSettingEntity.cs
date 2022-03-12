using FantasyCritic.Lib.Identity;

namespace FantasyCritic.MySQL.Entities.Identity
{
    public class FantasyCriticUserEmailSettingEntity
    {
        public FantasyCriticUserEmailSettingEntity()
        {

        }

        public FantasyCriticUserEmailSettingEntity(FantasyCriticUser user, EmailType emailType)
        {
            UserID = user.Id;
            EmailType = emailType.Value;
        }

        public Guid UserID { get; set; }
        public string EmailType { get; set; }
    }
}
