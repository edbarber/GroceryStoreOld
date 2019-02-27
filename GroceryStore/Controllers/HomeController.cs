using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GroceryStore.Models;
using Microsoft.EntityFrameworkCore;
using GroceryStore.Models.HomeViewModels;
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
            try
            {
                List<Category> categories = await _context.Category.Include(c => c.Grocery).Where(c => c.Grocery.Count > 0).ToListAsync();
                return View(categories);
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public async Task<IActionResult> Groceries(string categoryCode = null, string search = null, bool? orderPriceFromHighToLow = null, bool? orderPriceFromLowToHigh = null, bool? orderAlphabetically = null)
        {
            try
            {
                // both category code and search are optional, however, either one must exist
                if (string.IsNullOrWhiteSpace(categoryCode) && string.IsNullOrWhiteSpace(search))
                {
                    return RedirectToAction("Index", "Home");
                }

                HttpContext.Session.SetString("Search", search?.Trim() ?? string.Empty);    // used to persist search box value in layout page

                GroceriesViewModel model = new GroceriesViewModel
                {
                    Search = search?.Trim(),
                    CategoryCode = categoryCode?.Trim(),
                    OrderPriceFromLowToHigh = orderPriceFromLowToHigh,
                    OrderPriceFromHighToLow = orderPriceFromHighToLow,
                    OrderAlphabetically = orderAlphabetically,
                    Groceries = _context.Grocery.Include(g => g.Conversion).Include(g => g.Category)
                };

                bool searchExists = !string.IsNullOrWhiteSpace(model.Search);

                if (searchExists)
                {
                    model.Groceries = model.Groceries.Where(g => g.Name.Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) ||
                        g.Price.ToString().Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) ||
                        (g.Weight != null ? g.Weight.ToString().Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) : false) ||
                        (g.Conversion != null ? g.Conversion.Code.Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) : false) ||
                        (g.Description != null ? g.Description.Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) : false));
                }

                model.ValidCategories = model.Groceries.GroupBy(g => g.CategoryId).Select(g => new KeyValuePair<Category, int?>(g.FirstOrDefault().Category, searchExists ? g.Count() : (int?)null)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                  
                if (!string.IsNullOrWhiteSpace(model.CategoryCode))
                {
                    model.Groceries = model.Groceries.Where(g => g.Category.Code == model.CategoryCode);
                    ViewData["Title"] = (await _context.Category.FirstOrDefaultAsync(c => c.Code == model.CategoryCode)).Name;
                }

                // there will only be one passed in anyway
                if (orderPriceFromHighToLow == true)
                {
                    model.Groceries = model.Groceries.OrderByDescending(g => g.Price);
                }
                else if (orderPriceFromLowToHigh == true)
                {
                    model.Groceries = model.Groceries.OrderBy(g => g.Price);
                }
                else if (orderAlphabetically == true)
                {
                    model.Groceries = model.Groceries.OrderBy(g => g.Name);
                }

                return View(model);
            }
            catch
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public async Task<IActionResult> Stock(int id, string returnURL)
        {
            try
            {
                var stock = _context.Stock.Include(s => s.Location).Include(s => s.Location.ProvinceState).Where(s => s.GroceryId == id);
                var grocery = await _context.Grocery.FirstOrDefaultAsync(g => g.GroceryId == id);

                ViewData["Subtitle"] = grocery.Name;
                ViewData["ReturnURL"] = returnURL;

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
