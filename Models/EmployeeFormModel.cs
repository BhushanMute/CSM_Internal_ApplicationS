using System.ComponentModel.DataAnnotations;

namespace CSMTutorial.Models;

public class EmployeeFormModel
{
    public int? EmployeeId { get; set; }

    // Basic Information
    [Required(ErrorMessage = "Employee Code is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Employee Code must be 1-50 characters")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Employee Name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Employee Name must be 2-50 characters")]
    public string EmployeeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;

    // Device Information
    [StringLength(50)]
    public string? DeviceCode { get; set; }

    [StringLength(255)]
    public string? CardNumber { get; set; }

    // Company Details
    public int CompanyId { get; set; } = 1;
    public string? Company { get; set; }

    public int DepartmentId { get; set; } = 1;
    public string? Department { get; set; }

    [StringLength(255)]
    public string? SubDepartment { get; set; }

    [StringLength(255)]
    public string? Division { get; set; }

    [StringLength(255)]
    public string? Location { get; set; }

    [StringLength(255)]
    public string? Designation { get; set; }

    [StringLength(255)]
    public string? Grade { get; set; }

    [StringLength(255)]
    public string? Team { get; set; }

    public int CategoryId { get; set; } = 1;
    public string? Category { get; set; }

    [Required(ErrorMessage = "Employment Type is required")]
    public string EmploymentType { get; set; } = "Permanent";

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = "Active";

    public string? ShiftRoaster { get; set; }

    // Dates
    public DateTime? DOJ { get; set; }
    public DateTime? DOC { get; set; }
    public DateTime? DOR { get; set; }

    // Device Expiry
    public bool ApplyDeviceExpiryRule { get; set; }
    public DateTime? ExpiryStartDate { get; set; }
    public DateTime? ExpiryEndDate { get; set; }

    // Personal Information
    [StringLength(255)]
    public string? FatherName { get; set; }

    [StringLength(255)]
    public string? MotherName { get; set; }

    [StringLength(255)]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string? ContactNo { get; set; }

    [StringLength(255)]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    public DateTime? DOB { get; set; }

    [StringLength(255)]
    public string? BirthPlace { get; set; }

    [StringLength(255)]
    public string? BloodGroup { get; set; }

    // Nominee
    [StringLength(255)]
    public string? Nominee1 { get; set; }

    [StringLength(255)]
    public string? Nominee2 { get; set; }

    // Address
    [StringLength(500)]
    public string? ResidenceAddress { get; set; }

    [StringLength(500)]
    public string? PermanentAddress { get; set; }

    // Login
    [StringLength(255)]
    public string? LoginName { get; set; }

    // Additional
    [StringLength(4000)]
    public string? Remarks { get; set; }

    // Additional Fields
    [StringLength(255)]
    public string? MaritalStatus { get; set; }

    [StringLength(255)]
    public string? Nationality { get; set; }

    [StringLength(255)]
    public string? AadhaarNumber { get; set; }

    [StringLength(255)]
    public string? PassportNumber { get; set; }

    [StringLength(255)]
    public string? Qualifications { get; set; }

    [StringLength(255)]
    public string? OverallExperience { get; set; }

    [StringLength(255)]
    public string? EmergencyContact { get; set; }

    [StringLength(255)]
    public string? ReferenceDetail { get; set; }
}