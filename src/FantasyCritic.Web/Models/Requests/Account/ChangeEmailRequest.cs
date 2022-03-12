using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Account;

public class ChangeEmailRequest
{
    [Required]
    [EmailAddress]
    public string NewEmailAddress { get; set; }

    [Required]
    public string Code { get; set; }
}