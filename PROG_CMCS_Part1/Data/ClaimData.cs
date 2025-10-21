using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PROG_CMCS_Part1.Models;

namespace PROG_CMCS_Part1.Data
{
    // Static class for managing claim data in memory and persisting to JSON file
    public static class ClaimData
    {
        // Path to JSON file storing claims
        private static readonly string FilePath =
            Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "claims.json");
        // In-memory list of all claims
        private static List<Claim> _claims = new List<Claim>();
        // Tracks the next claim ID to assign
        private static int _nextId = 1;
        // Lock object for thread-safe operations
        private static readonly object _lock = new object();
        // Static constructor loads claims from file when class is first used
        static ClaimData()
        {
            LoadClaimsFromFile();
        }
        // Adds a new claim and persists it to the file
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
        // Returns all claims

        public static List<Claim> GetAllClaims()
        {
            lock (_lock) { return _claims; }
        }
        // Returns a single claim by its ID, or null if not found
        public static Claim GetClaimById(int id)
        {
            lock (_lock) { return _claims.FirstOrDefault(c => c.Id == id); }
        }
        // Updates an existing claim and saves changes to the file
        public static void UpdateClaim(Claim updatedClaim)
        {
            lock (_lock)
            {
                var existingClaim = _claims.FirstOrDefault(c => c.Id == updatedClaim.Id);
                if (existingClaim != null)
                {
                    // Update all relevant fields
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
        // Deletes a claim by ID and persists the change
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
        // Adds new encrypted file names to a claim
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
        // Adds new original file names to a claim
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
        // Saves the in-memory claim list to a JSON file
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
        // Loads claims from the JSON file into memory
        private static void LoadClaimsFromFile()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    _claims = JsonSerializer.Deserialize<List<Claim>>(json) ?? new List<Claim>();
                    // Set next ID based on the highest existing claim ID
                    _nextId = _claims.Any() ? _claims.Max(c => c.Id) + 1 : 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading claims: {ex.Message}");
                // Reset list if error occurs
                _claims = new List<Claim>();
            }
        }
    }
}
