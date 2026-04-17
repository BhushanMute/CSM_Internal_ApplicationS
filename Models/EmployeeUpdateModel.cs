using System.ComponentModel.DataAnnotations;

namespace CSMTutorial.Models;

public class EmployeeUpdateModel
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required]
    public string Gender { get; set; } = "Male";

    public string? Designation { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    public string? ContactNo { get; set; }

    public DateTime? DOJ { get; set; }
    public DateTime? DOB { get; set; }
    public DateTime? DOR { get; set; }
    public DateTime? DOC { get; set; }

    public string Status { get; set; } = "Active";

    // Changed to non-nullable with defaults to match Employee model
    public int CompanyId { get; set; } = 1;
    public int DepartmentId { get; set; } = 1;
    public int CategoryId { get; set; } = 1;

    public string? SubDepartment { get; set; }
    public string? Division { get; set; }
    public string? EmployementType { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? ResidentialAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? Nomenee1 { get; set; }
    public string? Nomenee2 { get; set; }
    public string? BloodGroup { get; set; }
    public string? Location { get; set; }
    public string? Grade { get; set; }
    public string? Team { get; set; }
    public string? AadhaarNumber { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Nationality { get; set; }
    public string? PassportNumber { get; set; }
    public string? OverallExperience { get; set; }
    public string? Qualifications { get; set; }
    public string? EmergencyContact { get; set; }
    public string? ReferenceDetail { get; set; }
    public string? Remarks { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? ExtensionNo { get; set; }
    public string? WorkPlace { get; set; }
    public string? LoginName { get; set; }

    // Device related
    public string? EmployeeCodeInDevice { get; set; }
    public string? EmployeeRFIDNumber { get; set; }

    public static EmployeeUpdateModel FromEmployee(Employee employee)
    {
        return new EmployeeUpdateModel
        {
            EmployeeId = employee.EmployeeId,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.EmployeeName,
            Gender = employee.Gender ?? "Male",
            Designation = employee.Designation,
            Email = employee.Email,
            ContactNo = employee.ContactNo,
            DOJ = employee.DOJ,
            DOB = employee.DOB,
            DOR = employee.DOR,
            DOC = employee.DOC,
            Status = employee.Status ?? "Active",
            CompanyId = employee.CompanyId,
            DepartmentId = employee.DepartmentId,
            CategoryId = employee.CategoryId,
            SubDepartment = employee.SubDepartment,
            Division = employee.Division,
            EmployementType = employee.EmployementType,
            FatherName = employee.FatherName,
            MotherName = employee.MotherName,
            ResidentialAddress = employee.ResidentialAddress,
            PermanentAddress = employee.PermanentAddress,
            Nomenee1 = employee.Nomenee1,
            Nomenee2 = employee.Nomenee2,
            BloodGroup = employee.BloodGroup,
            Location = employee.Location,
            Grade = employee.Grade,
            Team = employee.Team,
            AadhaarNumber = employee.AadhaarNumber,
            MaritalStatus = employee.MaritalStatus,
            Nationality = employee.Nationality,
            PassportNumber = employee.PassportNumber,
            OverallExperience = employee.OverallExperience,
            Qualifications = employee.Qualifications,
            EmergencyContact = employee.EmergencyContact,
            ReferenceDetail = employee.ReferenceDetail,
            Remarks = employee.Remarks,
            PlaceOfBirth = employee.PlaceOfBirth,
            ExtensionNo = employee.ExtensionNo,
            WorkPlace = employee.WorkPlace,
            LoginName = employee.LoginName,
            EmployeeCodeInDevice = employee.EmployeeCodeInDevice,
            EmployeeRFIDNumber = employee.EmployeeRFIDNumber
        };
    }

    public void ApplyTo(Employee employee)
    {
        employee.EmployeeName = EmployeeName;
        employee.Gender = Gender ?? "Male";
        employee.Designation = Designation;
        employee.Email = Email;
        employee.ContactNo = ContactNo;
        employee.DOJ = DOJ;
        employee.DOB = DOB;
        employee.DOR = DOR;
        employee.DOC = DOC;
        employee.Status = Status ?? "Active";
        employee.CompanyId = CompanyId;
        employee.DepartmentId = DepartmentId;
        employee.CategoryId = CategoryId;
        employee.SubDepartment = SubDepartment;
        employee.Division = Division;
        employee.EmployementType = EmployementType ?? "Permanent";
        employee.FatherName = FatherName;
        employee.MotherName = MotherName;
        employee.ResidentialAddress = ResidentialAddress;
        employee.PermanentAddress = PermanentAddress;
        employee.Nomenee1 = Nomenee1;
        employee.Nomenee2 = Nomenee2;
        employee.BloodGroup = BloodGroup;
        employee.Location = Location;
        employee.Grade = Grade;
        employee.Team = Team;
        employee.AadhaarNumber = AadhaarNumber;
        employee.MaritalStatus = MaritalStatus;
        employee.Nationality = Nationality;
        employee.PassportNumber = PassportNumber;
        employee.OverallExperience = OverallExperience;
        employee.Qualifications = Qualifications;
        employee.EmergencyContact = EmergencyContact;
        employee.ReferenceDetail = ReferenceDetail;
        employee.Remarks = Remarks;
        employee.PlaceOfBirth = PlaceOfBirth;
        employee.ExtensionNo = ExtensionNo;
        employee.WorkPlace = WorkPlace;
        employee.LoginName = LoginName;
        employee.EmployeeCodeInDevice = EmployeeCodeInDevice ?? EmployeeCode;
        employee.EmployeeRFIDNumber = EmployeeRFIDNumber;
    }

    public Employee ToEmployee()
    {
        return new Employee
        {
            EmployeeId = EmployeeId,
            EmployeeCode = EmployeeCode,
            EmployeeName = EmployeeName,
            StringCode = EmployeeCode,
            NumericCode = int.TryParse(EmployeeCode, out var num) ? num : 0,
            Gender = Gender ?? "Male",
            Designation = Designation,
            Email = Email,
            ContactNo = ContactNo,
            DOJ = DOJ,
            DOB = DOB,
            DOR = DOR ?? new DateTime(1900, 1, 1),
            DOC = DOC,
            Status = Status ?? "Active",
            CompanyId = CompanyId,
            DepartmentId = DepartmentId,
            CategoryId = CategoryId,
            SubDepartment = SubDepartment,
            Division = Division,
            EmployementType = EmployementType ?? "Permanent",
            FatherName = FatherName,
            MotherName = MotherName,
            ResidentialAddress = ResidentialAddress,
            PermanentAddress = PermanentAddress,
            Nomenee1 = Nomenee1,
            Nomenee2 = Nomenee2,
            BloodGroup = BloodGroup,
            Location = Location,
            Grade = Grade,
            Team = Team,
            AadhaarNumber = AadhaarNumber,
            MaritalStatus = MaritalStatus,
            Nationality = Nationality,
            PassportNumber = PassportNumber,
            OverallExperience = OverallExperience,
            Qualifications = Qualifications,
            EmergencyContact = EmergencyContact,
            ReferenceDetail = ReferenceDetail,
            Remarks = Remarks,
            PlaceOfBirth = PlaceOfBirth,
            ExtensionNo = ExtensionNo,
            WorkPlace = WorkPlace,
            LoginName = LoginName,
            EmployeeCodeInDevice = EmployeeCodeInDevice ?? EmployeeCode,
            EmployeeRFIDNumber = EmployeeRFIDNumber,
            RecordStatus = 1
        };
    }
}