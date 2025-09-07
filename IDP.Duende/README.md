dotnet tool install --global dotnet-ef

# In /src/AuthServer
dotnet ef migrations add InitIdentity -c AppDbContext -o Migrations/Identity
dotnet ef migrations add InitIdsvrCfg -c Duende.IdentityServer.EntityFramework.DbContexts.ConfigurationDbContext -o Migrations/IdsvrCfg
dotnet ef migrations add InitIdsvrOp  -c Duende.IdentityServer.EntityFramework.DbContexts.PersistedGrantDbContext -o Migrations/IdsvrOp

dotnet ef database update -c AppDbContext
dotnet ef database update -c Duende.IdentityServer.EntityFramework.DbContexts.ConfigurationDbContext
dotnet ef database update -c Duende.IdentityServer.EntityFramework.DbContexts.PersistedGrantDbContext


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
