using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GroceryStore.Models;
using GroceryStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    public class AddRoleModel : PageModel
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<AddAccountModel> _logger;
        private readonly UIHelper _uiHelper;

        public AddRoleModel(RoleManager<ApplicationRole> roleManager, ILogger<AddAccountModel> logger, DbCommonFunctionality dbCommonFunctionality, UIHelper uiHelper)
        {
            _roleManager = roleManager;
            _logger = logger;
            _uiHelper = uiHelper;
        }

        [TempData]
        public string StatusMessage { get; set; }

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

        public void OnGet()
        {
            Claims = _uiHelper.GetClaimsAsSelectListItems();
            Input = new InputModel();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = Input.Role
                });

                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin created a new role.");

                    ApplicationRole createdRole = await _roleManager.FindByNameAsync(Input.Role);

                    if (Input.SelectedClaimType != "N/A")  
                    {
                        result = await _roleManager.AddClaimAsync(createdRole, new Claim(Input.SelectedClaimType, "true"));

                        if (result.Succeeded)
                        {
                            _logger.LogInformation($"Assigned {Input.SelectedClaimType} claim to {Input.Role} role.");
                        }
                        else
                        {
                            StatusMessage = "Error: Role has been added but the claim wasn't assigned. Try editing the role and assigning the claim.";
                        }
                    }

                    if (result.Succeeded)   // check again as result may have changed depending if claim was added or not
                    {
                        StatusMessage = "Role has been added";
                    }

                    return RedirectToPage("Roles");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            Claims = _uiHelper.GetClaimsAsSelectListItems();

            return Page();
        }
    }
}