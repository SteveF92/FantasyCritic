using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.OpenCritic
{
    public class OpenCriticScoreResponse
    {
        public int ID { get; set;  }
        public string Name { get; set; }
        public decimal? Score { get; set; }
        public int? ReviewCount { get; set; }
    }
}
