using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GroceryStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Policy = "Admin")]
    public class EditRoleModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public EditRoleModel(RoleManager<ApplicationRole> roleManager, IConfiguration configuration, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [Required]
        [BindProperty]
        public string Role { get; set; }

        public bool AllowRoleEdit { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound($"Unable to load role with ID '{id}'.");
            }

            Role = role.Name;
            ViewData["Role"] = role.Name;
            IActionResult result = SetAndCheckForbidden(role.Name);

            if (result != null)
            {
                return result;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound($"Unable to load role with ID '{id}'.");
            }

            ViewData["Role"] = role.Name;

            IActionResult forbiddenResult = SetAndCheckForbidden(role.Name);

            if (forbiddenResult != null)
            {
                return forbiddenResult;
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            role.Name = Role;
            var updateResult = await _roleManager.UpdateAsync(role);

            if (updateResult.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(await _userManager.GetUserAsync(User));

                StatusMessage = "Role has been updated";
                return RedirectToPage("./Roles");
            }
            else
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }
        }

        private IActionResult SetAndCheckForbidden(string roleName)
        {
            // doing this in both get and post prevents ajax calls from bypassing edit
            AllowRoleEdit = roleName != _configuration.GetSection("AdminRole").Value;

            if (!AllowRoleEdit)
            {
                StatusMessage = "Error: Editing this role is forbidden.";
                return RedirectToPage("./Roles");
            }

            return null;
        }
    }
}