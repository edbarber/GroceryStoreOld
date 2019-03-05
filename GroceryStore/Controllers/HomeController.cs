using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GroceryStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace GroceryStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly GroceryStoreContext _context;

        public HomeController(GroceryStoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _context.Category.Include(c => c.Grocery).Where(c => c.Grocery.Count > 0).ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Stock(int id, string returnURL)
        {
            var stock = _context.Stock.Include(s => s.Location).Include(s => s.Location.ProvinceState).Where(s => s.GroceryId == id);
            var grocery = await _context.Grocery.FirstOrDefaultAsync(g => g.GroceryId == id);

            ViewData["Subtitle"] = grocery.Name;
            ViewData["ReturnURL"] = returnURL;

            return View(await stock.ToListAsync());
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
