using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using Newtonsoft.Json;

namespace FantasyCritic.Web.Models.RoundTrip
{
    public class SpecialGameSlotViewModel
    {
        public int SpecialSlotPosition { get; set; }
        public List<string> RequiredTags { get; set; }

        public SpecialGameSlot ToDomain(IReadOnlyDictionary<string, MasterGameTag> tagDictionary)
        {
            List<MasterGameTag> tags = new List<MasterGameTag>();
            foreach (var tag in RequiredTags)
            {
                bool hasTag = tagDictionary.TryGetValue(tag, out var foundTag);
                if (!hasTag)
                {
                    continue;
                }

                tags.Add(foundTag);
            }

            return new SpecialGameSlot(SpecialSlotPosition, tags);
        }
    }
}
