using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class SendChangeEmailRequest
    {
        [Required]
        [EmailAddress]
        public string NewEmailAddress { get; set; }
    }
}
