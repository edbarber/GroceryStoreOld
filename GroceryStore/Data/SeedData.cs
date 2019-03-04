using GroceryStore.Areas.Identity.Pages.Account;
using GroceryStore.Models;
using GroceryStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

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

        /// <summary>
        /// Initializes the default settings related to users and roles. Also performs integrity checks against the exisiting default users and roles.
        /// </summary>
        public static async Task Initialize(ApplicationDbContext context,
                          UserManager<ApplicationUser> userManager,
                          RoleManager<ApplicationRole> roleManager,
                          IConfiguration configuration,
                          ILogger<RegisterModel> logger, 
                          DbCommonFunctionality dbCommonFunctionality)
        {
            context.Database.EnsureCreated();

            var administration = configuration.GetSection("AdminDefault");

            string userName = administration.GetSection("UserName").Value;
            string email = administration.GetSection("Email").Value;
            string phoneNumber = administration.GetSection("PhoneNumber").Value;
            string adminRole = configuration.GetSection("AdminRole").Value;
            string password = administration.GetSection("Password").Value;
            string firstName = administration.GetSection("FirstName").Value;
            string lastName = administration.GetSection("LastName").Value;
            string defaultRole = configuration.GetSection("DefaultRole").Value;

            // create the role associated to the default admin if it doesn't exist
            if (await roleManager.FindByNameAsync(adminRole) == null)
            {
                var result = await roleManager.CreateAsync(new ApplicationRole(adminRole));

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

            ApplicationRole createdAdminRole = await roleManager.FindByNameAsync(adminRole);
            Claim newAdminClaim = new Claim(configuration.GetSection("Claims").GetSection("AdminClaim").GetSection("Identifier").Value, "true"); 
            Claim existingAdminClaim = (await roleManager.GetClaimsAsync(createdAdminRole)).FirstOrDefault();

            // if existing admin claim is incorrect 
            if (existingAdminClaim != null)
            {
                if (existingAdminClaim.Type != newAdminClaim.Type || existingAdminClaim.Value != newAdminClaim.Value)
                {
                    // remove the incorrect admin claim
                    var result = await roleManager.RemoveClaimAsync(createdAdminRole, existingAdminClaim);

                    if (result.Succeeded)
                    {
                        existingAdminClaim = null;  // make sure we add back the correct claim next
                        logger.LogInformation("Removed incorrect admin claim.");
                    }
                    else
                    {
                        LogErrors(result.Errors, logger);

                        return;
                    }
                }
            }

            // create and assign the admin claim to the admin role if the admin claim doesn't exist
            if (existingAdminClaim == null)
            {
                var result = await roleManager.AddClaimAsync(createdAdminRole, newAdminClaim);

                if (result.Succeeded)
                {
                    logger.LogInformation("Created admin claim.");
                }
                else
                {
                    LogErrors(result.Errors, logger);

                    return;
                }
            }

            // create the default role if it doesn't exist
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

            var user = await userManager.FindByNameAsync(userName); // find admin by default admin username

            if (user == null)
            {
                // if couldn't find admin by default admin username then create it
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    FirstName = firstName,
                    LastName = lastName
                };

                // add password associated to default admin
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    logger.LogInformation("Created default admin account with password.");

                    // find admin by default admin username again as a new id was created for this user so we need to get the user info again
                    user = await userManager.FindByNameAsync(userName); 
                }
                else
                {
                    LogErrors(result.Errors, logger);

                    return;
                }
            }

            // find current role associated to default admin
            var role = dbCommonFunctionality.GetRoleByUserId(user.Id);

            // if role is incorrect
            if (role != null && role.Name != adminRole)
            {
                // remove that incorrect role from the associated default admin
                IdentityResult result = await userManager.RemoveFromRoleAsync(user, role.Name);

                if (result.Succeeded)
                {
                    logger.LogInformation("Invalid role removed from default admin account.");
                    role = null;    // role instance needs to be updated
                }
                else
                {
                    LogErrors(result.Errors, logger);

                    return;
                }
            }

            // if there's no role associated to default admin
            if (role == null)
            {
                // add the correct role to the default admin
                IdentityResult result = await userManager.AddToRoleAsync(user, adminRole);

                if (result.Succeeded)
                {
                    logger.LogInformation("Admin role added to default admin account.");
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
