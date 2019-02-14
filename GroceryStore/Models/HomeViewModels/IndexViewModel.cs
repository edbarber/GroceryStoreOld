using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStore.Models.HomeViewModels
{
    public class IndexViewModel
    {
        public string SearchName { get; set; }
        public string SearchPrice { get; set; }
        public string SearchWeight { get; set; }
        public string SearchConversionCode { get; set; }
        public IEnumerable<Grocery> Groceries { get; set; }
    }
}
