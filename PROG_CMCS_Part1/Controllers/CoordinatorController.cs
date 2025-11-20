using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Models;
using PROG_CMCS_Part1.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PROG_CMCS_Part1.Controllers
{
    // Only users with Coordinator role can access
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        // DB context for accessing claims and users
        private readonly ApplicationDbContext _context;
        // Handles encryption/decryption of files
        private readonly FileEncryptionService _encryptionService;
        // Max upload size (5 MB) and allowed file extensions
        private readonly long _maxFileSize = 5 * 1024 * 1024;
        private readonly string[] _allowedExtensions =
            { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".jpg", ".jpeg", ".png", ".txt" };

        // Constructor injects services
        public CoordinatorController(ApplicationDbContext context, FileEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        // Show all claims to the coordinator
        public async Task<IActionResult> Dashboard()
        {
            var claims = await _context.Claims
                .OrderByDescending(c => c.DateSubmitted)
                .ToListAsync();

            foreach (var c in claims)
                c.LoadDocumentLists();

            return View(claims);
        }

        // POST: /Coordinator/UpdateStatus
        // status should be ClaimStatus.Verified or ClaimStatus.Rejected
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? comment)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest("Status is required.");

            // Validate allowed statuses for coordinator
            if (status != ClaimStatus.Verified && status != ClaimStatus.Rejected)
                return BadRequest("Invalid status for coordinator action.");

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
                return NotFound();

            // Coordinator only processes claims in Pending state
            if (claim.Status != ClaimStatus.Pending)
                return BadRequest("This claim has already been processed.");

            // Get the currently logged in user from the db
            var currentUserName = User?.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == currentUserName);

            if (user == null)
                return Unauthorized();

            // Apply coordinator action
            claim.Status = status;
            claim.CoordinatorId = user.Id;
            claim.CoordinatorName = $"{user.FirstName} {user.LastName}";
            claim.DateVerified = System.DateTime.UtcNow;

            if (status == ClaimStatus.Rejected)
            {
                if (string.IsNullOrWhiteSpace(comment))
                    return BadRequest("A comment is required when rejecting a claim.");

                claim.CoordinatorComment = comment.Trim();
            }
            else
            {
                // clear any comment for verified claims
                claim.CoordinatorComment = comment?.Trim();
            }

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Dashboard));
        }
        // Allows coordinator to download an encrypted claim document
        public async Task<IActionResult> DownloadFile(int claimId, string file)
        {
            var claim = await _context.Claims.FindAsync(claimId);

            if (claim == null)
                return NotFound();

            claim.LoadDocumentLists();

            if (!claim.EncryptedDocuments.Contains(file))
                return NotFound();

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                $"claim-{claimId}",
                file
            );

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            try
            {
                var memoryStream = await _encryptionService.DecryptFileAsync(filePath);

                var index = claim.EncryptedDocuments.IndexOf(file);
                var originalName = claim.OriginalDocuments.ElementAtOrDefault(index) ?? file;

                return File(memoryStream, "application/octet-stream", originalName);
            }
            catch
            {
                return BadRequest("Error decrypting the file.");
            }
        }
        // Displays details of a specific claim
        [HttpGet]
        public async Task<IActionResult> ClaimDetails(int id)
        {
            var claim = await _context.Claims.FindAsync(id);

            if (claim == null)
                return NotFound();

            claim.LoadDocumentLists();

            return View(claim);
        }
    }
}
