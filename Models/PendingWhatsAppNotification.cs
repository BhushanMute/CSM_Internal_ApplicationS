 
namespace CSMTutorial.Models
{
    public class PendingWhatsAppNotification
    {
        public int EmployeeId { get; set; }
        public string StudentName { get; set; } = "";
        public string ParentName { get; set; } = "";
        public string ParentMobileNo { get; set; } = "";
        public DateTime PunchTime { get; set; }
        public string PunchDirection { get; set; } = "";
        public string BranchName { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int? AttendanceId { get; set; }
    }
}