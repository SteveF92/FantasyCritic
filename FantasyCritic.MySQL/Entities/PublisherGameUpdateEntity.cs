using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class PublisherGameUpdateEntity
    {
        public PublisherGameUpdateEntity(KeyValuePair<Guid, PublisherGameCalculatedStats> keyValuePair)
        {
            PublisherGameID = keyValuePair.Key;
            FantasyPoints = keyValuePair.Value.FantasyPoints;
        }

        public Guid PublisherGameID { get; }
        public decimal? FantasyPoints { get; }
    }
}
