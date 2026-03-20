using System.ComponentModel.DataAnnotations;

namespace ScalableWebApp.Models
{
    public class ApplicationLog
    {
        [Key]
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? IpAddress { get; set; }
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public string? S3Key { get; set; }
    }

    public class DynamoLogEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");
        public string? FileName { get; set; }
        public string? S3Bucket { get; set; }
        public string? S3Key { get; set; }
    }
}
