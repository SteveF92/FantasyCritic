#nullable disable

using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FantasyCritic.Web.Pages.Account.Manage;

public class EmailModel : PageModel
{
    private readonly FantasyCriticUserManager _userManager;
    private readonly EmailSendingService _emailSendingService;

    public EmailModel(FantasyCriticUserManager userManager, EmailSendingService emailSendingService)
    {
        _userManager = userManager;
        _emailSendingService = emailSendingService;
    }

    public string Username { get; set; }

    public string Email { get; set; }

    public bool IsEmailConfirmed { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string NewEmail { get; set; }

        [Display(Name = "Public Bids")]
        public bool SendPublicBidEmails { get; set; }
    }

    private async Task LoadAsync(FantasyCriticUser user)
    {
        var email = await _userManager.GetEmailAsync(user);
        Email = email;

        Input = new InputModel
        {
            NewEmail = email,
        };

        var emailSettings = await _userManager.GetEmailSettings(user);
        Input.SendPublicBidEmails = emailSettings.Any(x => x.Equals(EmailType.PublicBids));

        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostChangeEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var email = await _userManager.GetEmailAsync(user);
        if (Input.NewEmail != email)
        {
            var changeEmailLink = await LinkBuilder.GetChangeEmailLink(_userManager, user, Input.NewEmail, Request);
            await _emailSendingService.SendChangeEmail(user, changeEmailLink);

            StatusMessage = "Confirmation link to change email sent. Please check your email.";
            return RedirectToPage();
        }

        StatusMessage = "Your email is unchanged.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var confirmLink = await LinkBuilder.GetConfirmEmailLink(_userManager, user, Request);
        await _emailSendingService.SendConfirmationEmail(user, confirmLink);

        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangeEmailSettingsAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await _userManager.SetEmailSettings(user, Input.SendPublicBidEmails);

        StatusMessage = "Email Settings Updated.";
        return RedirectToPage();
    }
}
