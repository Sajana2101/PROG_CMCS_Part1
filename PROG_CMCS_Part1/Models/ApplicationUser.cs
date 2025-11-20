using Microsoft.AspNetCore.Identity;

// Custom user class that extends IdentityUser
public class ApplicationUser : IdentityUser
{
    // User's first name
    public string FirstName { get; set; }

    // User's last name
    public string LastName { get; set; }

    // Hourly rate for calculating claims or payments
    public decimal HourlyRate { get; set; }

    // Maximum hours the user can work in a month
    public int MaxHours { get; set; }
}
