// Models/WhatsAppMessageLog.cs
namespace CSMTutorial.Models
{
    public class WhatsAppMessageLog
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string StudentName { get; set; } = "";
        public string Data { get; set; } = "";
        public string ParentMobileNo { get; set; } = "";
        public string MessageType { get; set; } = "";
        public string Message { get; set; } = "";
        public bool Status { get; set; }
        public string? APIResponse { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}