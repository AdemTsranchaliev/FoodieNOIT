using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class RestaurantViewModel
    {


        public RestaurantViewModel()
        {
          
        }

        public RestaurantViewModel(string name,string phone,int seats, string addres,int countImages,string postalCode,string town,bool isOpen,List<string> workingTime,List<string> advantages,string additionalInformation,string lat,string lon )
        {
            this.Name = name;
            this.Phone = phone;
            this.Seats = seats;
            this.Adress = addres;
            this.IsOpenNow = isOpen;
            this.WorkingTime = workingTime;
            this.Town = town;
            this.CountImages = countImages;
            this.PostalCode = postalCode;
            this.Advantages = advantages;
            this.AdditionalInformation = additionalInformation;
            this.Lat = lat;
            this.Lon = lon;

        }
        public RestaurantViewModel(string commenatorName,string commentatorEmail,string comment,bool recommended,string userId,int restaurantId)
        {
                
        }

        [Required(ErrorMessage = "Това поле е задължително")]
        [MinLength(3, ErrorMessage = "Минимум 3 знака")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Това поле е задължително")]
        public string Phone { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Това поле е задължително")]
        public int Seats { get; set; }

        [Required]
        public string Adress { get; set; }

        public int CountImages { get; set; }

        public bool IsOpenNow { get; set; }

        public List<string> WorkingTime { get; set; }

        [Required]
        public string Town { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public string Type { get; set; }

        public List<string> Advantages { get; set; }

        [Required]
        public string AdditionalInformation { get; set; }

        public List<Review> Reviews { get; set; }

        public string Lat { get; set; }

        public string Lon { get; set; }

        //Comment

        [Required]
        public string CommentatorName { get; set; }

        [Required]
        public string CommentatorEmail { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        public bool Reccomended { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int RestaurantId { get; set; }

        //Reservation

        public ReservationViewModel Reservation { get; set; }
    }
}
