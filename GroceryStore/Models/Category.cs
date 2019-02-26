using System;
using System.Collections.Generic;

namespace GroceryStore.Models
{
    public partial class Category
    {
        public Category()
        {
            Grocery = new HashSet<Grocery>();
        }

        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ImageUrl { get; set; }
        public string ImageAlt { get; set; }

        public ICollection<Grocery> Grocery { get; set; }
    }
}
