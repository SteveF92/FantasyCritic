using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Account
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
