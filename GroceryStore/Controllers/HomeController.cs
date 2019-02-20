﻿using System;
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

        public IActionResult Index(string search = null, bool orderPriceFromHighToLow = false, bool orderPriceFromLowToHigh = false, bool orderAlphabetically = false)
        {
            IndexViewModel model = new IndexViewModel
            {
                Search = search?.Trim(),
                Groceries = _context.Grocery.Include(g => g.Conversion)
            };

            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                model.Groceries = model.Groceries.Where(g => g.Name.Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) ||
                    g.Price.ToString().Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) ||
                    (g.Weight != null ? g.Weight.ToString().Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) : false) ||
                    (g.Conversion != null ? g.Conversion.Code.Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) : false) ||
                    (g.Description != null ? g.Description.Contains(model.Search, StringComparison.CurrentCultureIgnoreCase) : false));
            }

            if (orderPriceFromHighToLow)
            {
                model.Groceries = model.Groceries.OrderByDescending(g => g.Price);
            }
            else if (orderPriceFromLowToHigh)
            {
                model.Groceries = model.Groceries.OrderBy(g => g.Price);
            }
            else if (orderAlphabetically)
            {
                model.Groceries = model.Groceries.OrderBy(g => g.Name);
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
