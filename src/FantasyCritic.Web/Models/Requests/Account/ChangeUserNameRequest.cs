using System.ComponentModel.DataAnnotations;

namespace FantasyCritic.Web.Models.Requests.Account;

public class ChangeDisplayNameRequest
{
    [Required]
    public string NewDisplayName { get; set; }
}