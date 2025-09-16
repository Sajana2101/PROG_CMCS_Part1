using Microsoft.AspNetCore.Mvc;

namespace PROG_CMCS_Part1.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

    }
}
