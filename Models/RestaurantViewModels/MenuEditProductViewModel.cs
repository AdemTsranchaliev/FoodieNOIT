using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class MenuEditProductViewModel

    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string Name { get; set; }



        [Required(ErrorMessage = "Това поле е задължително")]
        public decimal Weight { get; set; }

        [Range(0.0, 1000000, ErrorMessage = "Грамовете трябва да бъдат по-малко от 1000000 и повече от 1")]
        public decimal Price { get; set; }

        public string Description { get; set; }

        public int? MenuCategoryId { get; set; }

        public string CategoryName { get; set; }

        public string Ingredients { get; set; }

        public int? categoryId { get; set; }

        public int? productId { get; set; }


    }
}
