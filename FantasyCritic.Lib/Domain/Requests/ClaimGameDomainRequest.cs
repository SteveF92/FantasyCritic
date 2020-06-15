using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Domain.Requests
{
    public class ClaimGameDomainRequest
    {
        public ClaimGameDomainRequest(Publisher publisher, string gameName, bool counterPick, bool managerOverride, bool autoDraft, Maybe<MasterGame> masterGame, 
            int? draftPosition, int? overallDraftPosition)
        {
            Publisher = publisher;
            GameName = gameName;
            CounterPick = counterPick;
            ManagerOverride = managerOverride;
            AutoDraft = autoDraft;
            MasterGame = masterGame;
            DraftPosition = draftPosition;
            OverallDraftPosition = overallDraftPosition;
        }

        public Publisher Publisher { get; }
        public string GameName { get; }
        public bool CounterPick { get; }
        public bool ManagerOverride { get; }
        public bool AutoDraft { get; }
        public Maybe<MasterGame> MasterGame { get; }
        public int? DraftPosition { get; }
        public int? OverallDraftPosition { get; }
    }
}
