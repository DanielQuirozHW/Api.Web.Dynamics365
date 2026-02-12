using Api.Web.Dynamics365.Models.Kudu;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Web.Dynamics365.Servicios.Kudu
{
    public interface IWebJobCurrentExecutionService
    {
        Task<(bool ok, WebJobCurrentExecutionResponse? data, int? httpStatus, string? error)> GetCurrentExecutionAsync(
            string appService,
            string webJobName,
            string jobType);
    }

    public class WebJobCurrentExecutionService : IWebJobCurrentExecutionService
    {
        private readonly IKuduHttpClientFactory _kudu;

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        // Recorta el payload (pero ahora además filtramos “idle”)
        private const int MAX_TRACE_ENTRIES = 300;

        public WebJobCurrentExecutionService(IKuduHttpClientFactory kudu)
        {
            _kudu = kudu;
        }

        public async Task<(bool ok, WebJobCurrentExecutionResponse? data, int? httpStatus, string? error)> GetCurrentExecutionAsync(
            string appService,
            string webJobName,
            string jobType)
        {
            var normalizedType = NormalizeJobType(jobType);
            if (normalizedType == null)
                return (false, null, 400, "jobType debe ser 'continuous' o 'triggered'.");

            var (okCfg, cfg, errCfg) = _kudu.TryGetConfig(appService);
            if (!okCfg) return (false, null, 400, errCfg);

            var client = _kudu.CreateAuthedClient(cfg);

            if (normalizedType == "continuous")
                return await HandleContinuous(client, cfg, appService, webJobName, normalizedType);

            return await HandleTriggered(client, cfg, appService, webJobName, normalizedType);
        }

        private async Task<(bool ok, WebJobCurrentExecutionResponse? data, int? httpStatus, string? error)> HandleContinuous(
            HttpClient client,
            KuduConfig cfg,
            string appService,
            string webJobName,
            string jobType)
        {
            // 1) Estado “Running/Stopped” desde Kudu
            var statusUrl = _kudu.CombineUrl(cfg.ScmBaseUrl, $"/api/continuouswebjobs/{Uri.EscapeDataString(webJobName)}");

            var statusResp = await SafeGet(client, statusUrl);
            if (!statusResp.ok) return statusResp.fail;

            var jobDto = JsonSerializer.Deserialize<KuduContinuousJobDto>(statusResp.body, JsonOpts);
            var status = jobDto?.Status ?? "Unknown";
            var kudusaysRunning = status.Equals("Running", StringComparison.OrdinalIgnoreCase);

            // Si Kudu no lo marca running, mínimo y listo
            if (!kudusaysRunning)
            {
                return (true, new WebJobCurrentExecutionResponse
                {
                    AppService = appService,
                    WebJobName = webJobName,
                    JobType = jobType,
                    IsRunningNow = false,
                    Status = status,
                    ExecutionId = null,
                    StartTimeUtc = null,
                    EndTimeUtc = null,
                    TruncatedByKudu = false,
                    Entries = new System.Collections.Generic.List<WebJobLogEntry>()
                }, 200, null);
            }

            // 2) Kudu dice “Running”, pero para vos “Running real” depende de que haya ejecución con log “útil”
            // Leemos job_log y recortamos SOLO a la ventana activa.
            var logUrl = _kudu.CombineUrl(cfg.ScmBaseUrl,
                $"/api/vfs/data/jobs/continuous/{Uri.EscapeDataString(webJobName)}/job_log.txt");

            var logResp = await SafeGet(client, logUrl);
            if (!logResp.ok) return logResp.fail;

            var sanitized = WebJobLogSanitizer.Sanitize(logResp.body);
            var parsed = WebJobLogParser.Parse(sanitized);

            // “último runId”, pero ojo: runId puede mantenerse aunque reinicie
            var onlyLastRunId = WebJobLogParser.KeepOnlyLastRun(parsed);

            // ✅ acá está la corrección: quedate SOLO con la última ejecución “real” en progreso
            var activeWindow = WebJobLogParser.KeepOnlyLastActiveWindow(onlyLastRunId);

            // Si no hay mensajes “reales”, entonces NO está corriendo “de verdad” (está en idle / sin mensajes)
            if (activeWindow.Count == 0)
            {
                return (true, new WebJobCurrentExecutionResponse
                {
                    AppService = appService,
                    WebJobName = webJobName,
                    JobType = jobType,
                    IsRunningNow = false,
                    Status = status, // Kudu puede decir Running, pero no hay ejecución útil
                    ExecutionId = null,
                    StartTimeUtc = null,
                    EndTimeUtc = null,
                    TruncatedByKudu = WebJobLogParser.DetectTruncatedByKudu(sanitized),
                    Entries = new System.Collections.Generic.List<WebJobLogEntry>()
                }, 200, null);
            }

            // recorte final por seguridad
            var trimmed = WebJobLogParser.TakeLast(activeWindow, MAX_TRACE_ENTRIES);

            var executionId = trimmed.LastOrDefault(e => !string.IsNullOrWhiteSpace(e.RunId))?.RunId;
            var startTs = trimmed.FirstOrDefault(e => e.Timestamp.HasValue)?.Timestamp;

            return (true, new WebJobCurrentExecutionResponse
            {
                AppService = appService,
                WebJobName = webJobName,
                JobType = jobType,
                IsRunningNow = true,
                Status = status,
                ExecutionId = executionId,
                StartTimeUtc = startTs,
                EndTimeUtc = null,
                TruncatedByKudu = WebJobLogParser.DetectTruncatedByKudu(sanitized),
                Entries = trimmed
            }, 200, null);
        }

        private async Task<(bool ok, WebJobCurrentExecutionResponse? data, int? httpStatus, string? error)> HandleTriggered(
            HttpClient client,
            KuduConfig cfg,
            string appService,
            string webJobName,
            string jobType)
        {
            var infoUrl = _kudu.CombineUrl(cfg.ScmBaseUrl, $"/api/triggeredwebjobs/{Uri.EscapeDataString(webJobName)}");

            var infoResp = await SafeGet(client, infoUrl);
            if (!infoResp.ok) return infoResp.fail;

            var dto = JsonSerializer.Deserialize<KuduTriggeredJobDto>(infoResp.body, JsonOpts);
            var run = dto?.LatestRun;

            var status = run?.Status ?? "Unknown";
            var isRunning = status.Equals("Running", StringComparison.OrdinalIgnoreCase);

            if (!isRunning)
            {
                return (true, new WebJobCurrentExecutionResponse
                {
                    AppService = appService,
                    WebJobName = webJobName,
                    JobType = jobType,
                    IsRunningNow = false,
                    Status = status,
                    ExecutionId = run?.Id,
                    StartTimeUtc = run?.StartTimeUtc,
                    EndTimeUtc = run?.EndTimeUtc,
                    TruncatedByKudu = false,
                    Entries = new System.Collections.Generic.List<WebJobLogEntry>()
                }, 200, null);
            }

            var outputUrl = _kudu.CombineUrl(cfg.ScmBaseUrl, run?.OutputUrl ?? "");
            var errorUrl = _kudu.CombineUrl(cfg.ScmBaseUrl, run?.ErrorUrl ?? "");

            var outputText = "";
            if (!string.IsNullOrWhiteSpace(outputUrl))
            {
                var outResp = await SafeGet(client, outputUrl);
                if (outResp.ok) outputText = outResp.body;
            }

            var errorText = "";
            if (!string.IsNullOrWhiteSpace(errorUrl))
            {
                var errResp = await SafeGet(client, errorUrl);
                if (errResp.ok) errorText = errResp.body;
            }

            var merged = string.IsNullOrWhiteSpace(errorText)
                ? outputText
                : (outputText + "\n\n=== STDERR/ERROR ===\n" + errorText);

            var sanitized = WebJobLogSanitizer.Sanitize(merged);
            var parsed = WebJobLogParser.Parse(sanitized);

            // Para triggered no hace falta “ventana” por runId, ya viene por output_url/error_url.
            var trimmed = WebJobLogParser.TakeLast(parsed, MAX_TRACE_ENTRIES);

            var start = run?.StartTimeUtc ?? trimmed.FirstOrDefault(e => e.Timestamp.HasValue)?.Timestamp;

            return (true, new WebJobCurrentExecutionResponse
            {
                AppService = appService,
                WebJobName = webJobName,
                JobType = jobType,
                IsRunningNow = true,
                Status = status,
                ExecutionId = run?.Id,
                StartTimeUtc = start,
                EndTimeUtc = run?.EndTimeUtc,
                TruncatedByKudu = WebJobLogParser.DetectTruncatedByKudu(sanitized),
                Entries = trimmed
            }, 200, null);
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

        private async Task<(bool ok, string body, (bool ok, WebJobCurrentExecutionResponse? data, int? httpStatus, string? error) fail)> SafeGet(
            HttpClient client,
            string url)
        {
            HttpResponseMessage resp;
            try
            {
                resp = await client.GetAsync(url);
            }
            catch (Exception ex)
            {
                return (false, "", (false, null, 502, $"Error llamando Kudu/SCM: {ex.Message}"));
            }

            if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
            {
                return (false, "", (false, null, (int)resp.StatusCode,
                    "No autorizado contra Kudu/SCM. Verificá Username/Password del publish profile y que SCM Basic Auth esté habilitado."));
            }

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                return (false, "", (false, null, 404, "No se encontró el recurso en Kudu (job o log)."));
            }

            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                return (false, "", (false, null, (int)resp.StatusCode, $"Kudu devolvió {(int)resp.StatusCode}: {body}"));
            }

            return (true, body, default);
        }
    }
}
