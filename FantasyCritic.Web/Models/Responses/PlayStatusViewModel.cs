using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Web.Models.Responses
{
    public class PlayStatusViewModel
    {
        public PlayStatusViewModel(PlayStatus domain)
        {
            Ready = domain.Ready;
            Started = domain.Started;
            Errors = domain.Errors.ToList();
        }

        public bool Started { get; set; }
        public bool Ready { get; set; }
        public IReadOnlyList<string> Errors { get; set; }
    }
}
