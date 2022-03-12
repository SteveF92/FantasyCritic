using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Account;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
}