namespace PROG_CMCS_Part1.Models
{
    public class HRManagement
    {
        public string? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal HourlyRate { get; set; }
        public int MaxHours { get; set; }
        public string? Password { get; set; }
        public string Role { get; set; }
    }
}