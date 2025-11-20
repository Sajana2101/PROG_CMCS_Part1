using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PROG_CMCS_Part1.Models;

namespace PROG_CMCS_Part1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Lecturer"))
                    return RedirectToAction("Dashboard", "Lecturer");
                else if (User.IsInRole("Coordinator"))
                    return RedirectToAction("Dashboard", "Coordinator");
                else if (User.IsInRole("Manager"))
                    return RedirectToAction("Dashboard", "Manager");
                else if (User.IsInRole("HR"))
                    return RedirectToAction("Index", "HR");
            }

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


