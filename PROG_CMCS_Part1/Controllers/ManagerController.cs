using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Models;
using PROG_CMCS_Part1.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PROG_CMCS_Part1.Controllers
{
    // Only managers can access this controller
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        // Database context
        private readonly ApplicationDbContext _context;
        // Handles encryption/decryption of uploaded files
        private readonly FileEncryptionService _encryptionService;
        // User management
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagerController(ApplicationDbContext context, FileEncryptionService encryptionService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _encryptionService = encryptionService;
            _userManager = userManager;
        }

        // Displays claims that have been verified and are ready for manager approval/rejection
        [HttpGet]
        public async Task<IActionResult> Dashboard(string statusFilter)
        {
            // Only show claims that are Verified, Approved, or Rejected
            var claims = await _context.Claims
                .Where(c => c.Status == ClaimStatus.Verified || c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Rejected)
                .OrderByDescending(c => c.DateSubmitted)
                .ToListAsync();

            foreach (var c in claims)
                // Load uploaded file lists
                c.LoadDocumentLists();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                claims = claims.Where(c => c.Status == statusFilter).ToList();

            ViewBag.StatusFilter = statusFilter ?? "All";
            return View(claims);
        }
        // Shows detailed information for a specific claim
        [HttpGet]
        public async Task<IActionResult> ClaimDetails(int id)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == id);
            if (claim == null)
                return NotFound();
            // Load encrypted/original document lists
            claim.LoadDocumentLists();
            return View(claim);
        }
        // Allows manager to download an uploaded claim file
        [HttpGet]
        public async Task<IActionResult> DownloadFile(int claimId, string file)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
            if (claim == null)
                return NotFound();

            claim.LoadDocumentLists();
            if (!claim.EncryptedDocuments.Contains(file))
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

        // Allows manager to approve or reject a verified claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus, string? comment)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                return BadRequest("Status is required.");

            // Only allowed statuses for manager
            if (newStatus != ClaimStatus.Approved && newStatus != ClaimStatus.Rejected)
                return BadRequest("Invalid status.");

            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == id);
            if (claim == null)
                return NotFound();
            // Only verified claims can be processed by manager
            if (claim.Status != ClaimStatus.Verified)
                return BadRequest("Only verified claims can be processed by a manager.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            claim.Status = newStatus;
            claim.ManagerId = user.Id;
            claim.ManagerName = $"{user.FirstName} {user.LastName}";
            claim.DateApproved = DateTime.UtcNow;

            if (newStatus == ClaimStatus.Rejected)
                claim.ManagerComment = comment?.Trim();

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            
            return RedirectToAction("Dashboard");
        }
    }
}

