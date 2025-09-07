namespace Accounting.Api;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddScopePolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("accounting.read", p => p.RequireClaim("scope", "accounting.read", "accounting.write"));
            options.AddPolicy("accounting.write", p => p.RequireClaim("scope", "accounting.write"));
        });
        return services;
    }
}