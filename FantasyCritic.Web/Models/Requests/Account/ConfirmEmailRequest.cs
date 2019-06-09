using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Account
{
    public class ConfirmEmailRequest
    {
        [Required]
        public string UserID { get; set; }
        [Required]
        public string Code { get; set; }
    }
}
