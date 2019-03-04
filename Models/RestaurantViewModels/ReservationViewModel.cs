using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class ReservationViewModel
    {


        public ReservationViewModel()
        {
          
        }




        //Reservation

        [Required(ErrorMessage = "Това поле е задължително")]
        public string ReservatorName { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string ReservatorPhone { get; set; }

        public string ReservatorComment { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string ReservationTime { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string ReservationDate { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public int PeopleCount { get; set; }

        [Required]
        public int RestaurantId { get; set; }
    }
}
