using Microsoft.AspNetCore.Mvc;
using ScalableWebApp.Services;

namespace ScalableWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogService _logService;

        public FilesController(IFileService fileService, ILogService logService)
        {
            _fileService = fileService;
            _logService = logService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var result = await _fileService.UploadFileAsync(file);
            if (result.Success) return Ok(result);
            else return BadRequest(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            var files = await _fileService.ListFilesAsync();
            return Ok(files);
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            var result = await _fileService.DeleteFileAsync(fileName);
            if (result) return Ok(new { Message = "File deleted successfully" });
            else return BadRequest(new { Message = "Failed to delete file" });
        }
    }
}
