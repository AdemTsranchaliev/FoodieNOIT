using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Foodie1.Models;
using Foodie1.Data;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Foodie1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            try { 
            var db = new FoodieContext();

            if (db.Restaurants.Count()>6)
            {
                var list = new List<Restaurant>();
                var restaurantAll = db.Restaurants.ToList();
                
               var restaurant= restaurantAll.OrderByDescending(x => x.Reviews.Count(y=>y.Recommended==true)).ToList().Take(6).ToList();
                return View(restaurant);
                
            }
            else
            {
                var restaurantAll = db.Restaurants.ToList();
                return View(restaurantAll.OrderByDescending(x => x.Reviews.Count(y => y.Recommended == true)).ToList());
            }

            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        public IActionResult Search(string where,string what)
        {
            try { 

            var db = new FoodieContext();

            if (where==null&&what==null)
            {
                return Redirect("Index");
            }
            else if (where==null)
            {
                var res = db.Categories.FirstOrDefault(x => x.Name == what);
                if (res!=null)
                {
                    return View(res.RestaurantCategories.Select(x => x.Restaurant).ToList());
                }

                return View();

            }
            else if (what==null)
            {
                var res = db.Towns.FirstOrDefault(x => x.Name == where);
                if (res!=null)
                {
                    return View(res.Addresses.Select(x=>x.Restaurant).ToList());
                }
                return View();
            }
            else
            {
                var cat = db.Categories.FirstOrDefault(x => x.Name == what);
                if (cat==null)
                {
                    var place =db.Towns.FirstOrDefault(x => x.Name == where);
                    if (place==null)
                    {
                        return View();
                    }
                    else
                    {
                        return View(place.Addresses.Select(x=>x.Restaurant).ToList());
                    }
                }
                var place2 = db.Towns.FirstOrDefault(x => x.Name == what);
                if (place2==null)
                {
                    var cat2 = db.Categories.FirstOrDefault(x => x.Name == what);
                    if (cat2 == null)
                    {
                        return View();
                    }
                    else
                    {
                        return View(cat2.RestaurantCategories.Select(x => x.Restaurant).ToList());
                    }
                }
                var p2 = place2.Addresses.Select(x => x.Restaurant.RestaurantCategories.Where(p=>p.Category.Name==what)).ToList();
                if (p2==null)
                {
                    var list = new List<Restaurant>();
                    foreach (var item in db.Towns.FirstOrDefault(x => x.Name == where).Addresses.Select(x => x.Restaurant).ToList())
                    {
                        list.Add(item);
                    }
                    foreach (var item in db.Categories.FirstOrDefault(x => x.Name == what).RestaurantCategories.Select(x => x.Restaurant).ToList())
                    {
                        list.Add(item);

                    }
                    return View(list);
                }

            }
            return View();

            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }

        }

    }
}
