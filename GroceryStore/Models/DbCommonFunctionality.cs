using GroceryStore.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStore.Models
{
    public class DbCommonFunctionality
    {
        private readonly ApplicationDbContext _context;

        public DbCommonFunctionality(DbContextOptions<ApplicationDbContext> options)
        {
            _context = new ApplicationDbContext(options);
        }

        public ApplicationRole GetRoleForUser(string userId)
        {
            return (from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where u.Id == userId
                    select r).FirstOrDefault();
        }
    }
}
