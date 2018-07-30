using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherViewModel
    {
        public PublisherViewModel(Publisher publisher)
        {
            PublisherName = publisher.PublisherName;
            Year = publisher.Year;
            Games = publisher.PublisherGames.Select(x => new PublisherGameViewModel(x)).ToList();
        }

        public string PublisherName { get; }
        public int Year { get; }
        public IReadOnlyList<PublisherGameViewModel> Games { get; }
    }
}
