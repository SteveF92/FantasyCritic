using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests.League.Trades
{
    public class TradeVoteRequest
    {
        [Required]
        public Guid TradeID { get; set; }
        [Required]
        public bool Approve { get; set; }
        public string Comment { get; set; }
    }
}
