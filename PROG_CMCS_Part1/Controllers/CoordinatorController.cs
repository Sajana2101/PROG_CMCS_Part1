using Microsoft.AspNetCore.Mvc;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Models;
using PROG_CMCS_Part1.Services;

namespace PROG_CMCS_Part1.Controllers
{
    public class CoordinatorController : Controller
    {
      
        private readonly FileEncryptionService _encryptionService;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".jpg", ".jpeg", ".png", ".txt" };

        public CoordinatorController(FileEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }
        [HttpGet]
        public IActionResult Dashboard()
        {
            var claims = ClaimData.GetAllClaims();
            return View(claims); 
        }

       
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status, string coordinatorName)
        {
            var claim = ClaimData.GetClaimById(id);
            if (claim == null)
                return NotFound();

            claim.Status = status;
            claim.CoordinatorName = coordinatorName;

            ClaimData.UpdateClaim(claim);

            return RedirectToAction("Dashboard");
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
        
        [HttpGet]
        public IActionResult ClaimDetails(int id)
        {
            var claim = ClaimData.GetClaimById(id);
            if (claim == null)
                return NotFound();

            return View(claim); 
        }
    }
}
