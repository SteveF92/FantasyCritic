using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.OpenCritic
{
    public class OpenCriticGame
    {

        public OpenCriticGame(OpenCriticScoreResponse scoreResponse, LocalDate? releaseDate)
        {
            ID = scoreResponse.ID;
            Name = scoreResponse.Name;
            Score = scoreResponse.Score;
            ReviewCount = scoreResponse.ReviewCount;
            ReleaseDate = releaseDate;
        }

        public int ID { get; }
        public string Name { get; }
        public decimal? Score { get; }
        public int? ReviewCount { get; }
        public LocalDate? ReleaseDate { get; }
    }
}
