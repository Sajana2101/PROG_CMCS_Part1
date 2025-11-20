using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Models;
using PROG_CMCS_Part1.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PROG_CMCS_Part1.Controllers
{
    // Only lecturers can access this controller
    [Authorize(Roles = "Lecturer")] 
    public class LecturerController : Controller
    {
        // Database context
        private readonly ApplicationDbContext _context;
        // Handles file encryption/decryption
        private readonly FileEncryptionService _encryptionService;
        // User management
        private readonly UserManager<ApplicationUser> _userManager; 

        // Allowed file types and max file size for uploads
        private readonly long _maxFileSize = 5 * 1024 * 1024;
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".jpg", ".jpeg", ".png", ".txt" };

        public LecturerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, FileEncryptionService encryptionService)
        {
            _context = context;
            _userManager = userManager;
            _encryptionService = encryptionService;
        }

       
        // Displays the lecturer's submitted claims with optional status filter
        [HttpGet]
        public async Task<IActionResult> Dashboard(string statusFilter)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Get claims submitted by the logged-in lecturer
            var claims = await _context.Claims
                                       .Where(c => c.UserId == user.Id)
                                       .Include(c => c.User)
                                       .ToListAsync();

            foreach (var c in claims)
                // Load file lists from JSON
                c.LoadDocumentLists(); 

            // Apply status filter if specified
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                claims = claims.Where(c => c.Status == statusFilter).ToList();

            ViewBag.StatusFilter = statusFilter ?? "All";
            ViewBag.PendingCount = claims.Count(c => c.Status == "Pending");
            ViewBag.ApprovedCount = claims.Count(c => c.Status == "Approved");
            ViewBag.RejectedCount = claims.Count(c => c.Status == "Rejected");

            return View(claims);
        }

       
        // Displays form for submitting a new claim
        [HttpGet]
        public async Task<IActionResult> SubmitClaim()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new Claim
            {
                UserId = user.Id,
                LecturerName = $"{user.FirstName} {user.LastName}",
                HourlyRate = user.HourlyRate,
                // Default to current month
                Month = DateTime.UtcNow.ToString("MMMM yyyy") 
            };

            return View(model);
        }

        
        // Handles claim submission with file uploads
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim newClaim, List<IFormFile>? uploadedFiles)
        {
            if (!User.Identity.IsAuthenticated) return Challenge();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            uploadedFiles ??= new List<IFormFile>();

            newClaim.PopulateFromUser(user);
            newClaim.DateSubmitted = DateTime.UtcNow;
            newClaim.Status = "Pending";

            // Validate model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value.Errors.Select(e => (Key: kvp.Key, Error: e.ErrorMessage)))
                    .ToList();

                if (errors.Any())
                    TempData["FormErrors"] = string.Join(" • ", errors.Select(e => $"{e.Key}: {e.Error}"));

                return View(newClaim);
            }

            // Check monthly hours limit
            var month = newClaim.Month ?? DateTime.UtcNow.ToString("MMMM yyyy");
            var monthlyHours = await _context.Claims
                .Where(c => c.UserId == user.Id && c.Month == month)
                .Select(c => (int?)c.HoursWorked)
                .SumAsync() ?? 0;

            if (monthlyHours + newClaim.HoursWorked > user.MaxHours)
            {
                ModelState.AddModelError(string.Empty, $"You have already submitted {monthlyHours} hours this month. Max allowed: {user.MaxHours}.");
                TempData["FormErrors"] = string.Join(" • ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(newClaim);
            }

            try
            {
                _context.Claims.Add(newClaim);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Could not save claim. " + ex.Message);
                TempData["FormErrors"] = string.Join(" • ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(newClaim);
            }

            // Handle file uploads
            var claimFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"claim-{newClaim.Id}");
            Directory.CreateDirectory(claimFolder);

            foreach (var file in uploadedFiles)
            {
                try
                {
                    if (file == null || file.Length == 0) continue;

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (!_allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("", $"File type {ext} not allowed for {file.FileName}.");
                        continue;
                    }

                    if (file.Length > _maxFileSize)
                    {
                        ModelState.AddModelError("", $"File {file.FileName} exceeds {_maxFileSize / (1024 * 1024)} MB limit.");
                        continue;
                    }

                    var encryptedName = $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{ext}.enc";
                    var filePath = Path.Combine(claimFolder, encryptedName);

                    using var stream = file.OpenReadStream();
                    await _encryptionService.EncryptFileAsync(stream, filePath);

                    newClaim.EncryptedDocuments.Add(encryptedName);
                    newClaim.OriginalDocuments.Add(file.FileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Failed to process {file?.FileName}: {ex.Message}");
                }
            }

            newClaim.SaveDocumentLists();

            try
            {
                _context.Claims.Update(newClaim);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update claim with file data. " + ex.Message);
                TempData["FormErrors"] = string.Join(" • ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(newClaim);
            }

            if (ModelState.ErrorCount > 0)
            {
                TempData["FormErrors"] = string.Join(" • ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(newClaim);
            }

            TempData["Success"] = "Your claim has been securely submitted!";
            return RedirectToAction("Dashboard");
        }

        
        // Allows lecturer to download their own uploaded files
        [HttpGet]
        public async Task<IActionResult> DownloadFile(int claimId, string file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) return NotFound();
            // Ensure lecturer owns the claim
            if (claim.UserId != user.Id) return Forbid();

            claim.LoadDocumentLists();

            if (!claim.EncryptedDocuments.Contains(file)) return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"claim-{claimId}", file);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            try
            {
                var memoryStream = await _encryptionService.DecryptFileAsync(filePath);
                var originalName = claim.OriginalDocuments.ElementAtOrDefault(claim.EncryptedDocuments.IndexOf(file)) ?? file;

                return File(memoryStream, "application/octet-stream", originalName);
            }
            catch
            {
                return BadRequest("Error decrypting the file.");
            }
        }

        
        // Displays detailed information about a single claim for the logged-in lecturer
        [HttpGet]
        public async Task<IActionResult> ClaimDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var claim = await _context.Claims
                                      .Include(c => c.User)
                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null) return NotFound();
            if (claim.UserId != user.Id) return Forbid();

            claim.LoadDocumentLists();
            return View(claim);
        }
    }
}
