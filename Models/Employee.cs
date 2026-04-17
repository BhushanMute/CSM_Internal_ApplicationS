namespace CSMTutorial.Models;

public class Employee
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string StringCode { get; set; } = string.Empty;
    public int NumericCode { get; set; }
    public string Gender { get; set; } = string.Empty;

    // Company Details
    public int CompanyId { get; set; }
    public int DepartmentId { get; set; }
    public int CategoryId { get; set; }
    public string? Designation { get; set; }
    public string? Location { get; set; }
    public string? Grade { get; set; }
    public string? Team { get; set; }
    public string? SubDepartment { get; set; }
    public string? Division { get; set; }
    public string? WorkPlace { get; set; }

    // Device Information
    public string EmployeeCodeInDevice { get; set; } = string.Empty;
    public string? EmployeeRFIDNumber { get; set; }
    public string? EmployeeDevicePassword { get; set; }
    public string? EmployeeDeviceGroup { get; set; }

    // Employment
    public string EmployementType { get; set; } = "Permanent";
    public string Status { get; set; } = "Active";

    // Dates
    public DateTime? DOJ { get; set; }
    public DateTime? DOR { get; set; }
    public DateTime? DOC { get; set; }
    public DateTime? DOB { get; set; }

    // Personal Information
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? BloodGroup { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Nationality { get; set; }
    public string? PassportNumber { get; set; }
    public string? AadhaarNumber { get; set; }

    // NEW: Age property
    public int? Age { get; set; }

    // Additional properties from database
    public string? ExtensionNo { get; set; }
    public string? EmergencyContact { get; set; }
    public string? OverallExperience { get; set; }
    public string? Qualifications { get; set; }
    public string? ReferenceDetail { get; set; }

    // Nominee
    public string? Nomenee1 { get; set; }
    public string? Nomenee2 { get; set; }

    // Address
    public string? ResidentialAddress { get; set; }
    public string? PermanentAddress { get; set; }

    // Login
    public string? LoginName { get; set; }
    public string? LoginPassword { get; set; }

    // Additional
    public string? Remarks { get; set; }
    public int RecordStatus { get; set; } = 1;

    // Shift & Holiday
    public int? HolidayGroup { get; set; }
    public int? ShiftGroupId { get; set; }
    public int? ShiftRosterId { get; set; }

    // Device Expiry
    public string? DeviceExpiryRule { get; set; }
    public DateTime? DeviceExpiryStartDate { get; set; }
    public DateTime? DeviceExpiryEndDate { get; set; }
    public int? DeviceId { get; set; }
    public DateTime? EnrolledDate { get; set; }
    public int? MasterDeviceId { get; set; }
    public int? MigrateToOtherCryptography { get; set; }
    public int? GeofenceId { get; set; }

    // Custom Fields
    public string? C1 { get; set; }
    public string? C2 { get; set; }
    public string? C3 { get; set; }
    public string? C4 { get; set; }
    public string? C5 { get; set; }
    public string? C6 { get; set; }
    public string? C7 { get; set; }

    // Tracking
    public string? LastModifiedBy { get; set; }
    public int? IsRecieveNotification { get; set; }

    // Navigation Properties (for display - populated by JOINs)
    public string? Company { get; set; }
    public string? CompanyName { get; set; }
    public string? Department { get; set; }
    public string? DepartmentName { get; set; }
    public string? Category { get; set; }
    public string? CategoryName { get; set; }
    public string? ShiftName { get; set; }
}