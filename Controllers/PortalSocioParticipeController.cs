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
using System.Net;
using static Api.Web.Dynamics365.Models.Megatlon;
using static Api.Web.Dynamics365.Models.PortalSocioParticipe;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Net.Mail;
using static Api.Web.Dynamics365.Models.PortalCASFOG;
using Azure;
using Api.Web.Dynamics365.Servicios;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class PortalSocioParticipeController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly IErrorLogService errorLogService;

        public PortalSocioParticipeController(IConfiguration _configuration,
            UserManager<ApplicationUser> userManager, ApplicationDbContext context,
            IErrorLogService errorLogService)
        {
            configuration = _configuration;
            this.userManager = userManager;
            this.context = context;
            this.errorLogService = errorLogService;
        }

        #region Relaciones de Vinculacion
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/relaciondevinculacion")]
        public async Task<IActionResult> RelacionDeVinculacion([FromBody] RelacionDeVinculacion relacion)
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
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(relacion);
                ApiDynamicsV2 apiDynamicsV2 = new(errorLogService, urlCompleta, jsonBody);
                ApiDynamics apiDynamics = new();
                string resultadoRelacion = string.Empty;

                if (relacion.cuenta != null)
                {
                    JArray respuestaCuenta = await BuscarCuenta(relacion.cuenta.new_nmerodedocumento, apiDynamicsV2, credenciales); //Verificamos si la cuenta ya existe en Dynamics
                    if (respuestaCuenta.Count == 0)
                    {
                        string cuenta_id = await CrearCuenta(apiDynamicsV2, relacion.cuenta, credenciales);

                        if (cuenta_id != string.Empty)
                        {
                            resultadoRelacion = await CrearRelacion(apiDynamicsV2, relacion, credenciales, relacion.esFirmante, cuenta_id);
                        }
                    }
                    else if (respuestaCuenta.Count > 0)
                    {
                        CuentaRelacionada cuenta = JsonConvert.DeserializeObject<CuentaRelacionada>(respuestaCuenta.First.ToString());

                        if (cuenta.accountid != null)
                            resultadoRelacion = await CrearRelacion(apiDynamicsV2, relacion, credenciales, relacion.esFirmante, cuenta.accountid);
                    }
                }
                else if (relacion.contacto != null)
                {
                    JArray respuestaContacto = await BuscarContacto(relacion.contacto.new_cuitcuil, apiDynamicsV2, credenciales); //Verificamos si el contacto ya existe en Dynamics
                    if (respuestaContacto.Count == 0)
                    {
                        string contacto_id = await CrearContacto(apiDynamicsV2, relacion.contacto, credenciales);

                        if (contacto_id != string.Empty)
                        {
                            resultadoRelacion = await CrearRelacion(apiDynamicsV2, relacion, credenciales, relacion.esFirmante, null, contacto_id);
                        }
                    }
                    else if (respuestaContacto.Count > 0)
                    {
                        ContactoRelacionado contacto = JsonConvert.DeserializeObject<ContactoRelacionado>(respuestaContacto.First.ToString());

                        if (contacto.contactid != null)
                            resultadoRelacion = await CrearRelacion(apiDynamicsV2, relacion, credenciales, relacion.esFirmante, null, contacto.contactid);
                    }
                }

                return Ok(resultadoRelacion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/socioparticipe/relaciondevinculacion")]
        public async Task<IActionResult> ActualizarRelacionDeVinculacion([FromBody] RelacionDeVinculacion relacion)
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
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(relacion);
                ApiDynamicsV2 apiDynamicsV2 = new(errorLogService, urlCompleta, jsonBody);
                ApiDynamics apiDynamics = new();
                string resultadoRelacion = string.Empty;
                string resultadoActualizacion = string.Empty;

                if (relacion.cuenta != null)
                {
                    //resultadoActualizacion = ActualizarCuenta(apiDynamics, relacion.cuenta, credenciales);
                }
                else if (relacion.contacto != null)
                {
                    resultadoActualizacion = await ActualizarContacto(apiDynamicsV2, relacion.contacto, credenciales);
                }

                if (resultadoActualizacion == "ERROR")
                {
                    return BadRequest(relacion.cuenta == null ? "Error en la actualización del contacto" : "Error en la actualización de la cuenta");
                }

                if (relacion.new_participacionaccionariaid != null && relacion.new_participacionaccionariaid != string.Empty)
                {
                    resultadoRelacion = await ActualizarRelacion(apiDynamicsV2, relacion, credenciales, relacion.esFirmante);
                }

                return Ok(resultadoRelacion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete]
        [Route("api/socioparticipe/relaciondevinculacion")]
        public async Task<IActionResult> EliminarRelacionDeVinculacion(string new_participacionaccionariaid)
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
                if (new_participacionaccionariaid == null || new_participacionaccionariaid == string.Empty)
                    return BadRequest("El id de la relacion esta vacio");

                ApiDynamics apiDynamics = new();

                string resultado = apiDynamics.DeleteRecord("new_participacionaccionarias", new_participacionaccionariaid, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public static async Task<JArray> BuscarCuentasPorSGR(string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_cuentasporsgrs";

                fetchXML = "<attribute name='new_name'/> " +
                                        "<attribute name='new_sgr'/> " +
                                        "<attribute name='new_cuentasporsgrid'/> " +
                                        "<attribute name='statuscode'/> " +
                                        "<attribute name='new_rol'/> " +
                                        "<attribute name='new_saldobrutogaratiasvigentes'/> " +
                                        "<attribute name='new_saldodeudaporgtiasabonada'/> " +
                                        "<attribute name='new_cantidadgtiasenmora'/> " +
                                        "<attribute name='new_situaciondeladueda'/> " +
                                        "<attribute name='new_diasdeatraso'/> " +
                                        "<filter type='and'>" +
                                            $"<condition attribute='new_socio' operator='eq' value='{cuenta_id}' />" +
                                            "<condition attribute='statecode' operator='eq' value='0' />" +
                                        "</filter>" +
                            "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarCuenta(string cuitCuil, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "accounts";

                fetchXML = "<entity name='account'>" +
                                        "<attribute name='accountid'/> " +
                                        "<filter type='and'>" +
                                            $"<condition attribute='new_nmerodedocumento' operator='eq' value='{cuitCuil}' />" +
                                        "</filter>" +
                            "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarContacto(decimal cuitCuil, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "contacts";

                fetchXML = "<entity name='contact'>" +
                                        "<attribute name='contactid'/> " +
                                        "<filter type='and'>" +
                                            $"<condition attribute='new_cuitcuil' operator='eq' value='{cuitCuil}' />" +
                                        "</filter>" +
                            "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;

                //if (api.EntityName != string.Empty)
                //{

                //    if (fetchXML != string.Empty)
                //    {
                //        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                //    }

                //    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                //}

                //return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<string> CrearCuenta(ApiDynamicsV2 api, CuentaRelacionada cuenta, Credenciales credenciales)
        {
            try
            {
                JObject Cuenta = new()
                {
                    { "new_rol", 100000005 } //Otro
                };

                if (cuenta.name != null && cuenta.name != string.Empty)
                    Cuenta.Add("name", cuenta.name);

                if (cuenta.new_nmerodedocumento != null && cuenta.new_nmerodedocumento != string.Empty)
                    Cuenta.Add("new_nmerodedocumento", cuenta.new_nmerodedocumento);

                if (cuenta.emailaddress1 != null && cuenta.emailaddress1 != string.Empty)
                    Cuenta.Add("emailaddress1", cuenta.emailaddress1);

                ResponseAPI respuesta = await api.CreateRecord("accounts", Cuenta, credenciales);

                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }

                return respuesta.descripcion;
            }
            catch (Exception ex)
            {
                throw ex; 
            }
        }
        public static string ActualizarCuenta(ApiDynamics api, CuentaRelacionada cuenta, Credenciales credenciales)
        {
            JObject Cuenta = new();

            if (cuenta.name != null && cuenta.name != string.Empty)
                Cuenta.Add("name", cuenta.name);

            if (cuenta.new_nmerodedocumento != null && cuenta.new_nmerodedocumento != string.Empty)
                Cuenta.Add("new_nmerodedocumento", cuenta.new_nmerodedocumento);

            if (cuenta.emailaddress1 != null && cuenta.emailaddress1 != string.Empty)
                Cuenta.Add("emailaddress1", cuenta.emailaddress1);

            //if (cuenta.new_tipodedocumentoid != null && cuenta.new_tipodedocumentoid != string.Empty)
            //    Cuenta.Add("new_TipodedocumentoId@odata.bind", "/new_tipodedocumentos(" + cuenta.new_tipodedocumentoid + ")");

            return api.UpdateRecord("accounts", cuenta.accountid, Cuenta, credenciales);
        }
        public static async Task<string> CrearContacto(ApiDynamicsV2 api, ContactoRelacionado contacto, Credenciales credenciales)
        {
            try
            {
                JObject Contacto = new();

                if (contacto.firstname != null && contacto.firstname != string.Empty)
                    Contacto.Add("firstname", contacto.firstname);

                if (contacto.lastname != null && contacto.lastname != string.Empty)
                    Contacto.Add("lastname", contacto.lastname);

                if (contacto.new_cuitcuil >= 0)
                    Contacto.Add("new_cuitcuil", contacto.new_cuitcuil);

                if (contacto.new_nrodedocumento > 0)
                    Contacto.Add("new_nrodedocumento", contacto.new_nrodedocumento);

                if (contacto.emailaddress1 != null && contacto.emailaddress1 != string.Empty)
                    Contacto.Add("emailaddress1", contacto.emailaddress1);

                if (contacto.birthdate != null && contacto.birthdate != string.Empty)
                    Contacto.Add("birthdate", DateTime.Parse(contacto.birthdate).ToString("yyyy-MM-dd"));

                if (contacto.new_lugardenacimiento != null && contacto.new_lugardenacimiento != string.Empty)
                    Contacto.Add("new_lugardenacimiento", contacto.new_lugardenacimiento);

                if (contacto.familystatuscode > 0)
                    Contacto.Add("familystatuscode", contacto.familystatuscode);

                if (contacto.spousesname != null && contacto.spousesname != string.Empty)
                    Contacto.Add("spousesname", contacto.spousesname);

                if (contacto.new_profesionoficioactividad != null && contacto.new_profesionoficioactividad != string.Empty)
                    Contacto.Add("new_profesionoficioactividad", contacto.new_profesionoficioactividad);

                if (contacto.new_correoelectrnicopararecibirestadodecuenta != null && contacto.new_correoelectrnicopararecibirestadodecuenta != string.Empty)
                    Contacto.Add("new_correoelectrnicopararecibirestadodecuenta", contacto.new_correoelectrnicopararecibirestadodecuenta);

                if (contacto.Telephone1 != null && contacto.Telephone1 != string.Empty)
                    Contacto.Add("telephone1", contacto.Telephone1);

                ResponseAPI respuesta = await api.CreateRecord("contacts", Contacto, credenciales);

                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }

                return respuesta.descripcion;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public static async Task<string> ActualizarContacto(ApiDynamicsV2 api, ContactoRelacionado contacto, Credenciales credenciales)
        {
            try
            {
                JObject Contacto = new();

                if (contacto.firstname != null && contacto.firstname != string.Empty)
                    Contacto.Add("firstname", contacto.firstname);

                if (contacto.lastname != null && contacto.lastname != string.Empty)
                    Contacto.Add("lastname", contacto.lastname);

                if (contacto.new_cuitcuil > 0)
                    Contacto.Add("new_cuitcuil", contacto.new_cuitcuil);

                if (contacto.new_nrodedocumento > 0)
                    Contacto.Add("new_nrodedocumento", contacto.new_nrodedocumento);

                if (contacto.emailaddress1 != null && contacto.emailaddress1 != string.Empty)
                    Contacto.Add("emailaddress1", contacto.emailaddress1);

                if (contacto.birthdate != null && contacto.birthdate != string.Empty)
                    Contacto.Add("birthdate", DateTime.Parse(contacto.birthdate).ToString("yyyy-MM-dd"));

                if (contacto.new_lugardenacimiento != null && contacto.new_lugardenacimiento != string.Empty)
                    Contacto.Add("new_lugardenacimiento", contacto.new_lugardenacimiento);

                if (contacto.familystatuscode > 0)
                    Contacto.Add("familystatuscode", contacto.familystatuscode);

                if (contacto.spousesname != null && contacto.spousesname != string.Empty)
                    Contacto.Add("spousesname", contacto.spousesname);

                if (contacto.new_profesionoficioactividad != null && contacto.new_profesionoficioactividad != string.Empty)
                    Contacto.Add("new_profesionoficioactividad", contacto.new_profesionoficioactividad);

                if (contacto.new_correoelectrnicopararecibirestadodecuenta != null && contacto.new_correoelectrnicopararecibirestadodecuenta != string.Empty)
                    Contacto.Add("new_correoelectrnicopararecibirestadodecuenta", contacto.new_correoelectrnicopararecibirestadodecuenta);

                if (contacto.Telephone1 != null && contacto.Telephone1 != string.Empty)
                    Contacto.Add("telephone1", contacto.Telephone1);

                ResponseAPI respuesta = await api.UpdateRecord("contacts", contacto.contactid, Contacto, credenciales);

                if (!respuesta.ok)
                {
                    return "ERROR";
                }

                return respuesta.descripcion;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task<string> CrearRelacion(ApiDynamicsV2 api, RelacionDeVinculacion relacion, Credenciales credenciales,
            string esFirmante, string cuenta_id = null, string contacto_id = null)
        {
            try
            {
                JObject relacionDeVinculacion = new()
                {
                    { "new_CuentaId@odata.bind", "/accounts(" + relacion.accountid + ")" }
                };

                if (cuenta_id != null)
                    relacionDeVinculacion.Add("new_CuentaContactoVinculado_account@odata.bind", "/accounts(" + cuenta_id + ")");

                if (contacto_id != null)
                    relacionDeVinculacion.Add("new_CuentaContactoVinculado_contact@odata.bind", "/contacts(" + contacto_id + ")");

                if (relacion.new_tipoderelacion > 0)
                    relacionDeVinculacion.Add("new_tipoderelacion", relacion.new_tipoderelacion);

                if (relacion.new_porcentajedeparticipacion >= 0)
                    relacionDeVinculacion.Add("new_porcentajedeparticipacion", relacion.new_porcentajedeparticipacion);

                if (relacion.new_observaciones != null)
                    relacionDeVinculacion.Add("new_observaciones", relacion.new_observaciones);

                if (relacion.new_porcentajebeneficiario >= 0)
                    relacionDeVinculacion.Add("new_porcentajebeneficiario", relacion.new_porcentajebeneficiario);

                if (relacion.new_cargo != null)
                    relacionDeVinculacion.Add("new_cargo", relacion.new_cargo);

                if (!string.IsNullOrWhiteSpace(relacion.new_relacion))
                {
                    if (relacion.new_relacion == "false")
                    {
                        relacionDeVinculacion.Add("new_relacion", false);
                    }
                    else if (relacion.new_relacion == "true")
                    {
                        relacionDeVinculacion.Add("new_relacion", true);
                    }
                }

                if (!string.IsNullOrEmpty(esFirmante))
                {
                    relacionDeVinculacion.Add("new_firmante", esFirmante);
                }

                ResponseAPI respuesta = await api.CreateRecord("new_participacionaccionarias", relacionDeVinculacion, credenciales);

                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }

                return respuesta.descripcion;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task<string> ActualizarRelacion(ApiDynamicsV2 api, RelacionDeVinculacion relacion, Credenciales credenciales, string esFirmante)
        {
            try
            {
                JObject relacionDeVinculacion = new();

                if (relacion.new_porcentajedeparticipacion >= 0)
                    relacionDeVinculacion.Add("new_porcentajedeparticipacion", relacion.new_porcentajedeparticipacion);

                if (relacion.new_observaciones != null)
                    relacionDeVinculacion.Add("new_observaciones", relacion.new_observaciones);

                if (relacion.new_porcentajebeneficiario >= 0)
                    relacionDeVinculacion.Add("new_porcentajebeneficiario", relacion.new_porcentajebeneficiario);

                if (relacion.new_cargo != null)
                    relacionDeVinculacion.Add("new_cargo", relacion.new_cargo);

                if (!string.IsNullOrWhiteSpace(relacion.new_relacion))
                {
                    if (relacion.new_relacion == "false")
                    {
                        relacionDeVinculacion.Add("new_relacion", false);
                    }
                    else if (relacion.new_relacion == "true")
                    {
                        relacionDeVinculacion.Add("new_relacion", true);
                    }
                }

                if (!string.IsNullOrEmpty(esFirmante))
                {
                    relacionDeVinculacion.Add("new_firmante", esFirmante);
                }

                ResponseAPI respuesta = await api.UpdateRecord("new_participacionaccionarias", relacion.new_participacionaccionariaid, relacionDeVinculacion, credenciales);

                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }

                return respuesta.descripcion;
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }
        #endregion
        #region Sociedad de Bolsa
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/sociedaddebolsa")]
        public async Task<IActionResult> SociedadDeBolsa([FromBody] SociedadDeBolsa sociedad)
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
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(sociedad);
                ApiDynamicsV2 apiDynamics = new(errorLogService, urlCompleta, jsonBody);

                JObject Sociedad = new()
                {
                    { "new_Sociedaddebolsa@odata.bind", "/new_sociedaddebolsas(" + sociedad.new_sociedaddebolsa + ")" },
                    { "new_Socio@odata.bind", "/accounts(" + sociedad.new_socio + ")" },
                    { "new_cuentacomitente", sociedad.new_cuentacomitente }
                };

                ResponseAPI respuesta = await apiDynamics.CreateRecord("new_sociedaddebolsaporsocios", Sociedad, credenciales);

                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/socioparticipe/sociedaddebolsa")]
        public async Task<IActionResult> ActualizarSociedadDeBolsa([FromBody] SociedadDeBolsa sociedad)
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
                if (sociedad.new_sociedaddebolsaporsocioid == null || sociedad.new_sociedaddebolsaporsocioid == string.Empty)
                    return BadRequest("El id de la sociedad de bolsa esta vacio");

                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(sociedad);
                ApiDynamicsV2 apiDynamics = new(errorLogService, urlCompleta, jsonBody);
                JObject Sociedad = new()
                {
                    { "new_Sociedaddebolsa@odata.bind", "/new_sociedaddebolsas(" + sociedad.new_sociedaddebolsa + ")" },
                    { "new_Socio@odata.bind", "/accounts(" + sociedad.new_socio + ")" },
                    { "new_cuentacomitente", sociedad.new_cuentacomitente }
                };


                ResponseAPI respuesta = await apiDynamics.UpdateRecord("new_sociedaddebolsaporsocios", sociedad.new_sociedaddebolsaporsocioid, Sociedad, credenciales);

                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete]
        [Route("api/socioparticipe/sociedaddebolsa")]
        public async Task<IActionResult> EliminarSociedadDeBolsa(string new_sociedaddebolsaporsocioid)
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
                if (new_sociedaddebolsaporsocioid == null || new_sociedaddebolsaporsocioid == string.Empty)
                    return BadRequest("El id de la sociedad de bolsa esta vacio");

                ApiDynamics apiDynamics = new ApiDynamics();

                string resultado = apiDynamics.DeleteRecord("new_sociedaddebolsaporsocios", new_sociedaddebolsaporsocioid, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/socioparticipe/inactivarsociedaddebolsa")]
        public async Task<IActionResult> InactivarSociedadDeBolsa([FromBody] SociedadDeBolsa sociedad)
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
                if (sociedad.new_sociedaddebolsaporsocioid == null || sociedad.new_sociedaddebolsaporsocioid == string.Empty)
                    return BadRequest("El id de la sociedad de bolsa esta vacio");

                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(sociedad);
                ApiDynamicsV2 apiDynamics = new(errorLogService, urlCompleta, jsonBody);
                string resultado = string.Empty;

                JObject Sociedad = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI respuesta = await apiDynamics.UpdateRecord("new_sociedaddebolsaporsocios", sociedad.new_sociedaddebolsaporsocioid, Sociedad, credenciales);

                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Mi Cuenta
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/socioparticipe/micuenta")]
        public async Task<IActionResult> ActualizarMiCuenta([FromBody] MiCuenta cuenta)
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
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(cuenta);
                ApiDynamicsV2 apiDynamics = new(errorLogService, urlCompleta, jsonBody);

                JObject Cuenta = new();
                if (cuenta.telephone2 != null && cuenta.telephone2 != string.Empty)
                    Cuenta.Add("telephone2", cuenta.telephone2);
                if (cuenta.address1_line1 != null && cuenta.address1_line1 != string.Empty)
                    Cuenta.Add("address1_line1", cuenta.address1_line1);
                if (cuenta.new_direccion1numero != null && cuenta.new_direccion1numero != string.Empty)
                    Cuenta.Add("new_direccion1numero", cuenta.new_direccion1numero);
                if (cuenta.address1_name != null && cuenta.address1_name != string.Empty)
                    Cuenta.Add("address1_name", cuenta.address1_name);
                if (cuenta.new_direccion1depto != null && cuenta.new_direccion1depto != string.Empty)
                    Cuenta.Add("new_direccion1depto", cuenta.new_direccion1depto);
                if (cuenta.new_provincia != null && cuenta.new_provincia != string.Empty)
                    Cuenta.Add("new_Provincia@odata.bind", "/new_provincias(" + cuenta.new_provincia + ")");
                if (cuenta.new_localidad != null && cuenta.new_localidad != string.Empty)
                    Cuenta.Add("new_localidad", cuenta.new_localidad);
                if (cuenta.address1_county != null && cuenta.address1_county != string.Empty)
                    Cuenta.Add("address1_county", cuenta.address1_county.Trim());
                if (cuenta.address1_postalcode != null && cuenta.address1_postalcode != string.Empty)
                    Cuenta.Add("address1_postalcode", cuenta.address1_postalcode);
                if (cuenta.new_inscripcionganancias > 0)
                    Cuenta.Add("new_inscripcionganancias", cuenta.new_inscripcionganancias);
                if (cuenta.new_pais != null && cuenta.new_pais != string.Empty)
                    Cuenta.Add("new_Pais@odata.bind", "/new_paises(" + cuenta.new_pais + ")");
                if (cuenta.new_condiciondeinscripcionanteafip != null && cuenta.new_condiciondeinscripcionanteafip != string.Empty)
                    Cuenta.Add("new_CondiciondeInscripcionanteAFIP@odata.bind", "/new_condiciondeinscipcionanteafips(" + cuenta.new_condiciondeinscripcionanteafip + ")");

                ResponseAPI respuesta = await apiDynamics.UpdateRecord("accounts", cuenta.accountid, Cuenta, credenciales);

                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Documentacion
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/documentacionporcuenta")]
        public async Task<IActionResult> DocumentacionPorCuenta(string documentacionporcuenta_id)
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
                if (documentacionporcuenta_id == null || documentacionporcuenta_id == string.Empty)
                    return BadRequest("El id de la cuenta esta vacio");

                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                ApiDynamicsV2 apiDynamics = new(errorLogService, urlCompleta, documentacionporcuenta_id);
                var archivos = HttpContext.Request.Form.Files;
                string nota_id = string.Empty;

                if (archivos.Count > 0)
                {
                    foreach (var file in archivos)
                    {
                        byte[] fileInBytes = new byte[file.Length];
                        using (BinaryReader theReader = new BinaryReader(file.OpenReadStream()))
                        {
                            fileInBytes = theReader.ReadBytes(Convert.ToInt32(file.Length));
                        }

                        string fileAsString = Convert.ToBase64String(fileInBytes);

                        JObject annotation = new()
                        {
                            { "subject", file.FileName },
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName }
                        };

                        if (documentacionporcuenta_id != string.Empty)
                            annotation.Add("objectid_new_documentacionporcuenta@odata.bind", "/new_documentacionporcuentas(" + documentacionporcuenta_id + ")");

                        ResponseAPI respuesta = await apiDynamics.CreateRecord("annotations", annotation, credenciales);

                        if (!respuesta.ok)
                        {
                            throw new Exception(respuesta.descripcion);
                        }

                        nota_id = respuesta.descripcion;

                        JObject documentacionPorCuenta = new()
                        {
                            { "statuscode", 100000000 }
                        };

                        ResponseAPI respuestaDoc = await apiDynamics.UpdateRecord("new_documentacionporcuentas", documentacionporcuenta_id, documentacionPorCuenta, credenciales);

                        if (!respuestaDoc.ok)
                        {
                            throw new Exception(respuestaDoc.descripcion);
                        }
                    }
                }

                return Ok(nota_id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/documentacionporcuentaynotificacionbo")]
        public async Task<IActionResult> DocumentacionPorCuentaYNotificacion([FromForm] DocumentacionAdjunta documentacion)
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
                if (string.IsNullOrEmpty(documentacion.DocumentacionPorCuentaId))
                    return BadRequest("El id de la cuenta esta vacio");

                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                ApiDynamicsV2 apiDynamics = new(errorLogService, urlCompleta, documentacion.DocumentacionPorCuentaId);
                //var archivos = HttpContext.Request.Form.Files;
                var archivos = documentacion.Archivos;
                string nota_id = string.Empty;

                if (archivos.Count > 0)
                {
                    foreach (var file in archivos)
                    {
                        byte[] fileInBytes = new byte[file.Length];
                        using (BinaryReader theReader = new BinaryReader(file.OpenReadStream()))
                        {
                            fileInBytes = theReader.ReadBytes(Convert.ToInt32(file.Length));
                        }

                        string fileAsString = Convert.ToBase64String(fileInBytes);

                        JObject annotation = new()
                        {
                            { "subject", file.FileName },
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName }
                        };

                        if (documentacion.DocumentacionPorCuentaId != string.Empty)
                            annotation.Add("objectid_new_documentacionporcuenta@odata.bind", "/new_documentacionporcuentas(" + documentacion.DocumentacionPorCuentaId + ")");

                        ResponseAPI respuesta = await apiDynamics.CreateRecord("annotations", annotation, credenciales);

                        if (!respuesta.ok)
                        {
                            throw new Exception(respuesta.descripcion);
                        }

                        nota_id = respuesta.descripcion;

                        JObject documentacionPorCuenta = new()
                        {
                            { "statuscode", 100000000 }
                        };

                        ResponseAPI respuestaDoc = await apiDynamics.UpdateRecord("new_documentacionporcuentas", documentacion.DocumentacionPorCuentaId, documentacionPorCuenta, credenciales);

                        if (!respuestaDoc.ok)
                        {
                            throw new Exception(respuestaDoc.descripcion);
                        }
                    }

                    if (string.IsNullOrEmpty(documentacion.teamid))
                        return Ok(nota_id);

                    List<UsuariosDeEquipo> listaUsuarios = new();
                    JArray usuariosDeEquipo = await BuscarUsuariosPorEquipo(documentacion.teamid, apiDynamics, credenciales);
                    if (usuariosDeEquipo.Count > 0)
                    {
                        listaUsuarios = JsonConvert.DeserializeObject<List<UsuariosDeEquipo>>(usuariosDeEquipo.ToString());
                        if (listaUsuarios.Count > 0)
                        {
                            var dataJson = $@"
                                {{
                                    ""actions"": [
                                    {{
                                        ""title"": ""Abrir registro"",
                                        ""data"": {{
                                        ""url"": ""?pagetype=entityrecord&etn=new_documentacionporcuenta&id={documentacion.DocumentacionPorCuentaId}"",
                                        ""navigationTarget"": ""dialog""
                                        }}
                                    }}
                                    ]
                                }}";

                            foreach (var usuario in listaUsuarios)
                            {
                                JObject Notificacion = new()
                                    {
                                        {"title", "Nuevo documento recibido"},
                                        {"body", $"El socio {documentacion.NombreSocio} ha subido un nuevo documento: {documentacion.NombreDocumento}."},
                                        {"icontype", 100000000},  //Infomarción
                                        {"toasttype", 200000000}, //Programada
                                        {"priority", 200000000}, //Normal
                                        {"ownerid@odata.bind", "/systemusers(" + usuario.systemuserid + ")"},
                                        {"data", dataJson}
                                    };

                                try
                                {
                                    ResponseAPI respuestaNotificacion = await apiDynamics.CreateRecord("appnotifications", Notificacion, credenciales);
                                    if (!respuestaNotificacion.ok)
                                    {
                                        continue;
                                    }
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }

                return Ok(nota_id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/documentacionporcuentayestado")]
        public async Task<IActionResult> DocumentacionPorCuentaYestado(string documentacionporcuenta_id)
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
                if (documentacionporcuenta_id == null || documentacionporcuenta_id == string.Empty)
                    return BadRequest("El id de la cuenta esta vacio");

                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                ApiDynamicsV2 api = new(errorLogService, urlCompleta, documentacionporcuenta_id);
                var archivos = HttpContext.Request.Form.Files;
                string nota_id = string.Empty;

                if (archivos.Count > 0)
                {
                    foreach (var file in archivos)
                    {
                        byte[] fileInBytes = new byte[file.Length];
                        using (BinaryReader theReader = new BinaryReader(file.OpenReadStream()))
                        {
                            fileInBytes = theReader.ReadBytes(Convert.ToInt32(file.Length));
                        }

                        string fileAsString = Convert.ToBase64String(fileInBytes);

                        JObject annotation = new()
                        {
                            { "subject", file.FileName },
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName }
                        };

                        if (documentacionporcuenta_id != string.Empty)
                            annotation.Add("objectid_new_documentacionporcuenta@odata.bind", "/new_documentacionporcuentas(" + documentacionporcuenta_id + ")");

                        ResponseAPI respuesta = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!respuesta.ok)
                        {
                            throw new Exception(respuesta.descripcion);
                        }

                        nota_id = respuesta.descripcion;

                        JObject documentacionPorCuenta = new()
                        {
                            { "statuscode", 100000000 }
                        };

                        ResponseAPI documentoResponse = await api.UpdateRecord("new_documentacionporcuentas", documentacionporcuenta_id, documentacionPorCuenta, credenciales);

                        if (!documentoResponse.ok)
                        {
                            throw new Exception(documentoResponse.descripcion);
                        }

                        nota_id = respuesta.descripcion;
                    }
                }

                return Ok(nota_id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public static async Task<JArray> BuscarUsuariosPorEquipo(string team_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "systemusers";

                fetchXML = "<entity name='systemuser'>" +
                                    "<attribute name='systemuserid'/> " +
                                    "<attribute name='fullname'/> " +
                                    "<link-entity name='teammembership' from='systemuserid' to='systemuserid' intersect='true'>" +
                                            "<filter type='and'>" +
                                            $"<condition attribute='teamid' operator='eq' value='{team_id}' />" +
                                            "</filter>" +
                                    "</link-entity>" +
                        "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        #region Operaciones
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/operaciones")]
        public async Task<IActionResult> CrearOperacion([FromBody] Operaciones op)
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
                ApiDynamics apiDynamics = new ApiDynamics();

                JObject Operacion = new();
                if (op.new_socioparticipe != null && op.new_socioparticipe != string.Empty)
                    Operacion.Add("new_SocioParticipe@odata.bind", "/accounts(" + op.new_socioparticipe + ")");

                if (op.new_tipooperacin > 0)
                    Operacion.Add("new_tipooperacin", op.new_tipooperacin);

                if (op.new_tipodecheque > 0)
                    Operacion.Add("new_tipodecheque", op.new_tipodecheque);

                if (op.new_destinodefondo != null && op.new_destinodefondo != string.Empty)
                    Operacion.Add("new_Destinodefondo@odata.bind", "/new_destinodefondoses(" + op.new_destinodefondo + ")");

                if (op.new_acreedor != null && op.new_acreedor != string.Empty)
                    Operacion.Add("new_Acreedor@odata.bind", "/new_acreedors(" + op.new_acreedor + ")");

                string resultadoOP = apiDynamics.CreateRecord("new_operacions", Operacion, credenciales);

                if (resultadoOP == "ERROR")
                    return BadRequest(resultadoOP);

                return Ok(resultadoOP);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/documentacionporoperacion")]
        public async Task<IActionResult> DocumentacionPorOperacion(string operacion_id)
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
                if (operacion_id == null || operacion_id == string.Empty)
                    return BadRequest("El id de la operacion esta vacio");

                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                ApiDynamicsV2 api = new(errorLogService, urlCompleta, operacion_id);
                var archivos = HttpContext.Request.Form.Files;
                string nota_id = string.Empty;

                if (archivos.Count > 0)
                {
                    foreach (var file in archivos)
                    {
                        byte[] fileInBytes = new byte[file.Length];
                        using (BinaryReader theReader = new BinaryReader(file.OpenReadStream()))
                        {
                            fileInBytes = theReader.ReadBytes(Convert.ToInt32(file.Length));
                        }

                        string fileAsString = Convert.ToBase64String(fileInBytes);

                        JObject annotation = new()
                        {
                            { "subject", file.FileName },
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName }
                        };

                        if (operacion_id != string.Empty)
                            annotation.Add("objectid_new_documentacionporoperacion@odata.bind", "/new_documentacionporoperacions(" + operacion_id + ")");

                        ResponseAPI respuesta = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!respuesta.ok)
                        {
                            throw new Exception(respuesta.descripcion);
                        }

                        nota_id = respuesta.descripcion;

                        JObject documentacionPorOP = new()
                        {
                            { "statuscode", 100000000}
                        };

                        ResponseAPI documentacion_response = await api.UpdateRecord("new_documentacionporoperacions", operacion_id, documentacionPorOP, credenciales);

                        if (!documentacion_response.ok)
                        {
                            throw new Exception(documentacion_response.descripcion);
                        }
                    }
                }

                return Ok(nota_id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public static async Task<string> ObtenerLibrador(Librador librador, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray libradores = await BuscarLibrador(librador.new_cuitlibrador, api, credenciales);

                string librador_id;
                if (libradores.Count == 0)
                {
                    JObject Librador = new()
                    {
                        { "new_name", librador.new_name },
                        { "new_cuitlibrador", librador.new_cuitlibrador }
                    };

                    ResponseAPI respuesta = await api.CreateRecord("new_libradors", Librador, credenciales);

                    if (!respuesta.ok)
                    {
                        throw new Exception(respuesta.descripcion);
                    }

                    librador_id = respuesta.descripcion;
                }
                else
                {
                    Librador librador_dynamics = JsonConvert.DeserializeObject<Librador>(libradores.First.ToString());
                    librador_id = librador_dynamics.new_libradorid;
                }

                return librador_id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task<JArray> BuscarLibrador(string cuitCuil, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_libradors";

                fetchXML = "<entity name='new_librador'>" +
                                        "<attribute name='new_libradorid'/> " +
                                        "<filter type='and'>" +
                                            $"<condition attribute='new_cuitlibrador' operator='eq' value='{cuitCuil}' />" +
                                        "</filter>" +
                            "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarGarantia(string garantia_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_garantias";

                fetchXML = "<entity name='new_garantia'>" +
                                            "<attribute name='new_monto'/> " +
                                            "<attribute name='new_fechadenegociacion'/> " +
                                            "<attribute name='new_name'/> " +
                                            "<attribute name='statuscode'/> " +
                                            "<attribute name='new_garantiaid'/> " +
                                            "<attribute name='createdon'/> " +
                                            "<attribute name='new_ndeordendelagarantiaotorgada'/> " +
                                            "<attribute name='new_fechadevencimiento'/> " +
                                            "<attribute name='new_tipodeoperacion'/> " +
                                            "<attribute name='new_acreedor'/> " +
                                            "<attribute name='new_tipodegarantias'/> " +
                                            "<attribute name='transactioncurrencyid'/> " +
                                            "<attribute name='new_referido'/> " +
                                            "<attribute name='new_sociosprotector'/> " +
                                            "<attribute name='new_fechadecancelada'/> " +
                                            "<attribute name='new_fechadeanulada'/> " +
                                            "<attribute name='new_operacion'/> " +
                                            "<filter type='and'>" +
                                                $"<condition attribute='new_garantiaid' operator='eq' value='{garantia_id}' />" +
                                            "</filter>" +
                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        #region Garantias
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/garantia")]
        public async Task<IActionResult> CrearGarantia([FromBody] PortalSocioParticipe.Garantia garantia)
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
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(garantia);
                ApiDynamicsV2 apiDynamics = new(errorLogService, urlCompleta, jsonBody);
                List<dynamic> listaEntidad = new();

                JObject Garantia = new()
                {
                    { "statuscode", 100000004 },
                    { "new_fechadeorigen", DateTime.Now.ToString("yyyy-MM-dd") } //En Cartera
                };

                if (garantia.new_socioparticipe != null && garantia.new_socioparticipe != string.Empty)
                    Garantia.Add("new_SocioParticipe@odata.bind", "/accounts(" + garantia.new_socioparticipe + ")");

                if (garantia.new_tipodeoperacion > 0)
                    Garantia.Add("new_tipodeoperacion", garantia.new_tipodeoperacion);

                if (garantia.new_formatodelcheque > 0)
                    Garantia.Add("new_formatodelcheque", garantia.new_formatodelcheque);

                if (garantia.new_tipochpd > 0)
                    Garantia.Add("new_tipochpd", garantia.new_tipochpd);

                if (garantia.new_acreedor != null && garantia.new_acreedor != string.Empty)
                    Garantia.Add("new_Acreedor@odata.bind", "/new_acreedors(" + garantia.new_acreedor + ")");

                if (garantia.new_monto >= 0)
                    Garantia.Add("new_monto", garantia.new_monto);

                if (garantia.new_operacion != null && garantia.new_operacion != string.Empty)
                    Garantia.Add("new_Operacion@odata.bind", "/new_operacions(" + garantia.new_operacion + ")");

                if (garantia.new_fechadevencimiento != null && garantia.new_fechadevencimiento != string.Empty)
                    Garantia.Add("new_fechadevencimiento", DateTime.Parse(garantia.new_fechadevencimiento).ToString("yyyy-MM-dd"));

                if (garantia.new_numerodecheque != null && garantia.new_numerodecheque != string.Empty)
                    Garantia.Add("new_numerodecheque", garantia.new_numerodecheque);

                if (garantia.new_fechadepago != null && garantia.new_fechadepago != string.Empty)
                    Garantia.Add("new_fechadepago", DateTime.Parse(garantia.new_fechadepago).ToString("yyyy-MM-dd"));

                if (garantia.librador != null)
                {
                    string librador_id = await ObtenerLibrador(garantia.librador, apiDynamics, credenciales);
                    if (librador_id != string.Empty)
                        Garantia.Add("new_LibradorCheque@odata.bind", "/new_libradors(" + librador_id + ")");
                }

                if (garantia.new_tasa > 0)
                    Garantia.Add("new_tasa", garantia.new_tasa);

                if (garantia.new_plazodias > 0)
                    Garantia.Add("new_plazodias", garantia.new_plazodias);

                if (garantia.new_periodogracia > 0)
                    Garantia.Add("new_periodogracia", garantia.new_periodogracia);

                if (garantia.new_sistemadeamortizacion > 0)
                    Garantia.Add("new_sistemadeamortizacion", garantia.new_sistemadeamortizacion);

                if (garantia.new_periodicidadpagos > 0)
                    Garantia.Add("new_periodicidadpagos", garantia.new_periodicidadpagos);

                if (garantia.new_observaciones != null && garantia.new_observaciones != string.Empty)
                    Garantia.Add("new_observaciones", garantia.new_observaciones);

                ResponseAPI respuesta = await apiDynamics.CreateRecord("new_garantias", Garantia, credenciales);

                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }

                JArray GarantiaCreada = await BuscarGarantia(respuesta.descripcion, apiDynamics, credenciales);
                if (GarantiaCreada.Count > 0)
                {
                    foreach (var item in GarantiaCreada.Children())
                    {
                        dynamic entidad = JsonConvert.DeserializeObject<dynamic>(item.ToString());
                        listaEntidad.Add(entidad);
                    }
                }

                if (listaEntidad.Count > 0)
                {
                    return Ok(JsonConvert.SerializeObject(listaEntidad));
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/cargargarantia")]
        public async Task<IActionResult> CargarGarantia([FromBody] PortalSocioParticipe.Garantia garantia)
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
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(garantia);
                ApiDynamicsV2 apiDynamics = new(errorLogService, urlCompleta, jsonBody);
                List<dynamic> listaEntidad = new();

                JObject Garantia = new()
                {
                    { "statuscode", 100000004 },
                    //{ "new_creadaporportal", true },
                    { "new_fechadeorigen", DateTime.Now.ToString("yyyy-MM-dd") } //En Cartera
                };

                if (garantia.new_socioparticipe != null && garantia.new_socioparticipe != string.Empty)
                    Garantia.Add("new_SocioParticipe@odata.bind", "/accounts(" + garantia.new_socioparticipe + ")");

                if (garantia.new_tipodeoperacion > 0)
                    Garantia.Add("new_tipodeoperacion", garantia.new_tipodeoperacion);

                if (garantia.new_formatodelcheque > 0)
                    Garantia.Add("new_formatodelcheque", garantia.new_formatodelcheque);

                if (garantia.new_tipochpd > 0)
                    Garantia.Add("new_tipochpd", garantia.new_tipochpd);

                if (garantia.new_acreedor != null && garantia.new_acreedor != string.Empty)
                    Garantia.Add("new_Acreedor@odata.bind", "/new_acreedors(" + garantia.new_acreedor + ")");

                if (garantia.new_monto >= 0)
                    Garantia.Add("new_monto", garantia.new_monto);

                if (garantia.new_operacion != null && garantia.new_operacion != string.Empty)
                    Garantia.Add("new_Operacion@odata.bind", "/new_operacions(" + garantia.new_operacion + ")");

                if (garantia.new_fechadevencimiento != null && garantia.new_fechadevencimiento != string.Empty)
                    Garantia.Add("new_fechadevencimiento", DateTime.Parse(garantia.new_fechadevencimiento).ToString("yyyy-MM-dd"));

                if (garantia.new_numerodecheque != null && garantia.new_numerodecheque != string.Empty)
                    Garantia.Add("new_numerodecheque", garantia.new_numerodecheque);

                if (garantia.new_fechadepago != null && garantia.new_fechadepago != string.Empty)
                    Garantia.Add("new_fechadepago", DateTime.Parse(garantia.new_fechadepago).ToString("yyyy-MM-dd"));

                if (garantia.librador != null)
                {
                    string librador_id = await ObtenerLibrador(garantia.librador, apiDynamics, credenciales);
                    if (librador_id != string.Empty)
                        Garantia.Add("new_LibradorCheque@odata.bind", "/new_libradors(" + librador_id + ")");
                }

                if (garantia.new_tasa > 0)
                    Garantia.Add("new_tasa", garantia.new_tasa);

                if (garantia.new_plazodias > 0)
                    Garantia.Add("new_plazodias", garantia.new_plazodias);

                if (garantia.new_periodogracia > 0)
                    Garantia.Add("new_periodogracia", garantia.new_periodogracia);

                if (garantia.new_sistemadeamortizacion > 0)
                    Garantia.Add("new_sistemadeamortizacion", garantia.new_sistemadeamortizacion);

                if (garantia.new_periodicidadpagos > 0)
                    Garantia.Add("new_periodicidadpagos", garantia.new_periodicidadpagos);

                if (garantia.new_observaciones != null && garantia.new_observaciones != string.Empty)
                    Garantia.Add("new_observaciones", garantia.new_observaciones);

                if (garantia.new_puntosporcentuales > 0)
                    Garantia.Add("new_puntosporcentuales", garantia.new_puntosporcentuales);

                if (!string.IsNullOrWhiteSpace(garantia.transactioncurrencyid))
                    Garantia.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + garantia.transactioncurrencyid + ")");
                
                ResponseAPI resultadoCreacion = await apiDynamics.CreateRecord("new_garantias", Garantia, credenciales);

                if (!resultadoCreacion.ok)
                {
                    return BadRequest(resultadoCreacion.descripcion);
                }

                return Ok(resultadoCreacion.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/adjuntosgarantia")]
        public async Task<IActionResult> AdjutnosGarantia(string garantia_id)
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
                if (garantia_id == null || garantia_id == string.Empty)
                    return BadRequest("El id de la operacion esta vacio");
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(garantia_id);
                ApiDynamicsV2 api = new(errorLogService, urlCompleta, jsonBody);
                var archivos = HttpContext.Request.Form.Files;
                string nota_id = string.Empty;

                if (archivos.Count > 0)
                {
                    JObject adjunto = new()
                    {
                        { "new_Garantia@odata.bind", "/new_garantias(" + garantia_id + ")"  },
                        { "new_tipo",  100000002},
                    };

                    ResponseAPI respuesta = await api.CreateRecord("new_adjuntoses", adjunto, credenciales);

                    if (!respuesta.ok)
                    {
                        throw new Exception(respuesta.descripcion);
                    }

                    foreach (var file in archivos)
                    {
                        byte[] fileInBytes = new byte[file.Length];
                        using (BinaryReader theReader = new BinaryReader(file.OpenReadStream()))
                        {
                            fileInBytes = theReader.ReadBytes(Convert.ToInt32(file.Length));
                        }

                        string fileAsString = Convert.ToBase64String(fileInBytes);

                        JObject annotation = new();
                        annotation.Add("subject", file.FileName);
                        annotation.Add("isdocument", true);
                        annotation.Add("mimetype", file.ContentType);
                        annotation.Add("documentbody", fileAsString);
                        annotation.Add("filename", file.FileName);

                        if (garantia_id != string.Empty)
                            annotation.Add("objectid_new_adjuntos@odata.bind", "/new_adjuntoses(" + respuesta.descripcion + ")");

                        ResponseAPI respuestaNota = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!respuestaNota.ok)
                        {
                            return BadRequest(respuestaNota.descripcion);
                        }

                        nota_id = respuestaNota.descripcion;
                    }
                }

                return Ok(nota_id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/importadorcheques")]
        public async Task<IActionResult> ImportadorDeCheques(string account_id)
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
                if (account_id == null || account_id == string.Empty)
                    return BadRequest("El id de la cuenta esta vacia");
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(account_id);
                ApiDynamicsV2 api = new(errorLogService, urlCompleta, jsonBody);
                var archivos = HttpContext.Request.Form.Files;
                string nota_id = string.Empty;

                if (archivos.Count > 0)
                {
                    JObject importador = new()
                    {
                        { "new_name", "E-cheq carga masiva portal"  },
                        { "new_tipo",  100000006},
                        { "new_Cuenta@odata.bind", "/accounts(" + account_id + ")" }
                    };

                    ResponseAPI respuesta = await api.CreateRecord("new_importacioneses", importador, credenciales);

                    if (!respuesta.ok)
                    {
                        throw new Exception(respuesta.descripcion);
                    }

                    foreach (var file in archivos)
                    {
                        byte[] fileInBytes = new byte[file.Length];
                        using (BinaryReader theReader = new BinaryReader(file.OpenReadStream()))
                        {
                            fileInBytes = theReader.ReadBytes(Convert.ToInt32(file.Length));
                        }

                        string fileAsString = Convert.ToBase64String(fileInBytes);

                        JObject annotation = new()
                        {
                            { "subject", file.FileName },
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName }
                        };

                        if (respuesta.descripcion != string.Empty)
                            annotation.Add("objectid_new_importaciones@odata.bind", "/new_importacioneses(" + respuesta.descripcion + ")");

                        ResponseAPI respuestaNota = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!respuestaNota.ok)
                        {
                            return BadRequest(respuestaNota.descripcion);
                        }

                        nota_id = respuestaNota.descripcion;
                    }
                }

                return Ok(nota_id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Tareas
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/socioparticipe/inactivartarea")]
        public async Task<IActionResult> InactivarUniversidadPorEmpleado([FromBody] ActividadTarea tarea)
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
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(tarea);
                ApiDynamicsV2 api = new(errorLogService, urlCompleta, jsonBody);
                if (tarea.activityid == null || tarea.activityid == string.Empty)
                    return BadRequest("El id de la tarea esta vacio");

                JObject tarea_soc = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("tasks", tarea.activityid, tarea_soc, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
