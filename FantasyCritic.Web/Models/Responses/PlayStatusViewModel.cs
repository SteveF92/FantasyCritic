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
        public PlayStatusViewModel(PlayStatus domain, bool readyToSetDraftOrder, bool readyToDraft, IEnumerable<string> startDraftErrors, DraftPhase draftPhase)
        {
            PlayStatus = domain.Value;
            ReadyToSetDraftOrder = readyToSetDraftOrder;
            ReadyToDraft = readyToDraft;
            PlayStarted = domain.PlayStarted;
            DraftIsActive = domain.DraftIsActive;
            DraftIsPaused = domain.DraftIsPaused;
            DraftFinished = domain.DraftFinished;
            StartDraftErrors = startDraftErrors.ToList();

            if (draftPhase.Equals(DraftPhase.CounterPicks))
            {
                DraftingCounterPicks = true;
            }
        }

        public string PlayStatus { get; }
        public bool ReadyToSetDraftOrder { get; }
        public bool ReadyToDraft { get; }
        public bool PlayStarted { get; }
        public bool DraftIsActive { get; }
        public bool DraftIsPaused { get; }
        public bool DraftFinished { get; }
        public bool DraftingCounterPicks { get; }
        public IReadOnlyList<string> StartDraftErrors { get; }
    }
}
