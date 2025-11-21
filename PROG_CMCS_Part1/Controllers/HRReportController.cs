using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PROG_CMCS_Part1.Data;
using System;
using System.IO;
using System.Linq;

[Authorize(Roles = "HR")]
public class HRReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public HRReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult GeneratePdfReport(DateTime? startDate, DateTime? endDate, string status)
    {
        // Start with DB query
        var claimsQuery = _context.Claims.AsQueryable();

        // Filter by submitted date
        if (startDate.HasValue)
            claimsQuery = claimsQuery.Where(c => c.DateSubmitted >= startDate.Value);

        if (endDate.HasValue)
            claimsQuery = claimsQuery.Where(c => c.DateSubmitted <= endDate.Value);

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status))
            claimsQuery = claimsQuery.Where(c => c.Status == status);

        // Execute query
        var claimsList = claimsQuery.ToList();

        using var stream = new MemoryStream();
        var pdf = new PdfDocument();
        var page = pdf.AddPage();
        var gfx = XGraphics.FromPdfPage(page);

        var fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
        var fontRow = new XFont("Arial", 10, XFontStyle.Regular);

        int xStart = 40;
        int yPoint = 60;
        int rowHeight = 20;

        // Draw table headers
        void DrawHeaders()
        {
            gfx.DrawString("ClaimID", fontHeader, XBrushes.Black, xStart, yPoint);
            gfx.DrawString("Lecturer", fontHeader, XBrushes.Black, xStart + 60, yPoint);
            gfx.DrawString("Month", fontHeader, XBrushes.Black, xStart + 200, yPoint);
            gfx.DrawString("Hours", fontHeader, XBrushes.Black, xStart + 260, yPoint);
            gfx.DrawString("Amount", fontHeader, XBrushes.Black, xStart + 320, yPoint);
            gfx.DrawString("Status", fontHeader, XBrushes.Black, xStart + 400, yPoint);
            gfx.DrawString("Submitted", fontHeader, XBrushes.Black, xStart + 460, yPoint);

            yPoint += rowHeight;
        }

        // Draw title
        gfx.DrawString("Claims Report",
            new XFont("Arial", 20, XFontStyle.Bold),
            XBrushes.Black,
            new XPoint(40, 30));

        DrawHeaders();

        foreach (var claim in claimsList)
        {
            string monthFormatted = claim.Month;

            // If month is "March 2024", format it same way as old JSON logic
            var parts = claim.Month?.Split(' ');
            if (parts?.Length == 2 && int.TryParse(parts[1], out int year))
            {
                monthFormatted = $"{parts[0]} {year}";
            }

            // Calculate total amount
            decimal totalAmount = claim.HoursWorked * claim.HourlyRate;

            gfx.DrawString(claim.Id.ToString(), fontRow, XBrushes.Black, xStart, yPoint);
            gfx.DrawString(claim.LecturerName, fontRow, XBrushes.Black, xStart + 60, yPoint);
            gfx.DrawString(monthFormatted, fontRow, XBrushes.Black, xStart + 200, yPoint);
            gfx.DrawString(claim.HoursWorked.ToString(), fontRow, XBrushes.Black, xStart + 260, yPoint);
            gfx.DrawString(totalAmount.ToString("C"), fontRow, XBrushes.Black, xStart + 320, yPoint);
            gfx.DrawString(claim.Status, fontRow, XBrushes.Black, xStart + 400, yPoint);
            gfx.DrawString(claim.DateSubmitted.ToShortDateString(), fontRow, XBrushes.Black, xStart + 460, yPoint);

            yPoint += rowHeight;

            // New page if needed
            if (yPoint > page.Height - 50)
            {
                page = pdf.AddPage();
                gfx = XGraphics.FromPdfPage(page);
                yPoint = 60;
                DrawHeaders();
            }
        }

        pdf.Save(stream, false);
        return File(stream.ToArray(), "application/pdf", "ClaimsReport.pdf");
    }
}
