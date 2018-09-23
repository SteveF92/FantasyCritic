using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class EligibilityLevel
    {
        public EligibilityLevel(int level, string name, string description, IEnumerable<string> examples)
        {
            Level = level;
            Name = name;
            Description = description;
            Examples = examples.ToList();
        }

        public int Level { get; }
        public string Name { get; }
        public string Description { get; }
        public IReadOnlyList<string> Examples { get; }
    }
}
