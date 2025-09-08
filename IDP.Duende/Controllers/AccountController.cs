using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using IDP.Duende.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IDP.Duende.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInMgr;
    private readonly UserManager<AppUser> _userMgr;
    private readonly IIdentityServerInteractionService _interaction;

    public AccountController(SignInManager<AppUser> s, UserManager<AppUser> u, IIdentityServerInteractionService i)
        => (_signInMgr, _userMgr, _interaction) = (s, u, i);

    [HttpGet("/Account/Login")]
    public IActionResult Login(string returnUrl) => View(new LoginVm { ReturnUrl = returnUrl });

    [ValidateAntiForgeryToken]
    [HttpPost("/Account/Login")]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userMgr.FindByNameAsync(vm.UserName) ?? await _userMgr.FindByEmailAsync(vm.UserName);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials");
            return View(vm);
        }

        var result = await _signInMgr.PasswordSignInAsync(user, vm.Password, vm.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials");
            return View(vm);
        }

        // Make sure the returnUrl is valid (authorize endpoints, local urls)
        if (_interaction.IsValidReturnUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // GET /Account/Logout?logoutId=xyz
    [HttpGet("/Account/Logout")]
    public async Task<IActionResult> Logout(string? logoutId)
    {
        // 1) Clear local login cookies (Identity + IdentityServer)
        await _signInMgr.SignOutAsync(); // clears IdentityConstants.ApplicationScheme
        await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme); // if you used external providers

        // 2) Ask IdentityServer what to do next (where to send the user)
        var ctx = await _interaction.GetLogoutContextAsync(logoutId);

        // 3) Redirect back to the client (SPA) or home if none provided
        var redirect = ctx?.PostLogoutRedirectUri ?? Url.Content("~/");
        return Redirect(redirect);
    }

    [HttpPost("/Account/Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost(string? logoutId = null)
    {
        await _signInMgr.SignOutAsync();
        var ctx = await _interaction.GetLogoutContextAsync(logoutId);
        return Redirect(ctx?.PostLogoutRedirectUri ?? Url.Content("~/"));
    }
}

public record LoginVm
{
    public string? ReturnUrl { get; init; }
    public string UserName { get; init; } = "";
    public string Password { get; init; } = "";
    public bool RememberMe { get; init; }
}

