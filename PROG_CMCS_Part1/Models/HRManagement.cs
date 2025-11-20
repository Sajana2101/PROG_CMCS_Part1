namespace PROG_CMCS_Part1.Models
{
    // Model used for HR management views (create, edit, display users)
    public class HRManagement
    {
        // User Id (nullable, because it may not exist when creating a new user)
        public string? Id { get; set; }

        // User's first name
        public string FirstName { get; set; }

        // User's last name
        public string LastName { get; set; }

        // User's email address (also used as username)
        public string Email { get; set; }

        // Hourly rate of the user
        public decimal HourlyRate { get; set; }

        // Maximum hours allowed per month for the user
        public int MaxHours { get; set; }

        // Password (nullable because not needed in all views, e.g., details)
        public string? Password { get; set; }

        // Role assigned to the user (HR, Lecturer, Coordinator, Manager)
        public string Role { get; set; }
    }
}
