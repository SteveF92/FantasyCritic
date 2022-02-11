using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Extensions;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class PublisherGame
    {
        public PublisherGame(Guid publisherID, Guid publisherGameID, string gameName, Instant timestamp, bool counterPick, decimal? manualCriticScore, bool manualWillNotRelease,
            decimal? fantasyPoints, Maybe<MasterGameYear> masterGame, int slotNumber, int? draftPosition, int? overallDraftPosition, uint? bidAmount)
        {
            PublisherID = publisherID;
            PublisherGameID = publisherGameID;
            GameName = gameName;
            Timestamp = timestamp;
            CounterPick = counterPick;
            ManualCriticScore = manualCriticScore;
            ManualWillNotRelease = manualWillNotRelease;
            FantasyPoints = fantasyPoints;
            MasterGame = masterGame;
            SlotNumber = slotNumber;
            DraftPosition = draftPosition;
            OverallDraftPosition = overallDraftPosition;
            BidAmount = bidAmount;
        }

        public Guid PublisherID { get; }
        public Guid PublisherGameID { get; }
        public string GameName { get; }
        public Instant Timestamp { get; }
        public bool CounterPick { get; }
        public decimal? ManualCriticScore { get; }
        public bool ManualWillNotRelease { get; }
        public decimal? FantasyPoints { get; }
        public Maybe<MasterGameYear> MasterGame { get; }
        public int SlotNumber { get; }
        public int? DraftPosition { get; }
        public int? OverallDraftPosition { get; }
        public uint? BidAmount { get; }

        public bool WillRelease()
        {
            if (ManualWillNotRelease)
            {
                return false;
            }

            if (MasterGame.HasNoValue)
            {
                return false;
            }

            return MasterGame.Value.WillRelease();
        }

        public override string ToString() => GameName;
    }
}
