using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FlowDocController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IMemoryCache cache;

        public FlowDocController(IConfiguration configuration, IMemoryCache cache)
        {
            this.configuration = configuration;
            this.cache = cache;
        }

        public class OrgItem
        {
            public string key { get; set; }
            public string name { get; set; }
        }

        public class RepoSearchItem
        {
            public string org { get; set; }
            public string repo { get; set; }
            public string fullName { get; set; }
            public string description { get; set; }
            public string defaultBranch { get; set; }
        }

        public class RepoSearchResponse
        {
            public bool ok { get; set; }
            public List<RepoSearchItem> items { get; set; } = new();
            public string error { get; set; }
        }

        public class ExplainProjectRequest
        {
            public string productKey { get; set; }
            public string threadId { get; set; }
            public string org { get; set; }
            public string repo { get; set; }
            public string question { get; set; }
        }

        public class ExplainProjectResponse
        {
            public bool ok { get; set; }
            public string threadId { get; set; }
            public string reply { get; set; }
            public object debug { get; set; }
            public string error { get; set; }
        }

        private string GetOpenAiApiKey()
        {
            var key = configuration["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("No está configurada la ApiKey de OpenAI en appsettings.");
            return key;
        }

        private string GetAssistantId(string productKey)
        {
            var assistantId = configuration[$"OpenAI:Assistants:{productKey}"];
            if (string.IsNullOrWhiteSpace(assistantId))
                throw new Exception($"No existe configuración de asistente para productKey={productKey}.");
            return assistantId;
        }

        private string GetGithubToken()
        {
            var t = configuration["GitHub:Token"];
            if (string.IsNullOrWhiteSpace(t))
                throw new Exception("Falta configurar GitHub:Token en appsettings.");
            return t;
        }

        private List<OrgItem> GetAllowedOrgs()
        {
            var raw = configuration["GitHub:Orgs"] ?? "";
            var list = raw
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => new OrgItem { key = x, name = x })
                .ToList();

            return list;
        }

        private bool IsOrgAllowed(string org)
        {
            return GetAllowedOrgs().Any(x => x.key.Equals(org, StringComparison.OrdinalIgnoreCase));
        }

        [HttpGet]
        [Route("api/flowdoc/orgs")]
        public IActionResult GetOrgs()
        {
            var orgs = GetAllowedOrgs();
            return Ok(new { ok = true, items = orgs });
        }

        [HttpGet]
        [Route("api/flowdoc/repos")]
        public async Task<IActionResult> GetRepos([FromQuery] string org)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(org))
                    return BadRequest(new { ok = false, error = "Debe enviar org." });

                if (!IsOrgAllowed(org))
                    return BadRequest(new { ok = false, error = "Organización no permitida." });

                var cacheKey = $"flowdoc:repos:{org.ToLowerInvariant()}";
                if (cache.TryGetValue(cacheKey, out List<RepoSearchItem> cached))
                    return Ok(new { ok = true, items = cached });

                var gh = CreateGitHubClient();

                var all = new List<RepoSearchItem>();
                int page = 1;

                while (true)
                {
                    var url = $"https://api.github.com/orgs/{Uri.EscapeDataString(org)}/repos?per_page=100&page={page}&sort=updated";
                    var res = await gh.GetAsync(url);
                    var json = await res.Content.ReadAsStringAsync();

                    if (!res.IsSuccessStatusCode)
                        return BadRequest(new { ok = false, error = $"GitHub error: {json}" });

                    var arr = JArray.Parse(json);
                    if (arr.Count == 0) break;

                    foreach (var it in arr)
                    {
                        all.Add(new RepoSearchItem
                        {
                            org = org,
                            repo = it["name"]?.ToString(),
                            fullName = it["full_name"]?.ToString(),
                            description = it["description"]?.ToString(),
                            defaultBranch = it["default_branch"]?.ToString()
                        });
                    }

                    if (page >= 10) break;
                    page++;
                }

                cache.Set(cacheKey, all, TimeSpan.FromMinutes(15));
                return Ok(new { ok = true, items = all });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/flowdoc/project")]
        public async Task<IActionResult> ExplainProject([FromBody] ExplainProjectRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new ExplainProjectResponse { ok = false, error = "Request inválido." });

                request.productKey = string.IsNullOrWhiteSpace(request.productKey) ? "flowdoc" : request.productKey;

                if (string.IsNullOrWhiteSpace(request.org))
                    return BadRequest(new ExplainProjectResponse { ok = false, error = "Debe enviar org." });

                if (string.IsNullOrWhiteSpace(request.repo))
                    return BadRequest(new ExplainProjectResponse { ok = false, error = "Debe enviar repo." });

                if (!IsOrgAllowed(request.org))
                    return BadRequest(new ExplainProjectResponse { ok = false, error = "Organización no permitida." });

                var gh = CreateGitHubClient();

                bool isFirstTime = string.IsNullOrWhiteSpace(request.threadId);
                string assistantId = GetAssistantId(request.productKey);

                var openai = CreateOpenAiClient();

                string threadId;
                object debug = null;

                if (isFirstTime)
                {
                    var repoInfo = await GetRepoInfo(gh, request.org, request.repo);
                    var defaultBranch = repoInfo.defaultBranch ?? "main";

                    var readme = await TryGetUsefulReadme(gh, request.org, request.repo);

                    var allCsPaths = await ListCsFilesRecursive(gh, request.org, request.repo, defaultBranch);

                    allCsPaths = allCsPaths
                        .Where(p =>
                            !p.Contains("/bin/", StringComparison.OrdinalIgnoreCase) &&
                            !p.Contains("/obj/", StringComparison.OrdinalIgnoreCase) &&
                            !p.Contains("/packages/", StringComparison.OrdinalIgnoreCase) &&
                            !p.Contains("/properties/", StringComparison.OrdinalIgnoreCase) &&
                            !IsNoisyCsFile(p)
                        )
                        .ToList();

                    if (allCsPaths.Count == 0 && string.IsNullOrWhiteSpace(readme))
                    {
                        return Ok(new ExplainProjectResponse
                        {
                            ok = true,
                            threadId = request.threadId,
                            reply = $"No encontré material relevante (.cs filtrados o README útil) en {request.org}/{request.repo}."
                        });
                    }

                    const int MAX_FILES = 25;
                    const int MAX_CHARS_PER_FILE = 18_000;
                    const int MAX_TOTAL_CHARS = 180_000;

                    var selectedPaths = allCsPaths
                        .OrderByDescending(p =>
                            p.EndsWith("Program.cs", StringComparison.OrdinalIgnoreCase) ? 100 :
                            p.Contains("/Servicios/", StringComparison.OrdinalIgnoreCase) ? 90 :
                            0
                        )
                        .ThenBy(p => p.Contains("/Modelos/", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                        .ThenBy(p => p)
                        .Take(MAX_FILES)
                        .ToList();

                    var files = new List<(string path, string content)>();
                    int totalChars = 0;

                    foreach (var path in selectedPaths)
                    {
                        var content = await TryGetRepoFileText(gh, request.org, request.repo, path);
                        if (string.IsNullOrWhiteSpace(content))
                            continue;

                        if (content.Length > MAX_CHARS_PER_FILE)
                            content = content.Substring(0, MAX_CHARS_PER_FILE) + "\n\n// [TRUNCATED]";

                        if (totalChars + content.Length > MAX_TOTAL_CHARS)
                            break;

                        totalChars += content.Length;
                        files.Add((path, content));
                    }

                    if (files.Count == 0 && string.IsNullOrWhiteSpace(readme))
                    {
                        return Ok(new ExplainProjectResponse
                        {
                            ok = true,
                            threadId = request.threadId,
                            reply = $"Encontré .cs, pero no pude descargar contenido (permisos/token). Revisá permisos del token o el repo."
                        });
                    }

                    var contextBuilder = new StringBuilder();
                    contextBuilder.AppendLine($"Proyecto (Repo): {request.org}/{request.repo}");
                    contextBuilder.AppendLine($"Branch: {defaultBranch}");

                    if (!string.IsNullOrWhiteSpace(readme))
                    {
                        contextBuilder.AppendLine();
                        contextBuilder.AppendLine("=== README (útil) ===");
                        contextBuilder.AppendLine(readme);
                    }

                    if (files.Count > 0)
                    {
                        contextBuilder.AppendLine();
                        contextBuilder.AppendLine("=== CÓDIGO RELEVANTE (fragmentos) ===");
                        foreach (var f in files)
                        {
                            contextBuilder.AppendLine($"\n--- FILE: {f.path} ---\n");
                            contextBuilder.AppendLine(f.content);
                        }
                    }

                    var userPrompt = new StringBuilder();
                    userPrompt.AppendLine("MODO: DOCUMENTACIÓN");
                    userPrompt.AppendLine("Generá la documentación completa siguiendo el FORMATO OBLIGATORIO.");

                    if (!string.IsNullOrWhiteSpace(request.question))
                    {
                        userPrompt.AppendLine();
                        userPrompt.AppendLine($"Además respondé esta consulta del usuario al final, en un párrafo separado: {request.question}");
                    }

                    threadId = await EnsureThreadWithMessage(openai, request.threadId, contextBuilder.ToString(), userPrompt.ToString());

                    var reply = await RunAssistantAndGetLastReply(openai, threadId, assistantId);
                    reply = Regex.Replace(reply ?? "", "【[^】]*】", "").Trim();

                    debug = new
                    {
                        defaultBranch,
                        csFilesFound = allCsPaths.Count,
                        downloadedFiles = files.Select(x => x.path).ToList(),
                        readmeIncluded = !string.IsNullOrWhiteSpace(readme),
                        totalChars
                    };

                    return Ok(new ExplainProjectResponse
                    {
                        ok = true,
                        threadId = threadId,
                        reply = reply,
                        debug = debug
                    });
                }
                else
                {
                    var followup = new StringBuilder();
                    followup.AppendLine("MODO: SEGUIMIENTO");
                    if (string.IsNullOrWhiteSpace(request.question))
                        followup.AppendLine("Aportá detalle adicional sobre el punto más relevante de la documentación previa.");
                    else
                        followup.AppendLine($"Pregunta del usuario: {request.question.Trim()}");

                    threadId = await AddMessageToThread(openai, request.threadId, followup.ToString());

                    var reply = await RunAssistantAndGetLastReply(openai, threadId, assistantId);
                    reply = Regex.Replace(reply ?? "", "【[^】]*】", "").Trim();

                    return Ok(new ExplainProjectResponse
                    {
                        ok = true,
                        threadId = threadId,
                        reply = reply,
                        debug = new { reusedThread = true }
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ExplainProjectResponse { ok = false, error = ex.Message });
            }
        }

        private bool IsNoisyCsFile(string path)
        {
            var file = path.Replace("\\", "/");
            var name = file.Split('/').LastOrDefault() ?? file;

            if (name.Equals("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.EndsWith(".AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private bool IsBoilerplateReadme(string text, string repoName)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;

            var t = text.Trim();

            if (t.Length < 120) return true;

            if (t.Contains("Repositorio migrado desde TFVC a GitHub", StringComparison.OrdinalIgnoreCase))
                return true;

            if (t.Contains("Sin descripcion disponible", StringComparison.OrdinalIgnoreCase))
                return true;

            var compact = Regex.Replace(t, @"\s+", " ").Trim();
            if (!string.IsNullOrWhiteSpace(repoName))
            {
                var rn = repoName.Trim();
                if (compact.Equals(rn, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (compact.StartsWith(rn, StringComparison.OrdinalIgnoreCase) && compact.Length < rn.Length + 80)
                    return true;
            }

            return false;
        }

        private async Task<string> TryGetUsefulReadme(HttpClient gh, string org, string repo)
        {
            var candidates = new[]
            {
                "README.md",
                "readme.md",
                "README.MD",
                "README",
                "readme"
            };

            foreach (var c in candidates)
            {
                var txt = await TryGetRepoFileText(gh, org, repo, c);
                if (string.IsNullOrWhiteSpace(txt)) continue;

                if (!IsBoilerplateReadme(txt, repo))
                    return txt.Trim();
            }

            return null;
        }

        private HttpClient CreateGitHubClient()
        {
            var token = GetGithubToken();
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FlowDocBot/1.0");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            return client;
        }

        private async Task<RepoSearchItem> GetRepoInfo(HttpClient gh, string org, string repo)
        {
            var cacheKey = $"flowdoc:repoInfo:{org.ToLowerInvariant()}:{repo.ToLowerInvariant()}";
            if (cache.TryGetValue(cacheKey, out RepoSearchItem cached))
                return cached;

            var url = $"https://api.github.com/repos/{Uri.EscapeDataString(org)}/{Uri.EscapeDataString(repo)}";
            var res = await gh.GetAsync(url);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception($"GitHub repo info error: {json}");

            var jo = JObject.Parse(json);
            var item = new RepoSearchItem
            {
                org = org,
                repo = repo,
                fullName = jo["full_name"]?.ToString(),
                description = jo["description"]?.ToString(),
                defaultBranch = jo["default_branch"]?.ToString()
            };

            cache.Set(cacheKey, item, TimeSpan.FromMinutes(30));
            return item;
        }

        private async Task<List<string>> ListCsFilesRecursive(HttpClient gh, string org, string repo, string branch)
        {
            var refUrl = $"https://api.github.com/repos/{Uri.EscapeDataString(org)}/{Uri.EscapeDataString(repo)}/git/ref/heads/{Uri.EscapeDataString(branch)}";
            var refRes = await gh.GetAsync(refUrl);
            var refJson = await refRes.Content.ReadAsStringAsync();

            if (!refRes.IsSuccessStatusCode)
                throw new Exception($"GitHub ref error: {refJson}");

            var refObj = JObject.Parse(refJson);
            var sha = refObj["object"]?["sha"]?.ToString();
            if (string.IsNullOrWhiteSpace(sha))
                throw new Exception("No pude obtener SHA del branch.");

            var treeUrl = $"https://api.github.com/repos/{Uri.EscapeDataString(org)}/{Uri.EscapeDataString(repo)}/git/trees/{sha}?recursive=1";
            var treeRes = await gh.GetAsync(treeUrl);
            var treeJson = await treeRes.Content.ReadAsStringAsync();

            if (!treeRes.IsSuccessStatusCode)
                throw new Exception($"GitHub tree error: {treeJson}");

            var treeObj = JObject.Parse(treeJson);
            var arr = (treeObj["tree"] as JArray) ?? new JArray();

            var paths = arr
                .Where(x => x["type"]?.ToString() == "blob")
                .Select(x => x["path"]?.ToString())
                .Where(p => !string.IsNullOrWhiteSpace(p) && p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return paths;
        }

        private async Task<string> TryGetRepoFileText(HttpClient gh, string org, string repo, string path)
        {
            var url = $"https://api.github.com/repos/{Uri.EscapeDataString(org)}/{Uri.EscapeDataString(repo)}/contents/{Uri.EscapeDataString(path)}";
            var res = await gh.GetAsync(url);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return null;

            var jo = JObject.Parse(json);
            var type = jo["type"]?.ToString();
            if (type != "file")
                return null;

            var encoding = jo["encoding"]?.ToString();
            var content = jo["content"]?.ToString();

            if (encoding == "base64" && !string.IsNullOrWhiteSpace(content))
            {
                var clean = content.Replace("\n", "").Replace("\r", "");
                var bytes = Convert.FromBase64String(clean);
                return Encoding.UTF8.GetString(bytes);
            }

            var downloadUrl = jo["download_url"]?.ToString();
            if (!string.IsNullOrWhiteSpace(downloadUrl))
            {
                var rawRes = await gh.GetAsync(downloadUrl);
                if (!rawRes.IsSuccessStatusCode) return null;
                return await rawRes.Content.ReadAsStringAsync();
            }

            return null;
        }

        private HttpClient CreateOpenAiClient()
        {
            var key = GetOpenAiApiKey();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
            return client;
        }

        private async Task<string> EnsureThreadWithMessage(HttpClient openai, string threadId, string context, string userTask)
        {
            var content = new StringBuilder();
            content.AppendLine(context);
            content.AppendLine();
            content.AppendLine("=== TAREA ===");
            content.AppendLine(userTask);

            if (string.IsNullOrWhiteSpace(threadId))
            {
                var threadBody = new
                {
                    messages = new[]
                    {
                        new { role = "user", content = content.ToString() }
                    }
                };

                var threadRes = await openai.PostAsync(
                    "https://api.openai.com/v1/threads",
                    new StringContent(JsonConvert.SerializeObject(threadBody), Encoding.UTF8, "application/json")
                );

                var threadJson = await threadRes.Content.ReadAsStringAsync();
                if (!threadRes.IsSuccessStatusCode)
                    throw new Exception($"Error al crear thread: {threadJson}");

                return JObject.Parse(threadJson)["id"]?.ToString();
            }
            else
            {
                var msgBody = new { role = "user", content = content.ToString() };

                var msgRes = await openai.PostAsync(
                    $"https://api.openai.com/v1/threads/{threadId}/messages",
                    new StringContent(JsonConvert.SerializeObject(msgBody), Encoding.UTF8, "application/json")
                );

                var msgJson = await msgRes.Content.ReadAsStringAsync();
                if (!msgRes.IsSuccessStatusCode)
                    throw new Exception($"Error al agregar mensaje: {msgJson}");

                return threadId;
            }
        }

        private async Task<string> AddMessageToThread(HttpClient openai, string threadId, string message)
        {
            if (string.IsNullOrWhiteSpace(threadId))
                throw new Exception("Falta threadId para agregar pregunta.");

            var msgBody = new { role = "user", content = message };

            var msgRes = await openai.PostAsync(
                $"https://api.openai.com/v1/threads/{threadId}/messages",
                new StringContent(JsonConvert.SerializeObject(msgBody), Encoding.UTF8, "application/json")
            );

            var msgJson = await msgRes.Content.ReadAsStringAsync();
            if (!msgRes.IsSuccessStatusCode)
                throw new Exception($"Error al agregar mensaje: {msgJson}");

            return threadId;
        }

        private async Task<string> RunAssistantAndGetLastReply(HttpClient openai, string threadId, string assistantId)
        {
            var runBody = new
            {
                assistant_id = assistantId,
            };

            var runRes = await openai.PostAsync(
                $"https://api.openai.com/v1/threads/{threadId}/runs",
                new StringContent(JsonConvert.SerializeObject(runBody), Encoding.UTF8, "application/json")
            );

            var runJson = await runRes.Content.ReadAsStringAsync();
            if (!runRes.IsSuccessStatusCode)
                throw new Exception($"Error al crear run: {runJson}");

            var runData = JObject.Parse(runJson);
            var runId = runData["id"]?.ToString();
            var status = runData["status"]?.ToString();

            while (status == "queued" || status == "in_progress")
            {
                await Task.Delay(1000);

                var checkRes = await openai.GetAsync($"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");
                var checkJson = await checkRes.Content.ReadAsStringAsync();

                if (!checkRes.IsSuccessStatusCode)
                    throw new Exception($"Error consultando run: {checkJson}");

                status = JObject.Parse(checkJson)["status"]?.ToString();
            }

            if (status != "completed")
                throw new Exception($"El asistente no completó. Estado final: {status}");

            var msgsRes = await openai.GetAsync($"https://api.openai.com/v1/threads/{threadId}/messages?limit=20&order=desc");
            var msgsJson = await msgsRes.Content.ReadAsStringAsync();

            if (!msgsRes.IsSuccessStatusCode)
                throw new Exception($"Error leyendo mensajes: {msgsJson}");

            var msgsData = JObject.Parse(msgsJson);

            var assistantMsg = msgsData["data"]
                ?.FirstOrDefault(x => x["role"]?.ToString() == "assistant");

            var reply = assistantMsg?["content"]
                ?.Where(x => x["type"]?.ToString() == "text")
                ?.Select(x => x["text"]?["value"]?.ToString())
                ?.FirstOrDefault();

            return string.IsNullOrWhiteSpace(reply) ? "No pude leer la respuesta del asistente." : reply;
        }
    }
}
