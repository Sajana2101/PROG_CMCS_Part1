using Microsoft.AspNetCore.Mvc;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Models;
using PROG_CMCS_Part1.Services;

namespace PROG_CMCS_Part1.Controllers
{
    public class ManagerController : Controller
    {
        private readonly FileEncryptionService _encryptionService;
        private readonly long _maxFileSize = 5 * 1024 * 1024; 
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".jpg", ".jpeg", ".png", ".txt" };

        public ManagerController(FileEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }
        [HttpGet]
        public IActionResult Dashboard(string statusFilter)
        {
            var claims = ClaimData.GetAllClaims();

         
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                claims = claims.Where(c => c.Status == statusFilter).ToList();

            ViewBag.StatusFilter = statusFilter ?? "All";
            return View(claims); 
        }

      
        [HttpGet]
        public IActionResult ClaimDetails(int id)
        {
            var claim = ClaimData.GetClaimById(id);
            if (claim == null)
                return NotFound();

            return View(claim); 
        }



        [HttpGet]
        public async Task<IActionResult> DownloadFile(int claimId, string file)
        {
            var claim = ClaimData.GetClaimById(claimId);
            if (claim == null || !claim.EncryptedDocuments.Contains(file))
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"claim-{claimId}", file);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            try
            {
                var memoryStream = await _encryptionService.DecryptFileAsync(filePath);
                var originalName = claim.OriginalDocuments[claim.EncryptedDocuments.IndexOf(file)];

                return File(memoryStream, "application/octet-stream", originalName);
            }
            catch
            {
                return BadRequest("Error decrypting the file.");
            }
        }
        
        [HttpPost]
        public IActionResult UpdateStatus(int id, string newStatus, string managerName)
        {
            var claim = ClaimData.GetClaimById(id);
            if (claim == null)
                return NotFound();

            claim.Status = newStatus;                
            claim.ManagerName = managerName;        
            ClaimData.UpdateClaim(claim);            

            TempData["Success"] = $"Claim {id} has been {newStatus.ToLower()} by {managerName}.";
            return RedirectToAction("Dashboard");
        }



    }
}
