using System;
using System.Collections.Generic;
using System.Text;

namespace FantasyCritic.Lib.Enums
{
    public class PlayStatus : TypeSafeEnum<PlayStatus>
    {
        // Define values here.
        public static readonly PlayStatus NotStartedDraft = new PlayStatus("NotStartedDraft");
        public static readonly PlayStatus DraftingStandard = new PlayStatus("DraftingStandard");
        public static readonly PlayStatus DraftingCounterpicks = new PlayStatus("DraftingCounterpicks");
        public static readonly PlayStatus FinalizingDraft = new PlayStatus("FinalizingDraft");
        public static readonly PlayStatus DraftFinal = new PlayStatus("DraftFinal");

        // Constructor is private: values are defined within this class only!
        private PlayStatus(string value)
            : base(value)
        {

        }

        public bool PlayStarted => (Value != NotStartedDraft.Value);
        public bool DraftFinished => (Value == DraftFinal.Value);
        public bool DraftIsActive => (Value == DraftingStandard.Value) || (Value == DraftingCounterpicks.Value);

        public override string ToString() => Value;
    }
}
