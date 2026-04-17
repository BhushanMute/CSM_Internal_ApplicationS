namespace CSMTutorial.Models
{
    public class WhatsAppDashboardStats
    {
        public int TotalSent { get; set; }
        public int TotalPending { get; set; }
        public int TotalSkipped { get; set; }
        public int TodayMessages { get; set; }
        public decimal SuccessRate { get; set; }
    }
}
