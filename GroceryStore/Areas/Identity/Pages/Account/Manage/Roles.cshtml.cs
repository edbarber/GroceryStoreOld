using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroceryStore.Data;
using GroceryStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Policy = "Admin")]
    public class RolesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RolesModel> _logger;
        private readonly IConfiguration _configuration; 

        public RolesModel(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<RolesModel> logger, IConfiguration configuration)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
        }

        public class OutputModel
        {
            public ApplicationRole Role { get; set; }
            public bool AllowEditAndDelete { get; set; }
            public List<ApplicationUser> Users { get; set; }
        }

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult OnPostSearch(string role, string user)
        {
            List<OutputModel> output = new List<OutputModel>();
            List<ApplicationRole> roles = _context.Roles.ToList();

            if (!string.IsNullOrWhiteSpace(role))
            {
                roles = roles.Where(ar => ar.Name.Contains(role, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            foreach (ApplicationRole currRole in roles)
            {
                OutputModel outputModel = new OutputModel
                {
                    Role = currRole,
                    // admin should not be able to delete the admin role (this is needed to keep the integrity of the account database) 
                    // set it here so we can disable button on front end if this is false
                    AllowEditAndDelete = currRole.Name != _configuration.GetSection("AdminRole").Value,
                    Users = GetUsers(currRole.Id)
                };

                if (!string.IsNullOrWhiteSpace(user))
                {
                    outputModel.Users = outputModel.Users.Where(au => au.UserName.Contains(user, StringComparison.CurrentCultureIgnoreCase)).ToList();
                }

                output.Add(outputModel);
            }

            return new JsonResult(output);
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                StatusMessage = $"Unable to load role with ID '{id}'.";
                return new JsonResult(string.Empty);
            }

            // check again if role can be deleted by admin (user shouldn't be able to remove disabled attribute from delete button)
            // as we cannot persist the allow delete boolean without compromising security after page load
            if (role.Name == _configuration.GetSection("AdminRole").Value)
            {
                StatusMessage = $"Error: deleting this admin role is forbidden.";
                return new JsonResult(string.Empty);
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                StatusMessage = $"Unexpected error occurred deleteing role with ID '{id}'.";

                return new JsonResult(string.Empty);
            }

            await _signInManager.RefreshSignInAsync(await _userManager.GetUserAsync(User));
            _logger.LogInformation($"Role with ID '{id}' deleted by admin.");

            StatusMessage = $"Role have been removed";

            return new JsonResult(string.Empty);
        }

        private List<ApplicationUser> GetUsers(string roleId)
        {
            return (from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where r.Id == roleId
                    select u).ToList();
        }
    }
}