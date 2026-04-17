using CSMTutorial.Models;

namespace CSMTutorial.Data.Repositories
{
    public interface ITeacherAbsenceService
    {
        Task<List<CompanyDropdown>> GetCompaniesAsync();
        Task<List<Student>> GetStudentsByCompanyAsync(int? companyId, int? departmentId);
        Task<long> CreateMessageAsync(TeacherAbsenceMessage message);
        Task InsertRecipientsAsync(long messageId, List<Student> recipients);
        Task<List<TeacherAbsenceRecipient>> GetMessageReportAsync(long? messageId, DateTime? fromDate, DateTime? toDate, int? companyId, string status, int pageNumber, int pageSize);
        Task UpdateRecipientStatusAsync(long recipientId, string status, string errorMessage);
        Task<List<TeacherAbsenceRecipient>> GetFailedRecipientsAsync(long? messageId);
        Task<List<TeacherAbsenceMessage>> GetMessageHistoryAsync(DateTime? fromDate, DateTime? toDate, int? companyId, int pageNumber, int pageSize);
        Task<WhatsAppConfig> GetWhatsAppConfigAsync();
    }
}
