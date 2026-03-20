using Amazon.S3;
using Amazon.S3.Model;
using ScalableWebApp.Models;

namespace ScalableWebApp.Services
{
    public class FileService : IFileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;
        private readonly string _bucketName;

        public FileService(IAmazonS3 s3Client, IConfiguration configuration, ILogService logService)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _logService = logService;
            _bucketName = _configuration["AWS:S3BucketName"] ?? throw new ArgumentNullException("S3BucketName not configured");
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new FileUploadResult { Success = false, Message = "No file provided" };
                }

                var key = $"uploads/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}-{file.FileName}";
                using var stream = file.OpenReadStream();
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = file.ContentType,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };
                await _s3Client.PutObjectAsync(request);

                await _logService.LogToDynamoAsync(new DynamoLogEntry
                {
                    Action = "file_upload",
                    Details = "File uploaded successfully",
                    FileName = file.FileName,
                    S3Bucket = _bucketName,
                    S3Key = key
                });

                return new FileUploadResult
                {
                    Success = true,
                    Message = "File uploaded successfully",
                    FileName = file.FileName,
                    S3Key = key
                };
            }
            catch (Exception ex)
            {
                await _logService.LogToDynamoAsync(new DynamoLogEntry
                {
                    Action = "file_upload_error",
                    Details = $"Error uploading file: {ex.Message}",
                    FileName = file?.FileName
                });
                return new FileUploadResult { Success = false, Message = $"Upload failed: {ex.Message}" };
            }
        }

        public async Task<List<string>> ListFilesAsync()
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = "uploads/",
                    MaxKeys = 100
                };
                var response = await _s3Client.ListObjectsV2Async(request);
                return response.S3Objects.Select(obj => obj.Key).ToList();
            }
            catch (Exception ex)
            {
                await _logService.LogToDynamoAsync(new DynamoLogEntry
                {
                    Action = "list_files_error",
                    Details = $"Error listing files: {ex.Message}"
                });
                return new List<string>();
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                await _s3Client.DeleteObjectAsync(_bucketName, fileName);
                await _logService.LogToDynamoAsync(new DynamoLogEntry
                {
                    Action = "file_delete",
                    Details = "File deleted successfully",
                    FileName = fileName,
                    S3Bucket = _bucketName,
                    S3Key = fileName
                });
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogToDynamoAsync(new DynamoLogEntry
                {
                    Action = "file_delete_error",
                    Details = $"Error deleting file: {ex.Message}",
                    FileName = fileName
                });
                return false;
            }
        }
    }
}
