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
using Foodie1.Models.ManageViewModels;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using Foodie1.Models;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Foodie1.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly Castle.Core.Logging.ILogger _logger;
        private readonly UrlEncoder _urlEncoder;

        public EmployeeController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
        }

        [HttpGet]
        public async Task<IActionResult> FirstLogInChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.isRegistrationFinished==true)
            {
                return RedirectToAction("Index","Home");
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> FirstLogInChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            try { 

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }


            var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);
            var db = new FoodieContext();
            user.isRegistrationFinished = true;
            db.Users.Update(user);
            db.SaveChanges();
            return RedirectToAction("WaiterControlPanel");
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
    
        [HttpGet]
        [Authorize(Roles = "Waiter")]
        public IActionResult WaiterControlPanel()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Waiter")]
        public IActionResult WaiterAddNewOrder(WaiterNewOrderViewModel model)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            var restaurant = db.Restaurants.FirstOrDefault(x=>x.Id==user.EmployerId);
            //var model = new WaiterNewOrderViewModel();
            var menCategories = restaurant.MenuCategories.ToList();
            model.MenuCategories = menCategories;

            if (model.EditOrder==true)
            {
                var items = model.HiddenVal.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                model.AddedBeforeProducts = items;
            }

            if (TempData["Error5"] != null)
            {
                ViewBag.tableNum = TempData["Error5"].ToString();
            }
            if (TempData["Error2"] != null)
            {
                ViewBag.tableNum = TempData["Error2"].ToString();
            }
            if (TempData["Error3"] != null)
            {
                ViewBag.tableNum = TempData["Error3"].ToString();
            }
            if (TempData["Error4"] != null)
            {
                ViewBag.tableNum = TempData["Error4"].ToString();
            }

            model.TableNumber = restaurant.Seats;
            return View(model);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
   

        [HttpPost]
        [Authorize(Roles = "Waiter")]
        public IActionResult WaiterControlPanelPreview(WaiterNewOrderViewModel waiterNewOrder)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            var restaurant = db.Restaurants.FirstOrDefault(x=>x.Id==user.EmployerId);
            if (waiterNewOrder.HiddenVal==null)
            {
                TempData["Error5"] = "Трябва да изберете поне 1 продукт за да продължите";
                return RedirectToAction("WaiterAddNewOrder");
            }
            if (waiterNewOrder.TableNumber==0)
            {

                TempData["Error2"] = "Изберете маса";

                return RedirectToAction("WaiterAddNewOrder");
            }
            if (waiterNewOrder.HiddenVal == null)
            {

                TempData["Error3"] = "Изберете поне 1 продукт";

                return RedirectToAction("WaiterAddNewOrder");
            }
            if (restaurant.Orders.Where(x => x.IsFinished == false).FirstOrDefault(x => x.TableNumber == waiterNewOrder.TableNumber) != null)
            {
              
                TempData["Error4"] = "Масата, която сте избрали има недовършена поръчка";

                return RedirectToAction("WaiterAddNewOrder");
            }
         
            var list = new List<FinishOrderViewModel>();
            var resMenu = db.Restaurants.Where(x => x.Id == user.EmployerId).FirstOrDefault();
            var sep = waiterNewOrder.HiddenVal.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in sep)
            {
                var pt = item.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (pt[2] != "0")
                {
                    var product = resMenu.MenuCategories.FirstOrDefault(x => x.Id == int.Parse(pt[0])).Products.FirstOrDefault(x => x.Id == int.Parse(pt[1]));
                    list.Add(new FinishOrderViewModel(product.Name, int.Parse(pt[2]), product.Price));
                }
            }
            waiterNewOrder.FinishOrderViewModels = list;
            return View(waiterNewOrder);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }


        [HttpPost]
        [Authorize(Roles = "Waiter")]
        public IActionResult OrderPost(WaiterNewOrderViewModel waiterNewOrder)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            var resMenu = db.Restaurants.FirstOrDefault(x => x.Id == user.EmployerId);
            var sep = waiterNewOrder.HiddenVal.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var order = new Order();
            order.Restaurant = resMenu;
            order.UserId = userId;
            order.TableNumber = waiterNewOrder.TableNumber;
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
            order.IsItConfirmed = true;
            order.OrderedOn = DateTime.Now;
            order.TotalPrice = order.OrderProducts.Sum(x => x.Amount * x.Product.Price);
            db.Update(order);
            db.SaveChanges();

            return RedirectToAction("WaiterControlPanel");
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Waiter")]
        public IActionResult NotFinishedOrders()
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            return View(user.EmployeeOrders.Where(x=>x.IsFinished==false).ToList());
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Waiter")]
        public IActionResult NotConfirmedOrders()
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            var restaurant = db.Restaurants.FirstOrDefault(x=>x.Id==user.EmployerId);
            return View(restaurant.Orders.Where(x => x.IsItConfirmed == false).ToList());
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Waiter")]
        [Route("/Employee/SeeOrder/{Id}")]
        public IActionResult SeeOrder(int Id)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            var order = user.EmployeeOrders.FirstOrDefault(x=>x.Id==Id);
            if (order==null)
            {
                var restaurant = db.Restaurants.FirstOrDefault(x=>x.Id==user.EmployerId);
                order = restaurant.Orders.FirstOrDefault(x=>x.Id==Id);
            }
            if (TempData["Error"]!=null)
            {
                ViewBag.Error = TempData["Error"].ToString();
            }
            var model = new SeeOrderViewModel { Order = order };

            return View(model);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Waiter")]
        [Route("/Employee/AddProducToExistingOrder/{Id}")]
        public IActionResult AddProducToExistingOrder(int Id)
        {
            try { 
            if (TempData["Order2"] != null)
            {
                ViewBag.order2 = TempData["Order2"].ToString();
            }
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            var order = user.EmployeeOrders.FirstOrDefault(x => x.Id == Id);
            if (order == null)
            {
                var restaurant = db.Restaurants.FirstOrDefault(x => x.Id == user.EmployerId);
                order = restaurant.Orders.FirstOrDefault(x => x.Id == Id);
            }
            var resMenu = db.Restaurants.FirstOrDefault(x => x.Id == user.EmployerId);

            return View(new AddProducToExistingOrderViewModel { OrderId=order.Id,MenuCategories=resMenu.MenuCategories.ToList()});
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Waiter")]
        public IActionResult AddProducToExistingOrderPost(AddProducToExistingOrderViewModel model)
        {
            try { 
            if (model.HiddenValue==null)
            {
                TempData["Order2"]= "Трябва да изберете поне 1 продукт за да продължите";
                return RedirectToAction("AddProducToExistingOrder",new { Id=model.OrderId});
            }

            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            var resMenu = db.Restaurants.FirstOrDefault(x => x.Id == user.EmployerId);
            var sep = model.HiddenValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var order = user.EmployeeOrders.FirstOrDefault(x=>x.Id==model.OrderId);


            foreach (var item in sep)
            {
                var pt = item.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (pt[2] != "0")
                {
                    var product = resMenu.MenuCategories.FirstOrDefault(x => x.Id == int.Parse(pt[0])).Products.FirstOrDefault(x => x.Id == int.Parse(pt[1]));
                    if (order.OrderProducts.FirstOrDefault(x => x.Product.Id == product.Id&&x.OrderId==order.Id) == null)
                    {
                        db.OrderProducts.Add(new OrderProduct { Order = order, Product = product, Amount = int.Parse(pt[2]) });
                    }
                    else
                    {
                        var op = order.OrderProducts.FirstOrDefault(x => x.Product.Id == product.Id);
                        op.Amount += int.Parse(pt[2]);
                        db.OrderProducts.Update(op);
                    }
                }
                
            }
            order.TotalPrice = order.OrderProducts.Sum(x => x.Amount * x.Product.Price);
            db.Update(order);
            db.SaveChanges();
            return RedirectToAction("SeeOrder",new { Id=order.Id});
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Waiter")]
        public IActionResult FinishOrder(int Id)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            var order = user.EmployeeOrders.FirstOrDefault(x => x.Id == Id);
            order.IsFinished = true;
            db.Orders.Update(order);
            db.SaveChanges();
            return RedirectToAction("NotFinishedOrders");
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Waiter")]
        public IActionResult ConfirmOrder(int OrderId)
        {
            try { 
            var db = new FoodieContext();
            var order = db.Orders.FirstOrDefault(x => x.Id == OrderId);
           
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            order.IsItConfirmed = true;
            order.User = user;

            db.Orders.Update(order);
            db.SaveChanges();

            return RedirectToAction("SeeOrder",new {Id = order.Id });
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Waiter")]
        public IActionResult WaiterSeeOrders(string hiddenVal)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            if (hiddenVal=="today")
            {
                return View(user.EmployeeOrders.Where(x => x.OrderedOn.Date.ToString("dd.MM.yyyy") == DateTime.Now.ToString("dd.MM.yyyy")).ToList());
            }
            else if (hiddenVal=="week")
            {
                var a = user.EmployeeOrders.Where(x => DateTime.Compare(DateTime.Now.AddDays(-7), x.OrderedOn) < 0).ToList();
                return View(a);
            }
            else
            {
                return View(user.EmployeeOrders.ToList());
            }
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Chef")]
        public IActionResult ChefControlPanel()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Chef")]
        public IActionResult ChefSeeOrders(string hiddenVal)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            var restaurant = db.Restaurants.FirstOrDefault(x=>x.Id==user.EmployerId);
            if (restaurant==null)
            {
                return RedirectToAction("ChefControlPanel");
            }

            var ordersProduct = restaurant.Orders.Where(x => x.IsFinished == false&&x.IsItConfirmed==true).ToList();

            return View(ordersProduct);
            }
            catch (Exception a)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Chef")]
        public IActionResult ReadyProduct(int orderId,int productId)
        {
            try { 
            var db = new FoodieContext();
            var productOrder = db.OrderProducts.Where(x=>x.OrderId==orderId).FirstOrDefault(x=>x.ProductId==productId);
            productOrder.Ready += 1;
            db.OrderProducts.Update(productOrder);
            db.SaveChanges();
            return RedirectToAction("ChefSeeOrders");
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpPost]
        public IActionResult Delivery(int orderId, int productId)
        {
            try { 
            var db = new FoodieContext();
            var productOrder = db.OrderProducts.Where(x => x.OrderId == orderId).FirstOrDefault(x => x.ProductId == productId);
            if (productOrder.Delivered==productOrder.Amount)
            {
                TempData["Error"] = "Няма нищо за доставяне";
                return RedirectToAction("SeeOrder", new { id=orderId });
            }
            productOrder.Delivered += 1;
            db.OrderProducts.Update(productOrder);
            db.SaveChanges();
            return RedirectToAction("SeeOrder", new { id = orderId });
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        public IActionResult Setting()
        {
             return View();
        }
        [HttpPost]
        public async Task<IActionResult> Setting(ChangePasswordViewModel changePasswordViewModel)
        {
            try { 

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);
            if (changePasswordResult.Succeeded)
            {
                var db = new FoodieContext();
                user.isRegistrationFinished = true;
                db.Users.Update(user);
                db.SaveChanges();
                ViewBag.message = "Успешно променихте паролата";

                return View();
            }
            else
            {
                ViewBag.messageError = "Паролата не можа да се смени";
            
                return View(changePasswordViewModel);
            }

            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

    }
}