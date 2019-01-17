using GroceryStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStore.Data
{
    public class AdminData
    {
        public static async Task Initialize(ApplicationDbContext context,
                          UserManager<ApplicationUser> userManager,
                          RoleManager<ApplicationRole> roleManager,
                          IConfiguration configuration)
        {
            context.Database.EnsureCreated();

            var administration = configuration.GetSection("AdminDefault");

            string userName = administration.GetSection("UserName").Value;
            string email = administration.GetSection("Email").Value;
            string phoneNumber = administration.GetSection("PhoneNumber").Value;
            string role = administration.GetSection("Role").Value;
            string password = administration.GetSection("Password").Value;
            string firstName = administration.GetSection("FirstName").Value;
            string lastName = administration.GetSection("LastName").Value;

            if (await roleManager.FindByNameAsync(role) == null)
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
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

                var result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    await userManager.AddPasswordAsync(user, password);
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
