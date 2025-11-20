using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace PROG_CMCS_Part1.Models
{
    public static class ClaimStatus
    {
        public const string Pending = "Pending";
        public const string Verified = "Verified";
        public const string Rejected = "Rejected";
        public const string Approved = "Approved";
    }

    public class Claim
    {
        [Key]
        public int Id { get; set; }

        
        public string? LecturerName { get; set; }
        public string? ModuleCode { get; set; }

        [BindNever]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        [BindNever]
        public ApplicationUser? User { get; set; }

        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }

        [NotMapped]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        public string? Comments { get; set; }

       
        public string? Status { get; set; } = ClaimStatus.Pending;

        public string? CoordinatorId { get; set; }
        public string CoordinatorName { get; set; } = "-";
        public string? CoordinatorComment { get; set; }
        public DateTime? DateVerified { get; set; }

      
        public string? ManagerId { get; set; }
        public string ManagerName { get; set; } = "-";
        public string? ManagerComment { get; set; }
        public DateTime? DateApproved { get; set; }

        public string? Month { get; set; }
        public DateTime DateSubmitted { get; set; }

        
        [NotMapped]
        public List<string> EncryptedDocuments { get; set; } = new List<string>();

        [NotMapped]
        public List<string> OriginalDocuments { get; set; } = new List<string>();

        public string EncryptedDocumentsJson { get; set; } = "[]";
        public string OriginalDocumentsJson { get; set; } = "[]";

        public void LoadDocumentLists()
        {
            EncryptedDocuments = JsonSerializer.Deserialize<List<string>>(EncryptedDocumentsJson) ?? new();
            OriginalDocuments = JsonSerializer.Deserialize<List<string>>(OriginalDocumentsJson) ?? new();
        }

        public void SaveDocumentLists()
        {
            EncryptedDocumentsJson = JsonSerializer.Serialize(EncryptedDocuments);
            OriginalDocumentsJson = JsonSerializer.Serialize(OriginalDocuments);
        }

        public void PopulateFromUser(ApplicationUser user)
        {
            if (user == null) return;

            UserId = user.Id;
            LecturerName = $"{user.FirstName} {user.LastName}";
            HourlyRate = user.HourlyRate;
        }
    }
}
