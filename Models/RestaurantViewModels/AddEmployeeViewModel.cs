using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class AddEmployeeViewModel
    {
        [Required(ErrorMessage = "Това поле е задължително")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string Role { get; set; }

        public string IDCard { get; set; }

        public string Id { get; set; }
    }
}
