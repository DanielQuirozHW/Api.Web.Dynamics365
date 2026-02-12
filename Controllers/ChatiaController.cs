using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Api.Web.Dynamics365.Servicios;
using Microsoft.EntityFrameworkCore;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class ChatiaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IErrorLogService errorLogService;

        public ChatiaController(ApplicationDbContext context, IErrorLogService errorLogService)
        {
            this.context = context;
            this.errorLogService = errorLogService;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/chatia/generarconsulta")]
        public async Task<ActionResult> DetalleDocumentos([FromBody] ChatIA chatIA)
        {
            try
            {
                #region Credenciales
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    return BadRequest("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    return BadRequest("No existen credenciales para ese cliente.");
                }
                #endregion
                ApiDynamicsV2 api = new(errorLogService);

                JObject _chatIA = new()
                {
                    { "new_Usuario@odata.bind", $"/contacts({chatIA.Contactid})" },
                    { "new_Cliente@odata.bind", $"/accounts({chatIA.Accountid})" },
                    { "new_consulta", chatIA.Consulta },
                    { "new_respuesta", chatIA.Respuesta }
                };

                if (chatIA.Segmento.HasValue)
                    _chatIA.Add("new_segmento", chatIA.Segmento.Value);

                if (!string.IsNullOrEmpty(chatIA.PaginaRespuesta))
                    _chatIA.Add("new_paginasderepuesta", chatIA.PaginaRespuesta);
                 
                ResponseAPI respuesta = await api.CreateRecord("new_chatias", _chatIA, credenciales);

                if (!respuesta.ok)
                {
                    return BadRequest(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
