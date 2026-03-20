using ScalableWebApp.Models;

namespace ScalableWebApp.Services
{
    public interface IFileService
    {
        Task<FileUploadResult> UploadFileAsync(IFormFile file);
        Task<List<string>> ListFilesAsync();
        Task<bool> DeleteFileAsync(string fileName);
    }
}
