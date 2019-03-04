using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Това поле е задължително")]
        [EmailAddress(ErrorMessage ="Невалиден Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public bool firstLog { get; set; } = true;
    }
}
