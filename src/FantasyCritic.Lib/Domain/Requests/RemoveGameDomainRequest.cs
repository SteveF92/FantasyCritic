using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class RemoveGameDomainRequest
    {
        public RemoveGameDomainRequest(Publisher publisher, PublisherGame publisherGame)
        {
            Publisher = publisher;
            PublisherGame = publisherGame;
        }

        public Publisher Publisher { get; }
        public PublisherGame PublisherGame { get; }
    }
}
