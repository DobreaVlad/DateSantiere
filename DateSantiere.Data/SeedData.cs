using DateSantiere.Models;
using Microsoft.AspNetCore.Identity;

namespace DateSantiere.Data;

public static class SeedData
{
    public static async Task Initialize(
        IServiceProvider serviceProvider,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "User", "Premium" };
        
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create default admin user
        var adminUser = await userManager.FindByEmailAsync("admin@datesantiere.ro");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@datesantiere.ro",
                Email = "admin@datesantiere.ro",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "DateSantiere",
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
