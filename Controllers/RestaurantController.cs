using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Foodie1.Data;
using Foodie1.Models;
using Foodie1.Models.ManageViewModels;
using Foodie1.Models.RestaurantViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nexmo.Api;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using ZXing;
using ZXing.QrCode;

namespace Foodie1.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class RestaurantController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly Castle.Core.Logging.ILogger _logger;
        private readonly UrlEncoder _urlEncoder;

        public RestaurantController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
        }



        [HttpGet()]
        [Route("See/RestaurantProfile/{Id}")]
        public IActionResult Restaurant(int Id, RestaurantViewModel restaurantViewModel)
        {

            try { 

            var db = new FoodieContext();
            IPrincipal principalUser = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var restaurant = new Restaurant();
            if ( _signInManager.IsSignedIn(User)&&User.IsInRole("Restaurant"))
            {
                var user = db.Users.FirstOrDefault(x => x.Id == userId);
                restaurant = user.Restaurant;
                if (restaurant == null)
                {
                    return RedirectToAction("FinishRestaurantRegistration", "Restaurant");
                }
            }
            else
            {
                restaurant = db.Restaurants.FirstOrDefault(x => x.Id == Id);
            }

            if (restaurant == null)
            {
                return RedirectToAction("Index", "Home");
            }


            var viewModel = new RestaurantViewModel();
            var isOpenNow = false;

            var openTime = restaurant.WorkTime.Split(new[] { "&& " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in openTime)
            {


                var arr = item.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (DateTime.Now.DayOfWeek.ToString() == arr[0])
                {
                    var startTime = arr[1].Split(':');
                    var endTime = arr[2].Split(':');

                    TimeSpan start = new TimeSpan(int.Parse(startTime[0]), int.Parse(startTime[1]), 0); //10 o'clock
                    TimeSpan end = new TimeSpan(int.Parse(endTime[0]), int.Parse(endTime[1]), 0); //12 o'clock
                    TimeSpan now = DateTime.Now.TimeOfDay;

                    if ((now > start) && (now < end))
                    {
                        isOpenNow = true;
                        break;
                    }
                }
            }
            for (int i = 0; i < openTime.Count; i++)
            {
                if (openTime[i].Contains("Monday"))
                {
                    openTime[i] = openTime[i].Replace("Monday", "Понеделник");
                }
                else if (openTime[i].Contains("Tuesday"))
                {
                    openTime[i] = openTime[i].Replace("Tuesday", "Вторник");
                }
                else if (openTime[i].Contains("Wednesday"))
                {
                    openTime[i] = openTime[i].Replace("Wednesday", "Сряда");
                }
                else if (openTime[i].Contains("Thursday"))
                {
                    openTime[i] = openTime[i].Replace("Thursday", "Четвъртък");
                }
                else if (openTime[i].Contains("Friday"))
                {
                    openTime[i] = openTime[i].Replace("Friday", "Петък");
                }
                else if (openTime[i].Contains("Saturday"))
                {
                    openTime[i] = openTime[i].Replace("Saturday", "Събота");
                }
                else if (openTime[i].Contains("Sunday"))
                {
                    openTime[i] = openTime[i].Replace("Sunday", "Неделя");
                }
            }

            var model = new RestaurantViewModel(restaurant.Name, restaurant.Phone, restaurant.Seats, restaurant.Address.Street, restaurant.GaleryImagesCount, restaurant.Address.Town.PostalCode, restaurant.Address.Town.Name, isOpenNow, openTime, new List<string> { }, restaurant.AdditionalInformation, restaurant.Lat, restaurant.Lon);
            model.RestaurantId = restaurant.Id;
            model.Reviews = restaurant.Reviews.Reverse().ToList();
            if (TempData["InvalidReservation"] != null)
            {
                ViewBag.reservation = TempData["InvalidReservation"].ToString();
                model.Reservation = restaurantViewModel.Reservation;
                return View(model);
            }
            if (TempData["InvalidReview"] != null)
            {
                ViewBag.review = TempData["InvalidReview"].ToString();
            }
            return View(model);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet()]
        [Authorize(Roles = "Restaurant")]
        public IActionResult FinishRestaurantRegistration(string returnUrl = null)
        {
            try { 
            var db = new FoodieContext();
            var cat = db.Categories.ToList();
            return View(new MainRestaurantInformationViewModel { CategoriesList = cat });
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpPost()]
        [Authorize(Roles = "Restaurant")]
        public IActionResult FinishRestaurantRegistration(List<Microsoft.AspNetCore.Http.IFormFile> file, MainRestaurantInformationViewModel mainRestaurantInformationViewModel, string returnUrl = null)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalUser = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            if (ModelState.IsValid)
            {

                Town town;
                if (db.Towns.Where(x => x.Name == mainRestaurantInformationViewModel.Town).FirstOrDefault() == null)
                {
                    town = new Town(mainRestaurantInformationViewModel.Town, mainRestaurantInformationViewModel.PostalCode);
                    db.Towns.Add(town);
                }
                else
                {
                    town = db.Towns.Where(x => x.Name == mainRestaurantInformationViewModel.Town).First();
                }



                Address address;

                address = new Address(mainRestaurantInformationViewModel.Adress, town);
                db.Addresses.Add(address);

                var workTime = string.Empty;

                if (mainRestaurantInformationViewModel.MondayIsOpen == true)
                {
                    workTime = workTime + "Monday " + mainRestaurantInformationViewModel.MondayStartTime + " " + mainRestaurantInformationViewModel.MondayEndTime + "&& ";
                }
                if (mainRestaurantInformationViewModel.TuesdayIsOpen == true)
                {
                    workTime = workTime + "Tuesday " + mainRestaurantInformationViewModel.TuesdayStartTime + " " + mainRestaurantInformationViewModel.TuesdayEndTime + "&& ";
                }
                if (mainRestaurantInformationViewModel.WednesdayIsOpen == true)
                {
                    workTime = workTime + "Wednesday " + mainRestaurantInformationViewModel.WednesdayStartTime + " " + mainRestaurantInformationViewModel.WednesdayEndTime + "&& ";
                }
                if (mainRestaurantInformationViewModel.ThursdayIsOpen == true)
                {
                    workTime = workTime + "Thursday " + mainRestaurantInformationViewModel.ThursdayStartTime + " " + mainRestaurantInformationViewModel.ThursdayEndTime + "&& ";
                }
                if (mainRestaurantInformationViewModel.FridayIsOpen == true)
                {
                    workTime = workTime + "Friday " + mainRestaurantInformationViewModel.FridayStartTime + " " + mainRestaurantInformationViewModel.FridayEndTime + "&& ";
                }
                if (mainRestaurantInformationViewModel.SaturdayIsOpen == true)
                {
                    workTime = workTime + "Saturday " + mainRestaurantInformationViewModel.SaturdayStartTime + " " + mainRestaurantInformationViewModel.SaturdayEndTime + "&& ";
                }
                if (mainRestaurantInformationViewModel.SundayIsOpen == true)
                {
                    workTime = workTime + "Sunday " + mainRestaurantInformationViewModel.SundayStartTime + " " + mainRestaurantInformationViewModel.SundayEndTime + "&& ";
                }


                var restaurant = new Restaurant(mainRestaurantInformationViewModel.Name, mainRestaurantInformationViewModel.Phone, mainRestaurantInformationViewModel.Seats, workTime, "", new TypeRestaurant("Български"), address);



                restaurant.AdditionalInformation = mainRestaurantInformationViewModel.AdditionalInformation;
                restaurant.UserId = userId;
                restaurant.GaleryImagesCount = file.Count();

                var categories = mainRestaurantInformationViewModel.Category.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in categories)
                {

                    var category1 = new Category();
                    if (db.Categories.Where(x => x.Name == item).FirstOrDefault() == null)
                    {
                        category1.Name = item;
                        db.Categories.Add(category1);
                    }
                    else
                    {
                        category1 = db.Categories.Where(x => x.Name == item).FirstOrDefault();
                    }
                    var categoryRestaurant = new RestaurantCategory();
                    categoryRestaurant.Category = category1;
                    categoryRestaurant.Restaurant = restaurant;
                    db.RestaurantCategories.Add(categoryRestaurant);

                }
                db.Restaurants.Add(restaurant);
                db.SaveChanges();
                user.RestaurantId = restaurant.Id;
                user.Restaurant.isRegistrationFinished = true;
                db.Users.Update(user);
                db.SaveChanges();
                if (file != null)
                {
                    for (int i = 0; i < file.Count; i++)
                    {
                        var path = $"C:/Users/Asus/source/repos/Foodie1/Foodie1/wwwroot/images/RestaurantGaleries/{restaurant.Id}-{i}-ProfilePicture.jpg";
                        var stream = new FileStream(path, FileMode.Create);
                        file[i].CopyTo(stream);
                        stream.Close();
                    }
                    for (int i = 0; i < file.Count; i++)
                    {

                        var path = $"C:/Users/Asus/source/repos/Foodie1/Foodie1/wwwroot/images/RestaurantGaleries/{restaurant.Id}-{i}-ProfilePicture.jpg";
                        FileStream fs = System.IO.File.Open(path, FileMode.Open);
                        var image = System.Drawing.Image.FromStream(fs);
                        fs.Close();
                        var stream = new FileStream(path, FileMode.Create);
                        image = ResizeImage(image, 550, 360);
                        image.Save(stream, ImageFormat.Jpeg);
                    }


                }
                return RedirectToAction("Map", "Restaurant");

            }
            else
            {
                return View(mainRestaurantInformationViewModel);
            }

            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpPost]
        public IActionResult WriteReview(RestaurantViewModel restaurantViewModel)
        {
            try { 
            var db = new FoodieContext();
            var review = new Review();

            review.CommentedOn = DateTime.Now;
            review.Recommended = restaurantViewModel.Reccomended;
            review.Comment = restaurantViewModel.Comment;
            review.RestaurantId = restaurantViewModel.RestaurantId;

            if (restaurantViewModel.CommentatorName == null && _signInManager.IsSignedIn(User))
            {
                IPrincipal uorincipalusEr = User;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = db.Users.FirstOrDefault(x => x.Id == userId);
                var userReview = new UserReview();
                review.Name = user.UserName;
                userReview.Review = review;
                userReview.User = user;
                db.UserReviews.Add(userReview);
            }
            else
            {
                review.Name = restaurantViewModel.CommentatorName;
                review.Email = restaurantViewModel.CommentatorEmail;
            }
            if (review.Comment == null || restaurantViewModel.CommentatorName == null || restaurantViewModel.CommentatorEmail == null)
            {
                TempData["InvalidReview"] = "Всички полета трябва да бъдат попълнени";
                return RedirectToAction("Restaurant", new { Id = restaurantViewModel.RestaurantId });
            }
            db.Reviews.Add(review);

            db.SaveChanges();
            return RedirectToAction("Restaurant", new { Id = restaurantViewModel.RestaurantId });
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpPost]
        public IActionResult MakeReservation(RestaurantViewModel model)
        {
            try { 
            var db = new FoodieContext();
            var reservation = new Reservation();
            if (model.Reservation.ReservatorName == null)
            {
                IPrincipal principalUser = User;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = db.Users.FirstOrDefault(x => x.Id == userId);

                reservation.User = user;
                reservation.Name = user.UserName;
                reservation.Phone = user.PhoneNumber;
            }
            else
            {
                reservation.Name = model.Reservation.ReservatorName;
                reservation.Phone = model.Reservation.ReservatorPhone;
            }
            if (model.Reservation.ReservatorName == null || model.Reservation.ReservatorPhone == null || model.Reservation.ReservationTime == null || model.Reservation.ReservationDate == null || model.Reservation.PeopleCount == 0)
            {
                TempData["InvalidReservation"] = "Всички полета трябва да бъдат попълнени";
                return RedirectToAction("Restaurant", new { Id = model.Reservation.RestaurantId, restaurantViewModel = model });
            }
            reservation.PeopleCount = model.Reservation.PeopleCount;
            reservation.RestaurantId = model.Reservation.RestaurantId;
            reservation.ReservedOn = DateTime.Now;
            reservation.DateTime = DateTime.Parse(model.Reservation.ReservationDate, CultureInfo.InvariantCulture);
            var sep = model.Reservation.ReservationTime.Split(":");
            var span = new TimeSpan(int.Parse(sep[0]), int.Parse(sep[1]), 0);
            reservation.DateTime=reservation.DateTime.Add(span);
            reservation.IsReservationFinished = false;
            db.Reservations.Add(reservation);
            db.SaveChanges();

            return RedirectToAction("Restaurant", new { Id = model.Reservation.RestaurantId });

            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Route("/Restaurant/SeeMenu/{Id}")]
        public IActionResult SeeMenu(int Id)
        {
            try { 
            var db = new FoodieContext();
            var restaurant = db.Restaurants.FirstOrDefault(x => x.Id == Id);
            if (restaurant == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.restaurantName = restaurant.Name;
            var menuCategories = restaurant.MenuCategories.ToList();
            return View(menuCategories);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        public IActionResult Map()
        {
            try { 
            var db = new FoodieContext();
            IPrincipal uorincipalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (user.Restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            if (user.Restaurant.Lat != null && user.Restaurant.Lon != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Restaurant")]
        public IActionResult Map(MapViewModel mapViewModel)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal uorincipalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (user.Restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            user.Restaurant.Lat = mapViewModel.Lat;
            user.Restaurant.Lon = mapViewModel.Lon;

            db.Restaurants.Update(user.Restaurant);
            db.SaveChanges();

            return RedirectToAction("RestaurantProfile", "See", new { Id = 0 });
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Route("/Restaurant/RestaurantChangePassword")]
        [Authorize(Roles = "Restaurant")]
        public IActionResult RestaurantChangePassword()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RestaurantChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            try { 
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }


                var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    ModelState.AddModelError("OldPassword", "Старата парола е грешна");
                }
                else
                {
                    ViewBag.message = "Вие успешно променихте вашата парола";
                }
                return View();
            }
            return View(changePasswordViewModel);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }



        [HttpGet]
        [Route("/Restaurant/RestaurantControlPanel")]
        [Authorize(Roles = "Restaurant")]
        public IActionResult RestaurantControlPanel()
        {
            try { 
            var db = new FoodieContext();
            IPrincipal uorincipalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (user.Restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            var reservations = user.Restaurant.Reservations.Where(x => x.IsReservationFinished == false).OrderByDescending(x => x.ReservedOn).ToList();
            return View(reservations);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Route("/Restaurant/MenuProductAdd")]
        [Authorize(Roles = "Restaurant")]
        public IActionResult MenuProductAdd()
        {
            try { 
            var db = new FoodieContext();

            IPrincipal uorincipalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (user.Restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            var pr = new ProductAddViewModel { menuCategories = user.Restaurant.MenuCategories.ToList(), IngredientsList = db.Ingredients.ToList() };
            return View(pr);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpPost]
        public IActionResult MenuProductAdd(ProductAddViewModel productAddViewModel)
        {
            try { 

            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (ModelState.IsValid)
            {


                MenuCategory menuCategory = user.Restaurant.MenuCategories.Where(x => x.Name == productAddViewModel.Category).FirstOrDefault();

                if (menuCategory == null)
                {
                    menuCategory = new MenuCategory(productAddViewModel.Category, user.Restaurant);
                    db.MenuCategories.Add(menuCategory);
                    db.SaveChanges();
                }

                if (user.Restaurant.MenuCategories.Where(x => x.Name == productAddViewModel.Category).FirstOrDefault().Products.Where(y => y.Name == productAddViewModel.ProductName).FirstOrDefault() == null)
                {
                    var newProduct = new Product(productAddViewModel.ProductName, menuCategory, decimal.Parse(productAddViewModel.Weight, CultureInfo.InvariantCulture), decimal.Parse(productAddViewModel.Price, CultureInfo.InvariantCulture));
                    db.Products.Add(newProduct);
                    if (productAddViewModel.Ingredients != null)
                    {
                        var ingredients = productAddViewModel.Ingredients.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in ingredients)
                        {
                            var ing = new Ingredients();
                            if (db.Ingredients.FirstOrDefault(x => x.Name == item) == null)
                            {
                                ing.Name = item;
                                db.Ingredients.Add(ing);
                            }
                            else
                            {
                                ing = db.Ingredients.FirstOrDefault(x => x.Name == item);
                            }
                            var ingPrd = new ProductIngredients();
                            ingPrd.Ingredients = ing;

                            ingPrd.Product = newProduct;
                            db.ProductIngredients.Add(ingPrd);
                        }



                    }

                }

                db.SaveChanges();
                ViewBag.message = $"Вие успешно добавихте \"{productAddViewModel.ProductName}\" към менюто";
                return View(new ProductAddViewModel { menuCategories = user.Restaurant.MenuCategories.ToList(), IngredientsList = db.Ingredients.ToList() });

            }
            else
            {
                productAddViewModel.menuCategories = user.Restaurant.MenuCategories.ToList();
                return View(new ProductAddViewModel { menuCategories = user.Restaurant.MenuCategories.ToList() });
            }
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        [Route("/Restaurant/SeeAllReviews/{Id}")]
        public IActionResult SeeAllReviews(int Id)
        {
            try { 
            var db = new FoodieContext();
            var restaurant = db.Restaurants.Where(x => x.Id == Id).FirstOrDefault();
            if (restaurant == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var restaurantReviews = new AllReviewsViewModel(restaurant.Reviews.ToList(), restaurant.GaleryImagesCount, restaurant.Name, restaurant.Id);
            return View(restaurantReviews);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        public IActionResult MenuRemoveEditProduct()
        {

            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;
            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            if (TempData["SuccessfulDelete"] != null)
            {
                ViewBag.successfullDelete = TempData["SuccessfulDelete"].ToString();
            }
            return View(restaurant.MenuCategories.ToList());
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        [Route("Restaurant/MenuEditProduct/{categoryId}/{productId}")]
        public IActionResult MenuEditProduct(int categoryId, int productId, MenuEditProductViewModel productViewModel)
        {
            try { 
            var db = new FoodieContext();

            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (user.Restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            var product = user.Restaurant.MenuCategories.FirstOrDefault(x => x.Id == categoryId).Products.FirstOrDefault(x => x.Id == productId);
            if (TempData["editProductSucces"] != null)
            {
                ViewBag.message = TempData["editProductSucces"].ToString();

            }
            var ingred = string.Empty;
            if (product.ProductIngredients.Count > 0)
            {

                foreach (var item in product.ProductIngredients)
                {
                    ingred = ingred + item.Ingredients.Name + ",";
                }
                ingred = ingred.Substring(0, ingred.Length - 1);
            }

            return View(new MenuEditProductViewModel { Id = product.Id, Name = product.Name, MenuCategoryId = product.MenuCategory.Id, Weight = product.Weight, Price = product.Price, CategoryName = product.MenuCategory.Name, Ingredients = ingred });
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Restaurant")]
        public IActionResult MenuEditProductPost(MenuEditProductViewModel product)
        {
            try { 
            if (ModelState.IsValid)
            {
                var db = new FoodieContext();
                IPrincipal principalusEr = User;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;

                if (restaurant == null)
                {
                    return RedirectToAction("FinishRestaurantRegistration");
                }

                var menCat = restaurant.MenuCategories.FirstOrDefault(x => x.Name == product.CategoryName);
                if (menCat == null)
                {
                    var menuCategory = new MenuCategory(product.CategoryName, restaurant);
                    db.MenuCategories.Add(menuCategory);
                    menCat = menuCategory;
                }
                var productToEdit = restaurant.MenuCategories.FirstOrDefault(x => x.Id == product.MenuCategoryId).Products.FirstOrDefault(x => x.Id == product.Id);
                if (product.Ingredients != null)
                {
                    var ingr = product.Ingredients.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in ingr)
                    {
                        if (productToEdit.ProductIngredients.FirstOrDefault(x => x.Ingredients.Name == item) == null)
                        {
                            if (db.Ingredients.FirstOrDefault(x => x.Name == item) == null)
                            {
                                var ing = new Ingredients();
                                ing.Name = item;
                                var newI = new ProductIngredients();
                                newI.Ingredients = ing;
                                newI.Product = productToEdit;

                                db.Ingredients.Add(ing);
                                db.ProductIngredients.Add(newI);
                            }
                            else
                            {
                                var ing = db.Ingredients.FirstOrDefault(x => x.Name == item);
                                var newI = new ProductIngredients();
                                newI.Ingredients = ing;
                                newI.Product = productToEdit;
                                db.ProductIngredients.Add(newI);
                            }

                        }
                    }
                }
                else
                {
                    foreach (var item in productToEdit.ProductIngredients)
                    {
                        db.ProductIngredients.RemoveRange((IEnumerable<ProductIngredients>)productToEdit.ProductIngredients);
                    }
                    db.SaveChanges();
                }


                productToEdit.MenuCategory = menCat;
                productToEdit.Name = product.Name;
                productToEdit.Price = product.Price;
                productToEdit.Weight = product.Weight;
                productToEdit.Description = product.Description;
                db.Products.Update(productToEdit);
                db.SaveChanges();
                TempData["editProductSucces"] = "Вие успешно редактирахте продукта";
                return RedirectToAction("MenuEditProduct", new { categoryId = productToEdit.MenuCategory.Id, productId = product.Id });
            }
            else
            {
                product.categoryId = product.MenuCategoryId;
                product.productId = product.Id;
                return RedirectToAction("MenuEditProduct", product);
            }
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }



        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        [Route("Restaurant/MenuDeleteProduct/{categoryId}/{productId}")]
        public IActionResult MenuDeleteProduct(int categoryId, int productId)
        {
            try { 
            var db = new FoodieContext();

            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (user.Restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            var product = user.Restaurant.MenuCategories.FirstOrDefault(x => x.Id == categoryId).Products.FirstOrDefault(x => x.Id == productId);
            if (TempData["editProductSucces"] != null)
            {
                ViewBag.message = TempData["editProductSucces"].ToString();

            }
            var ingred = string.Empty;
            if (product.ProductIngredients.Count > 0)
            {

                foreach (var item in product.ProductIngredients)
                {
                    ingred = ingred + item.Ingredients.Name + ",";
                }
                ingred = ingred.Substring(0, ingred.Length - 1);
            }

            return View(new MenuEditProductViewModel { Id = product.Id, Name = product.Name, MenuCategoryId = product.MenuCategory.Id, Weight = product.Weight, Price = product.Price, CategoryName = product.MenuCategory.Name, Ingredients = ingred });
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Restaurant")]
        public IActionResult MenuDeleteProductPost(int categoryId, int productId)
        {
            try { 
            var db = new FoodieContext();

            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);
            if (user.Restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            var product = db.MenuCategories.FirstOrDefault(x => x.Id == categoryId).Products.FirstOrDefault(x => x.Id == productId);
            db.Products.Remove(product);
            db.SaveChanges();
            TempData["SuccessfulDelete"] = "Успешно изтрихте продукта";
            return RedirectToAction("MenuRemoveEditProduct");
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        public IActionResult AddEmployee()
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;

            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            return View();
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }


        [HttpPost]
        [Authorize(Roles = "Restaurant")]
        public async Task<IActionResult> AddEmployee(AddEmployeeViewModel employee)
        {
            try { 
            if (ModelState.IsValid)
            {
                var db = new FoodieContext();
                IPrincipal principalusEr = User;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var restaurant = (db.Users.FirstOrDefault(x => x.Id == userId).Restaurant);

                if (restaurant == null)
                {
                    return RedirectToAction("FinishRestaurantRegistration");
                }
                var email = string.Empty;
                var employees = restaurant.EmployeesId;

                if (employees == null)
                {
                    email = restaurant.User.Email.Substring(0, restaurant.User.Email.IndexOf("@")) + "Employee1";
                }
                else
                {
                    var trt = employees.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    email = restaurant.User.Email.Substring(0, restaurant.User.Email.IndexOf("@")) + "Employee" + (trt.Count + 1).ToString();
                }
                if (db.Users.FirstOrDefault(x => x.PhoneNumber == employee.Phone) != null)
                {
                    ViewBag.phone = "Потребител с такъв номер вече съществува";
                    return View(employee);
                }
                email += "@foodie.bg";
                var password = CreatePassword(8);


                var user = new User { UserName = email, Email = email, RegisteredOn = DateTime.Now };
                user.PhoneNumber = employee.Phone;
                user.AutoGeneratedPassword = password;
                user.EmployeeFullName = employee.Name + " " + employee.LastName;
                var result = await _userManager.CreateAsync(user, password);


                if (result.Succeeded)
                {

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);



                    if (employee.Role == "Waiter")
                    {
                        await _userManager.AddToRoleAsync(user, "Waiter");

                    }
                    else if (employee.Role == "Chef")
                    {
                        await _userManager.AddToRoleAsync(user, "Chef");
                    }
                    user.EmployerId = restaurant.Id;

                    restaurant.EmployeesId += user.Id.ToString() + " ";
                    db.Restaurants.Update(restaurant);
                    db.Users.Update(user);
                    db.SaveChanges();
                    SendSMS($"Вашата парола за служебеният пофил е {password}, а потребителското име е {user.Email}");
                    return RedirectToAction("AddEmployee");
                }
            }
            return View(employee);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }

        }
        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        public IActionResult SeeAllEmployees()
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;
            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            var list = new List<User>();
            if (restaurant.EmployeesId == null)
            {
                return View(list);
            }

            var employeeId = restaurant.EmployeesId.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in employeeId)
            {
                var user = db.Users.FirstOrDefault(x => x.Id == item);
                if (user != null)
                {
                    list.Add(user);
                }
            }
            return View(list);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        [Route("Restaurant/EditEmployee/{Id}")]
        public IActionResult EditEmployee(string Id)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;
            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            if (restaurant.EmployeesId != null)
            {
                var employee = restaurant.EmployeesId.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => x == Id);
                if (employee != null)
                {
                    var empl = db.Users.FirstOrDefault(x => x.Id == employee);
                    var employeeToEdit = new AddEmployeeViewModel { Name = empl.EmployeeFullName.Split()[0], LastName = empl.EmployeeFullName.Split()[1], IDCard = empl.IdCard, Phone = empl.PhoneNumber, Id = empl.Id };
                    return View(employeeToEdit);
                }

                return View();
            }
            return View();
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Restaurant")]
        public IActionResult EditEmployee(AddEmployeeViewModel model)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;
            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            if (restaurant.EmployeesId != null)
            {
                var employee = restaurant.EmployeesId.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => x == model.Id);
                if (employee != null)
                {
                    var empl = db.Users.FirstOrDefault(x => x.Id == employee);
                    if (db.Users.FirstOrDefault(x => x.PhoneNumber == model.Phone) != null)
                    {
                        ViewBag.phone = "Потребител с такъв номер вече съществува";
                        return View(model);
                    }
                    empl.EmployeeFullName = model.Name + " " + model.LastName;
                    empl.IdCard = model.IDCard;
                    empl.PhoneNumber = model.Phone;
                    db.Users.Update(empl);
                    db.SaveChanges();
                    ViewBag.succesEdit = "Вие успешно редактирахте профила";
                    return View(model);
                }

                return View();
            }
            return View();
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        public IActionResult GenerateAndExportPdf()
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;
            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            return View(restaurant.Id);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "Restaurant")]
        public IActionResult GenerateAndExportPdf(int tableCount, int copyPerTable)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;
            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            if (tableCount > 500 || copyPerTable > 500)
            {
                ModelState.AddModelError("MaxLen", "Броя на масите, както и на копията за маса не трябва да надвишават 500");
                return View();
            }
            var str = restaurant.Id + "!" + restaurant.UserId + "!" + CreatePassword(5);
            var randomStr = "http://109.121.216.41:8012/Client/QrReader/" + str;
            QrEncoder(randomStr, restaurant.Id, tableCount);
            restaurant.QrCode = str;
            restaurant.Seats = tableCount;
            db.Restaurants.Update(restaurant);
            db.SaveChanges();
            ExportToPdf(tableCount, copyPerTable, restaurant.Id);

            return View(restaurant.Id);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }
        [HttpGet]
        [Route("Restaurant/SeeEmployeeOrders/{id}")]
        public IActionResult SeeEmployeeOrders(string hiddenVal, string id)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = db.Users.FirstOrDefault(x => x.Id == userId);

            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;
            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            var isEmployee = false;
            if (restaurant.EmployeesId != null)
            {
                var emplIds = restaurant.EmployeesId.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in emplIds)
                {
                    if (id == item)
                    {
                        isEmployee = true;
                        break;
                    }
                }
            }
            User employee = new User();
            if (isEmployee)
            {
                if (db.Users.Where(x => x.EmployerId == restaurant.Id) != null)
                {
                    employee = db.Users.Where(x => x.EmployerId == restaurant.Id).FirstOrDefault(x => x.Id == id);
                }
            }
            else
            {
                return RedirectToAction("");
            }

            if (hiddenVal == "today")
            {

                return View(employee.EmployeeOrders.Where(x => x.OrderedOn.Date.ToString("dd.MM.yyyy") == DateTime.Now.ToString("dd.MM.yyyy")).ToList());
            }
            else if (hiddenVal == "week")
            {
                var a = employee.EmployeeOrders.Where(x => DateTime.Compare(DateTime.Now.AddDays(-7), x.OrderedOn) < 0).ToList();
                return View(a);
            }
            else
            {
                return View(employee.EmployeeOrders.ToList());
            }
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        [Route("Restaurant/SeeOrder/{id}")]
        public IActionResult SeeOrder(int Id)
        {
            try { 
            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;
            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            var order = restaurant.Orders.FirstOrDefault(x => x.Id == Id);
            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(order);
            }
            catch (global::System.Exception)
            {
                return RedirectToAction("Internal", "Error");
            }
        }

        //methods

        public User GetCurrentUser(HttpContext context)
        {
            User user = new User();
            Task.Run(async () =>
            {
                user = await _userManager.GetUserAsync(context.User);
            }).Wait();


            return user;
        }

        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {

            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }



        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        public void QrEncoder(string text, int id, int countTables)
        {
            int j = 1;
            while (System.IO.File.Exists($"C:/Users/Asus/source/repos/Foodie1/Foodie1/wwwroot/images/QrCodes/{id}!{j}.png"))
            {

                System.IO.File.Delete($"C:/Users/Asus/source/repos/Foodie1/Foodie1/wwwroot/images/QrCodes/{id}!{j}.png");
                j++;
            }
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = 150,
                    Height = 150
                }
            };
            for (int i = 1; i <= countTables; i++)
            {
                var result = writer.Write(text + $"!{i}");
                var barcodeBitmap = new Bitmap(result);
                barcodeBitmap.Save($"C:/Users/Asus/source/repos/Foodie1/Foodie1/wwwroot/images/QRCodes/{id}!{i}.png");
            }
        }

        public void SendSMS(string content)
        {
            const string accountSid = "AC55cc299eeccfe0cf8adf42a7befa8903";
            const string authToken = "0ad73315572576fef9d47822c410622d";

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: content,
                from: new Twilio.Types.PhoneNumber("+12015080126"),
                to: new Twilio.Types.PhoneNumber("+35989 260 9802")
            );
        }

        public void ExportToPdf(int n1, int n2, int id)
        {
            var pdfDoc = new Document(PageSize.A4, 40f, 40f, 80f, 60f);
            string path = $"C:/Users/Asus/source/repos/Foodie1/Foodie1/wwwroot/images/PdfFiles/{id}.pdf";
            if (System.IO.File.Exists(path))
            {
                System.IO.File.SetAttributes(path, FileAttributes.Normal);
                System.IO.File.Delete(path);


            }

            PdfWriter.GetInstance(pdfDoc, new FileStream(path, FileMode.OpenOrCreate));
            pdfDoc.Open();
            string pathImage = "C:/Users/Asus/source/repos/Foodie1/Foodie1/wwwroot/images/logo2.png";

            using (FileStream fs = new FileStream(pathImage, FileMode.Open))
            {
                var png = iTextSharp.text.Image.GetInstance(System.Drawing.Image.FromStream(fs), ImageFormat.Png);
                png.ScalePercent(70f);
                png.SetAbsolutePosition(pdfDoc.Left, pdfDoc.Top);
                pdfDoc.Add(png);
            }
            var spacer = new Paragraph("")
            {
                SpacingBefore = 10f,
                SpacingAfter = 10f,
            };
            pdfDoc.Add(spacer);

            var headerTable = new PdfPTable(new[] { 1f, 1f })
            {
                HorizontalAlignment = 1,
                WidthPercentage = 90,
                DefaultCell = { FixedHeight = 120 }
            };
            for (int i = 1; i <= n1; i++)
            {
                string pathImage2 = $"C:/Users/Asus/source/repos/Foodie1/Foodie1/wwwroot/images/QRCodes/{id}!{i}.png";
                using (FileStream fs = new FileStream(pathImage2, FileMode.Open))
                {


                    var png = iTextSharp.text.Image.GetInstance(System.Drawing.Image.FromStream(fs), ImageFormat.Png);
                    png.ScalePercent(120);

                    var cell = new PdfPCell { PaddingTop = 2 };
                    var par1 = new Paragraph($"       {i}");
                    cell.AddElement(par1);
                    cell.AddElement(png);
                    for (int j = 1; j <= n2; j++)
                    {
                        headerTable.AddCell(cell);
                    }
                }


            }
            pdfDoc.Add(headerTable);
            pdfDoc.Close();
        }
        [HttpGet]
        [Authorize(Roles = "Restaurant")]
        public IActionResult IdCardReader(string name)
        {

            var db = new FoodieContext();
            IPrincipal principalusEr = User;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var restaurant = db.Users.FirstOrDefault(x => x.Id == userId).Restaurant;
            if (restaurant == null)
            {
                return RedirectToAction("FinishRestaurantRegistration");
            }
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Restaurant")]
        public IActionResult FinishReservation(int id)
        {

            var db = new FoodieContext();
            var res = db.Reservations.FirstOrDefault(x => x.Id == id);
            if (res == null)
            {
                return RedirectToAction("RestaurantControlPanel");
            }
            res.IsReservationFinished = true;
            db.Reservations.Update(res);
            db.SaveChanges();

            return RedirectToAction("RestaurantControlPanel");
        }
    }
}
