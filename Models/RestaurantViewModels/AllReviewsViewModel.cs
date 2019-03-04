using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Foodie1.Models.RestaurantViewModels
{
    public class AllReviewsViewModel
    {
        public AllReviewsViewModel(List<Review> reviews,int countImages,string restaurantName,int id)
        {
            this.Id = id;
            this.Reviews = reviews;
            this.CountImages = countImages;
            this.RestaurantName = restaurantName;
        }

        public List<Review> Reviews { get; set; }

        public int CountImages { get; set; }

        public string RestaurantName { get; set; }

        public int Id { get; set; }
    }
}
