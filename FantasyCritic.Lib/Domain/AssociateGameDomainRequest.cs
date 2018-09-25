using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Domain
{
    public class AssociateGameDomainRequest
    {
        public AssociateGameDomainRequest(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame, bool managerOverride)
        {
            Publisher = publisher;
            PublisherGame = publisherGame;
            MasterGame = masterGame;
            ManagerOverride = managerOverride;
        }

        public Publisher Publisher { get; }
        public PublisherGame PublisherGame { get; }
        public MasterGame MasterGame { get; }
        public bool ManagerOverride { get; }
    }
}
