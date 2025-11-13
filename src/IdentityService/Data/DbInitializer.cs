using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        ILogger<DbInitializer> logger)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        var roles = new[] { "Applicant", "Manager", "HeadManager", "Admin" };
        
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName);
                var result = await roleManager.CreateAsync(role);
                
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // Seed admin user
        var adminEmail = "admin@university.edu";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var adminPassword = GenerateSecurePassword();
            
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FullName = "System Administrator",
                Role = Shared.Contracts.Enums.UserRole.Admin,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogWarning("===============================================");
                logger.LogWarning("ADMIN USER CREATED");
                logger.LogWarning("Email: {Email}", adminEmail);
                logger.LogWarning("Password: {Password}", adminPassword);
                logger.LogWarning("PLEASE CHANGE THIS PASSWORD IMMEDIATELY!");
                logger.LogWarning("===============================================");
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed manager user
        var managerEmail = "manager@university.edu";
        var managerUser = await userManager.FindByEmailAsync(managerEmail);

        if (managerUser == null)
        {
            var managerPassword = GenerateSecurePassword();
            
            managerUser = new ApplicationUser
            {
                UserName = "manager",
                Email = managerEmail,
                FullName = "Admissions Manager",
                Role = Shared.Contracts.Enums.UserRole.Manager,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(managerUser, managerPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
                logger.LogWarning("===============================================");
                logger.LogWarning("MANAGER USER CREATED");
                logger.LogWarning("Email: {Email}", managerEmail);
                logger.LogWarning("Password: {Password}", managerPassword);
                logger.LogWarning("PLEASE CHANGE THIS PASSWORD IMMEDIATELY!");
                logger.LogWarning("===============================================");
            }
            else
            {
                logger.LogError("Failed to create manager user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static string GenerateSecurePassword()
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string all = upper + lower + digits + special;

        var random = new Random();
        var password = new char[16];

        // Ensure at least one of each required character type
        password[0] = upper[random.Next(upper.Length)];
        password[1] = lower[random.Next(lower.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        // Fill the rest randomly
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = all[random.Next(all.Length)];
        }

        // Shuffle the password
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}
