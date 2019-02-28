using GroceryStore.Data;
using GroceryStore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStore.Services
{
    public class DbCommonFunctionality
    {
        private readonly ApplicationDbContext _context;

        public DbCommonFunctionality(DbContextOptions<ApplicationDbContext> options)
        {
            _context = new ApplicationDbContext(options);
        }

        public List<ApplicationUser> GetUsersByRoleId(string roleId)
        {
            return (from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where r.Id == roleId
                    select u).ToList();
        }

        public ApplicationRole GetRoleByUserId(string userId)
        {
            return (from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where u.Id == userId
                    select r).FirstOrDefault();
        }
    }
}
