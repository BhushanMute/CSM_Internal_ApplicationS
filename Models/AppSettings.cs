namespace CSMTutorial.Models
{
    public class AppSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int PasswordResetTokenExpiryHours { get; set; } = 24;
        public int MaxFailedLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 30;
    }
}
