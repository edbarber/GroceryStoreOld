using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GroceryStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Policy = "Admin")]
    public class AddAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<AddAccountModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AddAccountModel(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, ILogger<AddAccountModel> logger, IEmailSender emailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

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

            [Display(Name = "Phone number")]
            [Phone]
            public string PhoneNumber { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async void OnGetAsync()
        {
            Roles = _roleManager.Roles.OrderBy(ar => ar.Name).Select(ar => new SelectListItem(ar.Name, ar.Id)).ToList();

            Input = new InputModel
            {
                SelectedRoleId = (await _roleManager.FindByNameAsync(_configuration.GetSection("DefaultRole").Value))?.Id
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                string roleError = string.Empty;

                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin created a new account with password.");

                    var roleToAddTo = await _roleManager.FindByIdAsync(Input.SelectedRoleId);

                    try
                    {
                        result = await _userManager.AddToRoleAsync(user, roleToAddTo?.Name);
                    }
                    catch 
                    {
                        roleError = "Critical error!";
                    }

                    if (result.Succeeded && roleError == string.Empty)
                    {
                        _logger.LogInformation($"{roleToAddTo.Name} role added to user account.");
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                if (await _userManager.FindByNameAsync(user.UserName) != null)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (roleError == string.Empty && result.Succeeded)
                    {
                        StatusMessage = "Profile has been added";
                    }
                    else
                    {
                        StatusMessage = $"Error: {roleError} You will need to reassign the role to user {user.UserName}";
                    }

                    return Redirect("./Accounts");
                }
            }

            // Roles need to be repopulated
            Roles = _roleManager.Roles.OrderBy(ar => ar.Name).Select(ar => new SelectListItem(ar.Name, ar.Id)).ToList();

            return Page();
        }
    }
}