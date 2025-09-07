namespace IDP.Duende;

using global::Duende.IdentityServer.EntityFramework.DbContexts;
using global::Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using System;

public static class IdentityServerHosting
{
    public static IServiceCollection AddConfiguredIdentityServer(this IServiceCollection services, IConfiguration cfg)
    {
        var conn = cfg.GetConnectionString("DefaultConnection")!;

        services.AddIdentityServer(options =>
        {
            options.EmitStaticAudienceClaim = true;
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
        })
            .AddConfigurationStore(opt =>
            {
                opt.ConfigureDbContext = b => b.UseSqlServer(conn, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "idsvr_cfg"));
            })
            .AddOperationalStore(opt =>
            {
                opt.ConfigureDbContext = b => b.UseSqlServer(conn, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "idsvr_op"));
                opt.EnableTokenCleanup = true;
                opt.TokenCleanupInterval = 3600; // seconds
            })
            .AddAspNetIdentity<AppUser>()
            .AddDeveloperSigningCredential(); // dev only; replace in prod with a real cert

        return services;
    }

    public static async Task EnsureSeedDataAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var cfgCtx = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        var opCtx = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        await cfgCtx.Database.MigrateAsync();
        await opCtx.Database.MigrateAsync();

        // Seed clients/scopes/resources if empty
        if (!cfgCtx.Clients.Any())
        {
            foreach (var c in ConfigSeed.Clients())
                cfgCtx.Clients.Add(c.ToEntity());
        }
        if (!cfgCtx.IdentityResources.Any())
        {
            foreach (var r in ConfigSeed.IdentityResources())
                cfgCtx.IdentityResources.Add(r.ToEntity());
        }
        if (!cfgCtx.ApiScopes.Any())
        {
            foreach (var s in ConfigSeed.ApiScopes())
                cfgCtx.ApiScopes.Add(s.ToEntity());
        }
        if (!cfgCtx.ApiResources.Any())
        {
            foreach (var r in ConfigSeed.ApiResources())
                cfgCtx.ApiResources.Add(r.ToEntity());
        }
        await cfgCtx.SaveChangesAsync();
    }
}