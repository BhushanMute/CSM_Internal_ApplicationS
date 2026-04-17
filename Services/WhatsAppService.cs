using CSMTutorial.Data.Repositories;
using CSMTutorial.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace CSMTutorial.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly string _connectionString;
        private readonly ILogger<WhatsAppService> _logger;
        private WhatsAppConfig _config;
        private readonly ITeacherAbsenceService _teacherService;
        private readonly HttpClient _httpClient;

        public WhatsAppService(IConfiguration configuration, HttpClient httpClient, ILogger<WhatsAppService> logger, ITeacherAbsenceService teacherService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _logger = logger;
            _teacherService = teacherService;
            _httpClient = httpClient;
        }

        #region ================= SETTINGS =================
        private async Task LoadConfigAsync()
        {
            if (_config == null)
            {
                _config = await _teacherService.GetWhatsAppConfigAsync();
            }
        }
        public async Task<WhatsAppSettings?> GetSettingsAsync()
        {
            using var con = new SqlConnection(_connectionString);

            return await con.QueryFirstOrDefaultAsync<WhatsAppSettings>(
                "sp_WhatsAppSettings_Get",
                commandType: CommandType.StoredProcedure
            );
        }
        // WhatsAppService.cs
        public async Task<bool> UpdateMessageStatusAsync(int messageId, string status, string updatedBy)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.ExecuteAsync(
                "sp_UpdateMessageStatus",
                new
                {
                    MessageId = messageId,
                    Status = status,
                    UpdatedBy = updatedBy
                },
                commandType: CommandType.StoredProcedure
            );
            return result > 0;
        }
        public async Task<List<WhatsAppMessageLog>> GetMessageHistoryAsync(
    DateTime fromDate,
    DateTime toDate,
    int? employeeId = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var result = await connection.QueryAsync<WhatsAppMessageLog>(
                    "sp_GetWhatsAppMessageHistory",
                    new
                    {
                        FromDate = fromDate,
                        ToDate = toDate,
                        EmployeeId = employeeId
                    },
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching WhatsApp message history");
                return new List<WhatsAppMessageLog>();
            }
        }
        public async Task<bool> UpdateSettingsAsync(WhatsAppSettings model)
        {
            using var con = new SqlConnection(_connectionString);

            var result = await con.ExecuteAsync(
                "sp_WhatsAppSettings_Save",
                model,
                commandType: CommandType.StoredProcedure
            );

            return result > 0;
        }

        #endregion
        public async Task<List<PendingWhatsAppNotification>> GetPendingNotificationsAsync(string notificationType)
        {
            return notificationType.ToLower() switch
            {
                "in" or "checkin" => await GetPendingCheckInNotificationsAsync(),
                "out" or "checkout" => await GetPendingCheckOutNotificationsAsync(),
                "absent" => await GetAbsentStudentsAsync(),
                _ => new List<PendingWhatsAppNotification>()
            };
        }
        public async Task<bool> SendWhatsAppMessageAsync(PendingWhatsAppNotification notification, string type)
        {
            var settings = await GetSettingsAsync();

            if (settings == null || !settings.IsEnabled)
                return false;

            try
            {
                string template = type.ToUpper() switch
                {
                    "CHECKIN" => settings.CheckInMessageFormat,
                    "CHECKOUT" => settings.CheckOutMessageFormat,
                    _ => settings.CheckInMessageFormat
                };

                string message = BuildMessage(template, notification);

                string mobile = FormatMobile(notification.ParentMobileNo, settings.DefaultCountryCode);

                string url = GenerateWhatsAppUrl(mobile, message);

                _logger.LogInformation("WhatsApp URL: {url}", url);

                await LogMessageAsync(
                    notification.EmployeeId,
                    notification.StudentName,
                    notification.ParentMobileNo,
                    type,
                    true,
                    message,
                    "Generated"
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendWhatsAppMessageAsync failed");

                await LogMessageAsync(
                    notification.EmployeeId,
                    notification.StudentName,
                    notification.ParentMobileNo,
                    type,
                    false,
                    "",
                    ex.Message
                );

                return false;
            }
        }
        public async Task LogMessageAsync(int employeeId, string studentName, string mobileNo,
    string messageType, bool success, string message, string? apiResponse = null)
        {
            using var con = new SqlConnection(_connectionString);

            await con.ExecuteAsync(
                "sp_WhatsAppLog_Insert",
                new
                {
                    EmployeeId = employeeId,
                    StudentName = studentName,
                    ParentMobileNo = mobileNo,
                    MessageType = messageType,
                    Status = success,
                    Message = message,
                    APIResponse = apiResponse
                },
                commandType: CommandType.StoredProcedure
            );
        }
        #region ================= PENDING MESSAGES =================

        public async Task<List<PendingWhatsAppNotification>> GetPendingCheckInNotificationsAsync(DateTime? date = null, int? companyId = null)
        {
            using var con = new SqlConnection(_connectionString);

            var result = await con.QueryAsync<PendingWhatsAppNotification>(
                "sp_GetPendingCheckInMessages",
                new
                {
                    ProcessingDate = date ?? DateTime.Today,
                    CompanyId = companyId
                },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<List<PendingWhatsAppNotification>> GetPendingCheckOutNotificationsAsync(DateTime? date = null, int? companyId = null)
        {
            using var con = new SqlConnection(_connectionString);

            var result = await con.QueryAsync<PendingWhatsAppNotification>(
                "sp_GetPendingCheckOutMessages",
                new
                {
                    ProcessingDate = date ?? DateTime.Today,
                    CompanyId = companyId
                },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<List<PendingWhatsAppNotification>> GetAbsentStudentsAsync(DateTime? date = null, int? companyId = null)
        {
            using var con = new SqlConnection(_connectionString);

            var result = await con.QueryAsync<PendingWhatsAppNotification>(
                "sp_GetAbsentStudentsMessages",
                new
                {
                    ProcessingDate = date ?? DateTime.Today,
                    CompanyId = companyId
                },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        #endregion

        #region ================= PROCESS ENGINE =================

        public async Task<bool> ProcessPendingNotificationsAsync()
        {
            var settings = await GetSettingsAsync();
            if (settings == null || !settings.IsEnabled)
                return false;

            int total = 0;

            if (settings.SendCheckInAlert)
            {
                var list = await GetPendingCheckInNotificationsAsync();

                foreach (var item in list)
                {
                    await SendMessageAsync(item, "CHECKIN", settings);
                    total++;
                }
            }

            if (settings.SendCheckOutAlert)
            {
                var list = await GetPendingCheckOutNotificationsAsync();

                foreach (var item in list)
                {
                    await SendMessageAsync(item, "CHECKOUT", settings);
                    total++;
                }
            }

            _logger.LogInformation("WhatsApp processed successfully. Total: {Total}", total);

            return true;
        }

        #endregion

        #region ================= SEND MESSAGE =================

        private async Task SendMessageAsync(PendingWhatsAppNotification n, string type, WhatsAppSettings settings)
        {
            try
            {
                string template = type switch
                {
                    "CHECKIN" => settings.CheckInMessageFormat,
                    "CHECKOUT" => settings.CheckOutMessageFormat,
                    _ => settings.CheckInMessageFormat
                };

                string message = BuildMessage(template, n);

                string mobile = FormatMobile(n.ParentMobileNo, settings.DefaultCountryCode);

                string url = GenerateWhatsAppUrl(mobile, message);

                _logger.LogInformation("WhatsApp Generated URL: {Url}", url);

                await LogAsync(n, type, message, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendMessageAsync failed");

                await LogAsync(n, type, "", false);
            }
        }

        #endregion

        #region ================= LOGGING =================

        private async Task LogAsync(PendingWhatsAppNotification n, string type, string message, bool status)
        {
            using var con = new SqlConnection(_connectionString);

            await con.ExecuteAsync(
                "sp_WhatsAppLog_Insert",
                new
                {
                    n.EmployeeId,
                    n.StudentName,
                    n.ParentMobileNo,
                    MessageType = type,
                    Message = message,
                    Status = status
                },
                commandType: CommandType.StoredProcedure
            );
        }

        #endregion

        #region ================= UTILITIES =================

        public string BuildMessage(string template, PendingWhatsAppNotification n)
        {
            return template
                .Replace("{{StudentName}}", n.StudentName ?? "")
                .Replace("{{ParentName}}", n.ParentName ?? "")
                .Replace("{{Date}}", n.PunchTime.ToString("dd-MM-yyyy"))
                .Replace("{{Time}}", n.PunchTime.ToString("hh:mm tt"))
                .Replace("{{BranchName}}", n.BranchName ?? "")
                .Replace("{{CourseName}}", n.CourseName ?? "");
        }

        public string GenerateWhatsAppUrl(string mobile, string message, string countryCode = "91")
        {
            mobile = FormatMobile(mobile, countryCode);
            var encoded = HttpUtility.UrlEncode(message);
            return $"https://wa.me/{mobile}?text={encoded}";
        }

        private string FormatMobile(string mobile, string code)
        {
            if (string.IsNullOrEmpty(mobile))
                return "";

            mobile = new string(mobile.Where(char.IsDigit).ToArray());

            if (mobile.Length == 10)
                mobile = code + mobile;

            return mobile;
        }

        #endregion
        #region ================= CONFIG MANAGEMENT =================

        public async Task<WhatsAppConfig?> GetConfigAsync()
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryFirstOrDefaultAsync<WhatsAppConfig>(
                "sp_WhatsAppConfig_Get",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> SaveConfigAsync(WhatsAppConfig config, string createdBy)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.ExecuteScalarAsync<int>(
                "sp_WhatsAppConfig_Save",
                new
                {
                    config.ConfigId,
                    config.IsEnabled,
                    config.InstituteName,
                    config.ContactNumber,
                    config.DefaultCountryCode,
                    config.RefreshInterval,
                    config.SendCheckInAlert,
                    config.SendCheckOutAlert,
                    config.SendAbsentAlert,
                    config.SendLateAlert,
                    config.AbsentAlertTime,
                    config.LateThresholdMinutes,
                    config.CheckInMessage,
                    config.CheckOutMessage,
                    config.AbsentMessage,
                    config.LateArrivalMessage,
                    CreatedBy = createdBy
                },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

        #endregion

        #region ================= DASHBOARD STATS =================

        public async Task<DashboardStats> GetDashboardStatsAsync(DateTime? date = null)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.QueryFirstOrDefaultAsync<DashboardStats>(
                "sp_GetDashboardStats",
                new { ProcessingDate = date ?? DateTime.Today },
                commandType: CommandType.StoredProcedure
            );
            return result ?? new DashboardStats();
        }

        #endregion

        #region ================= PENDING MESSAGES (NEW) =================

        public async Task<List<PendingMessage>> GetPendingCheckInMessagesAsync(DateTime? date = null)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.QueryAsync<PendingMessage>(
                "sp_GetPendingCheckInMessagesNew",
                new { ProcessingDate = date ?? DateTime.Today },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        public async Task<List<PendingMessage>> GetPendingCheckOutMessagesAsync(DateTime? date = null)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.QueryAsync<PendingMessage>(
                "sp_GetPendingCheckOutMessagesNew",
                new { ProcessingDate = date ?? DateTime.Today },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        public async Task<List<PendingMessage>> GetAbsentStudentsMessagesAsync(DateTime? date = null)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.QueryAsync<PendingMessage>(
                "sp_GetAbsentStudentsMessagesNew",
                new { ProcessingDate = date ?? DateTime.Today },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        #endregion

        #region ================= PREPARE MESSAGES =================

        public async Task<List<PendingMessage>> PrepareMessagesAsync(List<PendingMessage> messages, string messageType)
        {
            var config = await GetConfigAsync();
            if (config == null) return messages;

            string template = messageType switch
            {
                "CHECKIN" => config.CheckInMessage,
                "CHECKOUT" => config.CheckOutMessage,
                "ABSENT" => config.AbsentMessage,
                _ => config.CheckInMessage
            };

            foreach (var msg in messages)
            {
                msg.MessageType = messageType;
                msg.MessageText = BuildMessage(template, msg);
                msg.WhatsAppUrl = GenerateWhatsAppUrl(msg.ParentMobile, msg.MessageText, config.DefaultCountryCode);
            }

            return messages;
        }

        #endregion

        #region ================= MESSAGE LOGGING (NEW) =================

        public async Task LogMessageAsync(PendingMessage message, string status, string sentBy)
        {
            using var con = new SqlConnection(_connectionString);
            await con.ExecuteAsync(
                "sp_WhatsAppMessageLog_Insert",
                new
                {
                    message.EmployeeId,
                    message.StudentName,
                    ParentMobile = message.ParentMobile,
                    message.MessageType,
                    MessageText = message.MessageText,
                    Status = status,
                    SentBy = sentBy
                },
                commandType: CommandType.StoredProcedure
            );
        }

        #endregion

        #region ================= MESSAGE HISTORY (NEW) =================

        public async Task<ApiResponse<List<MessageLog>>> GetMessageHistoryAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var result = await con.QueryAsync<MessageLog>(
                    "sp_GetMessageHistory",
                    new { FromDate = fromDate, ToDate = toDate },
                    commandType: CommandType.StoredProcedure
                );

                return new ApiResponse<List<MessageLog>>
                {
                    Success = true,
                    Data = result.ToList(),
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching message history");
                return new ApiResponse<List<MessageLog>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        #endregion

        #region ================= BUILD MESSAGE (OVERLOAD) =================

        public string BuildMessage(string template, PendingMessage msg)
        {
            return template
                .Replace("{{StudentName}}", msg.StudentName ?? "")
                .Replace("{{ParentName}}", msg.ParentName ?? "")
                .Replace("{{Date}}", msg.PunchTime.ToString("dd-MMM-yyyy"))
                .Replace("{{Time}}", msg.PunchTime.ToString("hh:mm tt"))
                .Replace("{{BranchName}}", msg.BranchName ?? "")
                .Replace("{{CourseName}}", msg.CourseName ?? "");
        }

        #endregion
        #region ================= BULK SEND METHODS =================

        public async Task<List<PendingMessage>> GetPendingCheckInMessagesAsync(DateTime? date = null, int? companyId = null)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.QueryAsync<PendingMessage>(
                "sp_GetPendingCheckInMessagesForBulk",
                new
                {
                    ProcessingDate = date ?? DateTime.Today,
                    CompanyId = companyId
                },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        public async Task<List<PendingMessage>> GetPendingCheckOutMessagesAsync(DateTime? date = null, int? companyId = null)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.QueryAsync<PendingMessage>(
                "sp_GetPendingCheckOutMessagesForBulk",
                new
                {
                    ProcessingDate = date ?? DateTime.Today,
                    CompanyId = companyId
                },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        public async Task<List<PendingMessage>> GetAbsentStudentsMessagesAsync(DateTime? date = null, int? companyId = null)
        {
            using var con = new SqlConnection(_connectionString);
            var result = await con.QueryAsync<PendingMessage>(
                "sp_GetAbsentStudentsMessagesForBulk",
                new
                {
                    ProcessingDate = date ?? DateTime.Today,
                    CompanyId = companyId
                },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        //public async Task<List<PendingMessage>> PrepareMessagesAsync(List<PendingMessage> messages, string messageType)
        //{
        //    var config = await GetConfigAsync();
        //    if (config == null) return messages;

        //    string template = messageType.ToUpper() switch
        //    {
        //        "CHECKIN" => config.CheckInMessage,
        //        "CHECKOUT" => config.CheckOutMessage,
        //        "ABSENT" => config.AbsentMessage,
        //        "LATE" => config.LateArrivalMessage,
        //        _ => config.CheckInMessage
        //    };

        //    foreach (var msg in messages)
        //    {
        //        msg.MessageType = messageType;
        //        msg.MessageText = BuildMessageForBulk(template, msg, config);
        //        msg.FormattedMessage = msg.MessageText;
        //        msg.WhatsAppUrl = GenerateWhatsAppUrl(msg.ParentMobile, msg.MessageText, config.DefaultCountryCode);
        //    }

        //    return messages;
        //}

        //public async Task LogMessageAsync(PendingMessage message, string status, string sentBy)
        //{
        //    using var con = new SqlConnection(_connectionString);
        //    await con.ExecuteAsync(
        //        "sp_WhatsAppMessageLog_Insert",
        //        new
        //        {
        //            message.EmployeeId,
        //            message.EmployeeCode,
        //            message.StudentName,
        //            ParentMobile = message.ParentMobile,
        //            message.MessageType,
        //            MessageText = message.MessageText,
        //            message.WhatsAppUrl,
        //            Status = status,
        //            SentBy = sentBy,
        //            PunchTime = message.PunchTime,
        //            message.CompanyId,
        //            message.DepartmentId,
        //            Notes = $"Sent via Bulk Send - {sentBy}"
        //        },
        //        commandType: CommandType.StoredProcedure
        //    );
        //}

        private string BuildMessageForBulk(string template, PendingMessage msg, WhatsAppConfig config)
        {
            return template
                .Replace("{{StudentName}}", msg.StudentName ?? "")
                .Replace("{{ParentName}}", msg.ParentName ?? "")
                .Replace("{{Date}}", msg.PunchTime.ToString("dd-MMM-yyyy"))
                .Replace("{{Time}}", msg.PunchTime.ToString("hh:mm tt"))
                .Replace("{{BranchName}}", msg.BranchName ?? "")
                .Replace("{{CourseName}}", msg.CourseName ?? "")
                .Replace("{{InstituteName}}", config.InstituteName ?? "")
                .Replace("{{ContactNumber}}", config.ContactNumber ?? "");
        }
        public async Task<WhatsAppResponse> SendMessageAsync(string phoneNumber, string message)
        {
            try
            {
                await LoadConfigAsync();

                if (_config == null || !_config.IsEnabled)
                {
                    return new WhatsAppResponse
                    {
                        Success = false,
                        ErrorMessage = "WhatsApp service is not configured or disabled"
                    };
                }

                // Format phone number
                var formattedPhone = FormatPhoneNumber(phoneNumber);

                // Build API request based on your provider
                var requestBody = new
                {
                    phone = formattedPhone,
                    message = message,
                    apiKey = _config.APIKey
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add authorization header
                _httpClient.DefaultRequestHeaders.Clear();
                if (!string.IsNullOrEmpty(_config.AuthToken))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.AuthToken}");
                }

                var response = await _httpClient.PostAsync(_config.APIEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new WhatsAppResponse
                    {
                        Success = true,
                        MessageId = Guid.NewGuid().ToString(),
                        RawResponse = responseContent
                    };
                }
                else
                {
                    return new WhatsAppResponse
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode}",
                        RawResponse = responseContent
                    };
                }
            }
            catch (Exception ex)
            {
                return new WhatsAppResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public string GenerateWhatsAppUrl(string phoneNumber, string message)
        {
            var formattedPhone = FormatPhoneNumber(phoneNumber);
            var encodedMessage = Uri.EscapeDataString(message);
            return $"https://wa.me/{formattedPhone}?text={encodedMessage}";
        }

        private string FormatPhoneNumber(string phone)
        {
            // Remove spaces, dashes, etc.
            phone = new string(phone.Where(char.IsDigit).ToArray());

            // Add country code if not present
            if (phone.Length == 10)
            {
                phone = "91" + phone;
            }

            return phone;
        }
        #endregion
   
     

 
        // ================= SEND MESSAGE =================
 
        // ================= PROVIDER: TextMeBot (FREE) =================
        private async Task<WhatsAppResponse> SendViaTextMeBot(string phone, string message, WhatsAppConfig config)
        {
            try
            {
                var apiKey = config.APIKey;
                var encodedMessage = Uri.EscapeDataString(message);
                var url = $"https://api.textmebot.com/send.php?recipient={phone}&apikey={apiKey}&text={encodedMessage}";

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("TextMeBot Response: {Response}", responseContent);

                return new WhatsAppResponse
                {
                    Success = response.IsSuccessStatusCode && !responseContent.Contains("error", StringComparison.OrdinalIgnoreCase),
                    MessageId = Guid.NewGuid().ToString("N"),
                    RawResponse = responseContent,
                    ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ================= PROVIDER: WA API =================
        private async Task<WhatsAppResponse> SendViaWaApi(string phone, string message, WhatsAppConfig config)
        {
            try
            {
                var endpoint = config.APIEndpoint ?? "https://waapi.app/api/v1/messages/text";

                var requestBody = new
                {
                    to = phone,
                    text = message
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.AuthToken}");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("WaApi Response: {Response}", responseContent);

                return new WhatsAppResponse
                {
                    Success = response.IsSuccessStatusCode,
                    MessageId = Guid.NewGuid().ToString("N"),
                    RawResponse = responseContent,
                    ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ================= PROVIDER: Twilio =================
        private async Task<WhatsAppResponse> SendViaTwilio(string phone, string message, WhatsAppConfig config)
        {
            try
            {
                var accountSid = config.APIKey;
                var authToken = config.APISecret;
                var fromNumber = config.SenderNumber;

                var endpoint = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("To", $"whatsapp:+{phone}"),
                    new KeyValuePair<string, string>("From", $"whatsapp:+{fromNumber}"),
                    new KeyValuePair<string, string>("Body", message)
                });

                _httpClient.DefaultRequestHeaders.Clear();
                var authBytes = Encoding.ASCII.GetBytes($"{accountSid}:{authToken}");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(authBytes)}");

                var response = await _httpClient.PostAsync(endpoint, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Twilio Response: {Response}", responseContent);

                return new WhatsAppResponse
                {
                    Success = response.IsSuccessStatusCode,
                    MessageId = Guid.NewGuid().ToString("N"),
                    RawResponse = responseContent,
                    ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ================= PROVIDER: Meta (Official) =================
        private async Task<WhatsAppResponse> SendViaMeta(string phone, string message, WhatsAppConfig config)
        {
            try
            {
                var endpoint = config.APIEndpoint ?? $"https://graph.facebook.com/v18.0/{config.SenderNumber}/messages";

                var requestBody = new
                {
                    messaging_product = "whatsapp",
                    to = phone,
                    type = "text",
                    text = new { body = message }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.AuthToken}");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Meta Response: {Response}", responseContent);

                return new WhatsAppResponse
                {
                    Success = response.IsSuccessStatusCode,
                    MessageId = Guid.NewGuid().ToString("N"),
                    RawResponse = responseContent,
                    ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ================= PROVIDER: UltraMsg =================
        private async Task<WhatsAppResponse> SendViaUltraMsg(string phone, string message, WhatsAppConfig config)
        {
            try
            {
                var instanceId = config.APIKey;
                var token = config.AuthToken;
                var endpoint = $"https://api.ultramsg.com/{instanceId}/messages/chat";

                var requestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", token ?? ""),
                    new KeyValuePair<string, string>("to", $"+{phone}"),
                    new KeyValuePair<string, string>("body", message)
                });

                var response = await _httpClient.PostAsync(endpoint, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("UltraMsg Response: {Response}", responseContent);

                var isSuccess = response.IsSuccessStatusCode &&
                               !responseContent.Contains("error", StringComparison.OrdinalIgnoreCase);

                return new WhatsAppResponse
                {
                    Success = isSuccess,
                    MessageId = Guid.NewGuid().ToString("N"),
                    RawResponse = responseContent,
                    ErrorMessage = isSuccess ? null : $"UltraMsg Error: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ================= PROVIDER: Green API =================
        private async Task<WhatsAppResponse> SendViaGreenApi(string phone, string message, WhatsAppConfig config)
        {
            try
            {
                var instanceId = config.APIKey;
                var token = config.AuthToken;
                var endpoint = $"https://api.green-api.com/waInstance{instanceId}/sendMessage/{token}";

                var requestBody = new
                {
                    chatId = $"{phone}@c.us",
                    message = message
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("GreenApi Response: {Response}", responseContent);

                return new WhatsAppResponse
                {
                    Success = response.IsSuccessStatusCode,
                    MessageId = Guid.NewGuid().ToString("N"),
                    RawResponse = responseContent,
                    ErrorMessage = response.IsSuccessStatusCode ? null : responseContent
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ================= PROVIDER: WATI =================
        private async Task<WhatsAppResponse> SendViaWati(string phone, string message, WhatsAppConfig config)
        {
            try
            {
                var endpoint = config.APIEndpoint ?? "https://live-server.wati.io";
                var url = $"{endpoint}/api/v1/sendSessionMessage/{phone}?messageText={Uri.EscapeDataString(message)}";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.AuthToken}");

                var response = await _httpClient.PostAsync(url, null);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("WATI Response: {Response}", responseContent);

                return new WhatsAppResponse
                {
                    Success = response.IsSuccessStatusCode,
                    MessageId = Guid.NewGuid().ToString("N"),
                    RawResponse = responseContent,
                    ErrorMessage = response.IsSuccessStatusCode ? null : responseContent
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ================= GENERIC API PROVIDER =================
        private async Task<WhatsAppResponse> SendViaGenericApi(string phone, string message, WhatsAppConfig config)
        {
            try
            {
                if (string.IsNullOrEmpty(config.APIEndpoint))
                {
                    return new WhatsAppResponse
                    {
                        Success = false,
                        ErrorMessage = "API Endpoint is not configured. Please set up WhatsApp settings."
                    };
                }

                var requestBody = new
                {
                    phone = phone,
                    message = message,
                    apiKey = config.APIKey
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();

                if (!string.IsNullOrEmpty(config.AuthToken))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.AuthToken}");
                }

                if (!string.IsNullOrEmpty(config.APIKey))
                {
                    _httpClient.DefaultRequestHeaders.Add("X-API-Key", config.APIKey);
                }

                var response = await _httpClient.PostAsync(config.APIEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Generic API Response: {Response}", responseContent);

                return new WhatsAppResponse
                {
                    Success = response.IsSuccessStatusCode,
                    MessageId = Guid.NewGuid().ToString("N"),
                    RawResponse = responseContent,
                    ErrorMessage = response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                return new WhatsAppResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ================= GENERATE WHATSAPP URL =================
         

        // ================= FORMAT PHONE NUMBER =================
        private string FormatPhoneNumber(string phone, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            // Remove spaces, dashes, brackets, plus
            phone = new string(phone.Where(char.IsDigit).ToArray());

            // Remove leading zeros
            phone = phone.TrimStart('0');

            // Add country code if 10 digits (Indian number)
            if (phone.Length == 10)
            {
                phone = (countryCode ?? "91") + phone;
            }

            return phone;
        }
    } }
 
 
 