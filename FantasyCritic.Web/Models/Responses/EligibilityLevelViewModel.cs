using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class EligibilityLevelViewModel
    {
        public EligibilityLevelViewModel(EligibilityLevel eligibilityLevel, bool includeExamples)
        {
            Level = eligibilityLevel.Level;
            Name = eligibilityLevel.Name;
            Description = eligibilityLevel.Description;

            Examples = includeExamples ? eligibilityLevel.Examples.ToList() : null;
        }

        public int Level { get; }
        public string Name { get; }
        public string Description { get; }
        public IReadOnlyList<string> Examples { get; }
    }
}
