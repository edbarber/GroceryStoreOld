using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GroceryStore.Models;
using Microsoft.EntityFrameworkCore;
using GroceryStore.Models.HomeViewModels;

namespace GroceryStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly GroceryStoreContext _context;

        public HomeController(GroceryStoreContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchName, string searchPrice, string searchWeight, string searchConversionCode)
        {
            IndexViewModel model = new IndexViewModel
            {
                Groceries = _context.Grocery.Include(g => g.Conversion),
                SearchName = searchName,
                SearchPrice = searchPrice,
                SearchWeight = searchWeight,
                SearchConversionCode = searchConversionCode

            };

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                model.Groceries = model.Groceries.Where(g => g.Name.Contains(searchName, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchPrice))
            {
                model.Groceries = model.Groceries.Where(g => g.Price.ToString().Contains(searchPrice, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchWeight))
            {
                model.Groceries = model.Groceries.Where(g => g.Weight.ToString().Contains(searchWeight, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchConversionCode))
            {
                model.Groceries = model.Groceries.Where(g => g.Conversion.Code.Contains(searchConversionCode, StringComparison.CurrentCultureIgnoreCase));
            }

            return View(model);
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
