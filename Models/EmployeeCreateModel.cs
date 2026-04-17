using System.ComponentModel.DataAnnotations;

namespace CSMTutorial.Models
{
    public class EmployeeCreateModel
    {
        [Required(ErrorMessage = "Employee Code is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Employee Code must be between 2 and 50 characters")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Employee Name must be between 2 and 100 characters")]
        public string EmployeeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Designation { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20)]
        public string? ContactNo { get; set; }

        public DateTime? DOJ { get; set; }

        [Required]
        public string Status { get; set; } = "Active";

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
        public decimal? Salary { get; set; }
    }
}
