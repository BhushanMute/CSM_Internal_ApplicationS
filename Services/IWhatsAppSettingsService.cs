using CSMTutorial.Models;

namespace CSMTutorial.Services
{
    public interface IWhatsAppSettingsService
    {
        Task<WhatsAppSettings> GetSettingsAsync();
        Task<bool> SaveSettingsAsync(WhatsAppSettings settings, string updatedBy);
        Task<WhatsAppUsageStats> GetUsageStatsAsync();
        Task<bool> TestConnectionAsync(string apiUrl, string apiKey);
        Task<(bool Success, string Message)> SendTestMessageAsync(
            string phoneNumber, string message);
    }
}
