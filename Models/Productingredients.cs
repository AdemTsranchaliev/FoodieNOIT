using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models
{
    public class ProductIngredients
    {
        public int IngredientId { get; set; }
        public virtual Ingredients Ingredients { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
