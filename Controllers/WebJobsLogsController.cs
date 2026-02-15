using Api.Web.Dynamics365.Servicios.Kudu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebJobsLogsController : ControllerBase
    {
        private readonly IWebJobCurrentExecutionService _currentExecution;

        public WebJobsLogsController(IWebJobCurrentExecutionService currentExecution)
        {
            _currentExecution = currentExecution;
        }

        [HttpGet("webjob-log")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetWebJobLog(
            [FromQuery] string appService = "sgroneclickSepyme",
            [FromQuery] string webJobName = "GetTipoDeCambio",
            [FromQuery] string jobType = "continuous")
        {
            if (string.IsNullOrWhiteSpace(appService))
                return BadRequest("appService es requerido.");

            if (string.IsNullOrWhiteSpace(webJobName))
                return BadRequest("webJobName es requerido.");

            var (ok, data, httpStatus, error) =
                await _currentExecution.GetCurrentExecutionAsync(appService, webJobName, jobType);

            if (!ok)
                return StatusCode(httpStatus ?? 500, error);

            return Ok(data);
        }
    }
}
