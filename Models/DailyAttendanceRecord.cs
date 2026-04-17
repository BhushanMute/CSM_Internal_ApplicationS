namespace CSMTutorial.Models;

// Daily attendance record from AttendanceLogs
public class DailyAttendanceRecord
{
    public int AttendanceLogId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public int CompanyId { get; set; }
    public int DepartmentId { get; set; }
    public string? CompanyName { get; set; }
    public string? DepartmentName { get; set; }
    public string? Designation { get; set; }

    // Timing
    public string? InTime { get; set; }
    public string? InDeviceId { get; set; }
    public string? OutTime { get; set; }
    public string? OutDeviceId { get; set; }
    public double Duration { get; set; }
    public int LateBy { get; set; }
    public int EarlyBy { get; set; }

    // Leave
    public int IsOnLeave { get; set; }
    public string? LeaveType { get; set; }
    public double? LeaveDuration { get; set; }
    public string? LeaveRemarks { get; set; }

    // Weekly Off / Holiday
    public int WeeklyOff { get; set; }
    public int Holiday { get; set; }

    // Punch Records
    public string PunchRecords { get; set; } = string.Empty;

    // Shift
    public int ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public string? ShiftBeginTime { get; set; }
    public string? ShiftEndTime { get; set; }

    // Status
    public double Present { get; set; }
    public double Absent { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string? P1Status { get; set; }
    public string? P2Status { get; set; }
    public string? P3Status { get; set; }

    // Special
    public int IsonSpecialOff { get; set; }
    public string? SpecialOffType { get; set; }

    // Overtime
    public int OverTime { get; set; }
    public int OverTimeE { get; set; }

    // Missed Punch
    public int MissedOutPunch { get; set; }
    public int? MissedInPunch { get; set; }

    // Other
    public string? Remarks { get; set; }
    public int? LossOfHours { get; set; }

    // Computed Properties
    public string FormattedDuration => TimeSpan.FromMinutes(Duration).ToString(@"hh\:mm");
    public string FormattedLateBy => LateBy > 0 ? TimeSpan.FromMinutes(LateBy).ToString(@"hh\:mm") : "-";
    public string FormattedEarlyBy => EarlyBy > 0 ? TimeSpan.FromMinutes(EarlyBy).ToString(@"hh\:mm") : "-";
    public string FormattedOverTime => OverTime > 0 ? TimeSpan.FromMinutes(OverTime).ToString(@"hh\:mm") : "-";

    public string StatusDisplay => StatusCode switch
    {
        "P" => "Present",
        "A" => "Absent",
        "WO" => "Weekly Off",
        "H" => "Holiday",
        "L" => "On Leave",
        "HD" => "Half Day",
        "MP" => "Missed Punch",
        _ => Status
    };
}

 