using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GroceryStore.Models;
using GroceryStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Policy = "AdminRights")]
    public class EditRoleModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly UIHelper _uiHelper;

        public EditRoleModel(RoleManager<ApplicationRole> roleManager, IConfiguration configuration, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, UIHelper uiHelper)
        {
            _roleManager = roleManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _uiHelper = uiHelper;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public bool AllowRoleEdit { get; set; }
        public List<SelectListItem> Claims { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Role { get; set; }

            [Required]
            [Display(Name = "Permission")]
            public string SelectedClaimType { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound($"Unable to load role with ID '{id}'.");
            }

            Claims = _uiHelper.GetClaimsAsSelectListItems();

            Input = new InputModel
            {
                Role = role.Name,
                SelectedClaimType = (await _roleManager.GetClaimsAsync(role)).Select(c => c.Type).FirstOrDefault()
            };

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

            Claims = _uiHelper.GetClaimsAsSelectListItems();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            role.Name = Input.Role;

            var updateResult = await _roleManager.UpdateAsync(role);

            if (updateResult.Succeeded)
            {
                Claim oldClaim = (await _roleManager.GetClaimsAsync(role)).FirstOrDefault();

                // don't remove and add same claim
                if (oldClaim == null && Input.SelectedClaimType != "N/A" || oldClaim != null && Input.SelectedClaimType != oldClaim.Type)
                {
                    // don't remove a claim that doesn't exist because N/A is not a claim
                    if (oldClaim != null)
                    {
                        updateResult = await _roleManager.RemoveClaimAsync(role, oldClaim);
                    }

                    if (Input.SelectedClaimType != "N/A")
                    {
                        if (updateResult.Succeeded)
                        {
                            updateResult = await _roleManager.AddClaimAsync(role, new Claim(Input.SelectedClaimType, "true"));

                            if (!updateResult.Succeeded)
                            {
                                StatusMessage = "Error: Role has been updated but the permission has been removed and not updated. Try editing the role again.";
                            }
                        }
                        else
                        {
                            StatusMessage = "Error: Role has been updated but the permission hasn't been updated. Try editing the role again.";
                        }
                    }
                }

                // check again as result may have changed depending if claim was updated or not
                if (updateResult.Succeeded)
                {
                    StatusMessage = "Role has been updated";
                }

                await _signInManager.RefreshSignInAsync(await _userManager.GetUserAsync(User));
                return RedirectToPage("Roles");
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
                return RedirectToPage("Roles");
            }

            return null;
        }
    }
}