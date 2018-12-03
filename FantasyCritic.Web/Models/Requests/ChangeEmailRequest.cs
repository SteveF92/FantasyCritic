using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Requests
{
    public class ChangeEmailRequest
    {
        [Required]
        [EmailAddress]
        public string OldEmailAddress { get; set; }

        [Required]
        [EmailAddress]
        public string NewEmailAddress { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
