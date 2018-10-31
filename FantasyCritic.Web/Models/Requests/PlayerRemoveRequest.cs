using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class PlayerRemoveRequest
    {
        [Required]
        public Guid UserID { get; set; }
        [Required]
        public Guid LeagueID { get; set; }
    }
}
