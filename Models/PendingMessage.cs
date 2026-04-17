// Models/PendingMessage.cs
namespace CSMTutorial.Models
{
    public class PendingMessage
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string ParentName { get; set; } = "";
        public string ParentMobile { get; set; } = "";
        public DateTime PunchTime { get; set; }
        public string BranchName { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string MessageType { get; set; } = "";
        public string MessageText { get; set; } = "";
        public string WhatsAppUrl { get; set; } = "";
        public string FormattedMessage { get; set; } = ""; // For preview
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }

        // UI Properties
        public bool IsSelected { get; set; }
        public bool AlreadySent { get; set; }
        public string Gender { get; set; } = "";
    }
}