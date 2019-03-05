using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroceryStore.Models;
using GroceryStore.Models.GroceryViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Controllers
{
    public class GroceriesController : Controller
    {
        private readonly GroceryStoreContext _context;

        public GroceriesController(GroceryStoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string categoryCode = null, string search = null, bool? orderPriceFromHighToLow = null, bool? orderPriceFromLowToHigh = null, bool? orderAlphabetically = null)
        {
            // both category code and search are optional, however, either one must exist
            if (string.IsNullOrWhiteSpace(categoryCode) && string.IsNullOrWhiteSpace(search))
            {
                return RedirectToAction("Index", "Home");
            }

            HttpContext.Session.SetString("Search", search?.Trim() ?? string.Empty);    // used to persist search box value in layout page

            categoryCode = categoryCode?.Trim();

            GroceriesViewModel model = new GroceriesViewModel
            {
                Search = search?.Trim(),
                Category = await _context.Category.FirstOrDefaultAsync(c => c.Code == categoryCode),
                OrderPriceFromLowToHigh = orderPriceFromLowToHigh,
                OrderPriceFromHighToLow = orderPriceFromHighToLow,
                OrderAlphabetically = orderAlphabetically,
                Groceries = _context.Grocery.Include(g => g.Conversion).Include(g => g.Category)
            };

            bool searchExists = !string.IsNullOrWhiteSpace(model.Search);   // cache value instead of doing string comparison every category

            if (searchExists)
            {
                model.Groceries = model.Groceries.Where(g => g.Name.Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) ||
                    g.Price.ToString().Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) ||
                    (g.Weight != null ? g.Weight.ToString().Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) : false) ||
                    (g.Conversion != null ? g.Conversion.Code.Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) : false) ||
                    (g.Description != null ? g.Description.Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) : false));
            }

            model.ValidCategories = model.Groceries.GroupBy(g => g.CategoryId).Select(g => new KeyValuePair<Category, int?>(g.FirstOrDefault().Category, searchExists ? g.Count() : (int?)null)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (model.Category != null)
            {
                model.Groceries = model.Groceries.Where(g => g.Category.Code == model.Category.Code);
                ViewData["Title"] = (await _context.Category.FirstOrDefaultAsync(c => c.Code == model.Category.Code)).Name;
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
    }
}