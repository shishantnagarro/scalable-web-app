using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using ScalableWebApp.Data;
using ScalableWebApp.Models;

namespace ScalableWebApp.Services
{
    public class LogService : ILogService
    {
        private readonly IAmazonDynamoDB _dynamoClient;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly string _tableName;

        public LogService(IAmazonDynamoDB dynamoClient, ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dynamoClient = dynamoClient;
            _dbContext = dbContext;
            _configuration = configuration;
            _tableName = _configuration["AWS:DynamoDBTableName"] ?? "web-app-logs";
        }

        public async Task LogToDynamoAsync(DynamoLogEntry logEntry)
        {
            try
            {
                var item = new Dictionary<string, AttributeValue>
                {
                    ["Id"] = new AttributeValue { S = logEntry.Id },
                    ["Action"] = new AttributeValue { S = logEntry.Action },
                    ["Details"] = new AttributeValue { S = logEntry.Details },
                    ["Timestamp"] = new AttributeValue { S = logEntry.Timestamp }
                };
                if (!string.IsNullOrEmpty(logEntry.FileName)) item["FileName"] = new AttributeValue { S = logEntry.FileName };
                if (!string.IsNullOrEmpty(logEntry.S3Bucket)) item["S3Bucket"] = new AttributeValue { S = logEntry.S3Bucket };
                if (!string.IsNullOrEmpty(logEntry.S3Key)) item["S3Key"] = new AttributeValue { S = logEntry.S3Key };

                var request = new PutItemRequest { TableName = _tableName, Item = item };
                await _dynamoClient.PutItemAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log to DynamoDB: {ex.Message}");
            }
        }

        public async Task<List<DynamoLogEntry>> GetLogsAsync(int limit = 50)
        {
            try
            {
                var request = new ScanRequest { TableName = _tableName, Limit = limit };
                var response = await _dynamoClient.ScanAsync(request);
                var logs = new List<DynamoLogEntry>();
                foreach (var item in response.Items)
                {
                    var log = new DynamoLogEntry
                    {
                        Id = item.ContainsKey("Id") ? item["Id"].S : "",
                        Action = item.ContainsKey("Action") ? item["Action"].S : "",
                        Details = item.ContainsKey("Details") ? item["Details"].S : "",
                        Timestamp = item.ContainsKey("Timestamp") ? item["Timestamp"].S : "",
                        FileName = item.ContainsKey("FileName") ? item["FileName"].S : null,
                        S3Bucket = item.ContainsKey("S3Bucket") ? item["S3Bucket"].S : null,
                        S3Key = item.ContainsKey("S3Key") ? item["S3Key"].S : null
                    };
                    logs.Add(log);
                }
                return logs.OrderByDescending(l => l.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get logs from DynamoDB: {ex.Message}");
                return new List<DynamoLogEntry>();
            }
        }

        public async Task LogToRdsAsync(ApplicationLog log)
        {
            try
            {
                _dbContext.ApplicationLogs.Add(log);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log to RDS: {ex.Message}");
            }
        }

        public async Task<List<ApplicationLog>> GetRdsLogsAsync(int limit = 50)
        {
            try
            {
                return await Task.FromResult(_dbContext.ApplicationLogs
                    .OrderByDescending(l => l.Timestamp)
                    .Take(limit)
                    .ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get logs from RDS: {ex.Message}");
                return new List<ApplicationLog>();
            }
        }
    }
}
