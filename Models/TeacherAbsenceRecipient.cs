namespace CSMTutorial.Models
{
    public class TeacherAbsenceRecipient
    {
        public long RecipientId { get; set; }
        public long MessageId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string StudentName { get; set; }
        public string ParentMobile { get; set; }
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public string Status { get; set; } // Pending, Sent, Failed
        public int RetryCount { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? CreatedDate { get; set; }

        // Navigation
        public string CompanyName { get; set; }
        public string DepartmentName { get; set; }
        public string MessageBody { get; set; }
        public string Reason { get; set; }
        public string MessageType { get; set; }
        public string CreatedBy { get; set; }

    }
}
