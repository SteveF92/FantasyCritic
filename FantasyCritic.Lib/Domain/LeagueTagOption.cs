using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueTagOption
    {
        public LeagueTagOption(MasterGameTag tag, TagOption option)
        {
            Tag = tag;
            Option = option;
        }

        public MasterGameTag Tag { get; }
        public TagOption Option { get; }
    }
}
