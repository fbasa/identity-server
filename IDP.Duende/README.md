# Duende IdentityServer with ASP.NET Core Identity

```dotnet tool install --global dotnet-ef```

```cd src/AuthServer```

# In /src/AuthServer
```bash
dotnet ef migrations add InitIdentity -c AppDbContext -o Migrations/Identity
dotnet ef migrations add InitIdsvrCfg -c Duende.IdentityServer.EntityFramework.DbContexts.ConfigurationDbContext -o Migrations/IdsvrCfg
dotnet ef migrations add InitIdsvrOp  -c Duende.IdentityServer.EntityFramework.DbContexts.PersistedGrantDbContext -o Migrations/IdsvrOp
```

```bash
dotnet ef database update -c AppDbContext
dotnet ef database update -c Duende.IdentityServer.EntityFramework.DbContexts.ConfigurationDbContext
dotnet ef database update -c Duende.IdentityServer.EntityFramework.DbContexts.PersistedGrantDbContext
dotnet run
```

# Using SPA & machine clients

### SPA (Angular)

* Use **Authorization Code + PKCE** to redirect to `AuthServer`.
* Request scopes needed by the UI (e.g., `openid profile payments.read accounting.read`).
* API calls: attach `access_token` in `Authorization: Bearer <token>`.

### Machine-to-machine

```bash
curl -s -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=jobs-worker&client_secret=super-secret-worker&grant_type=client_credentials&scope=payments.write accounting.write"
```

Use the returned `access_token` against Payment/Accounting APIs.

---

# Optional: switch to **reference tokens** + introspection

If you need immediate revocation:

1. In `ConfigSeed.Clients()`, set `AccessTokenType = AccessTokenType.Reference` for selected clients.
2. In **APIs**, add introspection:

```csharp
builder.Services.AddAuthentication("token")
    .AddOAuth2Introspection("token", options =>
    {
        options.Authority = authAuthority;
        options.ClientId = "payments-api";          // define a protected API as introspection client
        options.ClientSecret = "payments-api-secret"; // store secret safely
        options.CacheDuration = TimeSpan.FromMinutes(5);
    });
```

…and register an **introspection client** in AuthServer with the `introspection` permission.

---

# Hardening checklist (prod)

* Replace `AddDeveloperSigningCredential()` with **AddSigningCredential(X509Certificate2)** (Key Vault/DPAPI).
* Configure **cookie** settings (SameSite=None, secure).
* Enable **CORS** only for trusted origins.
* Short **access token** TTL (10–15m), use **refresh tokens** for SPAs.
* Use **DPoP** or **MTLS** for high-security machine flows (optional).
* Centralized logging/metrics (Serilog + OpenTelemetry) with correlation IDs.
* Strict input validation and **rate limiting** at APIs.
* Rotate secrets; prefer **Managed Identity** / Key Vault.
* **Issuer URL** must be stable and reachable by all APIs.

---

# Quick start (dev)

1. **Run AuthServer**


2. **Run Payment.Api**

```bash
cd ../Payment.Api
dotnet run
```

3. **Test with client credentials**

```bash
# token
TOKEN=$(curl -s -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=jobs-worker&client_secret=super-secret-worker&grant_type=client_credentials&scope=payments.write" \
  | jq -r .access_token)

# call API
curl -k -H "Authorization: Bearer $TOKEN" https://localhost:5003/api/payments
```

---

# Multi-tenant touches (optional)

* Add a **TenantId** claim during sign-in (from `AppUser.TenantId`).
* In APIs, add a middleware that resolves tenant from claim → sets current tenant context → applies filters in your data layer.
* Create per-tenant policies like `"tenant:{id}:payments.write"` if you need row-level scoping.


