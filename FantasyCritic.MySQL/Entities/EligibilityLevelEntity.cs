using System.Collections.Generic;
using FantasyCritic.Lib.Domain;
using Newtonsoft.Json;

namespace FantasyCritic.MySQL.Entities
{
    internal class EligibilityLevelEntity
    {
        public int EligibilityLevel { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Examples { get; set; }

        public EligibilityLevel ToDomain()
        {
            var examples = JsonConvert.DeserializeObject<List<string>>(Examples);
            EligibilityLevel domain = new EligibilityLevel(EligibilityLevel, Name, Description, examples);
            return domain;
        }
    }
}