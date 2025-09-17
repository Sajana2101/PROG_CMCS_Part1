using Microsoft.AspNetCore.Mvc;

namespace PROG_CMCS_Part1.Controllers
{
    public class CoordinatorController : Controller
    {
        //List of sample claims to hardcode 
        private static List<dynamic> claims = new List<dynamic>
{
    new { ClaimId = 101, LecturerName = "Lecturer A", Month = "August", HoursWorked = 20, HourlyRate = 50, TotalAmount = 1000, Status = "Approved",  FinalisedBy = "Manager Smith", SupportingDocuments = "Document1.pdf" },
    new { ClaimId = 102, LecturerName = "Lecturer A", Month = "September", HoursWorked = 15, HourlyRate = 50, TotalAmount = 750, Status = "Pending Verification",  FinalisedBy = "-", SupportingDocuments = "Document2.pdf" },
    new { ClaimId = 103, LecturerName = "Lecturer A", Month = "October", HoursWorked = 18, HourlyRate = 50, TotalAmount = 900, Status = "Verified – Pending Approval", FinalisedBy = "-", SupportingDocuments = "Document3.pdf" },
    new { ClaimId = 104, LecturerName = "Lecturer A", Month = "August", HoursWorked = 22, HourlyRate = 50, TotalAmount = 1100, Status = "Rejected", FinalisedBy = "Manager Smith", SupportingDocuments = "Document4.pdf" }
};

        //Show dashboard with claims 
        public IActionResult Dashboard(string statusFilter)
        {
            var filteredClaims = claims;
            //filter by status
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                filteredClaims = claims.Where(c => c.Status == statusFilter).ToList();
            }
            //pass filter to the view
            ViewBag.StatusFilter = statusFilter ?? "All";
            //show filtered claims 
            return View(filteredClaims);
        }
        //method to show the details of one claim 
        public IActionResult ClaimDetails(int id)
        {
            var claim = claims.Find(c => c.ClaimId == id);
            if (claim == null) return NotFound();
            return View(claim);
        }
    }
}
