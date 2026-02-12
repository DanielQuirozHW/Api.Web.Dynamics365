using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration configuration;

        public OpenAIController(ApplicationDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        public class OpenAIChatRequest
        {
            public string productKey { get; set; }
            public string threadId { get; set; }
            public string message { get; set; }
        }

        public class OpenAIChatResponse
        {
            public bool ok { get; set; }
            public string threadId { get; set; }
            public string reply { get; set; }
            public string error { get; set; }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/openai/chat")]
        public async Task<IActionResult> Chat([FromBody] OpenAIChatRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.message))
                    return BadRequest("El mensaje es inválido.");

                if (string.IsNullOrWhiteSpace(request.productKey))
                    return BadRequest("Debe enviar productKey.");

                #region Credenciales
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                    return BadRequest("El usuario no contiene un cliente asociado para operar.");

                var cliente_db = clienteClaim.Value;

                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                    return BadRequest("No existen credenciales para ese cliente.");
                #endregion

                string OPENAI_API_KEY = configuration["OpenAI:ApiKey"];

                if (string.IsNullOrWhiteSpace(OPENAI_API_KEY))
                    return BadRequest("No está configurada la ApiKey de OpenAI en appsettings.");

                string assistantId = configuration[$"OpenAI:Assistants:{request.productKey}"];

                if (string.IsNullOrWhiteSpace(assistantId))
                    return BadRequest("No existe configuración de asistente para ese producto.");

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OPENAI_API_KEY);
                client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");

                string threadId = request.threadId;

                if (string.IsNullOrWhiteSpace(threadId))
                {
                    var threadBody = new
                    {
                        messages = new[]
                        {
                            new { role = "user", content = request.message }
                        }
                    };

                    var threadRes = await client.PostAsync(
                        "https://api.openai.com/v1/threads",
                        new StringContent(JsonConvert.SerializeObject(threadBody), Encoding.UTF8, "application/json")
                    );

                    var threadJson = await threadRes.Content.ReadAsStringAsync();

                    if (!threadRes.IsSuccessStatusCode)
                        return BadRequest($"Error al crear thread: {threadJson}");

                    threadId = JObject.Parse(threadJson)["id"]?.ToString();
                }
                else
                {
                    var msgBody = new
                    {
                        role = "user",
                        content = request.message
                    };

                    var msgRes = await client.PostAsync(
                        $"https://api.openai.com/v1/threads/{threadId}/messages",
                        new StringContent(JsonConvert.SerializeObject(msgBody), Encoding.UTF8, "application/json")
                    );

                    var msgJson = await msgRes.Content.ReadAsStringAsync();

                    if (!msgRes.IsSuccessStatusCode)
                        return BadRequest($"Error al agregar mensaje: {msgJson}");
                }

                var runBody = new
                {
                    assistant_id = assistantId
                };

                var runRes = await client.PostAsync(
                    $"https://api.openai.com/v1/threads/{threadId}/runs",
                    new StringContent(JsonConvert.SerializeObject(runBody), Encoding.UTF8, "application/json")
                );

                var runJson = await runRes.Content.ReadAsStringAsync();

                if (!runRes.IsSuccessStatusCode)
                    return BadRequest($"Error al crear run: {runJson}");

                var runData = JObject.Parse(runJson);
                string runId = runData["id"]?.ToString();
                string status = runData["status"]?.ToString();

                while (status == "queued" || status == "in_progress")
                {
                    await Task.Delay(1000);

                    var checkRes = await client.GetAsync($"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");
                    var checkJson = await checkRes.Content.ReadAsStringAsync();

                    if (!checkRes.IsSuccessStatusCode)
                        return BadRequest($"Error consultando run: {checkJson}");

                    status = JObject.Parse(checkJson)["status"]?.ToString();
                }

                if (status != "completed")
                    return BadRequest($"El asistente no completó. Estado final: {status}");

                var msgsRes = await client.GetAsync($"https://api.openai.com/v1/threads/{threadId}/messages?limit=10&order=desc");
                var msgsJson = await msgsRes.Content.ReadAsStringAsync();

                if (!msgsRes.IsSuccessStatusCode)
                    return BadRequest($"Error leyendo mensajes: {msgsJson}");

                var msgsData = JObject.Parse(msgsJson);

                var assistantMsg = msgsData["data"]
                    ?.FirstOrDefault(x => x["role"]?.ToString() == "assistant");

                string reply = assistantMsg?["content"]
                    ?.Where(x => x["type"]?.ToString() == "text")
                    ?.Select(x => x["text"]?["value"]?.ToString())
                    ?.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(reply))
                    reply = "No pude leer la respuesta del asistente.";

                reply = Regex.Replace(reply, "【[^】]*】", "").Trim();

                return Ok(new OpenAIChatResponse
                {
                    ok = true,
                    threadId = threadId,
                    reply = reply
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new OpenAIChatResponse
                {
                    ok = false,
                    error = ex.Message
                });
            }
        }
    }
}
