using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Api.Web.Dynamics365.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Configuration;
using static Api.Web.Dynamics365.Models.PortalSocioProtector;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class PortalSocioProtectorController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly IErrorLogService errorLogService;

        public PortalSocioProtectorController(IConfiguration _configuration,
            UserManager<ApplicationUser> userManager, ApplicationDbContext context,
            IErrorLogService errorLogService)
        {
            configuration = _configuration;
            this.userManager = userManager;
            this.context = context;
            this.errorLogService = errorLogService;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/protectores/aportes")]
        public async Task<IActionResult> ActualizarEmpleado([FromBody] Aportes aportes)
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
                JObject _aporte = new()
                {
                    { "new_montointegrado",  aportes.new_Montointegrado},
                    { "new_fechadelaporte",  aportes.new_Fechadelaporte},
                    { "statuscode", aportes.Statuscode },
                    { "new_Cuenta@odata.bind", "/accounts(" + aportes.new_Cuenta + ")" }
                };

                if (!string.IsNullOrEmpty(aportes.new_Comentarios))
                    _aporte.Add("new_comentarios", aportes.new_Comentarios);

                ResponseAPI respuesta = await api.CreateRecord("new_aportesalfondoderiesgos", _aporte, credenciales);

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
