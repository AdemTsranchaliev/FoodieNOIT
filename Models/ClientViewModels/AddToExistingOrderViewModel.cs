using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class AddToExistingOrderViewModel
    {
        public Order Order { get; set; }

        public int OrderId { get; set; }

        public int RestaurantId { get; set; }

        public List<FinishOrderViewModel> FinishOrders { get; set; }

        public string HiddenValue { get; set; }

        public string RestaurantName { get; set; }

        public List<FinishOrderViewModel> FinishOrderViewModels { get; set; }

        public int TableNumber { get; set; }

        public List<MenuCategory> MenuCategories { get; set; }
    }
}
