using System.Security.Claims;
using Identity.Core.Permissions;
using Microsoft.AspNetCore.Identity;
using Identity.Repository.Models;

namespace Identity.Repository.Seeds;

public class PermissionSeed
{
    public static async Task Seed(RoleManager<AppRole> roleManager)
    {
        var hasBasicRole = await roleManager.RoleExistsAsync("BasicRole");
        var hasAdvancedRole = await roleManager.RoleExistsAsync("AdvancedRole");
        var hasAdminRole = await roleManager.RoleExistsAsync("AdminRole");
        if (!hasBasicRole)//Basit rol'e ise sadece okuma rolü tanımladık.
        {
            await roleManager.CreateAsync(new AppRole { Name = "BasicRole" });
            var basicRole = await roleManager.FindByNameAsync("BasicRole");
            await AddReadPermission(basicRole, roleManager);
        }
        if (!hasAdvancedRole) //Advanced rol'e create update read
        {
            await roleManager.CreateAsync(new AppRole { Name = "AdvancedRole" });
            var advancedRole = await roleManager.FindByNameAsync("AdvancedRole");
            await AddReadPermission(advancedRole, roleManager);
            await AddUpdateAndCreatePermission(advancedRole, roleManager);
        }
        if (!hasAdminRole) //Admine crud operasyonlarının tümü
        {
            await roleManager.CreateAsync(new AppRole { Name = "AdminRole" });
            var adminRole = await roleManager.FindByNameAsync("AdminRole");
            await AddReadPermission(adminRole, roleManager);
            await AddUpdateAndCreatePermission(adminRole, roleManager);
            await AddDeletePermission(adminRole, roleManager);
        }
    }

    public static async Task AddReadPermission(AppRole role, RoleManager<AppRole> roleManager)
    {
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Stock.Read));

        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Order.Read));

        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Catalog.Read));
    }

    public static async Task AddUpdateAndCreatePermission(AppRole role, RoleManager<AppRole> roleManager)
    {
        //Stock
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Stock.Create));
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Stock.Update));
        //Order
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Order.Create));
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Order.Update));
        //Catalog
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Catalog.Create));
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Catalog.Update));
    }

    public static async Task AddDeletePermission(AppRole role, RoleManager<AppRole> roleManager)
    {
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Stock.Delete));
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Order.Delete));
        await roleManager.AddClaimAsync(role,
            new Claim("Permission", Permission.Catalog.Delete));
    }
}