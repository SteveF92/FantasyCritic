using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    public class MasterGameRequestEntity
    {
        public Guid RequestID { get; set; }
        public Guid UserID { get; set; }
        public string GameName { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public string RequestNote { get; set; }
        public bool Answered { get; set; }
        public DateTime? ResponseTimestamp { get; set; }
        public string ResponseNote { get; set; }
        public Guid? MasterGameID { get; set; }
    }
}
