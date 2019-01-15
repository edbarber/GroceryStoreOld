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
                          UserManager<IdentityUser> userManager,
                          RoleManager<IdentityRole> roleManager,
                          IConfiguration configuration)
        {
            context.Database.EnsureCreated();

            var administration = configuration.GetSection("Administrator");

            string userName = administration.GetSection("UserName").Value;
            string email = administration.GetSection("Email").Value;
            string phoneNumber = administration.GetSection("PhoneNumber").Value;
            string role = administration.GetSection("Role").Value;
            string password = administration.GetSection("Password").Value;

            if (await roleManager.FindByNameAsync(role) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            if (await userManager.FindByNameAsync(userName) == null)
            {
                var user = new IdentityUser
                {
                    UserName = userName,
                    Email = email,
                    PhoneNumber = phoneNumber
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
