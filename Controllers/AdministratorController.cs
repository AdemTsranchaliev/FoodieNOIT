using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foodie1.Data;
using Foodie1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodie1.Controllers
{
    public class AdministratorController : Controller
    {
        [Authorize(Roles = "Administrator")]
        public IActionResult ControlPanel()
        {
            try { 
            var db = new FoodieContext();
            return View(db.Restaurants.ToList());
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [Authorize(Roles = "Administrator")]
        public IActionResult SeeUser(int id)
        {
            try { 
            var db = new FoodieContext();
            return View(db.Restaurants.FirstOrDefault(x=>x.Id==id));
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [Authorize(Roles = "Administrator")]
        public IActionResult EditUser(Restaurant model)
        {
            try
            {
                var db = new FoodieContext();

                var res = db.Restaurants.FirstOrDefault(x => x.Id == model.Id);

                res.Name = model.Name;
                res.Phone = model.Phone;
                res.Address.Street = model.Address.Street;
                res.Address.Town.Name = model.Address.Town.Name;
                res.Address.Town.PostalCode = model.Address.Town.PostalCode;
                res.AdditionalInformation = model.AdditionalInformation;
                db.Restaurants.Update(res);
                db.SaveChanges();
                return RedirectToAction("ControlPanel");
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
    }
}