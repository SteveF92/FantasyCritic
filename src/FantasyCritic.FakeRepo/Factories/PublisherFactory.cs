using System.Collections.Generic;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.FakeRepo.Factories
{
    internal static class PublisherFactory
    {
        public static List<Publisher> GetPublishers()
        {
            List<Publisher> publishers = new List<Publisher>();
            return publishers;
        }

        public static List<PublisherGame> GetPublisherGames()
        {
            List<PublisherGame> publisherGames = new List<PublisherGame>();
            return publisherGames;
        }
    }
}
