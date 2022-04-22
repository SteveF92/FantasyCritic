#nullable disable

using System.ComponentModel.DataAnnotations;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using FantasyCritic.Lib.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Web.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly FantasyCriticUserManager _userManager;
    private readonly IEventService _events;
    private readonly SignInManager<FantasyCriticUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IIdentityServerInteractionService _interaction;

    public LoginModel(SignInManager<FantasyCriticUser> signInManager,
        ILogger<LoginModel> logger,
        IIdentityServerInteractionService interaction,
        FantasyCriticUserManager userManager,
        IEventService events)
    {
        _userManager = userManager;
        _events = events;
        _signInManager = signInManager;
        _logger = logger;
        _interaction = interaction;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public AuthenticationScheme GoogleLogin { get; set; }
    public AuthenticationScheme MicrosoftLogin { get; set; }
    public AuthenticationScheme TwitchLogin { get; set; }
    public AuthenticationScheme DiscordLogin { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var externalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        GoogleLogin = externalLogins.SingleOrDefault(x => x.Name == "Google");
        MicrosoftLogin = externalLogins.SingleOrDefault(x => x.Name == "Microsoft");
        TwitchLogin = externalLogins.SingleOrDefault(x => x.Name == "Twitch");
        DiscordLogin = externalLogins.SingleOrDefault(x => x.Name == "Discord");

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/home");

        var externalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        GoogleLogin = externalLogins.SingleOrDefault(x => x.Name == "Google");
        MicrosoftLogin = externalLogins.SingleOrDefault(x => x.Name == "Microsoft");
        TwitchLogin = externalLogins.SingleOrDefault(x => x.Name == "Twitch");
        DiscordLogin = externalLogins.SingleOrDefault(x => x.Name == "Discord");

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                var user = await _userManager.FindByEmailAsync(Input.Email);
                var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client.ClientId));
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
