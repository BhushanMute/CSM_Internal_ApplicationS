using CSMTutorial.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CSMTutorial.Data.Repositories
{
    public class TeacherAbsenceService : ITeacherAbsenceService
    {
        private readonly string _connectionString;

        public TeacherAbsenceService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<CompanyDropdown>> GetCompaniesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "sp_GetCompaniesDropdown";
            var result = await connection.QueryAsync<CompanyDropdown>(sql, commandType: CommandType.StoredProcedure);
            return result.AsList();
        }

        public async Task<List<Student>> GetStudentsByCompanyAsync(int? companyId, int? departmentId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "sp_GetStudentsByCompany";
            var parameters = new { CompanyId = companyId, DepartmentId = departmentId };
            var result = await connection.QueryAsync<Student>(sql, parameters, commandType: CommandType.StoredProcedure);
            return result.AsList();
        }

        public async Task<long> CreateMessageAsync(TeacherAbsenceMessage message)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "sp_InsertTeacherAbsenceMessage";
            var parameters = new
            {
                message.CompanyId,
                message.DepartmentId,
                message.Subject,
                message.Reason,
                message.MessageBody,
                message.MessageType,
                message.ScheduledDate,
                message.ScheduledTime,
                message.Status,
                message.TotalRecipients,
                message.CreatedBy
            };
            var result = await connection.QuerySingleAsync<long>(sql, parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task InsertRecipientsAsync(long messageId, List<Student> recipients)
        {
            using var connection = new SqlConnection(_connectionString);

            // Convert to JSON
            var recipientsJson = System.Text.Json.JsonSerializer.Serialize(recipients.Select(r => new
            {
                r.EmployeeId,
                r.EmployeeCode,
                r.StudentName,
                r.ParentMobile,
                r.CompanyId,
                r.DepartmentId
            }));

            var sql = "sp_InsertTeacherAbsenceRecipients";
            var parameters = new { MessageId = messageId, Recipients = recipientsJson };
            await connection.ExecuteAsync(sql, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<TeacherAbsenceRecipient>> GetMessageReportAsync(
            long? messageId, DateTime? fromDate, DateTime? toDate,
            int? companyId, string status, int pageNumber, int pageSize)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "sp_GetTeacherAbsenceMessageReport";
            var parameters = new
            {
                MessageId = messageId,
                FromDate = fromDate,
                ToDate = toDate,
                CompanyId = companyId,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var result = await connection.QueryAsync<TeacherAbsenceRecipient>(sql, parameters, commandType: CommandType.StoredProcedure);
            return result.AsList();
        }

        public async Task UpdateRecipientStatusAsync(long recipientId, string status, string errorMessage)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "sp_UpdateTeacherAbsenceRecipientStatus";
            var parameters = new { RecipientId = recipientId, Status = status, ErrorMessage = errorMessage };
            await connection.ExecuteAsync(sql, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<List<TeacherAbsenceRecipient>> GetFailedRecipientsAsync(long? messageId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "sp_GetFailedTeacherAbsenceRecipients";
            var parameters = new { MessageId = messageId };
            var result = await connection.QueryAsync<TeacherAbsenceRecipient>(sql, parameters, commandType: CommandType.StoredProcedure);
            return result.AsList();
        }

        public async Task<List<TeacherAbsenceMessage>> GetMessageHistoryAsync(
            DateTime? fromDate, DateTime? toDate, int? companyId, int pageNumber, int pageSize)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "sp_GetTeacherAbsenceMessageHistory";
            var parameters = new
            {
                FromDate = fromDate,
                ToDate = toDate,
                CompanyId = companyId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var result = await connection.QueryAsync<TeacherAbsenceMessage>(sql, parameters, commandType: CommandType.StoredProcedure);
            return result.AsList();
        }

        public async Task<WhatsAppConfig> GetWhatsAppConfigAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "sp_WhatsAppConfig_Get";
            var result = await connection.QueryFirstOrDefaultAsync<WhatsAppConfig>(sql, commandType: CommandType.StoredProcedure);
            return result;
        }
    }
}
