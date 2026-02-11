using DateSantiere.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace DateSantiere.Web.Services;

public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = principal.Identity as ClaimsIdentity;

        if (identity != null)
        {
            if (!string.IsNullOrEmpty(user.FirstName))
            {
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            }

            if (!string.IsNullOrEmpty(user.LastName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
            }

            // Add a custom claim for the display name
            var displayName = string.Empty;
            
            if (!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
            {
                displayName = $"{user.FirstName} {user.LastName}";
            }
            else if (!string.IsNullOrEmpty(user.FirstName))
            {
                displayName = user.FirstName;
            }
            else if (!string.IsNullOrEmpty(user.LastName))
            {
                displayName = user.LastName;
            }
            else
            {
                // Fallback to email username part
                displayName = user.Email?.Split('@')[0] ?? "User";
            }

            identity.AddClaim(new Claim("DisplayName", displayName));
        }

        return principal;
    }
}
