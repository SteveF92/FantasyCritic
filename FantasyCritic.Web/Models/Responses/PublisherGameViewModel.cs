using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherGameViewModel
    {
        public PublisherGameViewModel(PublisherGame publisherGame)
        {
            GameName = publisherGame.GameName;
            Timestamp = publisherGame.Timestamp.ToDateTimeUtc();
            Waiver = publisherGame.Waiver;
            AntiPick = publisherGame.AntiPick;
            FantasyScore = publisherGame.FantasyScore;

            if (publisherGame.MasterGame.HasValue)
            {
                GameName = publisherGame.MasterGame.Value.GameName;
                EstimatedReleaseDate = publisherGame.MasterGame.Value.EstimatedReleaseDate;
                if (publisherGame.MasterGame.Value.ReleaseDate.HasValue)
                {
                    ReleaseDate = publisherGame.MasterGame.Value.ReleaseDate.Value.ToDateTimeUnspecified();
                }
                CriticScore = publisherGame.MasterGame.Value.CriticScore;
            }
        }

        public string GameName { get; }
        public DateTime Timestamp { get; }
        public bool Waiver { get; }
        public bool AntiPick { get; }
        public string EstimatedReleaseDate { get; }
        public DateTime? ReleaseDate { get; }
        public decimal? FantasyScore { get; }
        public decimal? CriticScore { get; }
    }
}
