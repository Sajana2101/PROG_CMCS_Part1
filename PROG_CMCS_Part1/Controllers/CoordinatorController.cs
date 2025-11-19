using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Models;
using PROG_CMCS_Part1.Services;

namespace PROG_CMCS_Part1.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileEncryptionService _encryptionService;

        private readonly long _maxFileSize = 5 * 1024 * 1024;
        private readonly string[] _allowedExtensions =
            { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".jpg", ".jpeg", ".png", ".txt" };

        public CoordinatorController(ApplicationDbContext context, FileEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var claims = await _context.Claims.ToListAsync();

            foreach (var c in claims)
                c.LoadDocumentLists();

            return View(claims);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string coordinatorName)
        {
            var claim = await _context.Claims.FindAsync(id);

            if (claim == null)
                return NotFound();

            claim.Status = status;
            claim.CoordinatorName = coordinatorName;

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }


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
                var originalName = claim.OriginalDocuments[index];

                return File(memoryStream, "application/octet-stream", originalName);
            }
            catch
            {
                return BadRequest("Error decrypting the file.");
            }
        }

     
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
