using Microsoft.AspNetCore.Identity;
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
                          RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            if (await roleManager.FindByNameAsync("Admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (await userManager.FindByNameAsync("admin") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@grocerystore.com",
                    PhoneNumber = "(905) 123-4567"
                };

                var result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    await userManager.AddPasswordAsync(user, "Test123#");
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
