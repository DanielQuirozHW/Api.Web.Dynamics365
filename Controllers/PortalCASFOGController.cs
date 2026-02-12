using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;
using static Api.Web.Dynamics365.Models.HRFactors;
using static Api.Web.Dynamics365.Models.PortalCASFOG;
using static Api.Web.Dynamics365.Models.PortalSocioParticipe;
using static Api.Web.Dynamics365.Models.SgrOneClick;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class PortalCASFOGController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;

        public PortalCASFOGController(IConfiguration _configuration,
           UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            configuration = _configuration;
            this.userManager = userManager;
            this.context = context;
        }

        #region Pyme
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/portalcasfog/pyme")]
        public async Task<IActionResult> ActualizaPyme([FromBody] Pyme cuenta)
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
                if (cuenta.new_fechaltimaconsulta != null && cuenta.new_fechaltimaconsulta != string.Empty)
                    Cuenta.Add("new_fechaltimaconsulta", cuenta.new_fechaltimaconsulta);
                if (cuenta.new_respuestanosis != null && cuenta.new_respuestanosis != string.Empty)
                    Cuenta.Add("new_respuestanosis", cuenta.new_respuestanosis);
                if (cuenta.new_calificacion > 0)
                    Cuenta.Add("new_calificacion", cuenta.new_calificacion);
                if (cuenta.new_firmante != null && cuenta.new_firmante != string.Empty)
                    Cuenta.Add("new_Firmante@odata.bind", "/new_participacionaccionarias(" + cuenta.new_firmante + ")");
                if (cuenta.new_estadodeactividad > 0)
                    Cuenta.Add("new_estadodeactividad", cuenta.new_estadodeactividad);
                if (cuenta.new_estadodelsocio > 0)
                    Cuenta.Add("new_estadodelsocio", cuenta.new_estadodelsocio);
                if (cuenta.new_contactodenotificaciones != null && cuenta.new_contactodenotificaciones != string.Empty)
                    Cuenta.Add("new_ContactodeNotificaciones@odata.bind", "/contacts(" + cuenta.new_firmante + ")");
                if (string.IsNullOrEmpty(cuenta.emailaddress1))
                    Cuenta.Add("emailaddress1", cuenta.emailaddress1);

                ResponseAPI resultado = await apiDynamics.UpdateRecord("accounts", cuenta.accountid, Cuenta, credenciales);

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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/portalcasfog/activarpyme")]
        public async Task<IActionResult> ActivarPyme([FromBody] Pyme cuenta)
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

                if (cuenta.accountid == null || cuenta.accountid == string.Empty)
                {
                    return BadRequest("El id de la cuenta es requerido");
                }

                ApiDynamicsV2 apiDynamics = new ApiDynamicsV2();
                JObject Cuenta = new()
                {
                    { "new_estadodelsocio", 100000000 } //Activo
                };

                ResponseAPI resultado = await apiDynamics.UpdateRecord("accounts", cuenta.accountid, Cuenta, credenciales);

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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/portalcasfog/cargardocumentacionporcuenta")]
        public async Task<IActionResult> CargarDocumentacionPorCuenta(string documentacionporcuenta_id, string mantenerEstado)
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
                    return BadRequest("El id del documento esta vacio");

                ApiDynamicsV2 api = new ApiDynamicsV2();
                var archivos = HttpContext.Request.Form.Files;

                if (archivos.Count > 0)
                {
                    if (mantenerEstado != null && mantenerEstado != "null" && mantenerEstado != "undefined")
                    {
                        if(mantenerEstado == "false")
                        {
                            JObject documentacion = new()
                            {
                                { "statuscode", 100000000 }
                            };

                            await api.UpdateRecord("new_documentacionporcuentas", documentacionporcuenta_id, documentacion, credenciales);
                        }
                    }
                    else
                    {
                        JObject documentacion = new()
                        {
                            { "statuscode", 100000000 }
                        };

                        await api.UpdateRecord("new_documentacionporcuentas", documentacionporcuenta_id, documentacion, credenciales);
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

                        if (documentacionporcuenta_id != string.Empty)
                            annotation.Add("objectid_new_documentacionporcuenta@odata.bind", "/new_documentacionporcuentas(" + documentacionporcuenta_id + ")");

                        ResponseAPI notaResponse = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!notaResponse.ok)
                        {
                            return BadRequest(notaResponse.descripcion);
                        }

                        return Ok(notaResponse.descripcion);
                    }
                }

                return Ok("OK - Falta subir archivo");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/portalcasfog/actualizardocumentacionporcuenta")]
        public async Task<IActionResult> ActualizaDocumentacionPorCuenta([FromBody] PortalCASFOG.DocumentacionPorCuenta docuXCuenta)
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
                ApiDynamicsV2 apiDynamics = new ApiDynamicsV2();

                JObject DocumentacionPorCuenta = new();
                if (docuXCuenta.statuscode > 0)
                    DocumentacionPorCuenta.Add("statuscode", docuXCuenta.statuscode);
                if (docuXCuenta.new_fechadevencimiento != null && docuXCuenta.new_fechadevencimiento != string.Empty)
                    DocumentacionPorCuenta.Add("new_fechadevencimiento", DateTime.Parse(docuXCuenta.new_fechadevencimiento).ToString("yyyy-MM-dd"));
                if (docuXCuenta.new_visibleenportal != null && docuXCuenta.new_visibleenportal != string.Empty)
                    DocumentacionPorCuenta.Add("new_visibleenportal", docuXCuenta.new_visibleenportal);

                ResponseAPI resultado = await apiDynamics.UpdateRecord("new_documentacionporcuentas", docuXCuenta.new_documentacionporcuentaid, DocumentacionPorCuenta, credenciales);

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
        #region Garantias
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/portalcasfog/garantia")]
        public async Task<IActionResult> ActualizaGarantia([FromBody] GarantiaCasfog garantia)
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
                ApiDynamicsV2 apiDynamics = new ApiDynamicsV2();
                JObject Garantia = new();

                if (garantia.new_socioparticipe != null && garantia.new_socioparticipe != string.Empty)
                    Garantia.Add("new_SocioParticipe@odata.bind", "/accounts(" + garantia.new_socioparticipe + ")");

                if (garantia.new_tipodeoperacion > 0)
                    Garantia.Add("new_tipodeoperacion", garantia.new_tipodeoperacion);

                if (garantia.new_fechadeorigen != null && garantia.new_fechadeorigen != string.Empty)
                    Garantia.Add("new_fechadeorigen", garantia.new_fechadeorigen);

                if (garantia.new_acreedor != null && garantia.new_acreedor != string.Empty)
                    Garantia.Add("new_Acreedor@odata.bind", "/new_acreedors(" + garantia.new_acreedor + ")");

                if (garantia.new_nmerodeserie != null && garantia.new_nmerodeserie != string.Empty)
                    Garantia.Add("new_NmerodeSerie@odata.bind", "/new_seriedeoperacinsindicadas(" + garantia.new_nmerodeserie + ")");

                if (garantia.statuscode > 0)
                    Garantia.Add("statuscode", garantia.statuscode);

                if (garantia.new_referido != null && garantia.new_referido != string.Empty)
                    Garantia.Add("new_Referido@odata.bind", "/accounts(" + garantia.new_referido + ")");

                if (garantia.new_fechaemisindelcheque != null && garantia.new_fechaemisindelcheque != string.Empty)
                    Garantia.Add("new_fechaemisindelcheque", garantia.new_fechaemisindelcheque);

                if (garantia.new_numerodeprestamo > 0)
                    Garantia.Add("new_numerodeprestamo", garantia.new_numerodeprestamo);

                if (garantia.new_oficialdecuentas != null && garantia.new_oficialdecuentas != string.Empty)
                    Garantia.Add("new_Oficialdecuentas@odata.bind", "/contacts(" + garantia.new_oficialdecuentas + ")");

                if (garantia.new_fechadenegociacion != null && garantia.new_fechadenegociacion != string.Empty)
                    Garantia.Add("new_fechadenegociacion", garantia.new_fechadenegociacion); 

                if (garantia.new_sistemadeamortizacion > 0)
                    Garantia.Add("new_sistemadeamortizacion", garantia.new_sistemadeamortizacion);

                if (garantia.new_tasa > 0)
                    Garantia.Add("new_tasa", garantia.new_tasa);

                if (garantia.new_puntosporcentuales > 0)
                    Garantia.Add("new_puntosporcentuales", garantia.new_puntosporcentuales);

                if (garantia.new_periodicidadpagos > 0)
                    Garantia.Add("new_periodicidadpagos", garantia.new_periodicidadpagos);

                if (garantia.new_dictamendelaval > 0)
                    Garantia.Add("new_dictamendelaval", garantia.new_dictamendelaval);

                if (garantia.new_nroexpedientetad != null && garantia.new_nroexpedientetad != string.Empty)
                    Garantia.Add("new_nroexpedientetad", garantia.new_nroexpedientetad);

                if (garantia.new_creditoaprobado != null && garantia.new_creditoaprobado != string.Empty)
                    Garantia.Add("new_creditoaprobado", garantia.new_creditoaprobado);

                if (garantia.new_codigo != null && garantia.new_codigo != string.Empty)
                    Garantia.Add("new_codigo", garantia.new_codigo);

                if (garantia.new_tipodegarantias > 0)
                    Garantia.Add("new_tipodegarantias", garantia.new_tipodegarantias);

                if (garantia.new_nroexpedientetad != null && garantia.new_nroexpedientetad != string.Empty)
                    Garantia.Add("new_nroexpedientetad", garantia.new_nroexpedientetad);

                if (garantia.new_plazodias > 0)
                    Garantia.Add("new_plazodias", garantia.new_plazodias);

                if (garantia.new_fechadevencimiento != null && garantia.new_fechadevencimiento != string.Empty)
                    Garantia.Add("new_fechadevencimiento", garantia.new_fechadevencimiento);

                if (garantia.new_montocomprometidodelaval > 0)
                    Garantia.Add("new_montocomprometidodelaval", garantia.new_montocomprometidodelaval);

                if (garantia.new_determinadaenasamblea != null && garantia.new_determinadaenasamblea != string.Empty)
                    Garantia.Add("new_determinadaenasamblea", garantia.new_determinadaenasamblea);

                if (garantia.new_monto > 0)
                    Garantia.Add("new_monto", garantia.new_monto);

                ResponseAPI resultado = await apiDynamics.UpdateRecord("new_garantias", garantia.new_garantiaid, Garantia, credenciales);

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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/portalcasfog/desembolsogarantia")]
        public async Task<IActionResult> DesembolsoGarantia([FromBody] GarantiaCasfog garantia)
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
                ApiDynamicsV2 apiDynamics = new ApiDynamicsV2();

                JObject Garantia = new()
                {
                    { "statuscode", 100000004 } //En Cartera
                };

                if (garantia.new_socioparticipe != null && garantia.new_socioparticipe != string.Empty)
                    Garantia.Add("new_SocioParticipe@odata.bind", "/accounts(" + garantia.new_socioparticipe + ")");

                if (garantia.new_tipodeoperacion > 0)
                    Garantia.Add("new_tipodeoperacion", garantia.new_tipodeoperacion);

                if (garantia.new_fechadeorigen != null && garantia.new_fechadeorigen != string.Empty)
                    Garantia.Add("new_fechadeorigen", DateTime.Parse(garantia.new_fechadeorigen).ToString("yyyy-MM-dd"));

                if (garantia.new_acreedor != null && garantia.new_acreedor != string.Empty)
                    Garantia.Add("new_Acreedor@odata.bind", "/new_acreedors(" + garantia.new_acreedor + ")");

                if (garantia.new_nmerodeserie != null && garantia.new_nmerodeserie != string.Empty)
                    Garantia.Add("new_NmerodeSerie@odata.bind", "/new_seriedeoperacinsindicadas(" + garantia.new_nmerodeserie + ")");

                if (garantia.new_referido != null && garantia.new_referido != string.Empty)
                    Garantia.Add("new_Referido@odata.bind", "/accounts(" + garantia.new_referido + ")");

                if (garantia.new_fechaemisindelcheque != null && garantia.new_fechaemisindelcheque != string.Empty)
                    Garantia.Add("new_fechaemisindelcheque" ,DateTime.Parse(garantia.new_fechaemisindelcheque).ToString("yyyy-MM-dd"));

                if (garantia.new_numerodeprestamo > 0)
                    Garantia.Add("new_numerodeprestamo", garantia.new_numerodeprestamo);

                if (garantia.new_oficialdecuentas != null && garantia.new_oficialdecuentas != string.Empty)
                    Garantia.Add("new_Oficialdecuentas@odata.bind", "/contacts(" + garantia.new_oficialdecuentas + ")");

                if (garantia.new_sistemadeamortizacion > 0)
                    Garantia.Add("new_sistemadeamortizacion", garantia.new_sistemadeamortizacion);

                if (garantia.new_tasa > 0)
                    Garantia.Add("new_tasa", garantia.new_tasa);

                if (garantia.new_puntosporcentuales > 0)
                    Garantia.Add("new_puntosporcentuales", garantia.new_puntosporcentuales);

                if (garantia.new_periodicidadpagos > 0)
                    Garantia.Add("new_periodicidadpagos", garantia.new_periodicidadpagos);

                if (garantia.new_nroexpedientetad != null && garantia.new_nroexpedientetad != string.Empty)
                    Garantia.Add("new_nroexpedientetad", garantia.new_nroexpedientetad);

                if (garantia.new_creditoaprobado != null && garantia.new_creditoaprobado != string.Empty)
                    Garantia.Add("new_creditoaprobado", garantia.new_creditoaprobado);

                if (garantia.new_codigo != null && garantia.new_codigo != string.Empty)
                    Garantia.Add("new_codigo", garantia.new_codigo);

                if (garantia.new_tipodegarantias > 0)
                    Garantia.Add("new_tipodegarantias", garantia.new_tipodegarantias);

                if (garantia.new_nroexpedientetad != null && garantia.new_nroexpedientetad != string.Empty)
                    Garantia.Add("new_nroexpedientetad", garantia.new_nroexpedientetad);

                if (garantia.new_plazodias > 0)
                    Garantia.Add("new_plazodias", garantia.new_plazodias);

                if (garantia.new_fechadevencimiento != null && garantia.new_fechadevencimiento != string.Empty)
                    Garantia.Add("new_fechadevencimiento", DateTime.Parse(garantia.new_fechadevencimiento).ToString("yyyy-MM-dd")); 

                if (garantia.new_montocomprometidodelaval > 0)
                    Garantia.Add("new_montocomprometidodelaval", garantia.new_montocomprometidodelaval);

                if (garantia.new_determinadaenasamblea != null && garantia.new_determinadaenasamblea != string.Empty)
                    Garantia.Add("new_determinadaenasamblea", garantia.new_determinadaenasamblea);

                if (garantia.new_monto > 0)
                    Garantia.Add("new_monto", garantia.new_monto);

                if (garantia.new_DesembolsoAnterior != null && garantia.new_DesembolsoAnterior != string.Empty)
                    Garantia.Add("new_DesembolsoAnterior@odata.bind", "/new_garantias(" + garantia.new_DesembolsoAnterior + ")");

                if (garantia.new_condesembolsosparciales != null && garantia.new_condesembolsosparciales != string.Empty)
                    Garantia.Add("new_condesembolsosparciales", garantia.new_condesembolsosparciales);

                ResponseAPI resultado = await apiDynamics.CreateRecord("new_garantias", Garantia, credenciales);

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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/portalcasfog/adjuntosgarantia")]
        public async Task<IActionResult> AdjutnosGarantia(string garantia_id, string tipo, string visiblePortal = null)
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

                ApiDynamicsV2 api = new ApiDynamicsV2();
                var archivos = HttpContext.Request.Form.Files;

                if (archivos.Count > 0)
                {
                    JObject adjunto = new()
                    {
                        { "new_Garantia@odata.bind", "/new_garantias(" + garantia_id + ")"  },
                        { "new_tipo",  Convert.ToInt32(tipo)},
                    };

                    if (!string.IsNullOrEmpty(visiblePortal))
                    {
                        adjunto.Add("new_visibleenportal", visiblePortal);
                    }

                    ResponseAPI adjuntosResponse = await api.CreateRecord("new_adjuntoses", adjunto, credenciales);

                    if (adjuntosResponse.ok)
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

                            if (garantia_id != string.Empty)
                                annotation.Add("objectid_new_adjuntos@odata.bind", "/new_adjuntoses(" + adjuntosResponse.descripcion + ")");

                            ResponseAPI notaResponse = await api.CreateRecord("annotations", annotation, credenciales);

                            if (!notaResponse.ok)
                            {
                                return BadRequest(notaResponse.descripcion);
                            }

                            return Ok(notaResponse.descripcion);
                        }
                    }
                    else
                    {
                        return BadRequest(adjuntosResponse.descripcion);
                    }
                }

                return Ok("OK - Falta subir archivo");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region OperacionesSindicadas
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/portalcasfog/opsindicadas")]
        public async Task<IActionResult> ActualizaOperacionSindicada([FromBody] OperacionSindicadaCasfog operacion)
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
                ApiDynamicsV2 apiDynamics = new ApiDynamicsV2();

                JObject Operacion = new()
                {
                    { "statuscode", operacion.statuscode }
                };

                if (operacion.new_motivoderechazo != null)
                    Operacion.Add("new_motivoderechazo", operacion.new_motivoderechazo);

                ResponseAPI resultado = await apiDynamics.UpdateRecord("new_operacionsindicadas", operacion.new_operacionsindicadaid, Operacion, credenciales);

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
        #region SGRIndicadores
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/portalcasfog/sgr")]
        public async Task<IActionResult> ActualizaSGR([FromBody] SGR sgr)
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

                JObject SGR = new();

                if (sgr.new_fechadeiniciodeactividades != null && sgr.new_fechadeiniciodeactividades != string.Empty)
                    SGR.Add("new_fechadeiniciodeactividades", DateTime.Parse(sgr.new_fechadeiniciodeactividades).ToString("yyyy-MM-dd"));

                if (sgr.new_fechadeasociacinencasfog != null && sgr.new_fechadeasociacinencasfog != string.Empty)
                    SGR.Add("new_fechadeasociacinencasfog", DateTime.Parse(sgr.new_fechadeasociacinencasfog).ToString("yyyy-MM-dd"));

                if (sgr.new_fechainscripcinantebcra != null && sgr.new_fechainscripcinantebcra != string.Empty)
                    SGR.Add("new_fechainscripcinantebcra", DateTime.Parse(sgr.new_fechainscripcinantebcra).ToString("yyyy-MM-dd"));

                if (sgr.new_nombredelacalificadora != null && sgr.new_nombredelacalificadora != string.Empty)
                    SGR.Add("new_nombredelacalificadora", sgr.new_nombredelacalificadora);

                if (sgr.new_calificacin != null && sgr.new_calificacin != string.Empty)
                    SGR.Add("new_calificacin", sgr.new_calificacin);

                if (sgr.new_fechaultimacalificacion != null && sgr.new_fechaultimacalificacion != string.Empty)
                    SGR.Add("new_fechaultimacalificacion", DateTime.Parse(sgr.new_fechaultimacalificacion).ToString("yyyy-MM-dd"));

                if (sgr.new_cuitcalificadora != null && sgr.new_cuitcalificadora != string.Empty)
                    SGR.Add("new_cuitcalificadora", sgr.new_cuitcalificadora);

                string resultadoActualizacion = apiDynamics.UpdateRecord("new_sgrs", sgr.new_sgrid, SGR, credenciales);

                return Ok(resultadoActualizacion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/portalcasfog/estructurasgr")]
        public async Task<IActionResult> CrearEstructuraSGR([FromBody] EstructuraSGR estructuraSGR)
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

                JObject EstructuraSGR = new();

                if (estructuraSGR.new_sgr != null && estructuraSGR.new_sgr != string.Empty)
                    EstructuraSGR.Add("new_sgr@odata.bind", "/new_sgrs(" + estructuraSGR.new_sgr + ")");

                if (estructuraSGR.new_contacto != null && estructuraSGR.new_contacto != string.Empty)
                    EstructuraSGR.Add("new_Contacto@odata.bind", "/contacts(" + estructuraSGR.new_contacto + ")");

                if (estructuraSGR.new_cargo != null && estructuraSGR.new_cargo != string.Empty)
                    EstructuraSGR.Add("new_cargo", estructuraSGR.new_cargo);

                if (estructuraSGR.new_name != null && estructuraSGR.new_name != string.Empty)
                    EstructuraSGR.Add("new_name", estructuraSGR.new_name);

                if (estructuraSGR.new_rol > 0)
                    EstructuraSGR.Add("new_rol", estructuraSGR.new_rol);

                if (estructuraSGR.new_correoelectronico != null && estructuraSGR.new_correoelectronico != string.Empty)
                    EstructuraSGR.Add("new_correoelectronico", estructuraSGR.new_correoelectronico);

                if (estructuraSGR.new_numerodedocumento != null && estructuraSGR.new_numerodedocumento != string.Empty)
                    EstructuraSGR.Add("new_numerodedocumento", estructuraSGR.new_numerodedocumento);

                if (estructuraSGR.new_porcentaje > 0)
                    EstructuraSGR.Add("new_porcentaje", estructuraSGR.new_porcentaje);

                string resultado = apiDynamics.CreateRecord("new_estructurasgrs", EstructuraSGR, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/portalcasfog/estructurasgr")]
        public async Task<IActionResult> ActualizarEstructuraSGR([FromBody] EstructuraSGR estructuraSGR)
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

                JObject EstructuraSGR = new();

                if (estructuraSGR.new_sgr != null && estructuraSGR.new_sgr != string.Empty)
                    EstructuraSGR.Add("new_sgr@odata.bind", "/new_sgrs(" + estructuraSGR.new_sgr + ")");

                if (estructuraSGR.new_contacto != null && estructuraSGR.new_contacto != string.Empty)
                    EstructuraSGR.Add("new_Contacto@odata.bind", "/contacts(" + estructuraSGR.new_contacto + ")");

                if (estructuraSGR.new_cargo != null && estructuraSGR.new_cargo != string.Empty)
                    EstructuraSGR.Add("new_cargo", estructuraSGR.new_cargo);

                if (estructuraSGR.new_name != null && estructuraSGR.new_name != string.Empty)
                    EstructuraSGR.Add("new_name", estructuraSGR.new_name);

                if (estructuraSGR.new_rol > 0)
                    EstructuraSGR.Add("new_rol", estructuraSGR.new_rol);

                if (estructuraSGR.new_correoelectronico != null && estructuraSGR.new_correoelectronico != string.Empty)
                    EstructuraSGR.Add("new_correoelectronico", estructuraSGR.new_correoelectronico);

                if (estructuraSGR.new_numerodedocumento != null && estructuraSGR.new_numerodedocumento != string.Empty)
                    EstructuraSGR.Add("new_numerodedocumento", estructuraSGR.new_numerodedocumento);

                if (estructuraSGR.new_porcentaje > 0)
                    EstructuraSGR.Add("new_porcentaje", estructuraSGR.new_porcentaje);


                string resultado = apiDynamics.UpdateRecord("new_estructurasgrs", estructuraSGR.new_estructurasgrid, EstructuraSGR, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/documentacionporsgr")]
        public async Task<IActionResult> DocumentacionPorSGR([FromBody] DocumentacionSGR docuSGR)
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

                ApiDynamics api = new ApiDynamics();

                JObject DocumentacionPorSGR = new()
                {
                    { "new_sgr@odata.bind", "/new_sgrs(" + docuSGR.sgr_id + ")"  },
                    { "new_Documentacion@odata.bind", "/new_documentacions(" + docuSGR.documentacion_id + ")"  }
                };

                if (docuSGR.fechaVencimiento != null && docuSGR.fechaVencimiento != string.Empty)
                    DocumentacionPorSGR.Add("new_fechadevencimiento", DateTime.Parse(docuSGR.fechaVencimiento).ToString("yyyy-MM-dd"));

                string documentacionResponse = api.CreateRecord("new_documentacionporsgrs", DocumentacionPorSGR, credenciales);

                if(documentacionResponse == "ERROR")
                {
                    throw new Exception("ERROR");
                }

                return Ok(documentacionResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/socioparticipe/notadocumentacionporsgr")]
        public async Task<IActionResult> DocumentacionPorSGR(string documentacion_id)
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

                ApiDynamics api = new ApiDynamics();
                var archivos = HttpContext.Request.Form.Files;
                string nota_id = string.Empty;

                if (archivos.Count > 0)
                {
                    if (documentacion_id != null && documentacion_id != string.Empty)
                    {
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

                            if (documentacion_id != string.Empty)
                                annotation.Add("objectid_new_documentacionporsgr@odata.bind", "/new_documentacionporsgrs(" + documentacion_id + ")");

                            nota_id = api.CreateRecord("annotations", annotation, credenciales);

                            if (nota_id == "ERROR" || nota_id == "")
                            {
                                return BadRequest(nota_id);
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
        [Route("api/portalcasfog/indicadorsgr")]
        public async Task<IActionResult> IndicadorMensual([FromBody] IndicadorMensualSGR indicadorSGR)
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

                JObject IndicadorSGR = new()
                {
                    { "new_SGR@odata.bind", "/new_sgrs(" + indicadorSGR.new_sgr + ")" }
                };

                if (indicadorSGR.new_fechahasta != null && indicadorSGR.new_fechahasta != string.Empty)
                    //IndicadorSGR.Add("new_fechahasta", indicadorSGR.new_fechahasta);
                    IndicadorSGR.Add("new_fechahasta", DateTime.Parse(indicadorSGR.new_fechahasta).ToString("yyyy-MM-dd"));

                //INDICADORES MENSUALES(Fuente SGR)
                if (indicadorSGR.new_saldonetodegarantiasvigentes > 0)
                    IndicadorSGR.Add("new_saldonetodegarantiasvigentes", indicadorSGR.new_saldonetodegarantiasvigentes);

                if (indicadorSGR.new_solvencia > 0)
                    IndicadorSGR.Add("new_solvencia", indicadorSGR.new_solvencia);

                if (indicadorSGR.new_fondoderiesgointegrado > 0)
                    IndicadorSGR.Add("new_fondoderiesgointegrado", indicadorSGR.new_fondoderiesgointegrado);

                if (indicadorSGR.new_fondoderiesgodisponible > 0)
                    IndicadorSGR.Add("new_fondoderiesgodisponible", indicadorSGR.new_fondoderiesgodisponible);

                if (indicadorSGR.new_fondoderiesgocontingente > 0)
                    IndicadorSGR.Add("new_fondoderiesgocontingente", indicadorSGR.new_fondoderiesgocontingente);

                if (indicadorSGR.new_fondoderiesgoavalordemercado > 0)
                    IndicadorSGR.Add("new_fondoderiesgoavalordemercado", indicadorSGR.new_fondoderiesgoavalordemercado);

                //RIESGO MERCADO DE CAPITALES POR ENTIDAD DE GARANTÍA
                if (indicadorSGR.new_porcentajeriesgopropio > 0)
                    IndicadorSGR.Add("new_porcentajeriesgopropio", indicadorSGR.new_porcentajeriesgopropio);

                if (indicadorSGR.new_porcentajeriesgoterceros > 0)
                    IndicadorSGR.Add("new_porcentajeriesgoterceros", indicadorSGR.new_porcentajeriesgoterceros);

                //COMPOSICION CONTRAGARANTIAS SEGUN PYMES CON GARANTIAS VIGENTES
                if (indicadorSGR.new_porcentajeprenda > 0)
                    IndicadorSGR.Add("new_porcentajeprenda", indicadorSGR.new_porcentajeprenda);

                if (indicadorSGR.new_porcentajehipoteca > 0)
                    IndicadorSGR.Add("new_porcentajehipoteca", indicadorSGR.new_porcentajehipoteca);

                if (indicadorSGR.new_porcentajefianza > 0)
                    IndicadorSGR.Add("new_porcentajefianza", indicadorSGR.new_porcentajefianza);

                if (indicadorSGR.new_porcentajeotras > 0)
                    IndicadorSGR.Add("new_porcentajeotras", indicadorSGR.new_porcentajeotras);

                //Garantías Vigentes(riesgo vivo) CNV por Tipo de Acreedor
                if (indicadorSGR.new_entidadesfinancierascnv != null && indicadorSGR.new_entidadesfinancierascnv != string.Empty)
                    IndicadorSGR.Add("new_entidadesfinancierascnv", indicadorSGR.new_entidadesfinancierascnv);

                if (indicadorSGR.new_garantiascomercialescnv > 0)
                    IndicadorSGR.Add("new_garantiascomercialescnv", indicadorSGR.new_garantiascomercialescnv);

                if (indicadorSGR.new_garantastecnicascnv > 0)
                    IndicadorSGR.Add("new_garantastecnicascnv", indicadorSGR.new_garantastecnicascnv);

                if (indicadorSGR.new_mercadodecapitalescnv > 0)
                    IndicadorSGR.Add("new_mercadodecapitalescnv", indicadorSGR.new_mercadodecapitalescnv);

                //Garantías Vigentes(riesgo vivo) CNV por tipo de instrumento del Mercado de Capitales
                if (indicadorSGR.new_chequedepagodiferidocnv > 0)
                    IndicadorSGR.Add("new_chequedepagodiferidocnv", indicadorSGR.new_chequedepagodiferidocnv);

                if (indicadorSGR.new_pagarbursatilcnv > 0)
                    IndicadorSGR.Add("new_pagarbursatilcnv", indicadorSGR.new_pagarbursatilcnv);

                if (indicadorSGR.new_valoresdecortoplazocnv > 0)
                    IndicadorSGR.Add("new_valoresdecortoplazocnv", indicadorSGR.new_valoresdecortoplazocnv);

                if (indicadorSGR.new_obligacionesnegociablescnv > 0)
                    IndicadorSGR.Add("new_obligacionesnegociablescnv", indicadorSGR.new_obligacionesnegociablescnv);

                //Garantías Vigentes(riesgo vivo) CNV por tipo de instrumento del Mercado de Capitales
                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion1 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion1", indicadorSGR.new_garantasvigentesrvenpymesensituacion1);

                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion2 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion2", indicadorSGR.new_garantasvigentesrvenpymesensituacion2);

                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion3 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion3", indicadorSGR.new_garantasvigentesrvenpymesensituacion3);

                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion4 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion4", indicadorSGR.new_garantasvigentesrvenpymesensituacion4);

                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion5 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion5", indicadorSGR.new_garantasvigentesrvenpymesensituacion5);

                string resultado = apiDynamics.CreateRecord("new_indicadoresmensualessgrs", IndicadorSGR, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/portalcasfog/indicadorsgr")]
        public async Task<IActionResult> ActualizarIndicadorMensual([FromBody] IndicadorMensualSGR indicadorSGR)
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

                JObject IndicadorSGR = new()
                {
                    { "new_SGR@odata.bind", "/new_sgrs(" + indicadorSGR.new_sgr + ")" }
                };

                if (indicadorSGR.new_fechahasta != null && indicadorSGR.new_fechahasta != string.Empty)
                    IndicadorSGR.Add("new_fechahasta", DateTime.Parse(indicadorSGR.new_fechahasta).ToString("yyyy-MM-dd"));

                //INDICADORES MENSUALES(Fuente SGR)
                if (indicadorSGR.new_saldonetodegarantiasvigentes > 0)
                    IndicadorSGR.Add("new_saldonetodegarantiasvigentes", indicadorSGR.new_saldonetodegarantiasvigentes);

                if (indicadorSGR.new_solvencia > 0)
                    IndicadorSGR.Add("new_solvencia", indicadorSGR.new_solvencia);

                if (indicadorSGR.new_fondoderiesgointegrado > 0)
                    IndicadorSGR.Add("new_fondoderiesgointegrado", indicadorSGR.new_fondoderiesgointegrado);

                if (indicadorSGR.new_fondoderiesgodisponible > 0)
                    IndicadorSGR.Add("new_fondoderiesgodisponible", indicadorSGR.new_fondoderiesgodisponible);

                if (indicadorSGR.new_fondoderiesgocontingente > 0)
                    IndicadorSGR.Add("new_fondoderiesgocontingente", indicadorSGR.new_fondoderiesgocontingente);

                if (indicadorSGR.new_fondoderiesgoavalordemercado > 0)
                    IndicadorSGR.Add("new_fondoderiesgoavalordemercado", indicadorSGR.new_fondoderiesgoavalordemercado);

                //RIESGO MERCADO DE CAPITALES POR ENTIDAD DE GARANTÍA
                if (indicadorSGR.new_porcentajeriesgopropio > 0)
                    IndicadorSGR.Add("new_porcentajeriesgopropio", indicadorSGR.new_porcentajeriesgopropio);

                if (indicadorSGR.new_porcentajeriesgoterceros > 0)
                    IndicadorSGR.Add("new_porcentajeriesgoterceros", indicadorSGR.new_porcentajeriesgoterceros);

                //COMPOSICION CONTRAGARANTIAS SEGUN PYMES CON GARANTIAS VIGENTES
                if (indicadorSGR.new_porcentajeprenda > 0)
                    IndicadorSGR.Add("new_porcentajeprenda", indicadorSGR.new_porcentajeprenda);

                if (indicadorSGR.new_porcentajehipoteca > 0)
                    IndicadorSGR.Add("new_porcentajehipoteca", indicadorSGR.new_porcentajehipoteca);

                if (indicadorSGR.new_porcentajefianza > 0)
                    IndicadorSGR.Add("new_porcentajefianza", indicadorSGR.new_porcentajefianza);

                if (indicadorSGR.new_porcentajeotras > 0)
                    IndicadorSGR.Add("new_porcentajeotras", indicadorSGR.new_porcentajeotras);

                //Garantías Vigentes(riesgo vivo) CNV por Tipo de Acreedor
                if (indicadorSGR.new_entidadesfinancierascnv != null && indicadorSGR.new_entidadesfinancierascnv != string.Empty)
                    IndicadorSGR.Add("new_entidadesfinancierascnv", indicadorSGR.new_entidadesfinancierascnv);

                if (indicadorSGR.new_garantiascomercialescnv > 0)
                    IndicadorSGR.Add("new_garantiascomercialescnv", indicadorSGR.new_garantiascomercialescnv);

                if (indicadorSGR.new_garantastecnicascnv > 0)
                    IndicadorSGR.Add("new_garantastecnicascnv", indicadorSGR.new_garantastecnicascnv);

                if (indicadorSGR.new_mercadodecapitalescnv > 0)
                    IndicadorSGR.Add("new_mercadodecapitalescnv", indicadorSGR.new_mercadodecapitalescnv);

                //Garantías Vigentes(riesgo vivo) CNV por tipo de instrumento del Mercado de Capitales
                if (indicadorSGR.new_chequedepagodiferidocnv > 0)
                    IndicadorSGR.Add("new_chequedepagodiferidocnv", indicadorSGR.new_chequedepagodiferidocnv);

                if (indicadorSGR.new_pagarbursatilcnv > 0)
                    IndicadorSGR.Add("new_pagarbursatilcnv", indicadorSGR.new_pagarbursatilcnv);

                if (indicadorSGR.new_valoresdecortoplazocnv > 0)
                    IndicadorSGR.Add("new_valoresdecortoplazocnv", indicadorSGR.new_valoresdecortoplazocnv);

                if (indicadorSGR.new_obligacionesnegociablescnv > 0)
                    IndicadorSGR.Add("new_obligacionesnegociablescnv", indicadorSGR.new_obligacionesnegociablescnv);

                //Garantías Vigentes(riesgo vivo) CNV por tipo de instrumento del Mercado de Capitales
                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion1 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion1", indicadorSGR.new_garantasvigentesrvenpymesensituacion1);

                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion2 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion2", indicadorSGR.new_garantasvigentesrvenpymesensituacion2);

                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion3 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion3", indicadorSGR.new_garantasvigentesrvenpymesensituacion3);

                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion4 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion4", indicadorSGR.new_garantasvigentesrvenpymesensituacion4);

                if (indicadorSGR.new_garantasvigentesrvenpymesensituacion5 > 0)
                    IndicadorSGR.Add("new_garantasvigentesrvenpymesensituacion5", indicadorSGR.new_garantasvigentesrvenpymesensituacion5);

                string resultado = apiDynamics.UpdateRecord("new_indicadoresmensualessgrs", indicadorSGR.new_indicadoresmensualessgrid, IndicadorSGR, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/portalcasfog/indicadorsocioylibradores")]
        public async Task<IActionResult> CrearIndicadorSocioYLibrador([FromBody] IndicadorMensualSocioYLibradores indicadorSL)
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

                JObject IndicadorSL = new();

                if (indicadorSL.new_indicadormensualsgr != null && indicadorSL.new_indicadormensualsgr != string.Empty)
                    IndicadorSL.Add("new_indicadormensualsgr@odata.bind", "/new_indicadoresmensualessgrs(" + indicadorSL.new_indicadormensualsgr + ")");

                if (indicadorSL.new_name != null && indicadorSL.new_name != string.Empty)
                    IndicadorSL.Add("new_name", indicadorSL.new_name); 

                if (indicadorSL.new_librador != null && indicadorSL.new_librador != string.Empty)
                    IndicadorSL.Add("new_librador", indicadorSL.new_librador);

                if (indicadorSL.new_porcentajelibrador > 0)
                    IndicadorSL.Add("new_porcentajelibrador", indicadorSL.new_porcentajelibrador);

                if (indicadorSL.new_socioparticipetercero != null && indicadorSL.new_socioparticipetercero != string.Empty)
                    IndicadorSL.Add("new_socioparticipetercero", indicadorSL.new_socioparticipetercero);

                if (indicadorSL.new_porcentajesocioparticipetercero > 0)
                    IndicadorSL.Add("new_porcentajesocioparticipetercero", indicadorSL.new_porcentajesocioparticipetercero);

                if (indicadorSL.new_socioprotector != null && indicadorSL.new_socioprotector != string.Empty)
                    IndicadorSL.Add("new_socioprotector", indicadorSL.new_socioprotector);

                if (indicadorSL.new_porcentajesocioprotector > 0)
                    IndicadorSL.Add("new_porcentajesocioprotector", indicadorSL.new_porcentajesocioprotector);

                if (indicadorSL.new_fecha != null && indicadorSL.new_fecha != string.Empty)
                    IndicadorSL.Add("new_fecha", DateTime.Parse(indicadorSL.new_fecha).ToString("yyyy-MM-dd"));

                string resultado = apiDynamics.CreateRecord("new_indicadormensualsocioylibradoreses", IndicadorSL, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/portalcasfog/indicadorsocioylibradores")]
        public async Task<IActionResult> ActualizarIndicadorSocioYLibrador([FromBody] IndicadorMensualSocioYLibradores indicadorSL)
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

                JObject IndicadorSL = new();

                if (indicadorSL.new_indicadormensualsgr != null && indicadorSL.new_indicadormensualsgr != string.Empty)
                    IndicadorSL.Add("new_indicadormensualsgr@odata.bind", "/new_indicadoresmensualessgrs(" + indicadorSL.new_indicadormensualsgr + ")");

                if (indicadorSL.new_name != null && indicadorSL.new_name != string.Empty)
                    IndicadorSL.Add("new_name", indicadorSL.new_name);

                if (indicadorSL.new_librador != null && indicadorSL.new_librador != string.Empty)
                    IndicadorSL.Add("new_librador", indicadorSL.new_librador);

                if (indicadorSL.new_porcentajelibrador > 0)
                    IndicadorSL.Add("new_porcentajelibrador", indicadorSL.new_porcentajelibrador);

                if (indicadorSL.new_socioparticipetercero != null && indicadorSL.new_socioparticipetercero != string.Empty)
                    IndicadorSL.Add("new_socioparticipetercero", indicadorSL.new_socioparticipetercero);

                if (indicadorSL.new_porcentajesocioparticipetercero > 0)
                    IndicadorSL.Add("new_porcentajesocioparticipetercero", indicadorSL.new_porcentajesocioparticipetercero);

                if (indicadorSL.new_socioprotector != null && indicadorSL.new_socioprotector != string.Empty)
                    IndicadorSL.Add("new_socioprotector", indicadorSL.new_socioprotector);

                if (indicadorSL.new_porcentajesocioprotector > 0)
                    IndicadorSL.Add("new_porcentajesocioprotector", indicadorSL.new_porcentajesocioprotector);

                if (indicadorSL.new_fecha != null && indicadorSL.new_fecha != string.Empty)
                    IndicadorSL.Add("new_fecha", DateTime.Parse(indicadorSL.new_fecha).ToString("yyyy-MM-dd"));

                string resultado = apiDynamics.UpdateRecord("new_indicadormensualsocioylibradoreses", indicadorSL.new_indicadormensualsocioylibradoresid, IndicadorSL, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Serie
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/portalcasfog/adjuntosserie")]
        public async Task<IActionResult> AdjutnosSerie(string serie_id, string documento_id, string fechaVencimiento)
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
                if (serie_id == null || serie_id == string.Empty)
                    return BadRequest("El id de la serie esta vacio");

                ApiDynamicsV2 api = new ApiDynamicsV2();
                var archivos = HttpContext.Request.Form.Files;

                if (archivos.Count > 0)
                {
                    JObject adjunto = new()
                    {
                        { "new_serie@odata.bind", "/new_seriedeoperacinsindicadas(" + serie_id + ")"  },
                    };

                    if (!string.IsNullOrEmpty(documento_id))
                        adjunto.Add("new_documento@odata.bind", $"/new_documentacions({documento_id})");
                    if (!string.IsNullOrEmpty(fechaVencimiento))
                        adjunto.Add("new_fechadevencimiento", fechaVencimiento);

                    ResponseAPI adjuntosResponse = await api.CreateRecord("new_documentacionporseries", adjunto, credenciales);

                    if (adjuntosResponse.ok)
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

                            if (!string.IsNullOrEmpty(adjuntosResponse.descripcion))
                                annotation.Add("objectid_new_documentacionporserie@odata.bind", "/new_documentacionporseries(" + adjuntosResponse.descripcion + ")");

                            ResponseAPI notaResponse = await api.CreateRecord("annotations", annotation, credenciales);

                            if (!notaResponse.ok)
                            {
                                return BadRequest(notaResponse.descripcion);
                            }

                            return Ok(notaResponse.descripcion);
                        }
                    }
                    else
                    {
                        return BadRequest(adjuntosResponse.descripcion);
                    }
                }

                return Ok("OK - Falta subir archivo");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Templates
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/portalcasfog/adjuntogarantiatemplate")]
        public async Task<IActionResult> CrearAdjuntoGarantiaTemplate([FromBody] AdjuntoGarantia adjuntoGarantia)
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
                ApiDynamicsV2 api = new();
                string documento_id = string.Empty;
                JArray documentos = null;

                documentos = await ObtenerDocumentacionPorSerieYTipo(adjuntoGarantia.serie, adjuntoGarantia.tipoTemplate, api, credenciales);
                if (documentos.Count > 0)
                    documento_id = ObtenerDocumentoID(documentos[0]);

                if(documento_id == string.Empty)
                {
                    documentos = await ObtenerDocumentacionPorTipo(adjuntoGarantia.tipoTemplate, api, credenciales);
                    if (documentos.Count > 0)
                        documento_id = ObtenerDocumentoID(documentos[0]);
                }

                if (documento_id == string.Empty)
                {
                    return BadRequest("No se encontro documentacion para el template");
                }

                JObject _adjuntoGarantia = new()
                {
                    { "new_Garantia@odata.bind", "/new_garantias(" + adjuntoGarantia.garantia + ")" },
                    { "new_Documentacion@odata.bind", "/new_documentacions(" + documento_id + ")" },
                    { "new_tipo", adjuntoGarantia.tipoTemplate }
                };

                ResponseAPI responseApi = await api.CreateRecord("new_adjuntoses", _adjuntoGarantia, credenciales);

                if (!responseApi.ok)
                {
                    throw new Exception(responseApi.descripcion);
                }

                return Ok(responseApi.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/portalcasfog/actualizarTempalte")]
        public async Task<IActionResult> ActaulizarTemplate(string adjuntoGarantiaId, string notaid, bool visiblePortal)
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
                ApiDynamicsV2 api = new();
                ApiDynamics apiV1 = new();
                var archivos = HttpContext.Request.Form.Files;

                JObject _adjuntoGarantia = new()
                {
                    { "new_visibleenportal", visiblePortal }
                };

                ResponseAPI responseApi = await api.UpdateRecord("new_adjuntoses", adjuntoGarantiaId, _adjuntoGarantia, credenciales);

                if (!responseApi.ok)
                {
                    throw new Exception(responseApi.descripcion);
                }

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

                        if (!string.IsNullOrEmpty(adjuntoGarantiaId))
                            annotation.Add("objectid_new_adjuntos@odata.bind", "/new_adjuntoses(" + adjuntoGarantiaId + ")");

                        ResponseAPI notaResponse = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!notaResponse.ok)
                        {
                            return BadRequest(notaResponse.descripcion);
                        }
                    }

                    if (!string.IsNullOrEmpty(notaid))
                    {
                        apiV1.DeleteRecord("annotations", notaid, credenciales);
                    }
                }

                return Ok(responseApi.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public static async Task<JArray> ObtenerDocumentacionPorSerieYTipo(string serie, int tipoTemplate, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_documentacions";

                fetchXML = "<entity name='new_documentacion'>" +
                                "<attribute name='new_documentacionid'/> " +
                                "<attribute name='new_name'/> " +
                                "<filter type='and'>" +
                                    $"<condition attribute='new_serie' operator='eq' value='{serie}' />" +
                                    $"<condition attribute='new_tipodetemplate' operator='eq' value='{tipoTemplate}' />" +
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
        public static async Task<JArray> ObtenerDocumentacionPorTipo(int tipoTemplate, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_documentacions";

                fetchXML = "<entity name='new_documentacion'>" +
                                "<attribute name='new_documentacionid'/> " +
                                "<attribute name='new_name'/> " +
                                "<filter type='and'>" +
                                    $"<condition attribute='new_serie' operator='null'/>" +
                                    $"<condition attribute='new_tipodetemplate' operator='eq' value='{tipoTemplate}' />" +
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
        public static string ObtenerDocumentoID(JToken documentoJT)
        {
            string documento_id = string.Empty;

            DocumentoTemplate documento = JsonConvert.DeserializeObject<DocumentoTemplate>(documentoJT.ToString());
            if (documento.new_documentacionid != null)
                documento_id = documento.new_documentacionid;

            return documento_id;
        }
        #endregion
    }
}
