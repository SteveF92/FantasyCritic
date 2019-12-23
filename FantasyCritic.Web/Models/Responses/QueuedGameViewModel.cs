using System;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class QueuedGameViewModel
    {
        public QueuedGameViewModel(QueuedGame queuedGame, IClock clock)
        {
            MasterGame = new MasterGameViewModel(queuedGame.MasterGame, clock);
            Rank = queuedGame.Rank;
        }

        public MasterGameViewModel MasterGame { get; }
        public int Rank { get; }
    }
}
