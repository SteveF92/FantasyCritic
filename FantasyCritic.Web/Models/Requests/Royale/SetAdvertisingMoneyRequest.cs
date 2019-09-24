using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Web.Models.Requests.Royale
{
    public class SetAdvertisingMoneyRequest
    {
        [Required]
        public Guid PublisherID { get; set; }
        [Required]
        public Guid MasterGameID { get; set; }
        [Required]
        public decimal AdvertisingMoney { get; set; }
    }
}
