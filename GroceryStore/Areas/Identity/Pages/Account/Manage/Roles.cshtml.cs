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

        [TempData]
        public string StatusMessage { get; set; }

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
                    Users = GetUsers(role.Id)
                });
            }

            RolesAndUsers = output;

            return Page();
        }

        public IActionResult OnPost(string role, string user)
        {
            List<OutputModel> output = new List<OutputModel>();
            List<ApplicationRole> roles = _context.Roles.ToList();

            if (!string.IsNullOrWhiteSpace(role))
            {
                roles = roles.Where(ar => ar.Name.Contains(role, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            foreach (ApplicationRole currRole in roles)
            {
                OutputModel outputModel = new OutputModel
                {
                    Role = currRole.Name,
                    Users = GetUsers(currRole.Id)
                };

                if (!string.IsNullOrWhiteSpace(user))
                {
                    outputModel.Users = outputModel.Users.Where(au => au.UserName.Contains(user, StringComparison.CurrentCultureIgnoreCase)).ToList();
                }

                output.Add(outputModel);
            }

            return new JsonResult(output);
        }

        private List<ApplicationUser> GetUsers(string roleId)
        {
            return (from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where r.Id == roleId
                    select u).ToList();
        }
    }
}