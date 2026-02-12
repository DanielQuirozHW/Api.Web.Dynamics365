using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class ConsultafetchController : ControllerBase
    {
        public ConsultafetchController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public ApiDynamics api = new ApiDynamics();
        public JArray respuesta = null;
        private readonly ApplicationDbContext context;

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/consultafetch")]
        public async Task<ActionResult> EjecutarFetch([FromBody] Fetch fetch)
        {
            try
            {
                List<dynamic> listaEntidad = new List<dynamic>();
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    throw new Exception("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    throw new Exception("No existen credenciales para ese cliente.");
                }

                api.EntityName = fetch.entidad;
                api.FetchXML = WebUtility.UrlEncode(fetch.fetch);

                respuesta = api.RetrieveMultipleWithFetch(api, credenciales);

                if (respuesta != null)
                {
                    foreach (var item in respuesta.Children())
                    {
                        dynamic entidad = JsonConvert.DeserializeObject<dynamic>(item.ToString());
                        listaEntidad.Add(entidad);
                    }
                }

                string respuestaFetch = JsonConvert.SerializeObject(listaEntidad);

                return Ok(respuestaFetch);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/consultafetchs")]
        public async Task<ActionResult> EjecutarFetchs([FromBody] Fetch fetch)
        {
            try
            {
                List<dynamic> listaEntidad = new List<dynamic>();
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    throw new Exception("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    throw new Exception("No existen credenciales para ese cliente.");
                }

                ApiDynamicsV2 apiV2 = new()
                {
                    EntityName = fetch.entidad,
                    FetchXML = fetch.fetch
                };

                respuesta = await apiV2.RetrieveMultipleWithFetch(apiV2, credenciales);

                if (respuesta != null)
                {
                    foreach (var item in respuesta.Children())
                    {
                        dynamic entidad = JsonConvert.DeserializeObject<dynamic>(item.ToString());
                        listaEntidad.Add(entidad);
                    }
                }

                string respuestaFetch = JsonConvert.SerializeObject(listaEntidad);

                return Ok(respuestaFetch);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
