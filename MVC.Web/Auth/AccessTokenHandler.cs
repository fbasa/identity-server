
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;


namespace MVC.Web.Auth;

public class AccessTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AccessTokenHandler(IHttpContextAccessor accessor) => _httpContextAccessor = accessor;


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null && httpContext.User?.Identity?.IsAuthenticated == true)
        {
            // Access token saved in the auth session by the OIDC handler
            var token = await httpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}