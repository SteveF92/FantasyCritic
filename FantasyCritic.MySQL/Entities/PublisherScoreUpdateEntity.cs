using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities
{
    internal class PublisherScoreUpdateEntity
    {
        public PublisherScoreUpdateEntity(KeyValuePair<Guid, decimal?> keyValuePair)
        {
            PublisherGameID = keyValuePair.Key;
            FantasyPoints = keyValuePair.Value;
        }

        public Guid PublisherGameID { get; }
        public decimal? FantasyPoints { get; }
    }
}
