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
        public OpenCriticGame(int id, string name, decimal? score, LocalDate? releaseDate)
        {
            ID = id;
            Name = name;
            Score = score;
            ReleaseDate = releaseDate;
        }

        public int ID { get; }
        public string Name { get; }
        public decimal? Score { get; }
        public LocalDate? ReleaseDate { get; }
    }
}
