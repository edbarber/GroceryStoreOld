using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStore.Models.HomeViewModels
{
    public class GroceriesViewModel
    {
        public string CategoryCode { get; set; }
        public string Search { get; set; }
        public IEnumerable<Grocery> Groceries { get; set; }
    }
}
