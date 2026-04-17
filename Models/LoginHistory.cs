namespace CSMTutorial.Models
{
    public class LoginHistory
    {
        public int LoginHistoryId { get; set; }
        public int UserId { get; set; }
        public string LoginName { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string LoginStatus { get; set; } = string.Empty; // Success, Failed, Locked
        public string? FailureReason { get; set; }
    }
}
