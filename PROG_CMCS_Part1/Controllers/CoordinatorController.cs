using Microsoft.AspNetCore.Mvc;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Models;
using PROG_CMCS_Part1.Services;

namespace PROG_CMCS_Part1.Controllers
{
    public class CoordinatorController : Controller
    {

        // Service for handling file encryption and decryption
        private readonly FileEncryptionService _encryptionService;
        // Maximum file size allowed for uploads (5 MB)
        private readonly long _maxFileSize = 5 * 1024 * 1024;
        // List of file extensions allowed for claims
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".jpg", ".jpeg", ".png", ".txt" };
        // Inject the file encryption service
        public CoordinatorController(FileEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }
        // Display all claims on the coordinator dashboard
        [HttpGet]
        public IActionResult Dashboard()
        {
            var claims = ClaimData.GetAllClaims();
            return View(claims); 
        }

        // Update the status and coordinator assigned to a specific claim
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


        // Download and decrypt a specific file attached to a claim
        [HttpGet]
        public async Task<IActionResult> DownloadFile(int claimId, string file)
        {
            var claim = ClaimData.GetClaimById(claimId);
            // Ensure claim and file exist
            if (claim == null || !claim.EncryptedDocuments.Contains(file))
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"claim-{claimId}", file);
            // Ensure file exists on disk
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            try
            {
                // Decrypt the file into memory
                var memoryStream = await _encryptionService.DecryptFileAsync(filePath);
                // Use original filename for download
                var originalName = claim.OriginalDocuments[claim.EncryptedDocuments.IndexOf(file)];

                return File(memoryStream, "application/octet-stream", originalName);
            }
            catch

            {
                // Return error if decryption fails
                return BadRequest("Error decrypting the file.");
            }
        }
        // Show details for a single claim
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
