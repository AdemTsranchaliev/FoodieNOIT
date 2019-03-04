using ZXing;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ZXing.Common;
using ZXing.QrCode;
using System.Drawing.Imaging;
using Foodie1.Data;
using Foodie1.Models.RestaurantViewModels;
using System;
using System.Collections.Generic;
using Foodie1.Models;

namespace Foodie1.Controllers
{
    public class ClientController : Controller
    {
        [HttpGet]
        public IActionResult Test()
        {
            return View();

        }
        [HttpGet]
        public IActionResult QrReader()
        {
            if (TempData["invalidMessage"] != null)
            {
                ViewBag.message = TempData["invalidMessage"].ToString();
            }
            return View();

        }

        [HttpGet]
        [Route("/Client/QrReader/{qrCode}")]
        public IActionResult QrReader(string qrCode)
        {
            var model = new OrderViewModel { QrCode = qrCode };
            return RedirectToAction("Order", model);
        }

        
        [HttpGet]
        public IActionResult Order(OrderViewModel model)
        {
            try {
            var db = new FoodieContext();
            if (model.EditOrder == true)
            {

                var restaurantQr = db.Users.FirstOrDefault(x => x.RestaurantId == model.RestaurantId);
                var restaurantMenu = restaurantQr.Restaurant.MenuCategories.ToList();
                var orderViewModel = new OrderViewModel(restaurantMenu, restaurantQr.RestaurantId, restaurantQr.Restaurant.Name,model.TableNumber);
                return View(orderViewModel);
            }
            else if(model.QrCode!=null)
            {
               
                var qrCodeSep = model.QrCode.Split(new[] { '!' }, StringSplitOptions.RemoveEmptyEntries);
                if (qrCodeSep.Count()!=4)
                {
                    TempData["invalidMessage"] = "Невалиден код, рестурантът е сменил кода си";
                    return RedirectToAction("QrReader");
                }
                var restaurantQr = db.Users.FirstOrDefault(x => x.Id == qrCodeSep[1]);
                if (restaurantQr == null)
                {
                    TempData["invalidMessage"] = "Невалиден код";
                    return RedirectToAction("QrReader");
                }
                var res = restaurantQr.Restaurant;
                if (res == null || res.Id != int.Parse(qrCodeSep[0]))
                {
                    TempData["invalidMessage"] = "Невалиден код";
                    return RedirectToAction("QrReader");

                }
                if (res.QrCode != qrCodeSep[0] + "!" + qrCodeSep[1] + "!" + qrCodeSep[2])
                {
                    TempData["invalidMessage"] = "Невалиден код, рестурантът е сменил кода си";
                    return RedirectToAction("QrReader");
                }

                var notFinishedOrder = restaurantQr.Restaurant.Orders.FirstOrDefault(x => x.TableNumber == int.Parse(qrCodeSep[3]) && x.IsFinished == false);
                if(notFinishedOrder != null)
                {
                    var modelS = new AddToExistingOrderViewModel { OrderId = notFinishedOrder.Id, RestaurantId = res.Id, RestaurantName = res.Name,MenuCategories=res.MenuCategories.ToList(),TableNumber=notFinishedOrder.TableNumber };
                    return RedirectToAction("AddToExistingOrderClient", modelS );
                }
                var restaurantMenu = res.MenuCategories.ToList();
                var orderViewModel = new OrderViewModel(restaurantMenu, restaurantQr.RestaurantId, restaurantQr.Restaurant.Name,int.Parse(qrCodeSep[3]));
           
                return View(orderViewModel);
            }
            return RedirectToAction("QrReader");
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpPost]
        public IActionResult FinishOrder(OrderViewModel model)
        {
            try { 
            var db = new FoodieContext();
            var list = new List<FinishOrderViewModel>();
            var resMenu = db.Restaurants.Where(x=>x.Id==model.RestaurantId).FirstOrDefault();
            var sep = model.HiddenValue.Split(new[] { ' '},StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in sep)
            {
                var pt= item.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (pt[2] != "0")
                {
                    var product = resMenu.MenuCategories.FirstOrDefault(x => x.Id == int.Parse(pt[0])).Products.FirstOrDefault(x => x.Id == int.Parse(pt[1]));
                    list.Add(new FinishOrderViewModel(product.Name, int.Parse(pt[2]), product.Price));
                }
            }
            model.FinishOrderViewModels = list;
            return View(model);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpPost]
        public IActionResult FinishOrderPost(OrderViewModel model)
        {
            try { 
            var db = new FoodieContext();
            var resMenu = db.Restaurants.FirstOrDefault(x => x.Id == model.RestaurantId);
            var sep = model.HiddenValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var order = new Order();
            order.Restaurant = resMenu;

            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in sep)
            {
                var pt = item.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (pt[2] != "0")
                {
                    var product = resMenu.MenuCategories.FirstOrDefault(x => x.Id == int.Parse(pt[0])).Products.FirstOrDefault(x => x.Id == int.Parse(pt[1]));
                    db.OrderProducts.Add(new OrderProduct { Order = order, Product = product, Amount = int.Parse(pt[2]) });
                }
            }
            order.TotalPrice = order.OrderProducts.Sum(x => x.Amount * x.Product.Price);
            order.TableNumber = model.TableNumber;
            order.OrderedOn = DateTime.Now;
            db.Update(order);
            db.SaveChanges();

            return RedirectToAction("Index", "Home");
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        public IActionResult AddToExistingOrderClient(AddToExistingOrderViewModel model)
        {
            try { 
            var db = new FoodieContext();
          
            var resMenu = db.Restaurants.FirstOrDefault(x=>x.Id==model.RestaurantId).MenuCategories;
            var order = db.Orders.FirstOrDefault(x => x.Id == model.OrderId);
            
            if (model.HiddenValue != null)
            {
                var list = new List<FinishOrderViewModel>();
                var sep = model.HiddenValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in sep)
                {

                    var pt = item.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pt[2] != "0")
                    {
                        var product = resMenu.FirstOrDefault(x => x.Id == int.Parse(pt[0])).Products.FirstOrDefault(x => x.Id == int.Parse(pt[1]));
                        list.Add(new FinishOrderViewModel(product.Name, int.Parse(pt[2]), product.Price));
                    }
                }
                model.FinishOrders = list;
            }
           
            model.MenuCategories = resMenu.ToList();
            model.Order = order;
            return View(model);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        public IActionResult MenuAddToExistingOrderClient(AddToExistingOrderViewModel model)
        {
            try { 
            var db = new FoodieContext();
            var resMenu = db.Restaurants.FirstOrDefault(x => x.Id == model.RestaurantId).MenuCategories;
            var order = db.Orders.FirstOrDefault(x => x.Id == model.OrderId);
            model.MenuCategories = resMenu.ToList();
            model.Order = order;
            return View(model);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpPost]
        public IActionResult AddToExistingOrderClientPost(AddToExistingOrderViewModel model)
        {
            try { 
            var db = new FoodieContext();

            var resMenu = db.Restaurants.FirstOrDefault(x => x.Id == model.RestaurantId).MenuCategories;
            var order = db.Orders.FirstOrDefault(x => x.Id == model.OrderId);
           
           
            var sep = model.HiddenValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in sep)
            {
                var pt = item.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (pt[2]!="0")
                {
                    var product = resMenu.FirstOrDefault(x => x.Id == int.Parse(pt[0])).Products.FirstOrDefault(x => x.Id == int.Parse(pt[1]));
                    if (order.OrderProducts.FirstOrDefault(x => x.Product.Id == product.Id && x.OrderId == order.Id) == null)
                    {
                        var op = new OrderProduct();
                        op.Product = product;
                        op.Order = order;
                        op.Amount = int.Parse(pt[2]);
                        order.OrderProducts.Add(op);
                    }
                    else
                    {
                        var op = order.OrderProducts.FirstOrDefault(x => x.Product.Id == product.Id && x.OrderId == order.Id);
                        op.Amount += int.Parse(pt[2]);
                    }
                }
            }
            order.TotalPrice=order.OrderProducts.Sum(x=>x.Amount*x.Product.Price);

            db.Orders.Update(order);
            db.SaveChanges();

            return RedirectToAction("AddToExistingOrderClient",new AddToExistingOrderViewModel { OrderId=order.Id,RestaurantId=model.RestaurantId});
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
       



    }
}