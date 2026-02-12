using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Api.Web.Dynamics365.Servicios;
using DocumentFormat.OpenXml.Office2010.Word;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;
using static Api.Web.Dynamics365.Models.SgrOneClick;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class OnboardingController : ControllerBase
    {
        private IConfiguration Configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly IErrorLogService errorLogService;

        public OnboardingController(IConfiguration _configuration, 
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IErrorLogService errorLogService)
        {
            Configuration = _configuration;
            this.userManager = userManager;
            this.context = context;
            this.errorLogService = errorLogService;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboarding")]
        public async Task<IActionResult> Onboarding([FromBody] Onboarding onboarding)
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
                string response = string.Empty;
                string solicitudId = string.Empty;
                string monto = string.Empty;
                string montoG = string.Empty;

                if (onboarding.facturacion != null)
                {
                    monto = onboarding.facturacion.Replace("$", string.Empty);
                    monto = monto.Replace(",", string.Empty).Trim();
                }

                if (onboarding.montoGarantia != null && onboarding.montoGarantia != "undefinded" && onboarding.montoGarantia != string.Empty)
                {
                    montoG = onboarding.montoGarantia.Replace("$", string.Empty);
                    montoG = montoG.Replace(",", string.Empty).Trim();
                }

                JObject precalificacion = new JObject();
                if (onboarding.personeria != null && onboarding.personeria != string.Empty)
                    precalificacion.Add("new_personeria", Convert.ToInt32(onboarding.personeria));
                if (onboarding.razonSocial != null && onboarding.razonSocial != string.Empty)
                    precalificacion.Add("new_nombrerazonsocial", onboarding.razonSocial.Trim());
                if (onboarding.cuit != null && onboarding.cuit != string.Empty)
                    precalificacion.Add("new_cuit", onboarding.cuit);
                if (onboarding.cuitcuil != null && onboarding.cuitcuil != string.Empty)
                    precalificacion.Add("new_cuitcuil", onboarding.cuitcuil);
                if (onboarding.email != null && onboarding.email != string.Empty) 
                    precalificacion.Add("emailaddress", onboarding.email);
                if (onboarding.telefono != null && onboarding.telefono != string.Empty)
                    precalificacion.Add("new_telefonodecontacto", onboarding.telefono);
                if (onboarding.nombreContacto != null && onboarding.nombreContacto != string.Empty)
                    precalificacion.Add("new_nombredecontacto", onboarding.nombreContacto.Trim());
                if (onboarding.apellido != null && onboarding.apellido != string.Empty)
                    precalificacion.Add("new_apellido", onboarding.apellido.Trim());
                if (onboarding.tipoDocumento != null && onboarding.tipoDocumento != string.Empty)
                    precalificacion.Add("new_TipodeDocumento@odata.bind", "/new_tipodedocumentos(" + onboarding.tipoDocumento + ")");
                if (onboarding.productoServicio != null && onboarding.productoServicio != string.Empty)
                    precalificacion.Add("new_productoservicio", onboarding.productoServicio);
                if (onboarding.actividadAFIP != null && onboarding.actividadAFIP != string.Empty)
                    precalificacion.Add("new_ActividadAFIP@odata.bind", "/new_actividadafips(" + onboarding.actividadAFIP + ")");
                if (monto != null && monto != string.Empty)
                    precalificacion.Add("new_facturacinpromedio", Convert.ToDecimal(monto));
                if (onboarding.tipoRelacion != null && onboarding.tipoRelacion != string.Empty)
                    precalificacion.Add("new_relacinconlacuenta", Convert.ToInt32(onboarding.tipoRelacion));
                if (onboarding.tipoSocietario != null && onboarding.tipoSocietario != string.Empty)
                    precalificacion.Add("new_tiposocietario", Convert.ToInt32(onboarding.tipoSocietario));
                if (onboarding.condicionImpositiva != null && onboarding.condicionImpositiva != string.Empty)
                    precalificacion.Add("new_condicionimpositiva", Convert.ToInt32(onboarding.condicionImpositiva));
                if (onboarding.cantidadMujeres != null && onboarding.cantidadMujeres != string.Empty) 
                    precalificacion.Add("new_cantidaddemujeresenpuestosdetomadedecisio", Convert.ToInt32(onboarding.cantidadMujeres));
                if (onboarding.empleadas != null && onboarding.empleadas != string.Empty)
                    precalificacion.Add("new_cantidaddeempleadosmujeres", Convert.ToInt32(onboarding.empleadas));
                if (onboarding.discapacitados != null && onboarding.discapacitados != string.Empty)
                    precalificacion.Add("new_cantidaddepersonascondiscapacidad", Convert.ToInt32(onboarding.discapacitados));
                if (onboarding.otro != null && onboarding.otro != string.Empty)
                    precalificacion.Add("new_otro", onboarding.otro);
                if (onboarding.sectorEconomico != null && onboarding.sectorEconomico != string.Empty)
                    precalificacion.Add("new_SectorEconmico@odata.bind", "/new_condicionpymes(" + onboarding.sectorEconomico.Trim() + ")");
                if (onboarding.inicioActividad != null && onboarding.inicioActividad != string.Empty)
                    precalificacion.Add("new_inicioactividad", onboarding.inicioActividad.Trim());
                if (onboarding.resena != null && onboarding.resena != string.Empty)
                    precalificacion.Add("new_breveresena", onboarding.resena);
                if (onboarding.emailNotificaciones != null && onboarding.emailNotificaciones.Trim() != "null" && onboarding.emailNotificaciones.Trim() != "undefined") precalificacion.Add("new_emailnotificaciones", onboarding.emailNotificaciones.Trim());
                if (onboarding.invitacion != null && onboarding.invitacion.Trim() != "null" && onboarding.invitacion.Trim() != "undefined") precalificacion.Add("new_invitacion", onboarding.invitacion.Trim());
                if (onboarding.cuitReferidor != null && onboarding.cuitReferidor.Trim() != "null" && onboarding.cuitReferidor.Trim() != "undefined") precalificacion.Add("new_cuitdelreferidor", onboarding.cuitReferidor.Trim());
                //Garantia 
                if (montoG != string.Empty) precalificacion.Add("new_monto", Convert.ToDecimal(montoG.Trim()));
                if (onboarding.nroExpediente != null) precalificacion.Add("new_nroexpedientetda", onboarding.nroExpediente.Trim());
                if (onboarding.creditoAprobado != null) precalificacion.Add("new_creditoaprobado", onboarding.creditoAprobado.Trim());
                if (onboarding.serie != null) precalificacion.Add("new_Serie@odata.bind", "/new_seriedeoperacinsindicadas(" + onboarding.serie + ")");

                response = apiDynamics.CreateRecord("new_precalificacincrediticias", precalificacion, credenciales);

                if (response == "ERROR" || response == "")
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboardingdoscero")]
        public async Task<IActionResult> Onboarding20([FromBody] Onboarding onboarding)
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
                ApiDynamicsV2 apiDynamics = new();
                string solicitudId = string.Empty;
                string monto = string.Empty;
                string montoG = string.Empty;

                if (onboarding.facturacion != null)
                {
                    monto = onboarding.facturacion.Replace("$", string.Empty);
                    monto = monto.Replace(",", string.Empty).Trim();
                }

                if (onboarding.montoGarantia != null && onboarding.montoGarantia != "undefinded" && onboarding.montoGarantia != string.Empty)
                {
                    montoG = onboarding.montoGarantia.Replace("$", string.Empty);
                    montoG = montoG.Replace(",", string.Empty).Trim();
                }

                JObject precalificacion = new();
                if (!string.IsNullOrEmpty(onboarding.personeria))
                    precalificacion.Add("new_personeria", Convert.ToInt32(onboarding.personeria));

                if (!string.IsNullOrEmpty(onboarding.razonSocial))
                    precalificacion.Add("new_nombrerazonsocial", onboarding.razonSocial.Trim());

                if (!string.IsNullOrEmpty(onboarding.cuit))
                    precalificacion.Add("new_cuit", onboarding.cuit);

                if (!string.IsNullOrEmpty(onboarding.cuitcuil))
                    precalificacion.Add("new_cuitcuil", onboarding.cuitcuil);

                if (!string.IsNullOrEmpty(onboarding.email))
                    precalificacion.Add("emailaddress", onboarding.email);

                if (!string.IsNullOrEmpty(onboarding.telefono))
                    precalificacion.Add("new_telefonodecontacto", onboarding.telefono);

                if (!string.IsNullOrEmpty(onboarding.nombreContacto))
                    precalificacion.Add("new_nombredecontacto", onboarding.nombreContacto.Trim());

                if (!string.IsNullOrEmpty(onboarding.apellido))
                    precalificacion.Add("new_apellido", onboarding.apellido.Trim());

                if (!string.IsNullOrEmpty(onboarding.tipoDocumento))
                    precalificacion.Add("new_TipodeDocumento@odata.bind", "/new_tipodedocumentos(" + onboarding.tipoDocumento + ")");

                if (!string.IsNullOrEmpty(onboarding.productoServicio))
                    precalificacion.Add("new_productoservicio", onboarding.productoServicio);

                if (!string.IsNullOrEmpty(onboarding.actividadAFIP))
                    precalificacion.Add("new_ActividadAFIP@odata.bind", "/new_actividadafips(" + onboarding.actividadAFIP + ")");

                if (!string.IsNullOrEmpty(monto))
                    precalificacion.Add("new_facturacinpromedio", Convert.ToDecimal(monto));

                if (!string.IsNullOrEmpty(onboarding.tipoRelacion))
                    precalificacion.Add("new_relacinconlacuenta", Convert.ToInt32(onboarding.tipoRelacion));

                if (!string.IsNullOrEmpty(onboarding.tipoSocietario))
                    precalificacion.Add("new_tiposocietario", Convert.ToInt32(onboarding.tipoSocietario));

                if (!string.IsNullOrEmpty(onboarding.condicionImpositiva))
                    precalificacion.Add("new_CondiciondeInscripcionanteAFIP@odata.bind", "/new_condiciondeinscipcionanteafips(" + onboarding.condicionImpositiva + ")");

                if (!string.IsNullOrEmpty(onboarding.cantidadMujeres))
                    precalificacion.Add("new_cantidaddemujeresenpuestosdetomadedecisio", Convert.ToInt32(onboarding.cantidadMujeres));

                if (!string.IsNullOrEmpty(onboarding.empleadas))
                    precalificacion.Add("new_cantidaddeempleadosmujeres", Convert.ToInt32(onboarding.empleadas));

                if (!string.IsNullOrEmpty(onboarding.discapacitados))
                    precalificacion.Add("new_cantidaddepersonascondiscapacidad", Convert.ToInt32(onboarding.discapacitados));

                if (!string.IsNullOrEmpty(onboarding.otro))
                    precalificacion.Add("new_otro", onboarding.otro);

                if (!string.IsNullOrEmpty(onboarding.sectorEconomico))
                    precalificacion.Add("new_SectorEconmico@odata.bind", "/new_condicionpymes(" + onboarding.sectorEconomico.Trim() + ")");

                if (!string.IsNullOrEmpty(onboarding.inicioActividad))
                    precalificacion.Add("new_inicioactividad", onboarding.inicioActividad.Trim());

                if (!string.IsNullOrEmpty(onboarding.resena))
                    precalificacion.Add("new_breveresena", onboarding.resena);

                if (onboarding.emailNotificaciones != null && onboarding.emailNotificaciones.Trim() != "null" && onboarding.emailNotificaciones.Trim() != "undefined")
                    precalificacion.Add("new_emailnotificaciones", onboarding.emailNotificaciones.Trim());

                if (onboarding.invitacion != null && onboarding.invitacion.Trim() != "null" && onboarding.invitacion.Trim() != "undefined")
                    precalificacion.Add("new_invitacion", onboarding.invitacion.Trim());

                if (onboarding.cuitReferidor != null && onboarding.cuitReferidor.Trim() != "null" && onboarding.cuitReferidor.Trim() != "undefined")
                    precalificacion.Add("new_Referido@odata.bind", "/accounts(" + onboarding.cuitReferidor.Trim() + ")");

                if (onboarding.calle != null && onboarding.calle.Trim() != "null" && onboarding.calle.Trim() != "undefined")
                    precalificacion.Add("new_direccion", onboarding.calle);

                if (onboarding.destinoLineaDeCredito != null && onboarding.destinoLineaDeCredito.Trim() != "null" && onboarding.destinoLineaDeCredito.Trim() != "undefined")
                    precalificacion.Add("new_DestinodeFondos@odata.bind", "/new_destinodefondoses(" + onboarding.destinoLineaDeCredito.Trim() + ")");

                //Garantia 
                if (montoG != string.Empty) precalificacion.Add("new_monto", Convert.ToDecimal(montoG.Trim()));
                if (onboarding.nroExpediente != null) precalificacion.Add("new_nroexpedientetda", onboarding.nroExpediente.Trim());
                if (onboarding.creditoAprobado != null) precalificacion.Add("new_creditoaprobado", onboarding.creditoAprobado.Trim());
                if (onboarding.serie != null) precalificacion.Add("new_Serie@odata.bind", "/new_seriedeoperacinsindicadas(" + onboarding.serie + ")");

                ResponseAPI response = await apiDynamics.CreateRecord("new_precalificacincrediticias", precalificacion, credenciales);

                if (!response.ok)
                {
                    return BadRequest(response.descripcion);
                }

                return Ok(response.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboardingprueba")]
        public async Task<IActionResult> OnboardingPrueba([FromBody] Onboarding onboarding)
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
                ApiDynamicsV2 apiDynamics = new();
                string solicitudId = string.Empty;
                string monto = string.Empty;
                string montoG = string.Empty;

                if (onboarding.facturacion != null)
                {
                    monto = onboarding.facturacion.Replace("$", string.Empty);
                    monto = monto.Replace(",", string.Empty).Trim();
                }

                if (onboarding.montoGarantia != null && onboarding.montoGarantia != "undefinded" && onboarding.montoGarantia != string.Empty)
                {
                    montoG = onboarding.montoGarantia.Replace("$", string.Empty);
                    montoG = montoG.Replace(",", string.Empty).Trim();
                }

                JObject precalificacion = new();
                if (!string.IsNullOrEmpty(onboarding.personeria))
                    precalificacion.Add("new_personeria", Convert.ToInt32(onboarding.personeria));

                if (!string.IsNullOrEmpty(onboarding.razonSocial))
                    precalificacion.Add("new_nombrerazonsocial", onboarding.razonSocial.Trim());

                if (!string.IsNullOrEmpty(onboarding.cuit))
                    precalificacion.Add("new_cuit", onboarding.cuit);

                if (!string.IsNullOrEmpty(onboarding.cuitcuil))
                    precalificacion.Add("new_cuitcuil", onboarding.cuitcuil);

                if (!string.IsNullOrEmpty(onboarding.email))
                    precalificacion.Add("emailaddress", onboarding.email);

                if (!string.IsNullOrEmpty(onboarding.telefono))
                    precalificacion.Add("new_telefonodecontacto", onboarding.telefono);

                if (!string.IsNullOrEmpty(onboarding.nombreContacto))
                    precalificacion.Add("new_nombredecontacto", onboarding.nombreContacto.Trim());

                if (!string.IsNullOrEmpty(onboarding.apellido))
                    precalificacion.Add("new_apellido", onboarding.apellido.Trim());

                if (!string.IsNullOrEmpty(onboarding.tipoDocumento))
                    precalificacion.Add("new_TipodeDocumento@odata.bind", "/new_tipodedocumentos(" + onboarding.tipoDocumento + ")");

                if (!string.IsNullOrEmpty(onboarding.productoServicio))
                    precalificacion.Add("new_productoservicio", onboarding.productoServicio);

                if (!string.IsNullOrEmpty(onboarding.actividadAFIP))
                    precalificacion.Add("new_ActividadAFIP@odata.bind", "/new_actividadafips(" + onboarding.actividadAFIP + ")");

                if (!string.IsNullOrEmpty(monto))
                    precalificacion.Add("new_facturacinpromedio", Convert.ToDecimal(monto));

                if (!string.IsNullOrEmpty(onboarding.tipoRelacion))
                    precalificacion.Add("new_relacinconlacuenta", Convert.ToInt32(onboarding.tipoRelacion));

                if (!string.IsNullOrEmpty(onboarding.tipoSocietario))
                    precalificacion.Add("new_tiposocietario", Convert.ToInt32(onboarding.tipoSocietario));

                if (!string.IsNullOrEmpty(onboarding.condicionImpositiva))
                    precalificacion.Add("new_CondiciondeInscripcionanteAFIP@odata.bind", "/new_condiciondeinscipcionanteafips(" + onboarding.condicionImpositiva + ")");

                if (!string.IsNullOrEmpty(onboarding.cantidadMujeres))
                    precalificacion.Add("new_cantidaddemujeresenpuestosdetomadedecisio", Convert.ToInt32(onboarding.cantidadMujeres));

                if (!string.IsNullOrEmpty(onboarding.empleadas))
                    precalificacion.Add("new_cantidaddeempleadosmujeres", Convert.ToInt32(onboarding.empleadas));

                if (!string.IsNullOrEmpty(onboarding.discapacitados))
                    precalificacion.Add("new_cantidaddepersonascondiscapacidad", Convert.ToInt32(onboarding.discapacitados));

                if (!string.IsNullOrEmpty(onboarding.otro))
                    precalificacion.Add("new_otro", onboarding.otro);

                if (!string.IsNullOrEmpty(onboarding.sectorEconomico))
                    precalificacion.Add("new_SectorEconomico@odata.bind", "/new_condicionpymes(" + onboarding.sectorEconomico.Trim() + ")");

                if (!string.IsNullOrEmpty(onboarding.inicioActividad))
                    precalificacion.Add("new_inicioactividad", onboarding.inicioActividad.Trim());

                if (!string.IsNullOrEmpty(onboarding.resena))
                    precalificacion.Add("new_breveresena", onboarding.resena);

                if (onboarding.emailNotificaciones != null && onboarding.emailNotificaciones.Trim() != "null" && onboarding.emailNotificaciones.Trim() != "undefined")
                    precalificacion.Add("new_emailnotificaciones", onboarding.emailNotificaciones.Trim());

                if (onboarding.invitacion != null && onboarding.invitacion.Trim() != "null" && onboarding.invitacion.Trim() != "undefined")
                    precalificacion.Add("new_invitacion", onboarding.invitacion.Trim());

                if (onboarding.cuitReferidor != null && onboarding.cuitReferidor.Trim() != "null" && onboarding.cuitReferidor.Trim() != "undefined")
                    precalificacion.Add("new_Referido@odata.bind", "/accounts(" + onboarding.cuitReferidor.Trim() + ")");

                if (onboarding.creadaPorApiLufe != null && onboarding.creadaPorApiLufe.Trim() != "" && onboarding.creadaPorApiLufe.Trim() != "undefined")
                    precalificacion.Add("new_creadaporapilufe", onboarding.creadaPorApiLufe.Trim());

                if (!string.IsNullOrWhiteSpace(onboarding.calle))
                    precalificacion.Add("new_calle", onboarding.calle);

                if (!string.IsNullOrWhiteSpace(onboarding.numero))
                    precalificacion.Add("new_numero", onboarding.numero);

                if (!string.IsNullOrWhiteSpace(onboarding.piso))
                    precalificacion.Add("new_piso", onboarding.piso);

                if (!string.IsNullOrWhiteSpace(onboarding.departamento))
                    precalificacion.Add("new_departamento", onboarding.departamento);

                if (!string.IsNullOrWhiteSpace(onboarding.codigoPostal))
                    precalificacion.Add("new_codigopostal", onboarding.codigoPostal);

                if (!string.IsNullOrWhiteSpace(onboarding.municipio))
                    precalificacion.Add("new_municipio", onboarding.municipio);

                if (!string.IsNullOrWhiteSpace(onboarding.localidad))
                    precalificacion.Add("new_localidad", onboarding.localidad);

                if (!string.IsNullOrWhiteSpace(onboarding.provincia))
                    precalificacion.Add("new_Provincia@odata.bind", "/new_provincias(" + onboarding.provincia + ")");

                if (!string.IsNullOrWhiteSpace(onboarding.pais))
                    precalificacion.Add("new_Pais@odata.bind", "/new_paises(" + onboarding.pais + ")");

                if (!string.IsNullOrWhiteSpace(onboarding.lineaDeCredito))
                    precalificacion.Add("new_lineadecredito", Convert.ToDecimal(onboarding.lineaDeCredito));

                if (!string.IsNullOrWhiteSpace(onboarding.destinoLineaDeCredito))
                    precalificacion.Add("new_DestinoLineadeCredito@odata.bind", "/new_destinodefondoses(" + onboarding.destinoLineaDeCredito + ")");

                if (!string.IsNullOrEmpty(onboarding.observaciones))
                    precalificacion.Add("new_observaciones", onboarding.observaciones);

                //Garantia 
                if (montoG != string.Empty) precalificacion.Add("new_monto", Convert.ToDecimal(montoG.Trim()));
                if (onboarding.nroExpediente != null) precalificacion.Add("new_nroexpedientetda", onboarding.nroExpediente.Trim());
                if (onboarding.creditoAprobado != null) precalificacion.Add("new_creditoaprobado", onboarding.creditoAprobado.Trim());
                if (onboarding.serie != null) precalificacion.Add("new_Serie@odata.bind", "/new_seriedeoperacinsindicadas(" + onboarding.serie + ")");

                ResponseAPI response = await apiDynamics.CreateRecord("new_precalificacincrediticias", precalificacion, credenciales);

                if (!response.ok)
                {
                    return BadRequest(response.descripcion);
                }

                return Ok(response.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboardingnext")]
        public async Task<IActionResult> OnboardingNext([FromBody] Onboarding onboarding)
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
                string jsonBody = JsonConvert.SerializeObject(onboarding);
                ApiDynamicsV2 apiDynamics = new(errorLogService, urlCompleta, jsonBody);
                string solicitudId = string.Empty;
                string monto = string.Empty;
                string montoG = string.Empty;

                if (onboarding.facturacion != null)
                {
                    monto = onboarding.facturacion.Replace("$", string.Empty);
                    monto = monto.Replace(",", string.Empty).Trim();
                }

                if (onboarding.montoGarantia != null && onboarding.montoGarantia != "undefinded" && onboarding.montoGarantia != string.Empty)
                {
                    montoG = onboarding.montoGarantia.Replace("$", string.Empty);
                    montoG = montoG.Replace(",", string.Empty).Trim();
                }

                JObject precalificacion = new();
                if (!string.IsNullOrEmpty(onboarding.personeria))
                    precalificacion.Add("new_personeria", Convert.ToInt32(onboarding.personeria));

                if (!string.IsNullOrEmpty(onboarding.razonSocial))
                    precalificacion.Add("new_nombrerazonsocial", onboarding.razonSocial.Trim());

                if (!string.IsNullOrEmpty(onboarding.cuit))
                    precalificacion.Add("new_cuit", onboarding.cuit);

                if (!string.IsNullOrEmpty(onboarding.cuitcuil))
                    precalificacion.Add("new_cuitcuil", onboarding.cuitcuil);

                if (!string.IsNullOrEmpty(onboarding.email))
                    precalificacion.Add("emailaddress", onboarding.email);

                if (!string.IsNullOrEmpty(onboarding.telefono))
                    precalificacion.Add("new_telefonodecontacto", onboarding.telefono);

                if (!string.IsNullOrEmpty(onboarding.nombreContacto))
                    precalificacion.Add("new_nombredecontacto", onboarding.nombreContacto.Trim());

                if (!string.IsNullOrEmpty(onboarding.apellido))
                    precalificacion.Add("new_apellido", onboarding.apellido.Trim());

                if (!string.IsNullOrEmpty(onboarding.tipoDocumento))
                    precalificacion.Add("new_TipodeDocumento@odata.bind", "/new_tipodedocumentos(" + onboarding.tipoDocumento + ")");

                if (!string.IsNullOrEmpty(onboarding.productoServicio))
                    precalificacion.Add("new_productoservicio", onboarding.productoServicio);

                if (!string.IsNullOrEmpty(onboarding.actividadAFIP))
                    precalificacion.Add("new_ActividadAFIP@odata.bind", "/new_actividadafips(" + onboarding.actividadAFIP + ")");

                if (!string.IsNullOrEmpty(monto))
                    precalificacion.Add("new_facturacinpromedio", Convert.ToDecimal(monto));

                if (!string.IsNullOrEmpty(onboarding.tipoRelacion))
                    precalificacion.Add("new_relacinconlacuenta", Convert.ToInt32(onboarding.tipoRelacion));

                if (!string.IsNullOrEmpty(onboarding.tipoSocietario))
                    precalificacion.Add("new_tiposocietario", Convert.ToInt32(onboarding.tipoSocietario));

                if (!string.IsNullOrEmpty(onboarding.condicionImpositiva))
                    precalificacion.Add("new_CondiciondeInscripcionanteAFIP@odata.bind", "/new_condiciondeinscipcionanteafips(" + onboarding.condicionImpositiva + ")");

                if (!string.IsNullOrEmpty(onboarding.cantidadMujeres))
                    precalificacion.Add("new_cantidaddemujeresenpuestosdetomadedecisio", Convert.ToInt32(onboarding.cantidadMujeres));

                if (!string.IsNullOrEmpty(onboarding.empleadas))
                    precalificacion.Add("new_cantidaddeempleadosmujeres", Convert.ToInt32(onboarding.empleadas));

                if (!string.IsNullOrEmpty(onboarding.discapacitados))
                    precalificacion.Add("new_cantidaddepersonascondiscapacidad", Convert.ToInt32(onboarding.discapacitados));

                if (!string.IsNullOrEmpty(onboarding.otro))
                    precalificacion.Add("new_otro", onboarding.otro);

                if (!string.IsNullOrEmpty(onboarding.sectorEconomico))
                    precalificacion.Add("new_SectorEconomico@odata.bind", "/new_condicionpymes(" + onboarding.sectorEconomico.Trim() + ")");

                if (!string.IsNullOrEmpty(onboarding.inicioActividad))
                    precalificacion.Add("new_inicioactividad", onboarding.inicioActividad.Trim());

                if (!string.IsNullOrEmpty(onboarding.resena))
                    precalificacion.Add("new_breveresena", onboarding.resena);

                if (onboarding.emailNotificaciones != null && onboarding.emailNotificaciones.Trim() != "null" && onboarding.emailNotificaciones.Trim() != "undefined")
                    precalificacion.Add("new_emailnotificaciones", onboarding.emailNotificaciones.Trim());

                if (onboarding.invitacion != null && onboarding.invitacion.Trim() != "null" && onboarding.invitacion.Trim() != "undefined")
                    precalificacion.Add("new_invitacion", onboarding.invitacion.Trim());

                if (onboarding.cuitReferidor != null && onboarding.cuitReferidor.Trim() != "null" && onboarding.cuitReferidor.Trim() != "undefined")
                    precalificacion.Add("new_Referido@odata.bind", "/accounts(" + onboarding.cuitReferidor.Trim() + ")");

                if (onboarding.creadaPorApiLufe != null && onboarding.creadaPorApiLufe.Trim() != "" && onboarding.creadaPorApiLufe.Trim() != "undefined")
                    precalificacion.Add("new_creadaporapilufe", onboarding.creadaPorApiLufe.Trim());

                if (!string.IsNullOrWhiteSpace(onboarding.calle))
                    precalificacion.Add("new_calle", onboarding.calle);

                if (!string.IsNullOrWhiteSpace(onboarding.numero))
                    precalificacion.Add("new_numero", onboarding.numero);

                if (!string.IsNullOrWhiteSpace(onboarding.piso))
                    precalificacion.Add("new_piso", onboarding.piso);

                if (!string.IsNullOrWhiteSpace(onboarding.departamento))
                    precalificacion.Add("new_departamento", onboarding.departamento);

                if (!string.IsNullOrWhiteSpace(onboarding.codigoPostal))
                    precalificacion.Add("new_codigopostal", onboarding.codigoPostal);

                if (!string.IsNullOrWhiteSpace(onboarding.municipio))
                    precalificacion.Add("new_municipio", onboarding.municipio);

                if (!string.IsNullOrWhiteSpace(onboarding.localidad))
                    precalificacion.Add("new_localidad", onboarding.localidad);

                if (!string.IsNullOrWhiteSpace(onboarding.provincia))
                    precalificacion.Add("new_Provincia@odata.bind", "/new_provincias(" + onboarding.provincia + ")");

                if (!string.IsNullOrWhiteSpace(onboarding.pais))
                    precalificacion.Add("new_Pais@odata.bind", "/new_paises(" + onboarding.pais + ")");

                if (!string.IsNullOrWhiteSpace(onboarding.lineaDeCredito))
                    precalificacion.Add("new_lineadecredito", Convert.ToDecimal(onboarding.lineaDeCredito));

                if (!string.IsNullOrWhiteSpace(onboarding.destinoLineaDeCredito))
                    precalificacion.Add("new_DestinoLineadeCredito@odata.bind", "/new_destinodefondoses(" + onboarding.destinoLineaDeCredito + ")");

                //Garantia 
                if (montoG != string.Empty) precalificacion.Add("new_monto", Convert.ToDecimal(montoG.Trim()));
                if (onboarding.nroExpediente != null) precalificacion.Add("new_nroexpedientetda", onboarding.nroExpediente.Trim());
                if (onboarding.creditoAprobado != null) precalificacion.Add("new_creditoaprobado", onboarding.creditoAprobado.Trim());
                if (onboarding.serie != null) precalificacion.Add("new_Serie@odata.bind", "/new_seriedeoperacinsindicadas(" + onboarding.serie + ")");

                ResponseAPI response = await apiDynamics.CreateRecord("new_precalificacincrediticias", precalificacion, credenciales);

                if (!response.ok)
                {
                    return BadRequest(response.descripcion);
                }

                return Ok(response.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //VERSION MAS RECIENTE DEL ONBOARDING
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboardindigital")]
        public async Task<IActionResult> OnboardingDigital([FromBody] Onboarding onboarding)
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
                ApiDynamicsV2 apiDynamics = new();
                string solicitudId = string.Empty;
                string monto = string.Empty;
                string montoG = string.Empty;

                if (onboarding.facturacion != null)
                {
                    monto = onboarding.facturacion.Replace("$", string.Empty);
                    monto = monto.Replace(",", string.Empty).Trim();
                }

                if (onboarding.montoGarantia != null && onboarding.montoGarantia != "undefinded" && onboarding.montoGarantia != string.Empty)
                {
                    montoG = onboarding.montoGarantia.Replace("$", string.Empty);
                    montoG = montoG.Replace(",", string.Empty).Trim();
                }

                JObject precalificacion = new();
                if (!string.IsNullOrEmpty(onboarding.personeria))
                    precalificacion.Add("new_personeria", Convert.ToInt32(onboarding.personeria));

                if (!string.IsNullOrEmpty(onboarding.razonSocial))
                    precalificacion.Add("new_nombrerazonsocial", onboarding.razonSocial.Trim());

                if (!string.IsNullOrEmpty(onboarding.cuit))
                    precalificacion.Add("new_cuit", onboarding.cuit);

                if (!string.IsNullOrEmpty(onboarding.cuitcuil))
                    precalificacion.Add("new_cuitcuil", onboarding.cuitcuil);

                if (!string.IsNullOrEmpty(onboarding.email))
                    precalificacion.Add("emailaddress", onboarding.email);

                if (!string.IsNullOrEmpty(onboarding.telefono))
                    precalificacion.Add("new_telefonodecontacto", onboarding.telefono);

                if (!string.IsNullOrEmpty(onboarding.nombreContacto))
                    precalificacion.Add("new_nombredecontacto", onboarding.nombreContacto.Trim());

                if (!string.IsNullOrEmpty(onboarding.apellido))
                    precalificacion.Add("new_apellido", onboarding.apellido.Trim());

                if (!string.IsNullOrEmpty(onboarding.tipoDocumento))
                    precalificacion.Add("new_TipodeDocumento@odata.bind", "/new_tipodedocumentos(" + onboarding.tipoDocumento + ")");

                if (!string.IsNullOrEmpty(onboarding.productoServicio))
                    precalificacion.Add("new_productoservicio", onboarding.productoServicio);

                if (!string.IsNullOrEmpty(onboarding.actividadAFIP))
                    precalificacion.Add("new_ActividadAFIP@odata.bind", "/new_actividadafips(" + onboarding.actividadAFIP + ")");

                if (!string.IsNullOrEmpty(monto))
                    precalificacion.Add("new_facturacinpromedio", Convert.ToDecimal(monto));

                if (!string.IsNullOrEmpty(onboarding.tipoRelacion))
                    precalificacion.Add("new_relacinconlacuenta", Convert.ToInt32(onboarding.tipoRelacion));

                if (!string.IsNullOrEmpty(onboarding.tipoSocietario))
                    precalificacion.Add("new_tiposocietario", Convert.ToInt32(onboarding.tipoSocietario));

                if (!string.IsNullOrEmpty(onboarding.condicionImpositiva))
                    precalificacion.Add("new_CondiciondeInscripcionanteAFIP@odata.bind", "/new_condiciondeinscipcionanteafips(" + onboarding.condicionImpositiva + ")");

                if (!string.IsNullOrEmpty(onboarding.cantidadMujeres))
                    precalificacion.Add("new_cantidaddemujeresenpuestosdetomadedecisio", Convert.ToInt32(onboarding.cantidadMujeres));

                if (!string.IsNullOrEmpty(onboarding.empleadas))
                    precalificacion.Add("new_cantidaddeempleadosmujeres", Convert.ToInt32(onboarding.empleadas));

                if (!string.IsNullOrEmpty(onboarding.discapacitados))
                    precalificacion.Add("new_cantidaddepersonascondiscapacidad", Convert.ToInt32(onboarding.discapacitados));

                if (!string.IsNullOrEmpty(onboarding.otro))
                    precalificacion.Add("new_otro", onboarding.otro);

                if (!string.IsNullOrEmpty(onboarding.sectorEconomico))
                    precalificacion.Add("new_SectorEconmico@odata.bind", "/new_condicionpymes(" + onboarding.sectorEconomico.Trim() + ")");

                if (!string.IsNullOrEmpty(onboarding.inicioActividad))
                    precalificacion.Add("new_inicioactividad", onboarding.inicioActividad.Trim());

                if (!string.IsNullOrEmpty(onboarding.resena))
                    precalificacion.Add("new_breveresena", onboarding.resena);

                if (onboarding.emailNotificaciones != null && onboarding.emailNotificaciones.Trim() != "null" && onboarding.emailNotificaciones.Trim() != "undefined")
                    precalificacion.Add("new_emailnotificaciones", onboarding.emailNotificaciones.Trim());

                if (onboarding.invitacion != null && onboarding.invitacion.Trim() != "null" && onboarding.invitacion.Trim() != "undefined")
                    precalificacion.Add("new_invitacion", onboarding.invitacion.Trim());

                if (onboarding.cuitReferidor != null && onboarding.cuitReferidor.Trim() != "null" && onboarding.cuitReferidor.Trim() != "undefined")
                    precalificacion.Add("new_Referido@odata.bind", "/accounts(" + onboarding.cuitReferidor.Trim() + ")");

                if (onboarding.creadaPorApiLufe != null && onboarding.creadaPorApiLufe.Trim() != "" && onboarding.creadaPorApiLufe.Trim() != "undefined")
                    precalificacion.Add("new_creadaporapilufe", onboarding.creadaPorApiLufe.Trim());

                if (!string.IsNullOrWhiteSpace(onboarding.calle))
                    precalificacion.Add("new_calle", onboarding.calle);

                if (!string.IsNullOrWhiteSpace(onboarding.numero))
                    precalificacion.Add("new_numero", onboarding.numero);

                if (!string.IsNullOrWhiteSpace(onboarding.piso))
                    precalificacion.Add("new_piso", onboarding.piso);

                if (!string.IsNullOrWhiteSpace(onboarding.departamento))
                    precalificacion.Add("new_departamento", onboarding.departamento);

                if (!string.IsNullOrWhiteSpace(onboarding.codigoPostal))
                    precalificacion.Add("new_codigopostal", onboarding.codigoPostal);

                if (!string.IsNullOrWhiteSpace(onboarding.municipio))
                    precalificacion.Add("new_municipio", onboarding.municipio);

                if (!string.IsNullOrWhiteSpace(onboarding.localidad))
                    precalificacion.Add("new_localidad", onboarding.localidad);

                if (!string.IsNullOrWhiteSpace(onboarding.provincia))
                    precalificacion.Add("new_Provincia@odata.bind", "/new_provincias(" + onboarding.provincia + ")");

                if (!string.IsNullOrWhiteSpace(onboarding.pais))
                    precalificacion.Add("new_Pais@odata.bind", "/new_paises(" + onboarding.pais + ")");

                if (!string.IsNullOrWhiteSpace(onboarding.lineaDeCredito))
                    precalificacion.Add("new_lineadecredito", Convert.ToDecimal(onboarding.lineaDeCredito));

                if (!string.IsNullOrWhiteSpace(onboarding.destinoLineaDeCredito))
                    precalificacion.Add("new_DestinoLineadeCredito@odata.bind", "/new_destinodefondoses(" + onboarding.destinoLineaDeCredito + ")");

                //Garantia 
                if (montoG != string.Empty) precalificacion.Add("new_monto", Convert.ToDecimal(montoG.Trim()));
                if (onboarding.nroExpediente != null) precalificacion.Add("new_nroexpedientetda", onboarding.nroExpediente.Trim());
                if (onboarding.creditoAprobado != null) precalificacion.Add("new_creditoaprobado", onboarding.creditoAprobado.Trim());
                if (onboarding.serie != null) precalificacion.Add("new_Serie@odata.bind", "/new_seriedeoperacinsindicadas(" + onboarding.serie + ")");

                ResponseAPI response = await apiDynamics.CreateRecord("new_precalificacincrediticias", precalificacion, credenciales);

                if (!response.ok)
                {
                    return BadRequest(response.descripcion);
                }

                return Ok(response.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //VERSION MAS RECIENTE DEL ONBOARDING
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboardindigitalprotectores")]
        public async Task<IActionResult> OnboardingDigitalProtectores([FromBody] Onboarding onboarding)
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
                ApiDynamicsV2 apiDynamics = new();
                string solicitudId = string.Empty;
                string monto = string.Empty;
                string contacto_id = string.Empty;

                if (onboarding.facturacion != null)
                {
                    monto = onboarding.facturacion.Replace("$", string.Empty);
                    monto = monto.Replace(",", string.Empty).Trim();
                }

                JObject _socioProtector = new()
                {
                    { "new_rol", 100000000 },
                    { "new_onboarding", true },
                    { "new_fechadealta", DateTime.Now.ToString("yyyy-MM-dd")}
                };

                if (!string.IsNullOrEmpty(onboarding.personeria))
                    _socioProtector.Add("new_personeria", Convert.ToInt32(onboarding.personeria));

                if (!string.IsNullOrEmpty(onboarding.razonSocial))
                    _socioProtector.Add("name", onboarding.razonSocial.Trim());

                if (!string.IsNullOrEmpty(onboarding.cuit))
                    _socioProtector.Add("new_nmerodedocumento", onboarding.cuit);

                if (!string.IsNullOrEmpty(onboarding.tipoDocumento))
                    _socioProtector.Add("new_TipodedocumentoId@odata.bind", "/new_tipodedocumentos(" + onboarding.tipoDocumento + ")");

                if (!string.IsNullOrEmpty(onboarding.email))
                    _socioProtector.Add("emailaddress1", onboarding.email);

                if (!string.IsNullOrEmpty(onboarding.condicionImpositiva))
                    _socioProtector.Add("new_CondiciondeInscripcionanteAFIP@odata.bind", "/new_condiciondeinscipcionanteafips(" + onboarding.condicionImpositiva + ")");

                if (!string.IsNullOrEmpty(monto))
                    _socioProtector.Add("new_facturacionultimoanio", Convert.ToDecimal(monto));

                ResponseAPI response = await apiDynamics.CreateRecord("accounts", _socioProtector, credenciales);

                if (!response.ok)
                {
                    return BadRequest(response.descripcion);
                }

                if (!string.IsNullOrEmpty(onboarding.cuitcuil))
                {
                    JArray contactoSOC = await ExisteContactoAsync(onboarding.cuitcuil, credenciales);
                    if (contactoSOC.Count > 0)
                    {
                        contacto_id = ObtenerContactoID(contactoSOC);
                    }
                    else
                    {
                        //Contacto
                        JObject _contacto = new();
                        if (!string.IsNullOrEmpty(onboarding.nombreContacto))
                            _contacto.Add("firstname", onboarding.nombreContacto.Trim());

                        if (!string.IsNullOrEmpty(onboarding.apellido))
                            _contacto.Add("lastname", onboarding.apellido.Trim());

                        if (!string.IsNullOrEmpty(onboarding.cuitcuil))
                            _contacto.Add("new_cuitcuil", onboarding.cuitcuil);

                        if (!string.IsNullOrEmpty(onboarding.emailNotificaciones))
                            _contacto.Add("emailaddress1", onboarding.emailNotificaciones);

                        if (!string.IsNullOrEmpty(onboarding.telefono))
                            _contacto.Add("mobilephone", onboarding.telefono);

                        ResponseAPI responseContacto = await apiDynamics.CreateRecord("contacts", _contacto, credenciales);

                        if (!responseContacto.ok)
                        {
                            return BadRequest(responseContacto.descripcion);
                        }

                        contacto_id = responseContacto.descripcion;
                    }
                }

                if (contacto_id != string.Empty)
                {
                    JObject _socioProtectorActualizacion = new()
                    {
                        { "primarycontactid@odata.bind", "/contacts(" + contacto_id + ")" }
                    };

                    ResponseAPI responseActualizacion = await apiDynamics.UpdateRecord("accounts", response.descripcion, _socioProtectorActualizacion, credenciales);
                }

                return Ok(response.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboarding/documentacionporcuenta")]
        public async Task<IActionResult> DocumentacionPorCuenta(string socio_id, string documento_id, string solicitud_id, string invitacion = null)
        {
            try
            {
                #region credenciales
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
                ApiDynamicsV2 api = new ApiDynamicsV2();
                string documentacionporcuenta_id = string.Empty;
                var archivos = HttpContext.Request.Form.Files;
                string nota_id = string.Empty;

                if (documento_id != string.Empty && socio_id != string.Empty)
                {
                    documentacionporcuenta_id = await ExisteDocumentoAsync(socio_id, documento_id, credenciales);
                }

                if (documentacionporcuenta_id == string.Empty)
                {
                    JObject documento = new();

                    if (documento_id != null && documento_id != string.Empty && documento_id.Trim() != "undefined") 
                        documento.Add("new_DocumentoId@odata.bind", "/new_documentacions(" + documento_id + ")");
                    if (solicitud_id != null && solicitud_id != string.Empty && solicitud_id.Trim() != "undefined") 
                        documento.Add("new_SolicituddeAlta@odata.bind", "/new_precalificacincrediticias(" + solicitud_id + ")");
                    if (socio_id != null && socio_id != string.Empty && socio_id.Trim() != "undefined") 
                        documento.Add("new_CuentaId@odata.bind", "/accounts(" + socio_id + ")");

                    ResponseAPI responseAPI = await api.CreateRecord("new_documentacionporcuentas", documento, credenciales);

                    if (!responseAPI.ok)
                    {
                        return BadRequest(responseAPI.descripcion);
                    }

                    documentacionporcuenta_id = responseAPI.descripcion;
                }

                if (archivos.Count > 0)
                {
                    foreach (var file in archivos)
                    {
                        byte[] fileInBytes = new byte[file.Length];
                        using (BinaryReader theReader = new(file.OpenReadStream()))
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

                        ResponseAPI responseAPINota = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!responseAPINota.ok)
                        {
                            return BadRequest(responseAPINota.descripcion);
                        }

                        nota_id = responseAPINota.descripcion;
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
        [Route("api/onboarding/documentoporcuenta")]
        public async Task<IActionResult> DocumentacionPorCuentaSinAdjunto(string socio_id, string solicitud_id, [FromBody] DocumentacionPorCuentaOnboarding[] documentacionesXC)
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
                string jsonBody = JsonConvert.SerializeObject(documentacionesXC);
                ApiDynamicsV2 api = new(errorLogService, urlCompleta, jsonBody);
                string documentacionporcuenta_id = string.Empty;

                foreach (var documentacion in documentacionesXC)
                {
                    JObject documento = new()
                    {
                        { "new_DocumentoId@odata.bind", "/new_documentacions(" + documentacion.new_documentoid + ")" }
                    };

                    if (solicitud_id != null && solicitud_id != string.Empty)
                        documento.Add("new_SolicituddeAlta@odata.bind", "/new_precalificacincrediticias(" + solicitud_id + ")");
                    if (socio_id != null && socio_id != string.Empty)
                        documento.Add("new_CuentaId@odata.bind", "/accounts(" + socio_id + ")");

                    ResponseAPI responseAPI = await api.CreateRecord("new_documentacionporcuentas", documento, credenciales);

                    documentacionporcuenta_id = responseAPI.descripcion;
                }                

                return Ok("EXITO");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboarding/documentoporcuentaexistente")]
        public async Task<IActionResult> DocumentacionPorCuentaSinAdjuntoExistente(string socio_id, string solicitud_id, [FromBody] DocumentacionPorCuentaOnboarding[] documentacionesXC)
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
                string jsonBody = JsonConvert.SerializeObject(documentacionesXC);
                ApiDynamicsV2 api = new(errorLogService, urlCompleta, jsonBody);

                foreach (var documentacion in documentacionesXC)
                {
                    string documentacionporcuenta_id = string.Empty;

                    if (documentacion.new_documentoid != string.Empty && socio_id != string.Empty)
                    {
                        documentacionporcuenta_id = await ExisteDocumentoAsync(socio_id, documentacion.new_documentoid, credenciales);
                    }

                    if (documentacionporcuenta_id == string.Empty)
                    {
                        JObject documento = new()
                        {
                            { "new_DocumentoId@odata.bind", "/new_documentacions(" + documentacion.new_documentoid + ")" }
                        };

                        if (solicitud_id != null && solicitud_id != string.Empty)
                            documento.Add("new_SolicituddeAlta@odata.bind", "/new_precalificacincrediticias(" + solicitud_id + ")");
                        if (socio_id != null && socio_id != string.Empty)
                            documento.Add("new_CuentaId@odata.bind", "/accounts(" + socio_id + ")");

                        ResponseAPI responseAPI = await api.CreateRecord("new_documentacionporcuentas", documento, credenciales);

                        documentacionporcuenta_id = responseAPI.descripcion;
                    }
                }

                return Ok("EXITO");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboarding/adjuntosdocuxcuenta")]
        public async Task<IActionResult> Adjuntos(string socio_id)
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
                ApiDynamicsV2 api = new(errorLogService, urlCompleta);
                string resultado = "EXITO";
                var archivos = HttpContext.Request.Form.Files;

                if (archivos.Count > 0)
                {
                    foreach (var file in archivos)
                    {
                        try
                        {
                            string documentacionporcuenta_id = string.Empty;
                            string documento_id = ObtenerID(file.ContentDisposition);

                            if (documento_id != string.Empty && socio_id != string.Empty)
                            {
                                documentacionporcuenta_id = await ExisteDocumentoAsync(socio_id, documento_id, credenciales);
                            }

                            if (documentacionporcuenta_id != string.Empty)
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
                                    { "filename", file.FileName },
                                    { "objectid_new_documentacionporcuenta@odata.bind", "/new_documentacionporcuentas(" + documentacionporcuenta_id + ")" }
                                };

                                ResponseAPI notaResponse = await api.CreateRecord("annotations", annotation, credenciales);

                                if (!notaResponse.ok)
                                {
                                    if (resultado == "EXITO")
                                        resultado = file.FileName;
                                    else
                                        resultado = resultado + " | " + file.FileName;
                                }
                                else
                                {
                                    JObject docuXcuenta = new()
                                    {
                                        { "statuscode", 100000000 }
                                    };

                                    ResponseAPI docuResponse = await api.UpdateRecord("new_documentacionporcuentas", documentacionporcuenta_id, docuXcuenta, credenciales);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (resultado == "EXITO")
                                resultado = file.FileName;
                            else
                                resultado = resultado + " | " + file.FileName;
                        }
                    }
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboarding/adjuntosdocuxcuentaexistente")]
        public async Task<IActionResult> AdjuntosExistente(string socio_id)
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
                ApiDynamicsV2 api = new(errorLogService, urlCompleta);
                string resultado = "EXITO";
                var archivos = HttpContext.Request.Form.Files;

                if (archivos.Count > 0)
                {
                    foreach (var file in archivos)
                    {
                        try
                        {
                            string documentacionporcuenta_id = string.Empty;
                            string documento_id = ObtenerID(file.ContentDisposition);

                            if (documento_id != string.Empty && socio_id != string.Empty)
                            {
                                documentacionporcuenta_id = await ExisteDocumentoAsync(socio_id, documento_id, credenciales);
                            }

                            if (documentacionporcuenta_id == string.Empty)
                            {
                                JObject documento = new();

                                if (documento_id != null && documento_id != string.Empty && documento_id.Trim() != "undefined")
                                    documento.Add("new_DocumentoId@odata.bind", "/new_documentacions(" + documento_id + ")");
                                if (socio_id != null && socio_id != string.Empty && socio_id.Trim() != "undefined")
                                    documento.Add("new_CuentaId@odata.bind", "/accounts(" + socio_id + ")");

                                ResponseAPI responseAPI = await api.CreateRecord("new_documentacionporcuentas", documento, credenciales);

                                if (!responseAPI.ok)
                                {
                                    return BadRequest(responseAPI.descripcion);
                                }

                                documentacionporcuenta_id = responseAPI.descripcion;
                            }

                            if (documentacionporcuenta_id != string.Empty)
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
                                    { "filename", file.FileName },
                                    { "objectid_new_documentacionporcuenta@odata.bind", "/new_documentacionporcuentas(" + documentacionporcuenta_id + ")" }
                                };

                                ResponseAPI notaResponse = await api.CreateRecord("annotations", annotation, credenciales);

                                if (!notaResponse.ok)
                                {
                                    if (resultado == "EXITO")
                                        resultado = file.FileName;
                                    else
                                        resultado = resultado + " | " + file.FileName;
                                }
                                else
                                {
                                    JObject docuXcuenta = new()
                                    {
                                        { "statuscode", 100000000 }
                                    };

                                    ResponseAPI docuResponse = await api.UpdateRecord("new_documentacionporcuentas", documentacionporcuenta_id, docuXcuenta, credenciales);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (resultado == "EXITO")
                                resultado = file.FileName;
                            else
                                resultado = resultado + " | " + file.FileName;
                        }
                    }
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboarding/accionistas")]
        public async Task<ActionResult> Accionistas(string cuentaid, [FromBody] Accionistas[] accionistas)
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
                ApiDynamics apiV1 = new();
                ApiDynamicsV2 api = new();
                Account cuentaRelacionada = new();
                Contact contactoRelacionado = new();

                if (accionistas.Length > 0)
                {
                    foreach (var accionista in accionistas)
                    {
                        if (accionista.personeria == "100000000") //Juridica
                        {
                            api.EntityName = "accounts";
                            api.Filter = "new_nmerodedocumento eq '" + accionista.cuitcuil + "'";
                            JArray respuesta = apiV1.RetrieveMultipleAsync(apiV1, credenciales);
                            if (respuesta.Count == 0)
                            {
                                string cuenta_id = CrearCuentaAccionista(apiV1, accionista, credenciales);

                                if (cuenta_id != string.Empty)
                                {
                                    await CrearRelacionAccionistaAsync(api, cuentaid, cuenta_id, null, accionista, credenciales);
                                }
                            }
                            else if (respuesta.Count > 0)
                            {
                                cuentaRelacionada = JsonConvert.DeserializeObject<Account>(respuesta.First.ToString());
                                bool relacionCreada = await ComprobarRelacionAsync(api, cuentaid, cuentaRelacionada.accountid.ToString(),
                                    null, accionista.tipoRelacion, credenciales);
                                if (cuentaRelacionada.accountid != Guid.Empty && !relacionCreada)
                                    await CrearRelacionAccionistaAsync(api, cuentaid, cuentaRelacionada.accountid.ToString(), null, accionista, credenciales);
                            }
                        }
                        else if (accionista.personeria == "100000001") //Humana
                        {
                            api.EntityName = "contacts";
                            api.Filter = "new_cuitcuil eq " + accionista.cuitcuil;
                            JArray respuesta = apiV1.RetrieveMultipleAsync(apiV1, credenciales);

                            if (respuesta.Count == 0)
                            {
                                string contacto_id = await CrearContactoAccionista(api, accionista, credenciales);
                                if (contacto_id != string.Empty)
                                {
                                    await CrearRelacionAccionistaAsync(api, cuentaid, null, contacto_id, accionista, credenciales);
                                }
                            }
                            else if (respuesta.Count > 0)
                            {
                                contactoRelacionado = JsonConvert.DeserializeObject<Contact>(respuesta.First.ToString());
                                bool relacionCreada = await ComprobarRelacionAsync(api, cuentaid, null, contactoRelacionado.contactid.ToString(),
                                    accionista.tipoRelacion, credenciales);
                                if (contactoRelacionado.contactid != Guid.Empty && !relacionCreada)
                                    await CrearRelacionAccionistaAsync(api, cuentaid, null, contactoRelacionado.contactid.ToString(), accionista, credenciales);
                            }
                        }
                    }
                }

                return Ok("Accionistas creados con exito.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboarding/relacionaccionistas")]
        public async Task<ActionResult> RelacionAccionistas(string cuentaid, [FromBody] Accionistas[] accionistas)
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
                if (string.IsNullOrWhiteSpace(cuentaid) || !Guid.TryParse(cuentaid, out _))
                {
                    return BadRequest("El identificador de la cuenta es obligatorio y debe ser un GUID válido.");
                }

                if (accionistas == null || accionistas.Length == 0)
                {
                    return BadRequest("Debe proporcionar al menos un accionista para crear la relación.");
                }

                ApiDynamics api = new();
                HttpRequest request = HttpContext.Request;
                string urlCompleta = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
                string jsonBody = JsonConvert.SerializeObject(accionistas);
                ApiDynamicsV2 apiV2 = new(errorLogService, urlCompleta, jsonBody);
                Account cuentaRelacionada = new();
                Contact contactoRelacionado = new();

                for (int index = 0; index < accionistas.Length; index++)
                {
                    var accionista = accionistas[index];

                    if (accionista == null)
                    {
                        return BadRequest($"El accionista en la posición {index} es inválido.");
                    }

                    accionista.cuitcuil = accionista.cuitcuil?.Trim();
                    if (string.IsNullOrWhiteSpace(accionista.cuitcuil))
                    {
                        return BadRequest($"El accionista en la posición {index} debe incluir un CUIT/CUIL válido.");
                    }

                    if (accionista.personeria == "100000000") //Juridica
                    {
                        api.EntityName = "accounts";
                        api.Filter = "new_nmerodedocumento eq '" + accionista.cuitcuil + "'";
                        JArray respuesta = api.RetrieveMultipleAsync(api, credenciales);
                        if (respuesta.Count == 0)
                        {
                            string cuenta_id = CrearCuentaAccionista(api, accionista, credenciales);

                            if (!string.IsNullOrWhiteSpace(cuenta_id) && !string.Equals(cuenta_id, "ERROR", StringComparison.OrdinalIgnoreCase))
                            {
                                await CrearRelacionAccionistas(apiV2, cuentaid, cuenta_id, null, accionista, credenciales);
                            }
                            else
                            {
                                await errorLogService.CreateErrorLogAsync(new ErrorLog
                                {
                                    Level = "Error",
                                    Message = $"No se pudo crear la cuenta para el accionista con CUIT/CUIL {accionista.cuitcuil}.",
                                    Source = $"{nameof(OnboardingController)}.{nameof(RelacionAccionistas)}",
                                    Url = urlCompleta
                                });

                                return BadRequest($"No se pudo crear la cuenta para el accionista con CUIT/CUIL {accionista.cuitcuil}.");
                            }
                        }
                        else if (respuesta.Count > 0)
                        {
                            cuentaRelacionada = JsonConvert.DeserializeObject<Account>(respuesta.First.ToString());
                            bool relacionCreada = await ComprobarRelacionAsync(apiV2, cuentaid, cuentaRelacionada.accountid.ToString(),
                                null, accionista.tipoRelacion, credenciales);
                            if (cuentaRelacionada.accountid != Guid.Empty && !relacionCreada)
                                await CrearRelacionAccionistas(apiV2, cuentaid, cuentaRelacionada.accountid.ToString(), null, accionista, credenciales);
                        }
                    }
                    else if (accionista.personeria == "100000001") //Humana
                    {
                        decimal cuit = 0;
                        if (decimal.TryParse(accionista.cuitcuil, out decimal cuitcuilDecimal))
                        {
                            cuit = cuitcuilDecimal;
                        }
                        else
                        {
                            throw new ArgumentException($"El CUIT/CUIL '{accionista.cuitcuil}' no es válido o no puede convertirse a decimal.");
                        }
                        api.EntityName = "contacts";
                        api.Filter = $"new_cuitcuil eq {cuit}";
                        JArray respuesta = api.RetrieveMultipleAsync(api, credenciales);

                        if (respuesta.Count == 0)
                        {
                            string contacto_id = await CrearContactoAccionista(apiV2, accionista, credenciales);
                            if (!string.IsNullOrWhiteSpace(contacto_id) && !string.Equals(contacto_id, "ERROR", StringComparison.OrdinalIgnoreCase))
                            {
                                await CrearRelacionAccionistas(apiV2, cuentaid, null, contacto_id, accionista, credenciales);
                            }
                            else
                            {
                                await errorLogService.CreateErrorLogAsync(new ErrorLog
                                {
                                    Level = "Error",
                                    Message = $"No se pudo crear el contacto para el accionista con CUIT/CUIL {accionista.cuitcuil}.",
                                    Source = $"{nameof(OnboardingController)}.{nameof(RelacionAccionistas)}",
                                    Url = urlCompleta
                                });

                                return BadRequest($"No se pudo crear el contacto para el accionista con CUIT/CUIL {accionista.cuitcuil}.");
                            }
                        }
                        else if (respuesta.Count > 0)
                        {
                            contactoRelacionado = JsonConvert.DeserializeObject<Contact>(respuesta.First.ToString());
                            bool relacionCreada = await ComprobarRelacionAsync(apiV2, cuentaid, null, contactoRelacionado.contactid.ToString(),
                                accionista.tipoRelacion, credenciales);
                            if (contactoRelacionado.contactid != Guid.Empty && !relacionCreada)
                                await CrearRelacionAccionistas(apiV2, cuentaid, null, contactoRelacionado.contactid.ToString(), accionista, credenciales);
                        }
                    }
                }

                return Ok("Accionistas creados con exito.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboarding/generargarantia")]
        public async Task<IActionResult> GenerarGarantia([FromBody] GarantiaPublica garantia)
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
                ApiDynamicsV2 apiDynamics = new();
                string solicitudId = string.Empty;
                string monto = string.Empty;
                string montoG = string.Empty;
                List<SerieOnboarding> series = new();

                if (!string.IsNullOrEmpty(garantia.monto))
                {
                    montoG = garantia.monto.Replace("$", string.Empty);
                    montoG = montoG.Replace(",", string.Empty).Trim();
                }

                JArray Serie = await BuscarSerie(garantia.serie_id, credenciales);
                if (Serie.Count > 0)
                    series = ArmarSeries(Serie);

                JObject _garantia = new()
                {
                    { "new_NmerodeSerie@odata.bind", "/new_seriedeoperacinsindicadas(" + garantia.serie_id + ")" },
                    { "new_SocioParticipe@odata.bind", "/accounts(" + garantia.socio_id + ")" },
                    { "new_tipodeoperacion",  12 }, //Públicas
                    { "new_tipodegarantias",  100000001 }, //Garantias Comerciales
                    { "new_fechadeorigen",  DateTime.Now.ToString("yyyy-MM-dd") },
                    { "new_fechadevencimiento",  DateTime.Now.AddMonths(84).ToString("yyyy-MM-dd") },
                    { "statuscode", 100000004 }
                };

                if (montoG != string.Empty) 
                {
                    _garantia.Add("new_monto", Convert.ToDecimal(montoG.Trim()));
                    _garantia.Add("new_montocomprometidodelaval", Convert.ToDecimal(montoG.Trim()));
                }

                if (series.Count > 0)
                {
                    SerieOnboarding _serie = series[0];
                    if (_serie.new_tasa > 0)
                        _garantia.Add("new_tasa", _serie.new_tasa);
                    if (_serie.new_sistemadeamortizacion > 0)
                        _garantia.Add("new_sistemadeamortizacion", _serie.new_sistemadeamortizacion);
                    //if (_serie.new_porcentajeavaladodelaserie > 0)
                    //    _garantia.Add("new_porcentajeavaladodelaserie", _serie.new_porcentajeavaladodelaserie);
                    if (_serie.new_periodicidadpagos > 0)
                        _garantia.Add("new_periodicidadpagos", _serie.new_periodicidadpagos);
                    if (_serie.new_intersptosporcentuales > 0)
                        _garantia.Add("new_puntosporcentuales", _serie.new_intersptosporcentuales);
                }

                ResponseAPI response = await apiDynamics.CreateRecord("new_garantias", _garantia, credenciales);

                if (!response.ok)
                {
                    return BadRequest(response.descripcion);
                }

                return Ok(response.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/onboarding/crearactualizarcontacto")]
        public async Task<IActionResult> CrearActualizarContacto([FromBody] ContactoOnboardingCasfog contacto)
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
                ApiDynamicsV2 apiDynamics = new();
                string contacto_id = string.Empty;

                if (contacto.new_cuitcuil > 0)
                {
                    JArray contactoSOC = await ExisteContactoAsync(contacto.new_cuitcuil.ToString(), credenciales);
                    if (contactoSOC.Count > 0)
                    {
                        contacto_id = ObtenerContactoID(contactoSOC);
                    }   
                }

                JObject _contacto = new();
                if (!string.IsNullOrEmpty(contacto.firstname))
                    _contacto.Add("firstname", contacto.firstname);
                if (!string.IsNullOrEmpty(contacto.emailaddress1))
                    _contacto.Add("emailaddress1", contacto.emailaddress1);
                if (!string.IsNullOrEmpty(contacto.telephone1))
                    _contacto.Add("telephone1", contacto.telephone1);
                if (!string.IsNullOrEmpty(contacto.address1_line1))
                    _contacto.Add("address1_line1", contacto.address1_line1);
                if (contacto.new_cuitcuil > 0)
                    _contacto.Add("new_cuitcuil", contacto.new_cuitcuil);

                if (!string.IsNullOrEmpty(contacto_id))
                {
                    ResponseAPI responseActualizacion = await apiDynamics.UpdateRecord("contacts", contacto_id, _contacto, credenciales);

                    if (!responseActualizacion.ok)
                    {
                        return BadRequest(responseActualizacion.descripcion);
                    }

                    return Ok(responseActualizacion.descripcion);
                }

                ResponseAPI responseCreacion = await apiDynamics.CreateRecord("contacts", _contacto, credenciales);

                if (!responseCreacion.ok)
                {
                    return BadRequest(responseCreacion.descripcion);
                }

                if (contacto.new_cuitcuil > 0 && !string.IsNullOrEmpty(contacto.accountid))
                {
                    JObject cuenta = new()
                    {
                        {"new_ContactodeNotificaciones@odata.bind", "/contacts(" + responseCreacion.descripcion + ")" }
                    };

                    ResponseAPI responseAPI = await apiDynamics.UpdateRecord("accounts", contacto.accountid, cuenta, credenciales);
                    if (!responseAPI.ok)
                    {
                        return BadRequest(responseAPI.descripcion);
                    }
                }
                else if(!string.IsNullOrEmpty(contacto.accountid))
                {
                    JObject relacion = new()
                    {
                        { "new_CuentaId@odata.bind", "/accounts(" + contacto.accountid + ")" },
                        { "new_tipoderelacion", 100000005 } //OTRA
                    };

                    if (!string.IsNullOrEmpty(responseCreacion.descripcion))
                        relacion.Add("new_CuentaContactoVinculado_contact@odata.bind", "/contacts(" + responseCreacion.descripcion + ")");

                    ResponseAPI responseAPI = await apiDynamics.CreateRecord("new_participacionaccionarias", relacion, credenciales);

                    if (!responseAPI.ok)
                    {
                        return BadRequest(responseAPI.descripcion);
                    }
                }

                return Ok(responseCreacion.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public static async Task<string> ExisteDocumentoAsync(string account_id, string docuemnto_id, Credenciales credenciales)
        {
            string documentacion_id = string.Empty;
            DocumentacionPorCuentaONB docuXcuenta = new();
            ApiDynamicsV2 api = new()
            {
                EntityName = "new_documentacionporcuentas"
            };

            string fetchXML = "<entity name='new_documentacionporcuenta'>" +
                                        "<attribute name='new_name'/>" +
                                        "<attribute name='new_documentacionporcuentaid'/>" +
                                        "<attribute name='createdon'/> " +
                                        "<attribute name='statuscode'/> " +
                                                "<filter type='and'>" +
                                                        $"<condition attribute='new_cuentaid' operator='eq' value='{account_id}' />" +
                                                        $"<condition attribute='new_documentoid' operator='eq' value='{docuemnto_id}' />" +
                                                "</filter>" +
                                    "</entity>";

            if (fetchXML != string.Empty)
            {
                api.FetchXML = fetchXML;
            }

            JArray respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);

            if (respuesta != null)
            {
                foreach (var item in respuesta.Children())
                {
                    docuXcuenta = JsonConvert.DeserializeObject<DocumentacionPorCuentaONB>(item.ToString());
                }
            }

            if (docuXcuenta.new_documentacionporcuentaid != null)
            {
                documentacion_id = docuXcuenta.new_documentacionporcuentaid.ToString();
            }

            return documentacion_id;
        }

        public static async Task<bool> ComprobarRelacionAsync(ApiDynamicsV2 api, string accountid, string cuenta_id, string contacto_id, 
            string tipoRelacion, Credenciales credenciales)
        {
            try
            {
                bool existeRelacion = false;
                string cuentaContacto_id;

                if (cuenta_id != null)
                    cuentaContacto_id = cuenta_id;
                else
                    cuentaContacto_id = contacto_id;

                api.EntityName = "new_participacionaccionarias";

                string fetchXML = "<entity name='new_participacionaccionaria'>" +
                                        "<attribute name='new_name'/>" +
                                        "<attribute name='new_participacionaccionariaid'/>" +
                                        "<attribute name='createdon'/> " +
                                        "<attribute name='statuscode'/> " +
                                            "<filter type='and'>" +
                                                $"<condition attribute='new_cuentaid' operator='eq' value='{accountid}' />" +
                                                $"<condition attribute='new_cuentacontactovinculado' operator='eq' value='{cuentaContacto_id}' />" +
                                                $"<condition attribute='new_tipoderelacion' operator='eq' value='{tipoRelacion}' />" +
                                            "</filter>" +
                                    "</entity>";

                if (fetchXML != string.Empty)
                {
                    api.FetchXML = fetchXML;
                }

                JArray respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);

                if (respuesta?.Count > 0) existeRelacion = true;

                return existeRelacion;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string CrearCuentaAccionista(ApiDynamics api, Accionistas accionista, Credenciales credenciales)
        {
            try
            {
                JObject cuenta = new();
                if (accionista.razonSocial != string.Empty) cuenta.Add("name", accionista.razonSocial);
                if (accionista.cuitcuil != string.Empty) cuenta.Add("new_nmerodedocumento", accionista.cuitcuil);
                cuenta.Add("new_personeria", 100000000); //Juridica
                cuenta.Add("new_rol", 100000004); //Tercero
                return api.CreateRecord("accounts", cuenta, credenciales);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> CrearContactoAccionista(ApiDynamicsV2 api, Accionistas accionista, Credenciales credenciales)
        {
            try
            {
                JObject contacto = new();

                if (!string.IsNullOrWhiteSpace(accionista.nombre))
                {
                    contacto.Add("firstname", accionista.nombre);
                }

                if (!string.IsNullOrWhiteSpace(accionista.apellido))
                {
                    contacto.Add("lastname", accionista.apellido);
                }

                if (!string.IsNullOrWhiteSpace(accionista.cuitcuil))
                {
                    // Limpiamos el CUIT/CUIL de caracteres no numéricos
                    string cuitcuilLimpio = new string(accionista.cuitcuil.Where(char.IsDigit).ToArray());

                    if (decimal.TryParse(cuitcuilLimpio, out decimal cuitcuilDecimal))
                    {
                        contacto.Add("new_cuitcuil", cuitcuilDecimal);
                    }
                    else
                    {
                        throw new ArgumentException($"El CUIT/CUIL '{accionista.cuitcuil}' no es válido o no puede convertirse a decimal.");
                    }
                }

                ResponseAPI responseAPI = await api.CreateRecord("contacts", contacto, credenciales);

                if (!responseAPI.ok)
                {
                    return "ERROR";
                }

                return responseAPI.descripcion;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<string> CrearRelacionAccionistaAsync(ApiDynamicsV2 api, string accountid, string cuenta_id, 
            string contacto_id, Accionistas accionista, Credenciales credenciales)
        {
            try
            {
                JObject relacion = new()
                {
                    { "new_CuentaId@odata.bind", "/accounts(" + accountid + ")" }
                };

                if (cuenta_id != null && cuenta_id != string.Empty)
                    relacion.Add("new_CuentaContactoVinculado_account@odata.bind", "/accounts(" + cuenta_id + ")");

                if (contacto_id != null && contacto_id != string.Empty)
                    relacion.Add("new_CuentaContactoVinculado_contact@odata.bind", "/contacts(" + contacto_id + ")");

                if (accionista.tipoRelacion != null && accionista.tipoRelacion != String.Empty)
                    relacion.Add("new_tipoderelacion", Convert.ToInt32(accionista.tipoRelacion));

                if (accionista.porcentaje != null && accionista.porcentaje != String.Empty)
                    relacion.Add("new_porcentajedeparticipacion", Convert.ToDecimal(accionista.porcentaje));

                if (accionista.descripcion != null && accionista.descripcion != String.Empty)
                    relacion.Add("new_observaciones", accionista.descripcion);

                ResponseAPI responseAPI = await api.CreateRecord("new_participacionaccionarias", relacion, credenciales);

                if (!responseAPI.ok)
                {
                    return "ERROR";
                }

                return responseAPI.descripcion;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task CrearRelacionAccionistas(ApiDynamicsV2 api, string accountid, string cuenta_id,
           string contacto_id, Accionistas accionista, Credenciales credenciales)
        {
            try
            {
                JObject relacion = new()
                {
                    { "new_CuentaId@odata.bind", "/accounts(" + accountid + ")" }
                };

                if (cuenta_id != null && cuenta_id != string.Empty)
                    relacion.Add("new_CuentaContactoVinculado_account@odata.bind", "/accounts(" + cuenta_id + ")");

                if (contacto_id != null && contacto_id != string.Empty)
                    relacion.Add("new_CuentaContactoVinculado_contact@odata.bind", "/contacts(" + contacto_id + ")");

                if (accionista.tipoRelacion != null && accionista.tipoRelacion != String.Empty)
                    relacion.Add("new_tipoderelacion", Convert.ToInt32(accionista.tipoRelacion));

                if (accionista.porcentaje != null && accionista.porcentaje != String.Empty)
                {
                    relacion.Add("new_porcentajedeparticipacion", Convert.ToDecimal(accionista.porcentaje, new CultureInfo("en-US")));
                }

                if (accionista.descripcion != null && accionista.descripcion != String.Empty)
                    relacion.Add("new_observaciones", accionista.descripcion);

                if (!string.IsNullOrEmpty(accionista.tipoRelacionAccionista))
                {
                    if (accionista.tipoRelacionAccionista == "0")
                    {
                        relacion.Add("new_relacion", false);
                    }
                    else if(accionista.tipoRelacionAccionista == "1")
                    {
                        relacion.Add("new_relacion", true);
                    }
                }

                ResponseAPI respuesta = await api.CreateRecord("new_participacionaccionarias", relacion, credenciales);
                if (!respuesta.ok)
                {
                    throw new Exception(respuesta.descripcion);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string ObtenerID(string body)
        {
            string documento_id = string.Empty;
            if (body.Contains(';'))
            {
                var contenDisposition = body.Split(';');
                if (contenDisposition.Length > 0)
                {
                    if (contenDisposition[1].Contains('/'))
                    {
                        documento_id = Regex.Replace(contenDisposition[1].Split('/')[1], @"[^\w \-]", "");
                    }
                }
            }
            return documento_id;
        }

        public static async Task<JArray> BuscarSerie(string serie_id, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string documentacion_id = string.Empty;
                ApiDynamicsV2 api = new()
                {
                    EntityName = "new_seriedeoperacinsindicadas"
                };

                string fetchXML = "<entity name='new_seriedeoperacinsindicada'>" +
                                            "<attribute name='new_name'/>" +
                                            "<attribute name='new_tasa'/> " +
                                            "<attribute name='new_sistemadeamortizacion'/> " +
                                            "<attribute name='new_porcentajeavaladodelaserie'/> " +
                                            "<attribute name='new_periodicidadpagos'/> " +
                                            "<attribute name='new_intersptosporcentuales'/> " +
                                                    "<filter type='and'>" +
                                                            $"<condition attribute='new_seriedeoperacinsindicadaid' operator='eq' value='{serie_id}' />" +
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

        public static List<SerieOnboarding> ArmarSeries(JToken SeriesJT)
        {
            return JsonConvert.DeserializeObject<List<SerieOnboarding>>(SeriesJT.ToString());
        }

        public static async Task<JArray> ExisteContactoAsync(string cuitcuil, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                ApiDynamicsV2 api = new()
                {
                    EntityName = "contacts"
                };

                string fetchXML = "<entity name='contact'>" +
                                            "<attribute name='contactid'/>" +
                                                    "<filter type='and'>" +
                                                            $"<condition attribute='new_cuitcuil' operator='eq' value='{cuitcuil}' />" +
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
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string ObtenerContactoID(JToken contactoJT)
        {
            string contacto_id = string.Empty;

            ContactoOnboardingCasfog contactoOnboarding = JsonConvert.DeserializeObject<ContactoOnboardingCasfog>(contactoJT.First().ToString());
            if (contactoOnboarding.contactid != null)
                contacto_id = contactoOnboarding.contactid;

            return contacto_id;
        }
    }
}
