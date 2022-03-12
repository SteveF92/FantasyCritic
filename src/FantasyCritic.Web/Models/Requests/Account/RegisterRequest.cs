using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Account
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
        [Required, MinLength(1), MaxLength(30)]
        public string DisplayName { get; set; }
        [Required, MinLength(8), MaxLength(80)]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
