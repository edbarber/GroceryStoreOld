using System.Threading.Tasks;
using GroceryStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GroceryStore.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PersonalDataModel> _logger;
        private readonly IConfiguration _configuration;

        public bool AllowDelete { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public PersonalDataModel(
            UserManager<ApplicationUser> userManager,
            ILogger<PersonalDataModel> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            AllowDelete = user.UserName != _configuration.GetSection("AdminDefault").GetSection("UserName").Value;

            return Page();
        }
    }
}