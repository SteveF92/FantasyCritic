using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.OpenCritic
{
    public class OpenCriticGame
    {
        public OpenCriticGame(OpenCriticScoreResponse scoreResponse)
        {
            ID = scoreResponse.ID;
            Name = scoreResponse.Name;
            Score = scoreResponse.Score;
            ReviewCount = scoreResponse.ReviewCount;
        }

        public int ID { get; }
        public string Name { get; }
        public decimal? Score { get; }
        public int? ReviewCount { get; }
    }
}
