using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class ClaimResult
    {
        public ClaimResult(bool success, string error, bool overridable)
        {
            Success = success;
            Error = error;
            Overridable = overridable;
        }

        public bool Success { get; }
        public string Error { get; }
        public bool Overridable { get; }
    }
}
