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
using Microsoft.Extensions.Logging;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Admin")]
    public class AccountsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountsModel> _logger;

        [TempData]
        public string StatusMessage { get; set; }

        public class OutputModel
        {

            public string Username { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Role { get; set; }
            public string Id { get; set; }
            public bool IdSelected { get; set; }
        }

        public AccountsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AccountsModel> logger, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
        }

        public List<OutputModel> Users { get; set; }

        public IActionResult OnGet()
        {
            Users = (from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId into urGroup
                    from urItem in urGroup.DefaultIfEmpty()
                    join r in _context.Roles on urItem.RoleId equals r.Id into rGroup
                    from rItem in rGroup.DefaultIfEmpty()
                    select new OutputModel
                    {
                        Username = u.UserName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber ?? "N/A",
                        Role = rItem.Name ?? "N/A",
                        Id = u.Id
                    }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                StatusMessage = $"Unable to load user with ID '{id}'.";
                return new JsonResult(string.Empty);
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                await _signInManager.SignOutAsync();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                StatusMessage = $"Unexpected error occurred deleteing user with ID '{id}'.";
                return new JsonResult(string.Empty);
            }

            _logger.LogInformation($"User with ID '{id}' deleted by admin.");
            return new JsonResult(string.Empty);
        }
    }
}