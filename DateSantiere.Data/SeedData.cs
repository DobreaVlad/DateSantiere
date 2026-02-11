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

        // Create default SuperAdmin user
        var superAdmin = await userManager.FindByEmailAsync("superadmin@datesantiere.ro");
        if (superAdmin == null)
        {
            superAdmin = new ApplicationUser
            {
                UserName = "superadmin@datesantiere.ro",
                Email = "superadmin@datesantiere.ro",
                EmailConfirmed = true,
                FirstName = "Super",
                LastName = "Admin",
                IsActive = true,
                AdminType = "SuperAdmin",
                AccountType = "Enterprise",
                MonthlySearchLimit = -1,
                MonthlyExportLimit = -1
            };

            var result = await userManager.CreateAsync(superAdmin, "SuperAdmin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "Admin");
            }
        }
        else
        {
            // Update existing user
            superAdmin.AdminType = "SuperAdmin";
            superAdmin.AccountType = "Enterprise";
            superAdmin.MonthlySearchLimit = -1;
            superAdmin.MonthlyExportLimit = -1;
            superAdmin.IsActive = true;
            await userManager.UpdateAsync(superAdmin);
            
            // Ensure user has Admin role
            if (!await userManager.IsInRoleAsync(superAdmin, "Admin"))
            {
                await userManager.AddToRoleAsync(superAdmin, "Admin");
            }
        }
        
        // Create default Admin user
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
                IsActive = true,
                AdminType = "Admin",
                AccountType = "Premium",
                MonthlySearchLimit = 500,
                MonthlyExportLimit = 50
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // Update existing user
            adminUser.AdminType = "Admin";
            adminUser.AccountType = "Premium";
            adminUser.MonthlySearchLimit = 500;
            adminUser.MonthlyExportLimit = 50;
            adminUser.IsActive = true;
            await userManager.UpdateAsync(adminUser);
            
            // Ensure user has Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
