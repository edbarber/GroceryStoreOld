using System;
using System.Collections.Generic;

namespace GroceryStore.Models
{
    public partial class ProvinceState
    {
        public ProvinceState()
        {
            Location = new HashSet<Location>();
        }

        public int ProvinceStateId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool UseState { get; set; }

        public ICollection<Location> Location { get; set; }
    }
}
