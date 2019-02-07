using GroceryStore.Areas.Identity.Pages.Account;
using GroceryStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStore.Data
{
    public class SeedData
    {
        private static void LogErrors(IEnumerable<IdentityError> errors, ILogger<RegisterModel> logger)
        {
            foreach (var error in errors)
            {
                logger.LogInformation($"{error.Code}: {error.Description}");
            }
        }

        public static async Task Initialize(ApplicationDbContext context,
                          UserManager<ApplicationUser> userManager,
                          RoleManager<ApplicationRole> roleManager,
                          IConfiguration configuration,
                          ILogger<RegisterModel> logger)
        {
            context.Database.EnsureCreated();

            var administration = configuration.GetSection("AdminDefault");

            string userName = administration.GetSection("UserName").Value;
            string email = administration.GetSection("Email").Value;
            string phoneNumber = administration.GetSection("PhoneNumber").Value;
            string defaultAdminRole = administration.GetSection("Role").Value;
            string password = administration.GetSection("Password").Value;
            string firstName = administration.GetSection("FirstName").Value;
            string lastName = administration.GetSection("LastName").Value;
            string defaultRole = configuration.GetSection("DefaultRole").Value;

            if (await roleManager.FindByNameAsync(defaultAdminRole) == null)
            {
                var result = await roleManager.CreateAsync(new ApplicationRole(defaultAdminRole));

                if (result.Succeeded)
                {
                    logger.LogInformation("Created admin role.");
                }
                else
                {
                    LogErrors(result.Errors, logger);

                    return;
                }
            }

            if (await roleManager.FindByNameAsync(defaultRole) == null)
            {
                var result = await roleManager.CreateAsync(new ApplicationRole(defaultRole));

                if (result.Succeeded)
                {
                    logger.LogInformation("Created user role.");
                }
                else
                {
                    LogErrors(result.Errors, logger);

                    return;
                }
            }

            var user = await userManager.FindByNameAsync(userName);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    FirstName = firstName,
                    LastName = lastName
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    logger.LogInformation("Created admin account with password.");
                    user = await userManager.FindByNameAsync(userName); // do it again as a new id was created for this user so we need to get the user info again
                }
                else
                {
                    LogErrors(result.Errors, logger);

                    return;
                }
            }

            var role = (from u in context.Users
                        join ur in context.UserRoles on u.Id equals ur.UserId
                        join r in context.Roles on ur.RoleId equals r.Id
                        where u.Id == user.Id
                        select r).FirstOrDefault();

            if (role == null)
            {
                var result = await userManager.AddToRoleAsync(user, defaultAdminRole);

                if (result.Succeeded)
                {
                    logger.LogInformation("Admin role added to admin account.");
                }
                else
                {
                    LogErrors(result.Errors, logger);

                    return;
                }
            }
            else if (role.Name != defaultRole)
            {
                role.Name = defaultRole;
                var result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    logger.LogInformation("Admin role updated to admin account.");
                }
                else
                {
                    LogErrors(result.Errors, logger);

                    return;
                }
            }
        }
    }
}
