﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GroceryStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly GroceryStoreContext _context;

        public HomeController(GroceryStoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string name, string price, string weight, string conversionCode)
        {
            IEnumerable<Grocery> groceryStoreContext = _context.Grocery.Include(g => g.Conversion);

            if (!string.IsNullOrWhiteSpace(name))
            {
                groceryStoreContext = groceryStoreContext.Where(g => g.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(price))
            {
                groceryStoreContext = groceryStoreContext.Where(g => g.Price.ToString().Contains(price, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(weight))
            {
                groceryStoreContext = groceryStoreContext.Where(g => g.Weight.ToString().Contains(weight, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(conversionCode))
            {
                groceryStoreContext = groceryStoreContext.Where(g => g.Conversion.Code.Contains(conversionCode, StringComparison.CurrentCultureIgnoreCase));
            }

            return Json(groceryStoreContext);
        }

        public async Task<IActionResult> Stock(int id)
        {
            try
            {
                var stock = _context.Stock.Include(s => s.Location).Include(s => s.Location.ProvinceState).Where(s => s.GroceryId == id);
                var grocery = await _context.Grocery.FirstOrDefaultAsync(g => g.GroceryId == id);
                ViewData["Subtitle"] = grocery.Name;

                return View(await stock.ToListAsync());
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
