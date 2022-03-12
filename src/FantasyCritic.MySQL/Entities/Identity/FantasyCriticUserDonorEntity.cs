using FantasyCritic.Lib.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities.Identity
{
    public class FantasyCriticUserDonorEntity
    {
        public FantasyCriticUserDonorEntity()
        {

        }

        public FantasyCriticUserDonorEntity(FantasyCriticUser user, string donorName)
        {
            UserID = user.Id;
            DonorName = donorName;
        }

        public Guid UserID { get; set; }
        public string DonorName { get; set; }
    }
}
