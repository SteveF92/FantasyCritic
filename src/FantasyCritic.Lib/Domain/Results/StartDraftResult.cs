using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain.Results
{
    public class StartDraftResult
    {
        public StartDraftResult(bool ready, IEnumerable<string> errors)
        {
            Ready = ready;
            Errors = errors;
        }

        public bool Ready { get; }
        public IEnumerable<string> Errors { get; }

    }
}
