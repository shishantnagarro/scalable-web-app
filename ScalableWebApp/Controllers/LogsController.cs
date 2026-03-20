using Microsoft.AspNetCore.Mvc;
using ScalableWebApp.Models;
using ScalableWebApp.Services;

namespace ScalableWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet("dynamo")]
        public async Task<IActionResult> GetDynamoLogs([FromQuery] int limit = 50)
        {
            var logs = await _logService.GetLogsAsync(limit);
            return Ok(logs);
        }

        [HttpGet("rds")]
        public async Task<IActionResult> GetRdsLogs([FromQuery] int limit = 50)
        {
            var logs = await _logService.GetRdsLogsAsync(limit);
            return Ok(logs);
        }

        [HttpPost("rds")]
        public async Task<IActionResult> CreateRdsLog([FromBody] ApplicationLog log)
        {
            log.Timestamp = DateTime.UtcNow;
            log.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _logService.LogToRdsAsync(log);
            return Ok(new { Message = "Log created successfully" });
        }
    }
}
