// Services/IWhatsAppService.cs
using CSMTutorial.Models;

namespace CSMTutorial.Services
{
    public interface IWhatsAppService
    {
        // Settings (Old)
        Task<WhatsAppSettings?> GetSettingsAsync();
        Task<bool> UpdateSettingsAsync(WhatsAppSettings settings);

        // Config (New)
        Task<WhatsAppConfig?> GetConfigAsync();
        Task<int> SaveConfigAsync(WhatsAppConfig config, string createdBy);

        // Dashboard Stats
        Task<DashboardStats> GetDashboardStatsAsync(DateTime? date = null);

        // Pending Notifications (Old)
        Task<List<PendingWhatsAppNotification>> GetPendingNotificationsAsync(string notificationType);
        Task<List<PendingWhatsAppNotification>> GetPendingCheckInNotificationsAsync(DateTime? date = null, int? companyId = null);
        Task<List<PendingWhatsAppNotification>> GetPendingCheckOutNotificationsAsync(DateTime? date = null, int? companyId = null);
        Task<List<PendingWhatsAppNotification>> GetAbsentStudentsAsync(DateTime? date = null, int? companyId = null);

        // Pending Messages (New)
        Task<List<PendingMessage>> GetPendingCheckInMessagesAsync(DateTime? date = null);
        Task<List<PendingMessage>> GetPendingCheckOutMessagesAsync(DateTime? date = null);
        Task<List<PendingMessage>> GetAbsentStudentsMessagesAsync(DateTime? date = null);

        // Prepare Messages
        Task<List<PendingMessage>> PrepareMessagesAsync(List<PendingMessage> messages, string messageType);

        // Send Messages
        Task<bool> SendWhatsAppMessageAsync(PendingWhatsAppNotification notification, string messageType);
        Task<bool> ProcessPendingNotificationsAsync();

        // Logging
        Task LogMessageAsync(int employeeId, string studentName, string mobileNo, string punchStatus, bool success, string message, string? apiResponse = null);
        Task LogMessageAsync(PendingMessage message, string status, string sentBy);

        // Message History
        Task<List<WhatsAppMessageLog>> GetMessageHistoryAsync(DateTime fromDate, DateTime toDate, int? employeeId = null);
        Task<ApiResponse<List<MessageLog>>> GetMessageHistoryAsync(DateTime fromDate, DateTime toDate);
        Task<bool> UpdateMessageStatusAsync(int messageId, string status, string updatedBy);

        // Utilities
        string BuildMessage(string template, PendingWhatsAppNotification notification);
        string BuildMessage(string template, PendingMessage message);
        string GenerateWhatsAppUrl(string mobile, string message, string countryCode = "91");

        Task<List<PendingMessage>> GetPendingCheckInMessagesAsync(DateTime? date = null, int? companyId = null);
        Task<List<PendingMessage>> GetPendingCheckOutMessagesAsync(DateTime? date = null, int? companyId = null);
        Task<List<PendingMessage>> GetAbsentStudentsMessagesAsync(DateTime? date = null, int? companyId = null);

        Task<WhatsAppResponse> SendMessageAsync(string phoneNumber, string message);
        string GenerateWhatsAppUrl(string phoneNumber, string message);
    }
}