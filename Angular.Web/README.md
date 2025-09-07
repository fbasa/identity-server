Below is a compact, production‑ready Angular 16–20 starter that authenticates against **Duende IdentityServer** using **Authorization Code + PKCE**, requests the scopes `openid profile payments.read accounting.read`, and automatically attaches the `access_token` to API calls.

---

## Project structure (standalone Angular)

```
/angular-duende-oidc/
  package.json
  src/
    main.ts
    environments/
      environment.ts
      environment.development.ts
    app/
      app.component.ts
      app.routes.ts
      auth/
        auth.config.ts
        auth.service.ts
        auth.guard.ts
        callback.component.ts
        logout.component.ts
      components/
        login-status.component.ts
      services/
        payments-api.service.ts
        accounting-api.service.ts
```

> Uses **angular-oauth2-oidc** for OIDC/OAuth2 + PKCE. Tokens are stored in **sessionStorage** (safer than localStorage; prefer in‑memory stores for highly sensitive apps). In production, lock CORS, HTTPS, and CSP.

---

## 1) Dependencies (package.json)

---

## 2) Environment config

`src/environments/environment.ts`

`src/environments/environment.development.ts`

---

## 3) OAuth configuration

`src/app/auth/auth.config.ts`

---

## 4) Auth service (init, login, logout, refresh)

`src/app/auth/auth.service.ts`

---

## 5) Route guard for protected pages

`src/app/auth/auth.guard.ts`

---

## 6) Auth callback + logout components

`src/app/auth/callback.component.ts`

`src/app/auth/logout.component.ts`

---

## 7) Login status header (UX helper)

`src/app/components/login-status.component.ts`

---

## 8) API services (tokens auto‑attached)

### Option A (recommended): use OAuthModule resourceServer filter

If you configure **allowedUrls** in `main.ts` (see below), the library’s built‑in interceptor adds `Authorization: Bearer <token>` automatically.

`src/app/services/payments-api.service.ts`

`src/app/services/accounting-api.service.ts`

### Option B: custom HTTP interceptor (manual attach)

If you prefer explicit control, create an interceptor that reads the token from `AuthService` and sets the header. (Optional; not shown since Option A is simpler.)

---

## 9) App routes

`src/app/app.routes.ts`

---

## 10) Root component

`src/app/app.component.ts`


---

## 11) Bootstrap (standalone) + OAuthModule interceptor

`src/main.ts`

> The `OAuthModule` resourceServer configuration automatically adds the **Authorization** header to any HTTP request whose URL starts with the configured `allowedUrls`.

---

## 12) Duende client configuration (must match)

Ensure you have a Duende client like this (DB or in-memory) for the SPA:

```csharp
new Client
{
    ClientId = "angular-spa",
    ClientName = "Angular Web App",
    AllowedGrantTypes = GrantTypes.Code,
    RequirePkce = true,
    RequireClientSecret = false,
    RedirectUris = { "https://localhost:4200/auth/callback" },
    PostLogoutRedirectUris = { "https://localhost:4200/" },
    AllowedCorsOrigins = { "https://localhost:4200" },
    AllowedScopes = { "openid", "profile", "payments.read", "accounting.read" },
    AllowOfflineAccess = true,          // enables refresh tokens
    AccessTokenLifetime = 900           // 15 minutes (example)
}
```

---

## 13) Run & test

1. **Run Duende IdentityServer** (ensure the SPA client above exists).
2. **Run Payments.Api** (aud = `payments-api`) and **Accounting.Api** (aud = `accounting-api`).
3. **Start Angular**: `npm install && npm start` → open `https://localhost:4200`.
4. Click **Login** → Duende → consent (if enabled) → redirected back to `/auth/callback`.
5. Use the buttons to call the APIs; the `Authorization: Bearer <access_token>` header is auto‑attached.

---

## Hardening notes

* Keep tokens out of `localStorage`. Prefer `sessionStorage` or in‑memory if you can.
* Enforce HTTPS and strict CORS/Origin allow‑list.
* Consider silent refresh with refresh tokens + short access‑token TTLs.
* Add CSP/Trusted Types and sanitize any user‑generated content to reduce XSS risk.
* In Duende, configure **signed** & **rotated** keys; keep issuer stable.


All set! I dropped a complete Angular starter (standalone, PKCE, auto-attach Bearer) into the canvas. It includes:

* OAuth setup (`angular-oauth2-oidc`) with `openid profile payments.read accounting.read`
* Guarded routes and a login/logout flow
* Built-in interceptor that adds `Authorization: Bearer <token>` for your Payments/Accounting APIs
* Matching Duende client config snippet

If you want, I can also generate Dockerfiles (Nginx) and a GitHub Actions workflow to deploy the Angular app to GitHub Pages or your server.
