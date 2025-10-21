using Microsoft.AspNetCore.Mvc;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Models;
using PROG_CMCS_Part1.Services;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace PROG_CMCS_Part1.Controllers
{
    public class LecturerController : Controller
    {
        private readonly FileEncryptionService _encryptionService;
        private readonly long _maxFileSize = 5 * 1024 * 1024; 
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".jpg", ".jpeg", ".png", ".txt" };

        public LecturerController(FileEncryptionService encryptionService)
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
        public IActionResult SubmitClaim() => View();

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(
     string lecturerName,
     string moduleCode,
     string month,
     int hoursWorked,
     decimal hourlyRate,
     string? comments,
     List<IFormFile>? uploadedFiles)
        {
            if (!ModelState.IsValid)
                return View();

            uploadedFiles ??= new List<IFormFile>();

            var claim = new Claim
            {
                LecturerName = lecturerName,
                ModuleCode = moduleCode,
                Month = month,
                HoursWorked = hoursWorked,
                HourlyRate = hourlyRate,
                Comments = comments,
                Status = "Pending",
                DateSubmitted = DateTime.Now
            };

           
            ClaimData.AddClaim(claim);

            var claimFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"claim-{claim.Id}");
            Directory.CreateDirectory(claimFolder);

            foreach (var file in uploadedFiles)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!_allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", $"File type {ext} not allowed.");
                    return View(claim);
                }

                if (file.Length > _maxFileSize)
                {
                    ModelState.AddModelError("", $"File {file.FileName} exceeds {_maxFileSize / (1024 * 1024)} MB limit.");
                    return View(claim);
                }

               
                var encryptedName = $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{ext}.enc";
                var filePath = Path.Combine(claimFolder, encryptedName);

               
                using var stream = file.OpenReadStream();
                await _encryptionService.EncryptFileAsync(stream, filePath);

               
                claim.EncryptedDocuments.Add(encryptedName);
                claim.OriginalDocuments.Add(file.FileName);
            }

           
            ClaimData.UpdateClaim(claim);

            TempData["Success"] = "Your claim has been securely submitted!";
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
