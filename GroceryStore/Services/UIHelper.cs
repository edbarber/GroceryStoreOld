using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStore.Services
{
    public class UIHelper
    {
        private readonly IConfiguration _configuration;

        public UIHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<SelectListItem> GetClaimsAsSelectListItems()
        {
            List<SelectListItem> claims = new List<SelectListItem>();
            claims.Add(new SelectListItem
            {
                Value = null,
                Text = "Not applicable"
            });

            claims.AddRange(_configuration.GetSection("Claims").GetChildren().Select(c => new SelectListItem
            {
                Value = c.GetSection("Identifier").Value,
                Text = c.GetSection("Meaning").Value
            }));

            return claims;
        }
    }
}
