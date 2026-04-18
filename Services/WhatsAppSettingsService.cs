using CSMTutorial.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CSMTutorial.Services
{
    public class WhatsAppSettingsService : IWhatsAppSettingsService
    {
        private readonly string _connectionString;
        private readonly IWhatsAppService _whatsAppService;

        public WhatsAppSettingsService(
            IConfiguration configuration,
            IWhatsAppService whatsAppService)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection");
            _whatsAppService = whatsAppService;
        }

        public async Task<WhatsAppSettings> GetSettingsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<WhatsAppSettings>(
                "sp_GetWhatsAppSettings",
                commandType: CommandType.StoredProcedure
            );
            return result ?? new WhatsAppSettings();
        }

        // WhatsAppSettingsService.cs
        public async Task<bool> SaveSettingsAsync(
            CSMTutorial.Models.WhatsAppSettings s, string updatedBy)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "sp_SaveWhatsAppSettings",
                    new
                    {
                        // ✅ Map new names → old DB column names
                        ApiProvider = s.APIProvider,
                        ApiUrl = s.APIEndpoint,
                        ApiKey = s.APIKey,
                        ApiSecret = s.APISecret,
                        AccountSid = s.AccountSid,
                        FromNumber = s.SenderNumber,
                        IsActive = s.IsEnabled,
                        s.MessageDelay,
                        s.MaxRetryCount,
                        s.MessageTimeout,
                        s.EnableNotifications,
                        s.SendCheckInAlert,
                        s.SendCheckOutAlert,
                        s.CheckInMessageFormat,
                        s.CheckOutMessageFormat,
                        s.TodayTemplate,
                        s.TomorrowTemplate,
                        s.AdminWhatsAppNumber,
                        s.SendAdminConfirmation,
                        WorkingHoursFrom =
                            s.WorkingHoursFrom.ToString(@"hh\:mm\:ss"),
                        WorkingHoursTo =
                            s.WorkingHoursTo.ToString(@"hh\:mm\:ss"),
                        s.WorkMon,
                        s.WorkTue,
                        s.WorkWed,
                        s.WorkThu,
                        s.WorkFri,
                        s.WorkSat,
                        s.WorkSun,
                        s.IsTestMode,
                        s.TestMobileNumber,
                        UpdatedBy = updatedBy
                    },
                    commandType: CommandType.StoredProcedure
                );
                return true;
            }
            catch { return false; }
        }

        public async Task<WhatsAppUsageStats> GetUsageStatsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<WhatsAppUsageStats>(
                "sp_GetWhatsAppUsageStats",
                commandType: CommandType.StoredProcedure
            ) ?? new WhatsAppUsageStats();
        }

        public async Task<bool> TestConnectionAsync(string apiUrl, string apiKey)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders
                      .Add("Authorization", $"Bearer {apiKey}");
                var response = await client.GetAsync(apiUrl);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<(bool Success, string Message)> SendTestMessageAsync(
            string phoneNumber, string message)
        {
            try
            {
                var response = await _whatsAppService
                    .SendMessageAsync(phoneNumber, message);
                return response.Success
                    ? (true, "Test message sent successfully!")
                    : (false, response.ErrorMessage ?? "Failed");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
