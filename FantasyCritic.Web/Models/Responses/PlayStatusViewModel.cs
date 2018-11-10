using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Responses
{
    public class PlayStatusViewModel
    {
        public PlayStatusViewModel(PlayStatus domain, bool readyToSetDraftOrder, bool readyToDraft, IEnumerable<string> startDraftErrors)
        {
            PlayStatus = domain.Value;
            ReadyToSetDraftOrder = readyToSetDraftOrder;
            ReadyToDraft = readyToDraft;
            PlayStarted = domain.PlayStarted;
            DraftFinished = domain.DraftFinished;
            StartDraftErrors = startDraftErrors.ToList();
        }

        public string PlayStatus { get; }
        public bool ReadyToSetDraftOrder { get; }
        public bool ReadyToDraft { get; }
        public bool PlayStarted { get; }
        public bool DraftFinished { get; }
        public IReadOnlyList<string> StartDraftErrors { get; }
    }
}
