using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class CompleteMasterGameChangeRequestRequest
    {
        [Required]
        public Guid RequestID { get; set; }
        [Required]
        public string ResponseNote { get; set; }
    }
}
