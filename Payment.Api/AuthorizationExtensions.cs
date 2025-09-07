namespace Payment.Api;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddScopePolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("payments.read", p => p.RequireClaim("scope", "payments.read", "payments.write"));
            options.AddPolicy("payments.write", p => p.RequireClaim("scope", "payments.write"));
        });
        return services;
    }
}