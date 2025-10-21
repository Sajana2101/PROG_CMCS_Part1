using System;
using System.Collections.Generic;

namespace PROG_CMCS_Part1.Models
{
    public class Claim
    {
        // Unique identifier for each claim
        public int Id { get; set; }
        // Name of the lecturer submitting the claim
        public string? LecturerName { get; set; }
        // Module code related to the claim
        public string ?ModuleCode { get; set; }
        // Total hours worked for this claim
        public int HoursWorked { get; set; }
        // Hourly rate applied to the claim
        public decimal HourlyRate { get; set; }
        // Automatically calculated total payment for the claim
        public decimal TotalAmount => HoursWorked * HourlyRate;
        // Optional comments added by the lecturer
        public string? Comments { get; set; }
        // Current status of the claim (Pending, Approved, Rejected)
        public string? Status { get; set; }
        // Name of the coordinator handling the claim; default to "-" if none assigned
        public string CoordinatorName { get; set; } = "-";
        // Name of the manager approving or rejecting the claim; default to "-"
        public string ManagerName { get; set; } = "-";
        // Month that the claim corresponds to (e.g., "October")
        public string? Month { get; set; }
        // Date and time when the claim was submitted
        public DateTime DateSubmitted { get; set; }
        // List of encrypted file names uploaded with the claim
        public List<string> EncryptedDocuments { get; set; } = new List<string>();
        // List of original file names corresponding to the encrypted documents
        public List<string> OriginalDocuments { get; set; } = new List<string>();
    }
}
