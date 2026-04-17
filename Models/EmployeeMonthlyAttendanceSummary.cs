namespace CSMTutorial.Models
{
    public class EmployeeMonthlyAttendanceSummary
    {
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyGoingDays { get; set; }
        public int LeaveDays { get; set; }
        public int WeeklyOffDays { get; set; }
        public int HolidayDays { get; set; }
        public int MissedPunchDays { get; set; }
        public int HalfDays { get; set; }
        public int TotalOvertimeMinutes { get; set; }
        public double TotalWorkingMinutes { get; set; }
        public double AvgWorkingMinutes { get; set; }
        public double TotalLateMinutes { get; set; }
        public double TotalEarlyMinutes { get; set; }

        // Computed
        public string FormattedTotalWorking => TimeSpan.FromMinutes(TotalWorkingMinutes).ToString(@"hh\:mm");
        public string FormattedAvgWorking => TimeSpan.FromMinutes(AvgWorkingMinutes).ToString(@"hh\:mm");
        public string FormattedTotalOT => TimeSpan.FromMinutes(TotalOvertimeMinutes).ToString(@"hh\:mm");
        public string FormattedTotalLate => TimeSpan.FromMinutes(TotalLateMinutes).ToString(@"hh\:mm");
        public string FormattedTotalEarly => TimeSpan.FromMinutes(TotalEarlyMinutes).ToString(@"hh\:mm");
        public double PresentPercentage => TotalDays > 0 ? Math.Round((double)PresentDays / TotalDays * 100, 1) : 0;
    }
}
