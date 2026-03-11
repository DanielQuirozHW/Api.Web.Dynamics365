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

        private const int MAX_REAL_TRACE_ENTRIES = 2000;

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
            var statusUrl = _kudu.CombineUrl(cfg.ScmBaseUrl, $"/api/continuouswebjobs/{Uri.EscapeDataString(webJobName)}");

            var statusResp = await SafeGet(client, statusUrl);
            if (!statusResp.ok) return statusResp.fail;

            var jobDto = JsonSerializer.Deserialize<KuduContinuousJobDto>(statusResp.body, JsonOpts);
            var status = jobDto?.Status ?? "Unknown";
            var kudusaysRunning = status.Equals("Running", StringComparison.OrdinalIgnoreCase);

            var logUrl = _kudu.CombineUrl(cfg.ScmBaseUrl,
                $"/api/vfs/data/jobs/continuous/{Uri.EscapeDataString(webJobName)}/job_log.txt");

            var logResp = await SafeGet(client, logUrl);
            if (!logResp.ok) return logResp.fail;

            var rawText = logResp.body ?? "";

            var parsed = WebJobLogParser.Parse(rawText);

            var lastRun = WebJobLogParser.KeepOnlyLastRun(parsed);

            var truncated = WebJobLogParser.DetectTruncatedByKudu(lastRun);

            var activeRange = WebJobLogParser.FindLastActiveRange(lastRun);
            var hasActive = kudusaysRunning && activeRange.hasActive;

            var real = WebJobLogParser.ExtractRealTrace(lastRun, activeRange.startIndex, activeRange.endIndex, hasActive);

            if (real.Count > MAX_REAL_TRACE_ENTRIES)
                real = real.Skip(real.Count - MAX_REAL_TRACE_ENTRIES).ToList();

            var execId = hasActive
                ? lastRun.LastOrDefault(e =>
                    e.Index >= activeRange.startIndex &&
                    e.Index <= activeRange.endIndex &&
                    !string.IsNullOrWhiteSpace(e.RunId))?.RunId
                : null;

            var startTs = hasActive
                ? real.FirstOrDefault(e => e.IsCurrentExecution && e.Timestamp.HasValue)?.Timestamp
                : null;

            return (true, new WebJobCurrentExecutionResponse
            {
                AppService = appService,
                WebJobName = webJobName,
                JobType = jobType,
                IsRunningNow = hasActive,
                Status = status,
                ExecutionId = execId,
                StartTimeUtc = startTs,
                TruncatedByKudu = truncated,
                Entries = real
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

            var rawText = merged ?? "";
            var parsed = WebJobLogParser.Parse(rawText);
            var truncated = WebJobLogParser.DetectTruncatedByKudu(parsed);

            var range = (startIndex: 0, endIndex: parsed.Count > 0 ? parsed.Max(e => e.Index) : -1);
            var real = WebJobLogParser.ExtractRealTrace(parsed, range.startIndex, range.endIndex, isRunning);

            if (real.Count > MAX_REAL_TRACE_ENTRIES)
                real = real.Skip(real.Count - MAX_REAL_TRACE_ENTRIES).ToList();

            var start = run?.StartTimeUtc ?? real.FirstOrDefault(e => e.Timestamp.HasValue)?.Timestamp;

            return (true, new WebJobCurrentExecutionResponse
            {
                AppService = appService,
                WebJobName = webJobName,
                JobType = jobType,
                IsRunningNow = isRunning,
                Status = status,
                ExecutionId = run?.Id,
                StartTimeUtc = start,
                TruncatedByKudu = truncated,
                Entries = real
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
