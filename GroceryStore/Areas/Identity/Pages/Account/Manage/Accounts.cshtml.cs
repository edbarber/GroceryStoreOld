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

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Admin")]
    public class AccountsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public OutputModel Output { get; set; }

        public class OutputModel
        {
            public string Username { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Role { get; set; }
        }

        public AccountsModel(ApplicationDbContext context)
        {
            _context = context;
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
                        Role = rItem.Name ?? "N/A"
                    }).ToList();

            return Page();
        }
    }
}