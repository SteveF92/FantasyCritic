using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class MasterGameChangeRequestDismissRequest
    {
        [Required]
        public Guid RequestID { get; set; }
    }
}
