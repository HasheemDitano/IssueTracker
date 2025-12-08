using Microsoft.AspNetCore.Identity;

namespace IssueTracker.Data
{
    public static class IdentitySeeder
    {
        private static readonly string[] Roles =
        {
            "Customer",
            "Engineer",
            "Admin"
        };

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Ensure roles
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed admin user
            // DEFAULT ADMIN CREDENTIALS FOR DEVELOPMENT:
            // Email: admin@issue.local
            // Password: Admin123!
            // Roles: Admin, Engineer
            var adminEmail = "admin@issue.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                // Simple dev password – change in real app
                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddToRoleAsync(adminUser, "Engineer");
                }
            }

            // Seed sample engineer user
            // DEFAULT ENGINEER CREDENTIALS FOR DEVELOPMENT:
            // Email: engineer@issue.local
            // Password: Engineer123!
            // Roles: Engineer
            var engineerEmail = "engineer@issue.local";
            var engineerUser = await userManager.FindByEmailAsync(engineerEmail);

            if (engineerUser == null)
            {
                engineerUser = new IdentityUser
                {
                    UserName = engineerEmail,
                    Email = engineerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(engineerUser, "Engineer123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(engineerUser, "Engineer");
                }
            }

            // Seed sample customer user
            // DEFAULT CUSTOMER CREDENTIALS FOR DEVELOPMENT:
            // Email: customer@issue.local
            // Password: Customer123!
            // Roles: Customer
            var customerEmail = "customer@issue.local";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);

            if (customerUser == null)
            {
                customerUser = new IdentityUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(customerUser, "Customer123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }
        }
    }
}
