using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace IDP.Duende.Controllers;

public class HomeController : Controller
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IWebHostEnvironment _env;
    public HomeController(IIdentityServerInteractionService interaction, IWebHostEnvironment env)
        => (_interaction, _env) = (interaction, env);

    [HttpGet("/home/error")]
    public async Task<IActionResult> Error(string errorId)
    {
        var msg = await _interaction.GetErrorContextAsync(errorId);
        // In dev, show details
        if (_env.IsDevelopment() && msg is not null)
            return Content($"""
                error: {msg.Error}
                description: {msg.ErrorDescription}
                client_id: {msg.ClientId}
                redirect_uri: {msg.RedirectUri}
                request_id: {msg.RequestId}
            """, "text/plain");

        // Prod: render your friendly error view with minimal info
        return View("Error");
    }
}
