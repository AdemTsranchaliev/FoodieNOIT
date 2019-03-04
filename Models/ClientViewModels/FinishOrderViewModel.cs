using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class FinishOrderViewModel
    {
        public FinishOrderViewModel(string name,int amount, decimal pricePerProduct)
        {
            this.Name = name;
            this.Amount = amount;
            this.PricePerProduct = pricePerProduct;
            this.CommonPrice = pricePerProduct * amount;

        }

        public string Name { get; set; }

        public int Amount { get; set; }

        public decimal PricePerProduct { get; set; }

        public decimal CommonPrice { get; set; }
    }
}
