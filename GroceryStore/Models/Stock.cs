using System;
using System.Collections.Generic;

namespace GroceryStore.Models
{
    public partial class Stock
    {
        public int StockId { get; set; }
        public int LocationId { get; set; }
        public int GroceryId { get; set; }
        public int Quantity { get; set; }

        public Grocery Grocery { get; set; }
        public Location Location { get; set; }
    }
}
