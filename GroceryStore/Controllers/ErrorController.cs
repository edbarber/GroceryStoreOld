using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GroceryStore.Models.ErrorViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace GroceryStore.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            IndexViewModel model = new IndexViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            return View(model);
        }

        public IActionResult ErrorCode()
        {
            int errorCode = HttpContext.Response.StatusCode;

            ErrorCodeViewModel model = new ErrorCodeViewModel
            {
                ErrorCode = errorCode,
                ErrorDescription = ReasonPhrases.GetReasonPhrase(errorCode)
            };

            return View(model);
        }
    }
}