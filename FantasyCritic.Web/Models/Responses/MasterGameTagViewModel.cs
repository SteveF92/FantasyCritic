using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameTagViewModel
    {
        public MasterGameTagViewModel(MasterGameTag domain)
        {
            Name = domain.Name;
            ReadableName = domain.ReadableName;
            ShortName = domain.ShortName;
            TagType = domain.TagType.Name;
            Description = domain.Description;
            Examples = domain.Examples;
            BadgeColor = domain.BadgeColor;
        }

        public string Name { get; }
        public string ReadableName { get; }
        public string ShortName { get; }
        public string TagType { get; }
        public string Description { get; }
        public IReadOnlyList<string> Examples { get; }
        public string BadgeColor { get; }
    }
}
