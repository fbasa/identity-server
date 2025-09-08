using Duende.IdentityModel;
using IDP.Duende.Identity;
using IDP.Duende.ServerHosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Put migrations in *this* assembly (your app)
var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(conn, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "idsvr_identity")));

builder.Services
    .AddIdentity<AppUser, AppRole>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 8;
        opt.SignIn.RequireConfirmedEmail = false; // set true in prod + email sender
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//Map Identity claim types to OIDC (ensures a 'sub' claim)
builder.Services.Configure<IdentityOptions>(o =>
{
    o.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject; // "sub"
    o.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.Name;    // "name"
    o.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;    // "role"
});

//builder.Services.AddConfiguredIdentityServer(builder.Configuration);

builder.Services
    .AddIdentityServer(options =>
    {
        options.EmitStaticAudienceClaim = true;
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
    })
    .AddAspNetIdentity<AppUser>()   // <- this wires 'sub' from your Identity user
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseSqlServer(conn, sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseSqlServer(conn, sql => sql.MigrationsAssembly(migrationsAssembly));

        options.EnableTokenCleanup = true;
        options.TokenCleanupInterval = 3600;
    });

builder.Services.AddControllersWithViews(); // login/consent UI (Razor if you add it)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("spa", p => p
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseCors("spa");

app.UseIdentityServer();    // order matters: IdentityServer before Authorization
app.UseAuthorization();

app.MapControllers();

await IdentityServerHosting.EnsureSeedDataAsync(app.Services);
await app.Services.EnsureDefaultAdminAsync();

app.Run();