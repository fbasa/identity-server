using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;


var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));


// MVC
builder.Services.AddControllersWithViews();


// Strongly-typed settings
var auth = builder.Configuration.GetSection("Authentication");
var authority = auth["Authority"]!;
var clientId = auth["ClientId"]!;
var clientSecret = auth["ClientSecret"]!; // for confidential MVC apps
var callbackPath = auth["CallbackPath"] ?? "/signin-oidc";
var signedOutCallbackPath = auth["SignedOutCallbackPath"] ?? "/signout-callback-oidc";
var scopes = auth.GetSection("Scopes").Get<string[]>() ?? new[] { "openid", "profile" };

builder.Services.AddHttpContextAccessor();

builder.Services
.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "mvc.client";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = authority;
    options.ClientId = clientId;
    options.ClientSecret = clientSecret; // required for confidential clients
    options.ResponseType = "code"; // Authorization Code flow
    options.UsePkce = true; // PKCE on
    options.SaveTokens = true; // keep tokens in auth session
    options.GetClaimsFromUserInfoEndpoint = true;
    options.CallbackPath = callbackPath;
    options.SignedOutCallbackPath = signedOutCallbackPath;
    options.RequireHttpsMetadata = true;


    // Request API scopes in addition to OIDC scopes
    options.Scope.Clear();
    foreach (var s in scopes) options.Scope.Add(s);
    // Optional: refresh tokens for long sessions
    // options.Scope.Add("offline_access");


    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});


// Typed HttpClients that auto-attach the access token
builder.Services.AddTransient<MVC.Web.Auth.AccessTokenHandler>();


builder.Services.AddHttpClient("PaymentsApi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Apis:Payments"]!);
})
.AddHttpMessageHandler<MVC.Web.Auth.AccessTokenHandler>();


builder.Services.AddHttpClient("AccountingApi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Apis:Accounting"]!);
})
.AddHttpMessageHandler<MVC.Web.Auth.AccessTokenHandler>();


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.Run();