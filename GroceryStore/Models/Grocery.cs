using System;
using System.Collections.Generic;

namespace GroceryStore.Models
{
    public partial class Grocery
    {
        public Grocery()
        {
            Stock = new HashSet<Stock>();
        }

        public int GroceryId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal? Weight { get; set; }
        public int? ConversionId { get; set; }

        public Conversion Conversion { get; set; }
        public ICollection<Stock> Stock { get; set; }
    }
}
