using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class WaiterNewOrderViewModel
    {
        public List<MenuCategory> MenuCategories { get; set; }

        public List<FinishOrderViewModel> FinishOrderViewModels { get; set; }

        public string HiddenVal { get; set; }

        public List<string> AddedBeforeProducts { get; set; }

        public bool EditOrder { get; set; }

        [Required(ErrorMessage = "Изберете маса")]
        public int TableNumber { get; set; }
    }
}
