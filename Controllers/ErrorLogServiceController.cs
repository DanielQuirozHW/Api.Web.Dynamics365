using Api.Web.Dynamics365.Models;
using Api.Web.Dynamics365.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class ErrorLogsController : ControllerBase
    {
        private readonly IErrorLogService _errorLogService;

        public ErrorLogsController(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/errorlog")]
        public async Task<IActionResult> CreateErrorLog([FromBody] ErrorLog errorLogDto)
        {
            if (errorLogDto == null)
            {
                return BadRequest("Invalid error log data.");
            }

            await _errorLogService.CreateErrorLogAsync(errorLogDto);
            return CreatedAtAction(nameof(CreateErrorLog), new { id = errorLogDto.Timestamp }, errorLogDto);
        }
    }
}
