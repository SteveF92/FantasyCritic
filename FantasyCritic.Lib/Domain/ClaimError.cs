using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class ClaimError
    {
        public ClaimError(string error, bool overridable)
        {
            Error = error;
            Overridable = overridable;
        }

        public string Error { get; }
        public bool Overridable { get; }
    }
}
