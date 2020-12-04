using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGameTag
    {
        public MasterGameTag(string name, string readableName, MasterGameTagType tagType, bool hasCustomCode, string description, IEnumerable<string> examples)
        {
            Name = name;
            ReadableName = readableName;
            TagType = tagType;
            HasCustomCode = hasCustomCode;
            Description = description;
            Examples = examples.ToList();
        }

        public string Name { get; }
        public string ReadableName { get; }
        public MasterGameTagType TagType { get; }
        public bool HasCustomCode { get; }
        public string Description { get; }
        public IReadOnlyList<string> Examples { get; }
    }
}
