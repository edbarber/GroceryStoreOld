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
    public class EditAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public EditAccountModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool AllowUsernameEdit { get; set; }

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
                UserName = user.UserName
            };

            ViewData["User"] = user.UserName;
            ViewData["Id"] = user.Id;

            // admin should not be able to edit its default username (this is needed to keep the integrity of the account database) 
            // set it here so we can disable text box on front end if this is false
            AllowUsernameEdit = user.UserName != _configuration.GetSection("AdminDefault").GetSection("UserName").Value;

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
            AllowUsernameEdit = user.UserName != _configuration.GetSection("AdminDefault").GetSection("UserName").Value;

            if (!AllowUsernameEdit && Input.UserName != user.UserName)
            {
                StatusMessage = "Error: editing the username for this admin is forbidden";
                ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.UserName)}", "The Username field cannot be edited.");

                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (AllowUsernameEdit)
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

                StatusMessage = "Profile has been updated";
                return RedirectToPage("./Accounts");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }
        }
    }
}