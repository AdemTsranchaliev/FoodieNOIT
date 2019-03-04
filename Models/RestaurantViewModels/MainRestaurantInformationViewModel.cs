using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class MainRestaurantInformationViewModel
    {
        [Required(ErrorMessage = "Това поле е задължително")]
        [MinLength(3, ErrorMessage = "Минимум 3 знака")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        [Range(1, int.MaxValue, ErrorMessage = "Това поле е задължително")]
        public int Seats { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string Adress { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string Category { get; set; }

        public bool MondayIsOpen { get; set; }
        public string MondayStartTime { get; set; }
        public string MondayEndTime { get; set; }

        public bool TuesdayIsOpen { get; set; }
        public string TuesdayStartTime { get; set; }
        public string TuesdayEndTime { get; set; }

        public bool WednesdayIsOpen { get; set; }
        public string WednesdayStartTime { get; set; }
        public string WednesdayEndTime { get; set; }

        public bool ThursdayIsOpen { get; set; }
        public string ThursdayStartTime { get; set; }
        public string ThursdayEndTime { get; set; }

        public bool FridayIsOpen { get; set; }
        public string FridayStartTime { get; set; }
        public string FridayEndTime { get; set; }

        public bool SaturdayIsOpen { get; set; }
        public string SaturdayStartTime { get; set; }
        public string SaturdayEndTime { get; set; }

        public bool SundayIsOpen { get; set; }
        public string SundayStartTime { get; set; }
        public string SundayEndTime { get; set; }



        [Required(ErrorMessage = "Това поле е задължително")]
        public string Town { get; set; }



        [Required(ErrorMessage = "Това поле е задължително")]
        public string PostalCode { get; set; }


        public List<Category> CategoriesList { get; set; }

        [Required]
        public string AdditionalInformation { get; set; }

    }
}
