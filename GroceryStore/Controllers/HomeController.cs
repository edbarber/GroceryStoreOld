using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GroceryStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using GroceryStore.Models.HomeViewModels;
using GroceryStore.Services;

namespace GroceryStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly GroceryStoreContext _context;
        private readonly FileUtility _fileUtility;

        public HomeController(GroceryStoreContext context, FileUtility fileUtility)
        {
            _context = context;
            _fileUtility = fileUtility;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _context.Category.Include(c => c.Grocery)/*.Where(c => c.Grocery.Count > 0)*/.ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Stock(int id, string returnURL)
        {
            var stock = _context.Stock.Include(s => s.Location).Include(s => s.Location.ProvinceState).Where(s => s.GroceryId == id);
            var grocery = await _context.Grocery.FirstOrDefaultAsync(g => g.GroceryId == id);

            if (grocery == null)
            {
                return NotFound();
            }

            ViewData["Subtitle"] = grocery.Name;
            ViewData["ReturnURL"] = returnURL;

            return View(await stock.ToListAsync());
        }

        [Authorize(Policy = "ManagerialRights")]
        public IActionResult AddCategory()
        {
            CategoryViewModel model = new CategoryViewModel
            {
                Category = new Category()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerialRights")]
        public async Task<IActionResult> AddCategory(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Category.Name = model.Category.Name.Trim();
                model.Category.Code = model.Category.Code.Trim().ToUpper();
                model.Category.ImageAlt = model.Category.ImageAlt?.Trim();

                if (_context.Category.Any(c => c.Name.ToLower() == model.Category.Name.ToLower()))
                {
                    ModelState.AddModelError($"{nameof(Category)}.{nameof(Category.Name)}", "This name already exists.");
                }

                if (_context.Category.Any(c => c.Code == model.Category.Code))
                {
                    ModelState.AddModelError($"{nameof(Category)}.{nameof(Category.Code)}", "This code already exists.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (model.Image != null)
                {
                    model.Category.ImageUrl = await _fileUtility.UploadImageAsync(model.Image);
                }

                _context.Category.Add(model.Category);
                await _context.SaveChangesAsync();

                TempData["StatusMessage"] = $"{model.Category.Name} has been successfully added.";

                return RedirectToAction("Index", "Home");
            }

            return View(model);
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
