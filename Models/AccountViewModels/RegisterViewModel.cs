using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Това поле е задължително")]
        [EmailAddress(ErrorMessage = "Невалиден Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        [StringLength(100, ErrorMessage = "Паролата трябва да бъде поне {2} знака и не повече от {1}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; }


        [Required(ErrorMessage = "Трябва да изберете вид профил")]
        public string restaurantOrUser { get; set; }
    }
}
