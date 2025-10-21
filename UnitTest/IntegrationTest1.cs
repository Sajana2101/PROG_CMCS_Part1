using Xunit;
using PROG_CMCS_Part1.Controllers;
using PROG_CMCS_Part1.Data;
using PROG_CMCS_Part1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Unit_Testing.Tests
{
    public class CoordinatorControllerSimpleTests
    {
        private readonly CoordinatorController _controller;

        public CoordinatorControllerSimpleTests()
        {
            // Create controller with encryption service
            _controller = new CoordinatorController(new PROG_CMCS_Part1.Services.FileEncryptionService());
        }

        [Fact]
        public void Dashboard_ReturnsViewWithClaims()
        {
            // Add a test claim
            ClaimData.AddClaim(new Claim { LecturerName = "Test Lecturer", ModuleCode = "CS101" });

            // Call Dashboard action
            var result = _controller.Dashboard() as ViewResult;

            // Check that result and data are correct
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Claim>>(result.Model);
            Assert.Contains(model, c => c.LecturerName == "Test Lecturer");
        }

        [Fact]
        public void UpdateStatus_ChangesClaimStatus()
        {
            // Add a test claim with Pending status
            var claim = new Claim { LecturerName = "Status Test", Status = "Pending" };
            ClaimData.AddClaim(claim);

            // Call UpdateStatus action
            var result = _controller.UpdateStatus(claim.Id, "Verified", "Coordinator1") as RedirectToActionResult;

            // Verify redirect and updated data
            Assert.Equal("Dashboard", result.ActionName);
            var updatedClaim = ClaimData.GetClaimById(claim.Id);
            Assert.Equal("Verified", updatedClaim.Status);
            Assert.Equal("Coordinator1", updatedClaim.CoordinatorName);
        }

        [Fact]
        public void ClaimDetails_ExistingClaim_ReturnsView()
        {
            // Add a test claim
            var claim = new Claim { LecturerName = "Details Test" };
            ClaimData.AddClaim(claim);

            // Call ClaimDetails action
            var result = _controller.ClaimDetails(claim.Id) as ViewResult;

            // Check that result and data are correct
            Assert.NotNull(result);
            var model = Assert.IsType<Claim>(result.Model);
            Assert.Equal("Details Test", model.LecturerName);
        }

        [Fact]
        public void ClaimDetails_InvalidId_ReturnsNotFound()
        {
            // Call ClaimDetails with invalid ID
            var result = _controller.ClaimDetails(-1);

            // Should return NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void UpdateStatus_InvalidId_ReturnsNotFound()
        {
            // Call UpdateStatus with invalid ID
            var result = _controller.UpdateStatus(-1, "Verified", "Coordinator1");

            // Should return NotFound
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
