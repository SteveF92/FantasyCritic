using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities.Identity
{
    internal class RecoveryCodeEntity
    {
        public RecoveryCodeEntity()
        {

        }

        public RecoveryCodeEntity(Guid userID, string recoveryCode)
        {
            UserID = userID;
            RecoveryCode = recoveryCode;
        }

        public Guid UserID { get; set; }
        public string RecoveryCode { get; set; }
    }
}
