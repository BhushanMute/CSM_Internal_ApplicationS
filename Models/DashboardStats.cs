namespace CSMTutorial.Models
{
    public class DashboardStats
    {
        public int TotalStudents { get; set; }
        public int PresentToday { get; set; }
        public int AbsentToday { get; set; }
        public int TotalSent { get; set; }
        public double AttendancePercentage => TotalStudents > 0
            ? Math.Round((double)PresentToday / TotalStudents * 100, 2)
            : 0;
    }
}