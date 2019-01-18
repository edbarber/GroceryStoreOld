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
                await roleManager.CreateAsync(new ApplicationRole(defaultAdminRole));
            }

            if (await roleManager.FindByNameAsync(defaultRole) == null)
            {
                await roleManager.CreateAsync(new ApplicationRole(defaultRole));
            }

            if (await userManager.FindByNameAsync(userName) == null)
            {
                var user = new ApplicationUser
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
                }

                result = await userManager.AddToRoleAsync(user, defaultAdminRole);

                if (result.Succeeded)
                {
                    logger.LogInformation("Admin role added to admin account.");
                }
            }
        }
    }
}
