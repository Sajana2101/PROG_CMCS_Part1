using Microsoft.AspNetCore.Mvc;

namespace PROG_CMCS_Part1.Controllers
{
    public class CoordinatorController : Controller
    {
        private static List<dynamic> claims = new List<dynamic>
{
    new { ClaimId = 101, LecturerName = "Lecturer A", Month = "August", HoursWorked = 20, HourlyRate = 50, TotalAmount = 1000, Status = "Approved",  FinalisedBy = "Manager Smith", SupportingDocuments = "Document1.pdf" },
    new { ClaimId = 102, LecturerName = "Lecturer A", Month = "September", HoursWorked = 15, HourlyRate = 50, TotalAmount = 750, Status = "Pending Verification",  FinalisedBy = "-", SupportingDocuments = "Document2.pdf" },
    new { ClaimId = 103, LecturerName = "Lecturer A", Month = "October", HoursWorked = 18, HourlyRate = 50, TotalAmount = 900, Status = "Verified – Pending Approval", FinalisedBy = "-", SupportingDocuments = "Document3.pdf" },
    new { ClaimId = 104, LecturerName = "Lecturer A", Month = "August", HoursWorked = 22, HourlyRate = 50, TotalAmount = 1100, Status = "Rejected", FinalisedBy = "Manager Smith", SupportingDocuments = "Document4.pdf" }
};

        public IActionResult Dashboard()
        {
            return View(claims);
        }
        public IActionResult ClaimDetails(int id)
        {
            var claim = claims.Find(c => c.ClaimId == id);
            if (claim == null) return NotFound();
            return View(claim);
        }
    }
}
