using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    internal class ManagerMessageDismissalEntity
    {
        public Guid MessageID { get; set; }
        public Guid UserID { get; set; }
    }
}
