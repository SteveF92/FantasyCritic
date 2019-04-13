using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class EligibilityLevel : IEquatable<EligibilityLevel>
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

        public bool Equals(EligibilityLevel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Level == other.Level;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EligibilityLevel) obj);
        }

        public override int GetHashCode()
        {
            return Level;
        }
    }
}
