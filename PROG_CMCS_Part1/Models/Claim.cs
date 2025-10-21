using System;
using System.Collections.Generic;

namespace PROG_CMCS_Part1.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public string LecturerName { get; set; }
        public string ModuleCode { get; set; }
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount => HoursWorked * HourlyRate;
        public string? Comments { get; set; }
        public string Status { get; set; }
        public string CoordinatorName { get; set; } = "-";
        
        public string ManagerName { get; set; } = "-";
      
        public string Month { get; set; }
        public DateTime DateSubmitted { get; set; }

        public List<string> EncryptedDocuments { get; set; } = new List<string>();
        public List<string> OriginalDocuments { get; set; } = new List<string>();
    }
}
