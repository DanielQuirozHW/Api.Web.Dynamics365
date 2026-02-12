using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class TestConexionController : ControllerBase
    {

        private IConfiguration Configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;

        public TestConexionController(IConfiguration _configuration,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            Configuration = _configuration;
            this.userManager = userManager;
            this.context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("api/testsgr")]
        public async Task<IActionResult> Test()
        {
            try
            {
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

                ApiDynamics apiDynamics = new ApiDynamics();
                string conexion = string.Empty;

                apiDynamics.EntityName = "accounts";
                apiDynamics.FetchXML = "<fetch mapping='logical'>" +
                                                    "<entity name='account'>" +
                                                        "<attribute name='accountid'/>" +
                                                    "</entity>" +
                                                "</fetch>";

                JArray respuesta = apiDynamics.RetrieveMultipleWithFetch(apiDynamics, credenciales);

                if (respuesta.Count >= 0)
                {
                    conexion = "Exitosa";
                }
                else
                {
                    conexion = "Error";
                }

                return Ok(conexion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
