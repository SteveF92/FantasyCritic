using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    public class AcquisitionBidEntity
    {
        public Guid BidID { get; set; }
        public Guid PublisherID { get; set; }
        public Guid MasterGameID { get; set; }
        public DateTime Timestamp { get; set; }
        public int Priority { get; set; }
        public int BidAmount { get; set; }
        public bool? Sucessful { get; set; }
    }
}
