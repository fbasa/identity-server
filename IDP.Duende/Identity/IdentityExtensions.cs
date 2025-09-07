namespace IDP.Duende.Identity;

using Microsoft.AspNetCore.Identity;
using System;

public static class IdentityExtensions
{
    public static async Task EnsureDefaultAdminAsync(this IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        var adminRole = "admin";
        if (!await roleMgr.RoleExistsAsync(adminRole))
            await roleMgr.CreateAsync(new AppRole { Name = adminRole });

        var adminEmail = "admin@local.test";
        var user = await userMgr.FindByEmailAsync(adminEmail);
        if (user == null)
        {
            user = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                DisplayName = "System Admin"
            };
            await userMgr.CreateAsync(user, "Change_this_devP@ss1!");
            await userMgr.AddToRoleAsync(user, adminRole);
        }
    }
}