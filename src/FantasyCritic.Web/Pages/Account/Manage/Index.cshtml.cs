#nullable disable

using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FantasyCritic.Web.Areas.Identity.Pages.Account.Manage;

public partial class IndexModel : PageModel
{
    private readonly UserManager<FantasyCriticUser> _userManager;
    private readonly SignInManager<FantasyCriticUser> _signInManager;

    public IndexModel(
        UserManager<FantasyCriticUser> userManager,
        SignInManager<FantasyCriticUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [Display(Name = "Display Name")]
    public string DisplayName { get; set; }
    [Display(Name = "Display Number")]
    public int DisplayNumber { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }
    }


    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        DisplayName = user.UserName;
        DisplayNumber = user.DisplayNumber;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            DisplayName = user.UserName;
            DisplayNumber = user.DisplayNumber;
            return Page();
        }

        if (Input.DisplayName != user.UserName)
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.DisplayName);
            if (!setUserNameResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to change user name.";
                return RedirectToPage();
            }
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }
}
