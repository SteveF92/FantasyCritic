using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities.Identity
{
    public class FantasyCriticUserEmailSettingEntity
    {
        public Guid UserID { get; set; }
        public string EmailType { get; set; }
    }
}
