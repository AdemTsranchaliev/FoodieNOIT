using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class AddProducToExistingOrderViewModel
    {
        public List<MenuCategory> MenuCategories { get; set; }

        public int OrderId { get; set; }

        public string HiddenValue { get; set; }
    }
}
