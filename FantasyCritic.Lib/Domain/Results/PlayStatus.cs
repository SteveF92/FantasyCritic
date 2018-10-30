using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain.Results
{
    public class PlayStatus
    {
        public PlayStatus(bool ready, bool started, IEnumerable<string> errors)
        {
            Ready = ready;
            Started = started;
            Errors = errors;
        }

        public bool Ready { get; }
        public bool Started { get; }
        public IEnumerable<string> Errors { get; }
    }
}
