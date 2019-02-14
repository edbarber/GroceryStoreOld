using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<OutputModel> Roles { get; set; }

        public class InputModel
        {
            [Display(Name = "Search role")]
            public string SearchRole { get; set; }

            [Display(Name = "Search user")]
            public string SearchUser { get; set; }
        }

        public class OutputModel
        {
            public ApplicationRole Role { get; set; }
            public bool AllowEditAndDelete { get; set; }
            public List<ApplicationUser> Users { get; set; }
        }

        public void OnGet()
        {
            List<OutputModel> output = new List<OutputModel>();
            List<ApplicationRole> roles = _roleManager.Roles.OrderBy(ar => ar.Name).ToList();

            foreach (ApplicationRole currRole in roles)
            {
                OutputModel outputModel = new OutputModel
                {
                    Role = currRole,
                    // admin should not be able to delete the admin role (this is needed to keep the integrity of the account database) 
                    // set it here so we can disable button on front end if this is false
                    AllowEditAndDelete = currRole.Name != _configuration.GetSection("AdminRole").Value,
                    Users = GetUsersByRole(currRole.Id)
                };

                output.Add(outputModel);
            }

            Roles = output;
        }

        public IActionResult OnPostSearch()
        {
            List<OutputModel> output = new List<OutputModel>();
            List<ApplicationRole> roles = _context.Roles.ToList();

            if (!string.IsNullOrWhiteSpace(Input.SearchRole))
            {
                roles = roles.Where(ar => ar.Name.Contains(Input.SearchRole, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            foreach (ApplicationRole currRole in roles)
            {
                OutputModel outputModel = new OutputModel
                {
                    Role = currRole,
                    // admin should not be able to delete the admin role (this is needed to keep the integrity of the account database) 
                    // set it here so we can disable button on front end if this is false
                    AllowEditAndDelete = currRole.Name != _configuration.GetSection("AdminRole").Value,
                    Users = GetUsersByRole(currRole.Id)
                };

                if (!string.IsNullOrWhiteSpace(Input.SearchUser))
                {
                    outputModel.Users = outputModel.Users.Where(au => au.UserName.Contains(Input.SearchUser, StringComparison.CurrentCultureIgnoreCase)).ToList();
                }

                output.Add(outputModel);
            }

            Roles = output;

            return Page();
        }

        public async Task<JsonResult> OnPostDeleteAsync(string id)
        {
            JsonResult page = new JsonResult(Url.Page("Roles"));

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                StatusMessage = $"Unable to load role with ID '{id}'.";
                return page;
            }

            // check again if role can be deleted by admin (user shouldn't be able to remove disabled attribute from delete button)
            // as we cannot persist the allow delete boolean without compromising security after page load
            if (role.Name == _configuration.GetSection("AdminRole").Value)
            {
                StatusMessage = $"Error: deleting this admin role is forbidden.";
                return page;
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                StatusMessage = $"Unexpected error occurred deleteing role with ID '{id}'.";

                return page;
            }

            await _signInManager.RefreshSignInAsync(await _userManager.GetUserAsync(User));
            _logger.LogInformation($"Role with ID '{id}' deleted by admin.");

            StatusMessage = $"Role has been removed";

            return page;
        }

        private List<ApplicationUser> GetUsersByRole(string roleId)
        {
            return (from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where r.Id == roleId
                    select u).ToList();
        }
    }
}