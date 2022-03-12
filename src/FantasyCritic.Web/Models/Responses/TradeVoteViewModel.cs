using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.Trades;

namespace FantasyCritic.Web.Models.Responses
{
    public class TradeVoteViewModel
    {
        public TradeVoteViewModel(TradeVote domain)
        {
            UserID = domain.User.Id;
            DisplayName = domain.User.UserName;
            Approved = domain.Approved;
            Comment = domain.Comment.GetValueOrDefault();
        }

        public Guid UserID { get; }
        public string DisplayName { get; }
        public bool Approved { get; }
        public string Comment { get; }
    }
}
