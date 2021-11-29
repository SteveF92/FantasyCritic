using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class SpecialGameSlot
    {
        public SpecialGameSlot(int specialSlotPosition, IEnumerable<MasterGameTag> tags)
        {
            SpecialSlotPosition = specialSlotPosition;
            Tags = tags.ToList();
        }

        public int SpecialSlotPosition { get; init; }
        public IReadOnlyList<MasterGameTag> Tags { get; }

        //TODO Eliminate
        public static IReadOnlyList<SpecialGameSlot> DefaultSpecialGameSlots = new List<SpecialGameSlot>();
    }
}
