using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    public class PublisherActionStatusEntity
    {
        public PublisherActionStatusEntity()
        {

        }

        public PublisherActionStatusEntity(PublisherActionStatus domain)
        {
            PublisherID = domain.PublisherID;
            Budget = domain.Budget;
            FreeGamesDropped = domain.FreeGamesDropped;
            WillNotReleaseGamesDropped = domain.WillNotReleaseGamesDropped;
            WillReleaseGamesDropped = domain.WillReleaseGamesDropped;
        }

        public Guid PublisherID { get; set; }
        public uint Budget { get; set; }
        public int FreeGamesDropped { get ; set; }
        public int WillNotReleaseGamesDropped { get; set; }
        public int WillReleaseGamesDropped { get; set; }
    }
}
