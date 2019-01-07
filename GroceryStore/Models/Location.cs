using System;
using System.Collections.Generic;

namespace GroceryStore.Models
{
    public partial class Location
    {
        public Location()
        {
            Stock = new HashSet<Stock>();
        }

        public int LocationId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public int ProvinceStateId { get; set; }
        public string PostalCode { get; set; }

        public ProvinceState ProvinceState { get; set; }
        public ICollection<Stock> Stock { get; set; }
    }
}
