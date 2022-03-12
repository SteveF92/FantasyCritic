using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGameTagType
    {
        public MasterGameTagType(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
