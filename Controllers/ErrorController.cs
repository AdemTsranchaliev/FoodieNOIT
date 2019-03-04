using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Foodie1.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Internal()
        {
            return View();
        }
    public IActionResult NotFoundEr()
        {
            return View();
}
    }
}