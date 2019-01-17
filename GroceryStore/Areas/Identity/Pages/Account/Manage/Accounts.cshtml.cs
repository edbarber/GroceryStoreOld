using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroceryStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    public class AccountsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountsModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public List<ApplicationUser> Users { get; set; }

        public IActionResult OnGet()
        {
            var user = User;

            Users = _userManager.Users.ToList();

            return Page();
        }
    }
}