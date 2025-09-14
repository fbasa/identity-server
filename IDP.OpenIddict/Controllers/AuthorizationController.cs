using IDP.OpenIddict.Identity;
using IDP.OpenIddict.ServerHosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace IDP.OpenIddict.Controllers;

[ApiController]
public class AuthorizationController(
    SignInManager<AppUser> signInManager, 
    UserManager<AppUser> userManager
    ) : Controller
{
    // === Authorization Code (+PKCE) ===
    [Authorize] // ensure the user is logged in via cookies/Identity UI
    [HttpGet("~/connect/authorize")]
    public async Task<IActionResult> AuthorizeAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OIDC request is missing.");

        var user = await userManager.GetUserAsync(User)
            ?? throw new InvalidOperationException("User not found.");

        if (user == null) return Challenge();

        var principal = await signInManager.CreateUserPrincipalAsync(user);

        // --- Ensure required 'sub' claim is present ---
        if (!principal.HasClaim(c => c.Type == OpenIddictConstants.Claims.Subject))
        {
            var id = (ClaimsIdentity)principal.Identity!;
            id.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id.ToString()));
        }
        // ----------------------------------------------
        var scopes = request.GetScopes();

        // Scopes requested by the client
        principal.SetScopes(scopes);

        // Map scopes -> API resources (audience)
        var resources = new HashSet<string>(StringComparer.Ordinal);
        if (principal.HasScope("payments.read") || principal.HasScope("payments.write"))
            resources.Add("payments-api");
        if (principal.HasScope("accounting.read") || principal.HasScope("accounting.write"))
            resources.Add("accounting-api");
        principal.SetResources(resources);

        // Map claim destinations
        foreach (var claim in principal.Claims)
            claim.SetDestinations(ClaimsDestinations.For(principal, claim));

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // === Token endpoint: authorization_code/refresh_token/client_credentials ===
    [HttpPost("~/connect/token")]
    public async Task<IActionResult> ExchangeAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OIDC request is missing.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = result.Principal ?? throw new InvalidOperationException("Missing principal.");

            // Ensure claim destinations are present after refresh/code exchange
            foreach (var claim in principal.Claims)
                claim.SetDestinations(ClaimsDestinations.For(principal, claim));

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (request.IsClientCredentialsGrantType())
        {
            // Represent the client as the subject of the token (sub = client_id)
            var identity = new ClaimsIdentity(
                 authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                 nameType: ClaimTypes.Name,
                 roleType: ClaimTypes.Role);
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, request.ClientId!));

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());

            // Audiences by requested scopes
            var resources = new HashSet<string>(StringComparer.Ordinal);
            if (principal.HasScope("payments.write") || principal.HasScope("payments.read"))
                resources.Add("payments-api");
            if (principal.HasScope("accounting.write") || principal.HasScope("accounting.read"))
                resources.Add("accounting-api");
            principal.SetResources(resources);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize]
    [HttpGet("~/connect/userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        return Ok(new
        {
            sub = user.Id.ToString(),
            name = user.DisplayName ?? user.UserName,
            email = user.Email
        });
    }
}