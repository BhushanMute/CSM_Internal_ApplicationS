// Models/MessageLog.cs
namespace CSMTutorial.Models
{
    public class MessageLog
    {
        public int LogId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string ParentMobile { get; set; } = "";
        public string MessageType { get; set; } = "";
        public string MessageText { get; set; } = "";
        public string WhatsAppUrl => string.IsNullOrWhiteSpace(ParentMobile) ? "" : $"https://wa.me/{ParentMobile}";
        public string Status { get; set; } = "";
        public string SentBy { get; set; } = "";
        public DateTime? SentAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Notes { get; set; }
    }
}