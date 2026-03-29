#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FantasyCritic.Web.Pages.Account.Manage;

public class SupportTicketModel : PageModel
{
    private readonly FantasyCriticUserManager _userManager;

    public SupportTicketModel(FantasyCriticUserManager userManager)
    {
        _userManager = userManager;
    }

    [TempData]
    public string StatusMessage { get; set; }

    public string SupportVerificationCode { get; set; }
    public bool CanUserEditOrCloseTicket { get; set; }
    public bool HasActiveSupportTicket { get; set; }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Display(Name = "What do you need help with?")]
        public string IssueDescription { get; set; }
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

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        string description = Input?.IssueDescription?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(description))
        {
            ModelState.AddModelError("Input.IssueDescription", "Please describe your issue.");
        }
        else if (description.Length < 10)
        {
            ModelState.AddModelError("Input.IssueDescription", "Please enter at least 10 characters describing your issue.");
        }
        else if (description.Length > 4000)
        {
            ModelState.AddModelError("Input.IssueDescription", "Description is too long (max 4000 characters).");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        try
        {
            await _userManager.OpenSupportTicket(user, description, openedByUser: true);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        StatusMessage = "Your support ticket was created. Use the verification code below when you contact support.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var supportTicket = await _userManager.GetActiveSupportTicket(user.Id);
        if (supportTicket is null)
        {
            StatusMessage = "You do not have an active support ticket.";
            return RedirectToPage();
        }

        if (!supportTicket.OpenedByUser)
        {
            ModelState.AddModelError(string.Empty, "Only tickets that you opened yourself can be edited.");
            await LoadAsync(user);
            return Page();
        }

        string description = Input?.IssueDescription?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(description))
        {
            ModelState.AddModelError("Input.IssueDescription", "Please describe your issue.");
        }
        else if (description.Length < 10)
        {
            ModelState.AddModelError("Input.IssueDescription", "Please enter at least 10 characters describing your issue.");
        }
        else if (description.Length > 4000)
        {
            ModelState.AddModelError("Input.IssueDescription", "Description is too long (max 4000 characters).");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        await _userManager.UpdateSupportTicketIssue(supportTicket, description);
        StatusMessage = "Your support ticket issue was updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCloseAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var supportTicket = await _userManager.GetActiveSupportTicket(user.Id);
        if (supportTicket is null)
        {
            StatusMessage = "You do not have an active support ticket.";
            return RedirectToPage();
        }

        if (!supportTicket.OpenedByUser)
        {
            ModelState.AddModelError(string.Empty, "Only tickets that you opened yourself can be closed from this page.");
            await LoadAsync(user);
            return Page();
        }

        await _userManager.CloseSupportTicket(supportTicket, "Closed by user.");
        StatusMessage = "Your support ticket has been closed.";
        return RedirectToPage();
    }

    private async Task LoadAsync(FantasyCriticUser user)
    {
        var supportTicket = await _userManager.GetActiveSupportTicket(user.Id);
        HasActiveSupportTicket = supportTicket is not null;
        CanUserEditOrCloseTicket = supportTicket?.OpenedByUser ?? false;
        SupportVerificationCode = supportTicket is null
            ? null
            : SupportTicket.FormatVerificationCodeForDisplay(supportTicket.VerificationCode);

        Input ??= new InputModel();
        Input.IssueDescription = supportTicket?.IssueDescription ?? string.Empty;
    }
}
