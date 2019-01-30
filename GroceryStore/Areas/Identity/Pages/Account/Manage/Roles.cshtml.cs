using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroceryStore.Data;
using GroceryStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Admin")]
    public class RolesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RolesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public class OutputModel
        {
            public string Role { get; set; }
            public List<ApplicationUser> Users { get; set; }
        }

        public List<OutputModel> RolesAndUsers { get; set; }

        public IActionResult OnGet()
        {
            List<OutputModel> output = new List<OutputModel>();
            List<ApplicationRole> roles = _context.Roles.ToList();

            foreach (ApplicationRole role in roles)
            {
                output.Add(new OutputModel
                {
                    Role = role.Name,
                    Users = (from u in _context.Users
                             join ur in _context.UserRoles on u.Id equals ur.UserId
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where r.Id == role.Id
                             select u).ToList()
                });
            }

            RolesAndUsers = output;

            return Page();
        }
    }
}