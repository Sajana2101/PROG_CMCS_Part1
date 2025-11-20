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
    // Unique identifier for each claim
    public class Claim
    {
        [Key]
        public int Id { get; set; }
        // Name of the lecturer submitting the claim
        //Lecturer info 
        public string? LecturerName { get; set; }
        // Module code related to the claim
        public string? ModuleCode { get; set; }
        // Total hours worked for this claim
        [BindNever]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        [BindNever]
        public ApplicationUser? User { get; set; }

        public int HoursWorked { get; set; }
        // Hourly rate applied to the claim
        public decimal HourlyRate { get; set; }
        // Automatically calculated total payment for the claim
        [NotMapped]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        public string? Comments { get; set; }
        // Current status of the claim (Pending, Approved, Rejected)
        public string? Status { get; set; } = ClaimStatus.Pending;
        // Name of the coordinator handling the claim; default to "-" if none assigned

        // Coordinator fields (new)
        public string? CoordinatorId { get; set; }
        // Name of the manager approving or rejecting the claim; default to "-"
        public string CoordinatorName { get; set; } = "-";

        public string? CoordinatorComment { get; set; }
        public DateTime? DateVerified { get; set; }

        // Manager fields 
        public string? ManagerId { get; set; }
        public string ManagerName { get; set; } = "-";
        // Month that the claim corresponds to (e.g., "October")
        public string? ManagerComment { get; set; }
        public DateTime? DateApproved { get; set; }

        public string? Month { get; set; }
        // Date and time when the claim was submitted
        public DateTime DateSubmitted { get; set; }

        
        [NotMapped]
        public List<string> EncryptedDocuments { get; set; } = new List<string>();
        // List of encrypted file names uploaded with the claim

        // Document storage (kept as before)
        [NotMapped]
        public List<string> OriginalDocuments { get; set; } = new List<string>();
        // JSON strings used to persist lists of file names in the database.
        public string EncryptedDocumentsJson { get; set; } = "[]";
        public string OriginalDocumentsJson { get; set; } = "[]";
        // Loads the lists of encrypted and original filenames from their JSON representations.
        // Ensures that the lists are always initialized, even if the JSON is empty or null.
        public void LoadDocumentLists()
        {
            EncryptedDocuments = JsonSerializer.Deserialize<List<string>>(EncryptedDocumentsJson) ?? new();
            OriginalDocuments = JsonSerializer.Deserialize<List<string>>(OriginalDocumentsJson) ?? new();
        }
        // Serializes the current lists of filenames into JSON for database storage.
        public void SaveDocumentLists()
        {
            EncryptedDocumentsJson = JsonSerializer.Serialize(EncryptedDocuments);
            OriginalDocumentsJson = JsonSerializer.Serialize(OriginalDocuments);
        }
        // Populates claim-specific fields using data from an ApplicationUser object.
        public void PopulateFromUser(ApplicationUser user)
        {
            if (user == null) return;

            UserId = user.Id;
            LecturerName = $"{user.FirstName} {user.LastName}";
            HourlyRate = user.HourlyRate;
        }
    }
}
