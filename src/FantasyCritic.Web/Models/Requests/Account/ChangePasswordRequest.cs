using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Account
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required, MinLength(8), MaxLength(80)]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
