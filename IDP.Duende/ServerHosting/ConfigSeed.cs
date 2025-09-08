namespace IDP.Duende.ServerHosting;

using global::Duende.IdentityServer.Models;

public static class ConfigSeed
{
    // OIDC identity resources: openid, profile (extend as needed)
    public static IEnumerable<IdentityResource> IdentityResources() => new IdentityResource[] 
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile()
    };

    // API scopes (fine-grained authorization)
    public static IEnumerable<ApiScope> ApiScopes() => new[]
    {
        new ApiScope("payments.read",  "Read payments"),
        new ApiScope("payments.write", "Write payments"),
        new ApiScope("accounting.read","Read accounting"),
        new ApiScope("accounting.write","Write accounting")
    };

    // API resources (JWT aud + claims aggregation)
    public static IEnumerable<ApiResource> ApiResources() => new[]
    {
        new ApiResource("payments-api","Payments API")
        {
            Scopes = { "payments.read", "payments.write" }
        },
        new ApiResource("accounting-api","Accounting API")
        {
            Scopes = { "accounting.read", "accounting.write" }
        }
    };

    public static IEnumerable<Client> Clients() => new[]
    {
        // SPA (Authorization Code + PKCE; no client secret)
        new Client
        {
            ClientId = "angular-spa",
            ClientName = "Angular Web App",
            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            RequireClientSecret = false,
           // RedirectUris = { "http://localhost:4200/auth/callback" },
            RedirectUris = { "http://localhost:4200/auth/callback" },
            PostLogoutRedirectUris = { "http://localhost:4200/" },
            AllowedCorsOrigins = { "http://localhost:4200" },
            AllowAccessTokensViaBrowser = true,
            AllowedScopes = {
                "openid", "profile",
                "payments.read","payments.write",
                "accounting.read","accounting.write"
            },
            AccessTokenLifetime = 900,     // 15m
            AllowOfflineAccess = true      // enables refresh tokens
        },

        // Machine-to-machine (Client Credentials) for jobs/worker
        new Client
        {
            ClientId = "jobs-worker",
            ClientName = "Background Jobs Worker",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("super-secret-worker".Sha256()) },
            AllowedScopes = { "payments.write","accounting.write" },
            AccessTokenLifetime = 600,
        },

        new Client
        {
            ClientId = "mvc-client",
            ClientName = "MVC Web App",
            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            RequireClientSecret = true, // confidential web app
            ClientSecrets = { new Secret("mvc-secret".Sha256()) },


            RedirectUris = { "https://localhost:5002/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
            FrontChannelLogoutUri = "https://localhost:5002/signout-oidc",
            AllowedCorsOrigins = { "https://localhost:5002" },


            AllowedScopes = { "openid", "profile", "payments.read", "accounting.read" },
            AllowOfflineAccess = false, // set true if you want refresh tokens
            AccessTokenLifetime = 900 // 15 minutes (example)
        }
    };
}