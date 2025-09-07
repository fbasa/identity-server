using Microsoft.AspNetCore.Identity;

namespace IDP.OpenIddict.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public Guid? TenantId { get; set; }
}
