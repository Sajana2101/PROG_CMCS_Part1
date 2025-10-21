using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PROG_CMCS_Part1.Models;

namespace PROG_CMCS_Part1.Data
{
    public static class ClaimData
    {
        private static readonly string FilePath =
            Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "claims.json");

        private static List<Claim> _claims = new List<Claim>();
        private static int _nextId = 1;
        private static readonly object _lock = new object();

        static ClaimData()
        {
            LoadClaimsFromFile();
        }

        public static void AddClaim(Claim claim)
        {
            lock (_lock)
            {
                claim.Id = _nextId++;
                claim.DateSubmitted = DateTime.Now;

                claim.EncryptedDocuments ??= new List<string>();
                claim.OriginalDocuments ??= new List<string>();

                if (string.IsNullOrWhiteSpace(claim.Status))
                    claim.Status = "Submitted";

                _claims.Add(claim);
                SaveClaimsToFile();
            }
        }

        public static List<Claim> GetAllClaims()
        {
            lock (_lock) { return _claims; }
        }

        public static Claim GetClaimById(int id)
        {
            lock (_lock) { return _claims.FirstOrDefault(c => c.Id == id); }
        }

        public static void UpdateClaim(Claim updatedClaim)
        {
            lock (_lock)
            {
                var existingClaim = _claims.FirstOrDefault(c => c.Id == updatedClaim.Id);
                if (existingClaim != null)
                {
                    existingClaim.LecturerName = updatedClaim.LecturerName;
                    existingClaim.ModuleCode = updatedClaim.ModuleCode;
                    existingClaim.HoursWorked = updatedClaim.HoursWorked;
                    existingClaim.HourlyRate = updatedClaim.HourlyRate;
                    existingClaim.Month = updatedClaim.Month;
                    existingClaim.Status = updatedClaim.Status;
                    existingClaim.Comments = updatedClaim.Comments;
                    existingClaim.CoordinatorName = updatedClaim.CoordinatorName;

                    SaveClaimsToFile();
                }
            }
        }

        public static void DeleteClaim(int id)
        {
            lock (_lock)
            {
                var claim = _claims.FirstOrDefault(c => c.Id == id);
                if (claim != null)
                {
                    _claims.Remove(claim);
                    SaveClaimsToFile();
                }
            }
        }

        public static void AppendEncryptedDocuments(int claimId, IEnumerable<string> files)
        {
            lock (_lock)
            {
                var claim = _claims.FirstOrDefault(c => c.Id == claimId);
                if (claim == null) return;
                claim.EncryptedDocuments.AddRange(files);
                SaveClaimsToFile();
            }
        }

        public static void AppendOriginalDocuments(int claimId, IEnumerable<string> files)
        {
            lock (_lock)
            {
                var claim = _claims.FirstOrDefault(c => c.Id == claimId);
                if (claim == null) return;
                claim.OriginalDocuments.AddRange(files);
                SaveClaimsToFile();
            }
        }

        private static void SaveClaimsToFile()
        {
            try
            {
                var directory = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonSerializer.Serialize(_claims, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving claims: {ex.Message}");
            }
        }

        private static void LoadClaimsFromFile()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    _claims = JsonSerializer.Deserialize<List<Claim>>(json) ?? new List<Claim>();
                    _nextId = _claims.Any() ? _claims.Max(c => c.Id) + 1 : 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading claims: {ex.Message}");
                _claims = new List<Claim>();
            }
        }
    }
}
