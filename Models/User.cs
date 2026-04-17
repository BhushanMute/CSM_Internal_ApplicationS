namespace CSMTutorial.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string LoginName { get; set; } = string.Empty;
        public string? LoginPassword { get; set; }
        public string? Role { get; set; }
        public int IsAdmin { get; set; }
        public int AccessI { get; set; }
        public int? RecordStatus { get; set; }
        public string? C1 { get; set; } // Full Name stored here
        public string? C2 { get; set; }
        public string? C3 { get; set; }
        public string? C4 { get; set; }
        public string? C5 { get; set; }
        public string? C6 { get; set; }
        public string? C7 { get; set; }
        public int? IsWebAPI { get; set; }
        public int? MigrateToOtherCryptography { get; set; }

        // New authentication fields
        public string? Email { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public int EmailVerified { get; set; }
        public string? EmailVerificationToken { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int FailedLoginAttempts { get; set; }
        public int IsLocked { get; set; }
        public DateTime? LockoutEndDate { get; set; }
    }
}
