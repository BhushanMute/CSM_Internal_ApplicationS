namespace CSMTutorial.Models
{
    public class EmployeeListItem
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public string? Designation { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public string? EmployementType { get; set; }
        public DateTime? DOJ { get; set; }
        public int CompanyId { get; set; }
        public int DepartmentId { get; set; }
        public string? CompanyName { get; set; }
        public string? DepartmentName { get; set; }
    }
}
