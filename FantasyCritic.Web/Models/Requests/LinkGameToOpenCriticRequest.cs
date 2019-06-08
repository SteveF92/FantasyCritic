using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class LinkGameToOpenCriticRequest
    {
        [Required]
        public Guid MasterGameID { get; set; }
        [Required]
        public int OpenCriticID { get; set; }
    }
}
