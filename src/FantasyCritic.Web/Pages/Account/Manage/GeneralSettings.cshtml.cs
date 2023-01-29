#nullable disable

using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FantasyCritic.Web.Pages.Account.Manage;

public class GeneralSettingsModel : PageModel
{
    private readonly FantasyCriticUserManager _userManager;

    public GeneralSettingsModel(FantasyCriticUserManager userManager)
    {
        _userManager = userManager;
    }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Display(Name = "Show Decimal Points on League Page")]
        public bool ShowDecimalPoints { get; set; }
    }
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Input = new InputModel
        {
            ShowDecimalPoints = user.GeneralUserSettings.ShowDecimalPoints,
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var generalSettings = new GeneralUserSettings(Input.ShowDecimalPoints);
        await _userManager.SetGeneralSettings(user, generalSettings);

        StatusMessage = "Email Settings Updated.";
        return RedirectToPage();
    }
}
