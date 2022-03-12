using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<FantasyCriticUser> _userManager;
        private readonly EmailSendingService _emailSendingService;

        public ResendEmailConfirmationModel(UserManager<FantasyCriticUser> userManager, EmailSendingService emailSendingService)
        {
            _userManager = userManager;
            _emailSendingService = emailSendingService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string message = "If you have an account, a verification email has been sent. Please check your email.";
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return Page();
            }

            var confirmLink = await LinkBuilder.GetConfirmEmailLink(_userManager, user, Request);
            await _emailSendingService.SendConfirmationEmail(user, confirmLink);

            ModelState.AddModelError(string.Empty, message);
            return Page();
        }
    }
}
