namespace CSMTutorial.Models
{
    public class AttendanceSummary
    {
        public int TotalRecords { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int EarlyGoingCount { get; set; }
        public int OnLeaveCount { get; set; }
        public int WeeklyOffCount { get; set; }
        public int HolidayCount { get; set; }
        public int MissedPunchCount { get; set; }
        public int OvertimeCount { get; set; }
        public double TotalOvertimeMinutes { get; set; }
        public double AvgDuration { get; set; }

        public string FormattedAvgDuration => TimeSpan.FromMinutes(AvgDuration).ToString(@"hh\:mm");
    }
}
