namespace IDP.Duende.Identity;

using Microsoft.AspNetCore.Identity;
using System;

public class AppUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    // Example tenant support
    public Guid? TenantId { get; set; }
}
