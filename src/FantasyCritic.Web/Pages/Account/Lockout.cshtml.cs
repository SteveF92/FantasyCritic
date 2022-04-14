using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FantasyCritic.Web.Pages.Account;

[AllowAnonymous]
public class LockoutModel : PageModel
{
    public void OnGet()
    {

    }
}
