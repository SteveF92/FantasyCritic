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
    public class FormerPublisherGame
    {
        public FormerPublisherGame(PublisherGame publisherGame, Instant removedTimestamp, string removedNote)
        {
            PublisherGame = publisherGame;
            RemovedTimestamp = removedTimestamp;
            RemovedNote = removedNote;
        }

        public PublisherGame PublisherGame { get; }
        public Instant RemovedTimestamp { get; }
        public string RemovedNote { get; }

        public override string ToString() => PublisherGame.ToString();
    }
}
