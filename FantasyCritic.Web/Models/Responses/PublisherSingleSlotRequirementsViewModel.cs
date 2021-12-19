using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherSingleSlotRequirementsViewModel
    {
        public PublisherSingleSlotRequirementsViewModel(IEnumerable<LeagueTagStatus> tags)
        {
            BannedTags = tags.Where(x => x.Status.Equals(TagStatus.Banned)).Select(x => x.Tag.Name).ToList();
            RequiredTags = tags.Where(x => x.Status.Equals(TagStatus.Required)).Select(x => x.Tag.Name).ToList();
        }

        public PublisherSingleSlotRequirementsViewModel(IEnumerable<string> bannedTags)
        {
            BannedTags = bannedTags.ToList();
            RequiredTags = new List<string>();
        }

        public PublisherSingleSlotRequirementsViewModel(bool counterPick)
        {
            CounterPick = counterPick;
        }

        public IReadOnlyList<string> BannedTags { get; }
        public IReadOnlyList<string> RequiredTags { get; }
        public bool CounterPick { get; }
    }
}
