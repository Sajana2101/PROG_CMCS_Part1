using PROG_CMCS_Part1.Models;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PROG_CMCS_Part1.Data
{
    // DbContext for the application, includes Identity tables for authentication
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            // Pass options to the base IdentityDbContext
            : base(options)
        {
        }

        // Customize EF Core model mappings
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Ensure Identity tables are configured
            base.OnModelCreating(builder);
            // Set precision for decimal HourlyRate in ApplicationUser
            builder.Entity<ApplicationUser>()
                   .Property(u => u.HourlyRate)
                   .HasPrecision(18, 2);
            // Set precision for decimal HourlyRate in Claim entity
            builder.Entity<Claim>()
               .Property(u => u.HourlyRate)
               .HasPrecision(18, 2);
            // enable cascade delete 
            builder.Entity<ApplicationUser>()
                .HasMany<IdentityUserClaim<string>>()      // Identity claims table
                .WithOne()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
        // DbSet representing the Claims table
        public DbSet<Claim> Claims { get; set; }

    }
}
