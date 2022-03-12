using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Admin
{
    public class AdminResendConfirmationEmail
    {
        [Required]
        public Guid UserID { get; set; }
    }
}
