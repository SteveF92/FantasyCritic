using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Account
{
    public class SendChangeEmailRequest
    {
        [Required]
        [EmailAddress]
        public string NewEmailAddress { get; set; }
    }
}
