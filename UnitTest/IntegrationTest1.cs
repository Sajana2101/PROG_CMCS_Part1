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
           
            _controller = new CoordinatorController(new PROG_CMCS_Part1.Services.FileEncryptionService());
        }

        [Fact]
        public void Dashboard_ReturnsViewWithClaims()
        {
          
            ClaimData.AddClaim(new Claim { LecturerName = "Test Lecturer", ModuleCode = "CS101" });

           
            var result = _controller.Dashboard() as ViewResult;

           
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Claim>>(result.Model);
            Assert.Contains(model, c => c.LecturerName == "Test Lecturer");
        }

        [Fact]
        public void UpdateStatus_ChangesClaimStatus()
        {
           
            var claim = new Claim { LecturerName = "Status Test", Status = "Pending" };
            ClaimData.AddClaim(claim);

          
            var result = _controller.UpdateStatus(claim.Id, "Verified", "Coordinator1") as RedirectToActionResult;

           
            Assert.Equal("Dashboard", result.ActionName);
            var updatedClaim = ClaimData.GetClaimById(claim.Id);
            Assert.Equal("Verified", updatedClaim.Status);
            Assert.Equal("Coordinator1", updatedClaim.CoordinatorName);
        }

        [Fact]
        public void ClaimDetails_ExistingClaim_ReturnsView()
        {
           
            var claim = new Claim { LecturerName = "Details Test" };
            ClaimData.AddClaim(claim);

          
            var result = _controller.ClaimDetails(claim.Id) as ViewResult;

           
            Assert.NotNull(result);
            var model = Assert.IsType<Claim>(result.Model);
            Assert.Equal("Details Test", model.LecturerName);
        }

        [Fact]
        public void ClaimDetails_InvalidId_ReturnsNotFound()
        {
           
            var result = _controller.ClaimDetails(-1);

           
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void UpdateStatus_InvalidId_ReturnsNotFound()
        {
          
            var result = _controller.UpdateStatus(-1, "Verified", "Coordinator1");

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
