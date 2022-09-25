#nullable disable

using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Pages.Account;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly FantasyCriticSignInManager _signInManager;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(FantasyCriticSignInManager signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage();
    }
}
