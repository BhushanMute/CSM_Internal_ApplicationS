namespace CSMTutorial.Models
{
    public class ExistingEmployeeInfo
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public string? Email { get; set; }
        public string? Location { get; set; }
    }
}
