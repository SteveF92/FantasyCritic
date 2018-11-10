using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class InProgessDraft
    {
        public InProgessDraft(IReadOnlyDictionary<Publisher, IReadOnlyList<DraftGame>> standardGames, IReadOnlyDictionary<Publisher, IReadOnlyList<DraftGame>> counterPicks)
        {
            StandardGames = standardGames;
            CounterPicks = counterPicks;
        }

        public IReadOnlyDictionary<Publisher, IReadOnlyList<DraftGame>> StandardGames { get; }
        public IReadOnlyDictionary<Publisher, IReadOnlyList<DraftGame>> CounterPicks { get; }
    }
}
