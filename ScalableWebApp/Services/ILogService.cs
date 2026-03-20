using ScalableWebApp.Models;

namespace ScalableWebApp.Services
{
    public interface ILogService
    {
        Task LogToDynamoAsync(DynamoLogEntry logEntry);
        Task<List<DynamoLogEntry>> GetLogsAsync(int limit = 50);
        Task LogToRdsAsync(ApplicationLog log);
        Task<List<ApplicationLog>> GetRdsLogsAsync(int limit = 50);
    }
}
