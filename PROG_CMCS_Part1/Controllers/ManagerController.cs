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
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileEncryptionService _encryptionService;

        private readonly long _maxFileSize = 5 * 1024 * 1024;
        private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".jpg", ".jpeg", ".png", ".txt" };

        public ManagerController(ApplicationDbContext context, FileEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(string statusFilter)
        {
            var claims = await _context.Claims.ToListAsync();

            foreach (var c in claims)
                c.LoadDocumentLists();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                claims = claims.Where(c => c.Status == statusFilter).ToList();

            ViewBag.StatusFilter = statusFilter ?? "All";
            return View(claims);
        }

        [HttpGet]
        public async Task<IActionResult> ClaimDetails(int id)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == id);
            if (claim == null)
                return NotFound();

            claim.LoadDocumentLists();
            return View(claim);
        }

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

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus, string managerName)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == id);
            if (claim == null)
                return NotFound();

            claim.Status = newStatus;
            claim.ManagerName = managerName;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Claim {id} has been {newStatus.ToLower()} by {managerName}.";
            return RedirectToAction("Dashboard");
        }
    }
}
