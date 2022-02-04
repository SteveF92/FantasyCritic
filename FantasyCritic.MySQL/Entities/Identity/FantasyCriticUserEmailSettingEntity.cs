using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
