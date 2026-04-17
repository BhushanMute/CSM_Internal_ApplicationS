namespace CSMTutorial.Models;

public class EmployeeImportDto
{
    public int RowNumber { get; set; }

    // For existing records (will be updated)
    public int? ExistingEmployeeId { get; set; }
    public string? ExistingEmployeeCode { get; set; }
    public bool IsUpdate { get; set; }  // true = update existing, false = insert new

    // From Excel
    public string EmployeeName { get; set; } = string.Empty;
    public string? EmployeeLastName { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? AadhaarNumber { get; set; }
    public string? Address { get; set; }
    public int? Age { get; set; }
    public string? Email { get; set; }
    public string? Location { get; set; }
    public string ContactNo { get; set; } = string.Empty;

    // Additional fields
    public string? Designation { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? BloodGroup { get; set; }
    public DateTime? DOJ { get; set; }
    public DateTime? DOB { get; set; }
    public string? EmploymentType { get; set; }
    public string? Status { get; set; }
    public string? PermanentAddress { get; set; }

    // Legacy fields
    public string EmployeeCode { get; set; } = string.Empty;
    public string? DeviceCode { get; set; }
    public string? CardNumber { get; set; }
    public string? Company { get; set; }
    public string? Department { get; set; }
    public string? SubDepartment { get; set; }
    public string? Division { get; set; }
    public string? Grade { get; set; }
    public string? Team { get; set; }
    public string? Category { get; set; }
    public string? ShiftRoaster { get; set; }
    public string? BirthPlace { get; set; }
    public string? Nominee1 { get; set; }
    public string? Nominee2 { get; set; }
    public string? ResidenceAddress { get; set; }
    public string? LoginName { get; set; }
    public string? Remarks { get; set; }
    public DateTime? DOC { get; set; }
    public DateTime? DOR { get; set; }
    public bool ApplyDeviceExpiryRule { get; set; }
    public DateTime? ExpiryStartDate { get; set; }
    public DateTime? ExpiryEndDate { get; set; }

    // Validation
    public List<string> ValidationErrors { get; set; } = new();
    public bool IsDuplicate { get; set; }  // Duplicate within the file
    public string? DuplicateReason { get; set; }

    public bool IsValid => !ValidationErrors.Any() && !IsDuplicate;

    public string FullName => string.IsNullOrEmpty(EmployeeLastName)
        ? EmployeeName
        : $"{EmployeeName} {EmployeeLastName}".Trim();

    // Display status for UI
    public string ImportStatus => IsDuplicate ? "Duplicate in File" :
                                  !IsValid ? "Invalid" :
                                  IsUpdate ? "Update" : "New";
}