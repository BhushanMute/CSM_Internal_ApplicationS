namespace CSMTutorial.Models
{
    public class TeacherAbsenceMessage
    {
        public long MessageId { get; set; }
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public string Subject { get; set; }
        public string Reason { get; set; }
        public string MessageBody { get; set; }
        public string MessageType { get; set; } // "Today" or "Tomorrow"
        public DateTime? ScheduledDate { get; set; }
        public DateTime? ScheduledTime { get; set; }
        public string Status { get; set; }
        public int? TotalRecipients { get; set; }
        public int? SentCount { get; set; }
        public int? FailedCount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? SentDate { get; set; }

        // Navigation
        public string CompanyName { get; set; }
        public string DepartmentName { get; set; }
    }
}
