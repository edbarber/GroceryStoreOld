using System;
using System.Collections.Generic;

namespace GroceryStore.Models
{
    public partial class Conversion
    {
        public Conversion()
        {
            Grocery = new HashSet<Grocery>();
        }

        public int ConversionId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public ICollection<Grocery> Grocery { get; set; }
    }
}
