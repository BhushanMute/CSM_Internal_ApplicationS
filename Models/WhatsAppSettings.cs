// Models/WhatsAppSettings.cs
namespace CSMTutorial.Models
{
    public class WhatsAppSettings
    {
        public int Id { get; set; }

        // ===== API Configuration =====
        public bool IsEnabled { get; set; } = true;

        // ✅ New name - maps to IsEnabled
        public bool IsActive
        {
            get => IsEnabled;
            set => IsEnabled = value;
        }

        public string APIProvider { get; set; } = "Gupshup";

        // ✅ New name - maps to APIProvider
        public string ApiProvider
        {
            get => APIProvider;
            set => APIProvider = value;
        }

        public string SenderNumber { get; set; } = "";

        // ✅ New name - maps to SenderNumber
        public string FromNumber
        {
            get => SenderNumber;
            set => SenderNumber = value;
        }

        public string APIEndpoint { get; set; } = "";

        // ✅ New name - maps to APIEndpoint
        public string ApiUrl
        {
            get => APIEndpoint;
            set => APIEndpoint = value;
        }

        public string APIKey { get; set; } = "";

        // ✅ New name - maps to APIKey
        public string ApiKey
        {
            get => APIKey;
            set => APIKey = value;
        }

        public string APISecret { get; set; } = "";

        // ✅ New name - maps to APISecret
        public string ApiSecret
        {
            get => APISecret;
            set => APISecret = value;
        }

        // ✅ New fields
        public string AccountSid { get; set; } = "";
        public string DefaultCountryCode { get; set; } = "91";

        // ===== Check IN/OUT Alerts =====
        public bool SendCheckInAlert { get; set; } = true;
        public bool SendCheckOutAlert { get; set; } = true;

        public string CheckInMessageFormat { get; set; } =
            "Dear Parent, {StudentName} has checked IN at {BranchName} on {Date} at {Time}.";

        public string CheckOutMessageFormat { get; set; } =
            "Dear Parent, {StudentName} has checked OUT from {BranchName} on {Date} at {Time}.";

        // ===== Absence Notification Settings =====
        public bool EnableNotifications { get; set; } = true;
        public bool SendAdminConfirmation { get; set; } = true;
        public string AdminWhatsAppNumber { get; set; } = "";

        // ===== Absence Templates =====
        public string TodayTemplate { get; set; } =
@"Dear {StudentName},

Today's class ({Date}) is cancelled due to: {Reason}.

We apologize for the inconvenience.

Regards,
Management";

        public string TomorrowTemplate { get; set; } =
@"Dear {StudentName},

Tomorrow's class ({Date}) is cancelled due to: {Reason}.

We apologize for the inconvenience.

Regards,
Management";

        // ===== Message Sending Settings =====
        public int MessageDelay { get; set; } = 500;
        public int MaxRetryCount { get; set; } = 3;
        public int MessageTimeout { get; set; } = 30;

        // ===== Schedule Settings =====
        public TimeSpan WorkingHoursFrom { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan WorkingHoursTo { get; set; } = new TimeSpan(18, 0, 0);
        public bool WorkMon { get; set; } = true;
        public bool WorkTue { get; set; } = true;
        public bool WorkWed { get; set; } = true;
        public bool WorkThu { get; set; } = true;
        public bool WorkFri { get; set; } = true;
        public bool WorkSat { get; set; } = true;
        public bool WorkSun { get; set; } = false;

        // ===== Test Mode =====
        public bool IsTestMode { get; set; } = false;
        public string TestMobileNumber { get; set; } = "";

        // ===== Meta =====
        public string UpdatedBy { get; set; } = "";
        public DateTime? UpdatedDate { get; set; }
    }

    public class WhatsAppUsageStats
    {
        public int TotalSentThisMonth { get; set; }
        public int TotalSentAllTime { get; set; }
        public int TotalSuccess { get; set; }
        public int TotalFailed { get; set; }
        public double SuccessRate { get; set; }
        public double FailedRate { get; set; }
    }
}