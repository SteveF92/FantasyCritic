using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class BudgetExpenditureEntity
    {
        public BudgetExpenditureEntity(BudgetExpenditure domain)
        {
            PublisherID = domain.Publisher.PublisherID;
            BidAmount = domain.Value;
        }

        public Guid PublisherID { get; }
        public uint BidAmount { get; }
    }
}
