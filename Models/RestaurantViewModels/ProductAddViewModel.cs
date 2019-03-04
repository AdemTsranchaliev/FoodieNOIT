using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class ProductAddViewModel
    {
        [Required(ErrorMessage = "Това поле е задължително")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        [Range(0.0, 1000000,ErrorMessage = "Грамовете трябва да бъдат по-малко от 1000000 и повече от 1")]
        public string Weight { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        [Range(0.0, 1000000, ErrorMessage = "Цената трябва да бъдат по-малко от 1000000 и по-голяма от 1")]
        public string Price { get; set; }

        public string Ingredients { get; set; }

        public List<Ingredients> IngredientsList { get; set; }

        public List<MenuCategory> menuCategories { get; set; }

    }
}
