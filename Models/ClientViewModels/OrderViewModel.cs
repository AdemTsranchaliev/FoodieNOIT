using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class OrderViewModel
    {
        public OrderViewModel(List<MenuCategory> menuCategories,int restaurantId,string restaurantName,int tableNumber)
        {
            this.MenuCategories = menuCategories;
            this.RestaurantId = restaurantId;
            this.RestaurantName = restaurantName;
            this.TableNumber = tableNumber;
        }
        public OrderViewModel()
        {

        }

        public int TableNumber { get; set; }

        public List<MenuCategory> MenuCategories { get; set; }

        public string QrCode{ get; set; }

        public string HiddenValue { get; set; }

        public List<FinishOrderViewModel> FinishOrderViewModels { get; set; }

        public bool EditOrder { get; set; }

        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; }
    }
}
