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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Policy = "Admin")]
    public class EditAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly DbCommonFunctionality _dbCommonFunctionality;

        public EditAccountModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<ApplicationRole> roleManager, ApplicationDbContext context, DbCommonFunctionality dbCommonFunctionality)
        {               
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _dbCommonFunctionality = dbCommonFunctionality;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool AllowUsernameAndRoleEdit { get; set; }
        public bool AllowRoleEdit { get; set; }
        public List<SelectListItem> Roles { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Required]
            [Display(Name = "Role")]
            public string SelectedRoleId { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }

            Input = new InputModel
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                SelectedRoleId = _dbCommonFunctionality.GetRoleForUser(id)?.Id
            };

            Roles = _roleManager.Roles.OrderBy(ar => ar.Name).Select(ar => new SelectListItem(ar.Name, ar.Id)).ToList();

            ViewData["User"] = user.UserName;
            ViewData["Id"] = user.Id;

            // admin should not be able to edit its default username (this is needed to keep the integrity of the account database) 
            // set it here so we can disable text box on front end if this is false
            AllowUsernameAndRoleEdit = user.UserName != _configuration.GetSection("AdminDefault").GetSection("UserName").Value;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }

            ViewData["User"] = user.UserName;
            ViewData["Id"] = user.Id;

            // set it again as we don't want to expose this on the front end (user can change) and this gets set back to false alone after page is loaded
            AllowUsernameAndRoleEdit = user.UserName != _configuration.GetSection("AdminDefault").GetSection("UserName").Value;

            // list gets cleared after page load so recreate it
            Roles = _roleManager.Roles.OrderBy(ar => ar.Name).Select(ar => new SelectListItem(ar.Name, ar.Id)).ToList();

            var roleToRemove = _dbCommonFunctionality.GetRoleForUser(user.Id);

            if (!AllowUsernameAndRoleEdit)
            {
                bool error = false; // flag to check if there has been an error (want to output all errors incase first if statement validates as true)

                if (Input.UserName != user.UserName)
                {
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.UserName)}", "The Username field cannot be edited.");
                    error = true;
                }

                if (Input.SelectedRoleId != roleToRemove?.Id)
                {
                    ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.SelectedRoleId)}", "The role cannot be selected.");
                    error = true;
                }

                if (error)
                {
                    StatusMessage = "Critical error happened! Please check messages below.";
                    return Page();
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (AllowUsernameAndRoleEdit)
            {
                user.UserName = Input.UserName;
            }

            user.Email = Input.Email;
            user.PhoneNumber = Input.PhoneNumber;
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(await _userManager.GetUserAsync(User));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();  // don't bother continuing
            }

            if (roleToRemove != null)
            {
                result = await _userManager.RemoveFromRoleAsync(user, roleToRemove.Name);

                if (!result.Succeeded)
                {
                    StatusMessage = $"Error: Profile has been updated, however, the role wasn't updated. Please try editing the role for this user again.";
                    return RedirectToPage("./EditAccount", new { id = user.Id });
                }
                else
                {
                    await _signInManager.RefreshSignInAsync(await _userManager.GetUserAsync(User));
                }
            }

            var roleToAddTo = await _roleManager.FindByIdAsync(Input.SelectedRoleId);
            string roleError = string.Empty;

            try
            {
                result = await _userManager.AddToRoleAsync(user, roleToAddTo.Name);
            }
            catch 
            {
                roleError = "Critical Error!";
            }

            if (roleError != string.Empty || !result.Succeeded)
            {
                StatusMessage = $"Error: {roleError} Profile has been updated, however, the role doesn't exist anymore. Please try updating the role for this user.";
                return RedirectToPage("./EditAccount", new { id = user.Id });
            }
            else
            {
                StatusMessage = $"Profile {user.UserName} has been updated successfully!";
                await _signInManager.RefreshSignInAsync(await _userManager.GetUserAsync(User));
                return RedirectToPage("./Accounts");
            }
        }
    }
}