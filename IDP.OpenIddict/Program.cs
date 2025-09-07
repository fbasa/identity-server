using IDP.OpenIddict;
using IDP.OpenIddict.Identity;
using IDP.OpenIddict.ServerHosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(conn, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "auth")));

builder.Services.AddIdentity<AppUser, AppRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequiredLength = 8;
    opt.SignIn.RequireConfirmedEmail = false; // true in prod when email sender is configured
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddConfiguredOpenIddict(builder.Configuration);

builder.Services.AddHostedService<OAuthSeed>();

builder.Services.AddControllersWithViews();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("spa", p => p
        .WithOrigins("https://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("spa");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// DB + admin user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
await app.Services.EnsureDefaultAdminAsync();

app.Run();
