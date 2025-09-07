using IDP.Duende;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Put migrations in *this* assembly (your app)
var migrationsAssembly = "Duende.IdentityServer.EntityFramework.DbContexts";//typeof(Program).Assembly.GetName().Name;

var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(conn, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "idsvr_identity")));

builder.Services
    .AddIdentityServer()
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

builder.Services
    .AddIdentity<AppUser, AppRole>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 8;
        opt.SignIn.RequireConfirmedEmail = false; // set true in prod + email sender
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddConfiguredIdentityServer(builder.Configuration);

builder.Services.AddControllersWithViews(); // login/consent UI (Razor if you add it)
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("spa", p => p
        .WithOrigins("https://localhost:4200")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("spa");

app.UseIdentityServer();
app.UseAuthorization();

app.MapControllers();

await IdentityServerHosting.EnsureSeedDataAsync(app.Services);
await app.Services.EnsureDefaultAdminAsync();

app.Run();