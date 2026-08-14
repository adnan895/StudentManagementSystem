//using Microsoft.AspNetCore.Identity;
//using StudentManagementSystem.Models;

//namespace StudentManagementSystem.Data
//{
//    public static class DbInitializer
//    {
//        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
//        {
//            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

//            // 1. Ensure Roles Exist
//            string[] roles = { "Admin", "Instructor", "Student" };
//            foreach (var role in roles)
//            {
//                if (!await roleManager.RoleExistsAsync(role))
//                {
//                    await roleManager.CreateAsync(new IdentityRole(role));
//                }
//            }

//            // 2. Seed Admin Account
//            await CreateUserWithRoleAsync(userManager, "admin@institution.edu", "Admin123!", "Admin");

//            // 3. Seed Instructor Account
//            await CreateUserWithRoleAsync(userManager, "instructor@institution.edu", "Teacher123!", "Instructor");

//            // 4. Seed Student Account
//            await CreateUserWithRoleAsync(userManager, "student@institution.edu", "Student123!", "Student");
//        }

//        private static async Task CreateUserWithRoleAsync(
//            UserManager<ApplicationUser> userManager,
//            string email,
//            string password,
//            string role)
//        {
//            var user = await userManager.FindByEmailAsync(email);
//            if (user == null)
//            {
//                user = new ApplicationUser
//                {
//                    UserName = email,
//                    Email = email,
//                    EmailConfirmed = true
//                };

//                var createResult = await userManager.CreateAsync(user, password);
//                if (createResult.Succeeded)
//                {
//                    await userManager.AddToRoleAsync(user, role);
//                }
//            }
//            else
//            {
//                // Assign role if missing
//                if (!await userManager.IsInRoleAsync(user, role))
//                {
//                    await userManager.AddToRoleAsync(user, role);
//                }
//            }
//        }
//    }
//}




using Microsoft.AspNetCore.Identity;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Ensure Roles Exist
            string[] roles = { "Admin", "Instructor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Accounts with exact table credentials
            await EnsureUserWithPasswordAsync(userManager, "admin@institution.edu", "Admin123!", "Admin");
            await EnsureUserWithPasswordAsync(userManager, "instructor@institution.edu", "Instructor123!", "Instructor");
            await EnsureUserWithPasswordAsync(userManager, "student@institution.edu", "Student123!", "Student");
        }

        private static async Task EnsureUserWithPasswordAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            else
            {
                // Ensure role assignment
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }

                // Reset password to match 'password' argument if previous seed had a different value
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, password);
            }
        }
    }
}