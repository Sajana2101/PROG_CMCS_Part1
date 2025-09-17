using Microsoft.AspNetCore.Mvc;

namespace PROG_CMCS_Part1.Controllers
{
    public class LecturerController : Controller
    {
        //sample data for hardcoding claims 
        private static List<dynamic> claims = new List<dynamic>
        {
            new { ClaimId = 101, LecturerName = "Lecturer A", Month = "August", HoursWorked = 20, HourlyRate = 50, TotalAmount = 1000, Status = "Approved",  FinalisedBy = "Manager Smith", SupportingDocuments = "Document1.pdf" },
            new { ClaimId = 102, LecturerName = "Lecturer A", Month = "September", HoursWorked = 15, HourlyRate = 50, TotalAmount = 750, Status = "Pending Verification", FinalisedBy = "-", SupportingDocuments = "Document2.pdf" },
            new { ClaimId = 103, LecturerName = "Lecturer A", Month = "October", HoursWorked = 18, HourlyRate = 50, TotalAmount = 900, Status = "Verified – Pending Approval", FinalisedBy = "-", SupportingDocuments = "Document3.pdf" },
            new { ClaimId = 104, LecturerName = "Lecturer A", Month = "August", HoursWorked = 22, HourlyRate = 50, TotalAmount = 1100, Status = "Rejected",  FinalisedBy = "Coordinator John", SupportingDocuments = "Document4.pdf" }
        };
        //show dashboard with claims 
        public IActionResult Dashboard(string statusFilter)
        {
            var filteredClaims = claims;
            //apply filter

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                filteredClaims = claims.Where(c => c.Status == statusFilter).ToList();
            }
            
            ViewBag.StatusFilter = statusFilter;
            //show filtered content in the view
            return View(filteredClaims);
        }

        //method that will be used for claim submission
        public IActionResult SubmitClaim()
        {
            return View();
        }
        //method to show the details of a claim 
        public IActionResult ClaimDetails(int id)
        {
            var claim = claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null)
            {
                return NotFound();
            }
            return View(claim);
        }


       

    }
}
