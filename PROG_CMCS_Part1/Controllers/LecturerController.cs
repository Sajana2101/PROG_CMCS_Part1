using Microsoft.AspNetCore.Mvc;

namespace PROG_CMCS_Part1.Controllers
{
    public class LecturerController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult SubmitClaim()
        {
            return View();
        }

        public IActionResult ClaimStatus()
        {
            return View();
        }
    }
}
