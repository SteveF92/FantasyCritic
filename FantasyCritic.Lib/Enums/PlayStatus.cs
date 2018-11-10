using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class PlayStatus : TypeSafeEnum<PlayStatus>
    {
        // Define values here.
        public static readonly PlayStatus NotStartedDraft = new PlayStatus("NotStartedDraft", false, false);
        public static readonly PlayStatus DraftingStandard = new PlayStatus("DraftingStandard", true, false);
        public static readonly PlayStatus DraftingCounterpicks = new PlayStatus("DraftingCounterpicks", true, false);
        public static readonly PlayStatus FinalizingDraft = new PlayStatus("FinalizingDraft", true, false);
        public static readonly PlayStatus DraftFinal = new PlayStatus("DraftFinal", true, true);

        // Constructor is private: values are defined within this class only!
        private PlayStatus(string value, bool playStarted, bool draftFinished)
            : base(value)
        {
            PlayStarted = playStarted;
            DraftFinished = draftFinished;
        }

        public bool PlayStarted { get; }
        public bool DraftFinished { get; }

        public override string ToString() => Value;
    }
}
