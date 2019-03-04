using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models
{
    public class Ingredients
    {
       
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ProductIngredients> ProductIngredients { get; set; }
    }
}
