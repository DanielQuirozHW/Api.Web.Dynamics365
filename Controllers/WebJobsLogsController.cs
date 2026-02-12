using Api.Web.Dynamics365.Models.Kudu;
using Api.Web.Dynamics365.Servicios.Kudu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebJobsLogsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<KuduOptions> _kuduOptions;
        private readonly IWebJobCurrentExecutionService _currentExecution;

        public WebJobsLogsController(IHttpClientFactory httpClientFactory, IOptionsMonitor<KuduOptions> kuduOptions, IWebJobCurrentExecutionService currentExecution)
        {
            _httpClientFactory = httpClientFactory;
            _kuduOptions = kuduOptions;
            _currentExecution = currentExecution;
        }

        [HttpGet("job-log")]
        public async Task<IActionResult> GetJobLog(
            [FromQuery] string appService,
            [FromQuery] string webJobName,
            [FromQuery] string jobType = "continuous")
        {
            if (string.IsNullOrWhiteSpace(appService))
                return BadRequest("appService es requerido.");

            if (string.IsNullOrWhiteSpace(webJobName))
                return BadRequest("webJobName es requerido.");

            var normalizedType = NormalizeJobType(jobType);
            if (normalizedType == null)
                return BadRequest("jobType debe ser 'continuous' o 'triggered'.");

            var (ok, cfg, error) = ReadKuduConfig(appService);
            if (!ok) return BadRequest(error);

            var relativePath =
                $"/api/vfs/data/jobs/{normalizedType}/{Uri.EscapeDataString(webJobName)}/job_log.txt";
            var url = CombineUrl(cfg.ScmBaseUrl, relativePath);

            var client = _httpClientFactory.CreateClient();
            AddBasicAuth(client, cfg.Username, cfg.Password);

            HttpResponseMessage resp;
            try
            {
                resp = await client.GetAsync(url);
            }
            catch (Exception ex)
            {
                return StatusCode(502, $"Error llamando Kudu/SCM: {ex.Message}");
            }

            if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
            {
                return StatusCode((int)resp.StatusCode,
                    "No autorizado contra Kudu/SCM. Verificá Username/Password del publish profile y que SCM Basic Auth esté habilitado.");
            }

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound("No se encontró el job_log.txt. Verificá el nombre del WebJob y si es continuous/triggered.");
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                return StatusCode((int)resp.StatusCode, $"Kudu devolvió {(int)resp.StatusCode}: {body}");
            }

            var logText = await resp.Content.ReadAsStringAsync();
            return Content(logText, "text/plain; charset=utf-8");
        }

        [HttpGet("job-current")]
        public async Task<IActionResult> GetCurrentExecution(
            [FromQuery] string appService = "api-educativa-ean-uat",
            [FromQuery] string webJobName = "WebJob.EAN.EjecucionAPI",
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


        private (bool ok, KuduConfig cfg, string error) ReadKuduConfig(string appServiceKey)
        {
            var all = _kuduOptions.CurrentValue;

            // por si querés hacerlo case-insensitive
            var match = all.FirstOrDefault(k =>
                string.Equals(k.Key, appServiceKey, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(match.Key))
                return (false, new KuduConfig(), $"No existe configuración Kudu para appService '{appServiceKey}'.");

            var cfg = match.Value;

            if (string.IsNullOrWhiteSpace(cfg.ScmBaseUrl) ||
                string.IsNullOrWhiteSpace(cfg.Username) ||
                string.IsNullOrWhiteSpace(cfg.Password))
            {
                return (false, new KuduConfig(),
                    $"La configuración Kudu para '{match.Key}' está incompleta (ScmBaseUrl/Username/Password).");
            }

            cfg.ScmBaseUrl = NormalizeBaseUrl(cfg.ScmBaseUrl);
            return (true, cfg, "");
        }

        private static string NormalizeBaseUrl(string scmBaseUrl)
        {
            scmBaseUrl = scmBaseUrl.Trim().Replace(":443", "").TrimEnd('/');

            if (!scmBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !scmBaseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                scmBaseUrl = "https://" + scmBaseUrl;
            }
            return scmBaseUrl;
        }

        private static void AddBasicAuth(HttpClient client, string username, string password)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        }

        private static string CombineUrl(string baseUrl, string relative)
        {
            baseUrl = baseUrl.TrimEnd('/');
            relative = relative.TrimStart('/');
            return $"{baseUrl}/{relative}";
        }

        private static string? NormalizeJobType(string? jobType)
        {
            if (string.IsNullOrWhiteSpace(jobType)) return "continuous";
            var jt = jobType.Trim().ToLowerInvariant();
            return jt switch
            {
                "continuous" => "continuous",
                "triggered" => "triggered",
                _ => null
            };
        }


    }
}
