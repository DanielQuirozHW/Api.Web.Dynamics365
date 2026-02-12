using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Aspose.Pdf.Operators;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Net;
using System.ServiceModel.Channels;
using static Api.Web.Dynamics365.Controllers.SgrOneClickController;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;
using static Api.Web.Dynamics365.Models.Documents;
using static Api.Web.Dynamics365.Models.PortalSocioParticipe;
using static Api.Web.Dynamics365.Models.SgrOneClick;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class Casfog_SindicadasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration configuration;
        private string cliente;
        public Casfog_SindicadasController(ApplicationDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/generarsociosgrlider")]
        public async Task<IActionResult> GenerarSocioEnSgrLider([FromBody] OperacionSindicadaLider op)
        {
            Credenciales credencialesHWA = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == op.entornoExcepciones);
            Excepciones excepcion = new(credencialesHWA);
            string url_cliente = string.Empty;
            string cuit_socio = string.Empty;
            try
            {
                #region Validaciones
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
                Credenciales credencialesCliente = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == op.cliente);
                if (credencialesCliente == null)
                {
                    return BadRequest("No existen credenciales para el cliente de sgroneclick.");
                }
                url_cliente = credencialesCliente.url;
                cliente = credencialesCliente.cliente;
                if (credencialesCliente == null)
                {
                    return BadRequest("No existen credenciales para el cliente de hwa.");
                }
                #endregion
                ApiDynamicsV2 api = new();
                string socio_id = string.Empty;
                JArray resultadoGarantiaPorOP = await BuscarGarantiaYSocioV2(op.garantia_id, api, credenciales);
                if (resultadoGarantiaPorOP.Count <= 0)
                {
                    return BadRequest("No se encontro la garantia con el id: " + op.garantia_id + " en casfog.");
                }

                GarantiaV2 garantia = ArmarGarantiaV2(resultadoGarantiaPorOP);
                cuit_socio = garantia.cuenta.new_nmerodedocumento;
                Casfog_Sindicadas.Socio socio = await VerificarSocioV2(garantia.cuenta.new_nmerodedocumento, api, credencialesCliente); //Verificamos si la pyme ya existe en el sistema de la sgr.
                if (socio == null || socio.accountid == null)
                {
                    //SI EL SOCIO NO EXISTE, SE CREA LA CUENTA - RELACIONES - CONTACTOS ASOCIADOS - DOCUMENTACION - CERTIFICADOS. 
                    socio_id = await CrearSocioRelacionesDocumentosCertificados(garantia, api, credenciales, credencialesCliente, op.cuitSgr, excepcion);

                    if (string.IsNullOrEmpty(socio_id))
                    {
                        //logger.LogInformation($"Error al crear socio {nombreSocio}.");
                        await excepcion.CrearExcepcionHWAV2($"Error al crear socio {socio.accountid}.", credencialesCliente.url, op.cuitSgr, "Error al crear socio.", "WebAPI API Sindicadas");
                        return BadRequest($"Error al crear socio {socio.accountid}. {credencialesCliente.url}, {op.cuitSgr}");
                    }
                }
                else
                {
                    return BadRequest($"El socio con cuit {cuit_socio} ya existe en la sgr. No se puede crear nuevamente.");
                }

                return Ok(socio_id);
            }
            catch (Exception ex)
            {
                await excepcion.CrearExcepcionHWAV2($"Error al crear socio con cuit {cuit_socio}.", url_cliente, op.cuitSgr, ex.Message, "WebAPI Sindicadas");
                return BadRequest($"Error al crear socio con cuit {cuit_socio}. {url_cliente}, {op.cuitSgr}");
            }
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/generargarantia")]
        public async Task<IActionResult> GenerarAval([FromBody] OperacionSindicada op)
        {
            try
            {
                #region Validaciones
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
                Credenciales credencialesCliente = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == op.cliente);
                if (credencialesCliente == null)
                {
                    return BadRequest("No existen credenciales para el cliente de sgroneclick.");
                }
                cliente = credencialesCliente.cliente;
                #endregion
                ApiDynamics api = new();
                string resultado = string.Empty;
                string resultadoEnvioMonetizacion = string.Empty;
                //JArray buscarTareas = BuscarTareas(api, credenciales);

                JArray resultadoGarantiaPorOP = BuscarGarantiaYSocio(op.garantia_id, api, credenciales); //Buscamos la garantia con la pyme y sus contactos y documentos relacionados en casfog. El socio del aval.

                if (resultadoGarantiaPorOP.Count > 0)
                {
                    Casfog_Sindicadas.Garantia garantia = ArmarGarantia(resultadoGarantiaPorOP);

                    if (garantia != null && garantia.cuenta != null && garantia.cuenta.accountid != null)
                    {
                        JArray garantiaEnSgr = BuscarGarantiasEnSGR(garantia.new_garantiaid, api, credencialesCliente);

                        if (garantiaEnSgr.Count > 0)
                        {
                            GarantiaSGR garantiaSGR = ArmarGarantiaSGR(garantiaEnSgr);
                            if(garantiaSGR.new_fechadenegociacion == null)
                            {
                                resultadoEnvioMonetizacion = EnviarAMonetizarGarantiaEnSGR(op.id, api, credenciales);
                                if (resultadoEnvioMonetizacion != string.Empty && resultadoEnvioMonetizacion != "ERROR")
                                {
                                    return Ok("La garantia ya existe en la sgr. Se actualizo el flag en la OP para la monetización de la misma.");
                                }
                                else
                                {
                                    return Ok("La garantia ya existe en la sgr.");
                                }
                            }
                            else
                            {
                                return Ok("La garantia ya existe en la sgr y esta monetizada.");
                            }
                        }

                        string socio_id = VerificarSocio(garantia.cuenta.new_nmerodedocumento, api, credencialesCliente); //Verificamos si la pyme ya existe en el sistema de la sgr.

                        if (socio_id == string.Empty)
                        {
                            string tipoDocumento_id = string.Empty;
                            string actividadAfip_id = string.Empty;
                            string condicionPyme_id = string.Empty;
                            string categoria_id = string.Empty;
                            string provincia_id = string.Empty;
                            string pais_id = string.Empty;

                            JArray relacionesPorSocio = BuscarRelacionesPorSocio(garantia.cuenta.accountid, api, credenciales); //Buscamos las relaciones de vinculacion de la pyme en casfog.

                            List<Relacion> listaRelaciones = ArmarRelaciones(relacionesPorSocio);

                            if (listaRelaciones.Count > 0)
                            {
                                if (garantia.cuenta != null)
                                {
                                    garantia.cuenta.relaciones = listaRelaciones;
                                }
                            }

                            if (garantia.cuenta.tipoDocumento != null && garantia.cuenta.tipoDocumento.new_codigo != null)
                            {
                                JArray TiposDocumento = BuscarTipoDeDocumento(garantia.cuenta.tipoDocumento.new_codigo, api, credencialesCliente);
                                if (TiposDocumento.Count > 0)
                                    tipoDocumento_id = ObtenerTipoDocumentoID(TiposDocumento);
                            }

                            if (garantia.cuenta.actividadAFIP != null && garantia.cuenta.actividadAFIP.new_codigo != 0)
                            {
                                JArray ActividadesAFIP = BuscarActividadAFIP(garantia.cuenta.actividadAFIP.new_codigo, api, credencialesCliente);
                                if (ActividadesAFIP.Count > 0)
                                    actividadAfip_id = ObtenerActividadAfipID(ActividadesAFIP);
                            }

                            if (garantia.cuenta.condicionPyme != null && garantia.cuenta.condicionPyme.new_codigo != 0)
                            {
                                JArray CondicionesPyme = BuscarCondicionPyme(garantia.cuenta.condicionPyme.new_codigo, api, credencialesCliente);
                                if (CondicionesPyme.Count > 0)
                                    condicionPyme_id = ObtenerCondicionPymeID(CondicionesPyme);
                            }

                            if (garantia.cuenta.categoria != null && garantia.cuenta.categoria.new_codigo != null)
                            {
                                JArray Categorias = BuscarCategoriaCertificadoPyme(garantia.cuenta.categoria.new_codigo, api, credencialesCliente);
                                if (Categorias.Count > 0)
                                    categoria_id = ObtenerCategoriaID(Categorias);
                            }

                            if (garantia.cuenta.provincia != null && garantia.cuenta.provincia.new_codprovincia != null)
                            {
                                JArray Provincias = BuscarProvincia(garantia.cuenta.provincia.new_codprovincia, api, credencialesCliente);
                                if (Provincias.Count > 0)
                                    provincia_id = ObtenerProvinciaID(Provincias);
                            }

                            if (garantia.cuenta.pais != null && garantia.cuenta.pais.new_codpais != null)
                            {
                                JArray Paises = BuscarPais(garantia.cuenta.pais.new_codpais, api, credencialesCliente);
                                if (Paises.Count > 0)
                                    pais_id = ObtenerPaisID(Paises);
                            }

                            string resultadoSocio = CrearSocio(garantia.cuenta, api, credencialesCliente, tipoDocumento_id, actividadAfip_id,
                                condicionPyme_id, categoria_id, provincia_id, pais_id); //Creamos la cuenta.

                            if (resultadoSocio != "ERROR")
                            {
                                string firmante_id = string.Empty;
                                string contactoNotificaciones_id = string.Empty;
                                string resultadoContactoNotificaciones = string.Empty;
                                string verificarFirmante = string.Empty;
                                socio_id = resultadoSocio;

                                if (garantia.cuenta.certificados != null && garantia.cuenta.certificados.Count > 0)
                                {
                                    CrearCertificados(garantia.cuenta.certificados, socio_id, api, credencialesCliente, condicionPyme_id, categoria_id);
                                }

                                if (garantia.cuenta.contactoNotificaciones != null)
                                    resultadoContactoNotificaciones = VerificarContacto(garantia.cuenta.contactoNotificaciones.new_cuitcuil, api, credencialesCliente); //Verificamos si el contacto de notificaciones existe como contacto en el sistema de la sgr.

                                if (garantia.cuenta.firmante != null)
                                    verificarFirmante = VerificarContacto(garantia.cuenta.firmante.new_cuitcuil, api, credencialesCliente); //Verificamos si el contacto firmante existe como contacto en el sistema de la sgr.

                                if (resultadoContactoNotificaciones == string.Empty && garantia.cuenta.contactoNotificaciones != null)
                                {
                                    contactoNotificaciones_id = CrearContacto(garantia.cuenta.contactoNotificaciones, api, credencialesCliente); //Crea contacto de notificaciones en el sistema de la sgr.
                                }
                                else
                                {
                                    contactoNotificaciones_id = resultadoContactoNotificaciones;
                                }

                                if (verificarFirmante == string.Empty && garantia.cuenta.firmante != null)
                                {
                                    firmante_id = CrearContacto(garantia.cuenta.firmante, api, credencialesCliente); //Crea contacto firmante en el sistema de la sgr.
                                }
                                else
                                {
                                    firmante_id = verificarFirmante;
                                }

                                if (firmante_id != string.Empty || contactoNotificaciones_id != string.Empty)
                                {
                                    JObject actualizarCuenta = new JObject();

                                    if (contactoNotificaciones_id != string.Empty)
                                        actualizarCuenta.Add("new_ContactodeNotificaciones@odata.bind", "/contacts(" + contactoNotificaciones_id + ")");

                                    if (firmante_id != string.Empty)
                                        actualizarCuenta.Add("new_ContactoFirmante@odata.bind", "/contacts(" + firmante_id + ")");

                                    api.UpdateRecord("accounts", socio_id, actualizarCuenta, credencialesCliente); //Asociar los contactos a la pyme en el sistema de la sgr.
                                }

                                if (garantia.cuenta.relaciones != null && garantia.cuenta.relaciones.Count > 0)
                                {
                                    foreach (var relacion in garantia.cuenta.relaciones)
                                    {
                                        CrearRelacion(relacion, socio_id, api, credencialesCliente); //Crear Relaciones de Vinculacion en el sistema de la sgr.
                                    }
                                }

                                if (garantia.cuenta.documentos != null && garantia.cuenta.documentos.Count > 0)
                                {
                                    foreach (var documento in garantia.cuenta.documentos)
                                    {
                                        string documento_id = string.Empty;
                                        //Ver de matchear documentos por codigo (por el momento no son iguales entre las sgrs y casfog)
                                        JArray documentos = BuscarDocumentacionCASFOG(documento.new_codigo, api, credencialesCliente);

                                        if (documentos.Count > 0)
                                        {
                                            documento_id = ObtenerDocumentoID(documentos);
                                        }
                                        else if (documento.new_documentacionid_documento != null)
                                        {
                                            documento_id = CrearDocumento(documento, api, credencialesCliente);
                                        }

                                        CrearDocumentacionPorCuenta(documento, socio_id, documento_id, api, credencialesCliente); //Crear documentacion por cuenta en el sistema de la sgr.
                                    }
                                }
                            }
                            else
                            {
                                return BadRequest("Error al crear socio");
                            }

                        } //Si el socio no existe se crea la cuenta, las relaciones, contactos asociados y documentacion.

                        string acreedor_id = string.Empty;
                        string divisa_id = string.Empty;
                        string desembolsoAnterior_id = string.Empty;

                        if (garantia.acreedor != null && garantia.acreedor.new_cuit != null)
                        {
                            JArray Acreedores = BuscarAcreedor(garantia.acreedor.new_cuit, api, credencialesCliente);

                            if (Acreedores.Count > 0)
                                acreedor_id = ObtenerAcreedorID(Acreedores);
                            else
                                acreedor_id = CrearAcreedor(garantia.acreedor, api, credencialesCliente);
                        }

                        if (garantia.divisa != null && garantia.divisa.isocurrencycode != null)
                        {
                            JArray Divisas = BuscarDivisa(garantia.divisa.isocurrencycode, api, credencialesCliente);
                            if (Divisas.Count > 0)
                                divisa_id = ObtenerDivisaID(Divisas);
                        }

                        bool desembolsoInexistente = false;

                        if (garantia.new_condesembolsosparciales == true) //Desembolso (Buscar el desembolso anterior antes de crear la garantia y asociarla con la que vamos a crear)
                        {
                            JArray garantiasSGR = BuscarGarantiasEnSGR(garantia._new_desembolsoanterior_value, api, credencialesCliente);

                            if (garantiasSGR.Count > 0)
                            {
                                GarantiaSGR garantiaSgr = ArmarGarantiaSGR(garantiasSGR);

                                if (garantiaSgr.new_garantiaid != null)
                                {
                                    desembolsoAnterior_id = garantiaSgr.new_garantiaid;
                                }
                            }
                            else
                            {
                                garantia.new_condesembolsosparciales = false;
                                desembolsoInexistente = true;
                            }
                        }

                        string resultadoGarantia = string.Empty;

                        JArray garantiaEnSgrValidacion2 = BuscarGarantiasEnSGR(garantia.new_garantiaid, api, credencialesCliente);

                        if (garantiaEnSgrValidacion2.Count == 0)
                        {
                            //Crea Garantia
                            resultadoGarantia = CrearGarantia(garantia, op, socio_id, op.garantia_id, api, credencialesCliente,
                            acreedor_id, divisa_id, desembolsoAnterior_id);
                            if (resultadoGarantia != string.Empty && resultadoGarantia != "ERROR")
                            {
                                resultadoEnvioMonetizacion = EnviarAMonetizarGarantiaEnSGR(op.id, api, credenciales);
                            }
                        }
                        else
                        {
                            return BadRequest("Intento de duplicar garantia.");
                        }

                        if (resultadoGarantia == "ERROR")
                        {
                            return BadRequest("Error al crear garantia");
                        }
                        else if (resultadoEnvioMonetizacion != string.Empty && resultadoEnvioMonetizacion != "ERROR")
                        {
                            resultado = "Sindicada finalizada con exito. Se actualizo el flag en la OP para la monetización de la garantía.";
                        }
                        else
                        {
                            resultado = "Sindicada finalizada con exito. No se actualizo el flag en la OP para la monetización de la garantía.";
                        }
                    }
                }
                else
                {
                    resultado = "No se encontro la garantia";
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error en metodo api generar garantia en sindicada " + " | Descripción: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/monetizargarantia")]
        public async Task<IActionResult> MonetizarGarantia([FromBody] GarantiaMonetizada garantia)
        {
            try
            {
                #region Validaciones
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

                ApiDynamics api = new();
                JArray resultadoGarantiaYCuotas = BuscarGarantiasYCuotas(garantia.garantiaid, api, credenciales);

                if (resultadoGarantiaYCuotas.Count > 0)
                {
                    Casfog_Sindicadas.Garantia garantiaYCuotas = ArmarGarantiaYCuotas(resultadoGarantiaYCuotas);

                    if (garantiaYCuotas != null)
                    {
                        if (garantiaYCuotas.operacionSindicada.Count > 0)
                        {
                            Credenciales credencialesCliente = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == garantia.cliente);
                            JArray garantiasSGR = BuscarGarantiasEnSGR(garantia.garantiaid, api, credencialesCliente);
                            //JArray garantiasSGR = BuscarGarantiasEnSGR("4387f141-4b38-ed11-9db0-000d3ac16f71", api, credencialesCliente);
                            if (garantiasSGR.Count > 0)
                            {
                                GarantiaSGR garantiaSgr = ArmarGarantiaSGR(garantiasSGR);
                                if (garantiaSgr.new_garantiaid != null)
                                {
                                    string resultado = string.Empty;
                                    if (garantiaYCuotas.cuotas != null && garantiaYCuotas.cuotas.Count > 0)
                                    {
                                        decimal porcentaje = 0;
                                        decimal montoTotal = 0;
                                        decimal montoTotalAmortizacion = 0;
                                        decimal montoTotalGarantia = 0;
                                        OperacionSindicadaVinculada op = garantiaYCuotas.operacionSindicada.FirstOrDefault(x => x.new_operacionsindicadaid == garantia.operacionid);
                                        if (op != null && op.new_porcentaje != 0)
                                        {
                                            porcentaje = op.new_porcentaje;
                                            porcentaje = Math.Truncate(porcentaje * 10000) / 10000;
                                        }

                                        porcentaje = porcentaje / 100;

                                        if (op != null && op.new_importeenpesos != 0)
                                        {
                                            montoTotalGarantia = op.new_importeenpesos;
                                        }

                                        List<Cuota> listaCuotas = garantiaYCuotas.cuotas.OrderBy(x => x.new_numero).ToList();
                                        if (listaCuotas.Count > 0 && garantiaSgr.cuotas.Count == 0)
                                        {
                                            decimal montoCuota;
                                            decimal montoAmortizacion;
                                            bool errorCuota = false;
                                            foreach (var cuota in listaCuotas)
                                            {
                                                montoCuota = 0;
                                                montoAmortizacion = 0;
                                                montoCuota = cuota.new_montocuota * porcentaje;
                                                montoCuota = Math.Truncate(montoCuota * 10000) / 10000;
                                                montoAmortizacion = cuota.new_amortizacion * porcentaje;
                                                montoAmortizacion = Math.Truncate(montoAmortizacion * 10000) / 10000;
                                                montoTotal += montoCuota;
                                                montoTotalAmortizacion += montoAmortizacion;

                                                if (cuota == listaCuotas.Last())
                                                {
                                                    if (montoTotalAmortizacion != montoTotalGarantia)
                                                    {
                                                        if (montoTotalAmortizacion > montoTotalGarantia)
                                                        {
                                                            decimal montoExcedente = montoTotalAmortizacion - montoTotalGarantia;
                                                            montoAmortizacion = montoAmortizacion - montoExcedente;
                                                        }
                                                        else
                                                        {
                                                            decimal montoExcedente = montoTotalGarantia - montoTotalAmortizacion;
                                                            montoAmortizacion = montoAmortizacion + montoExcedente;
                                                        }
                                                    }
                                                }

                                                string resultadoCuota = CrearCuota(cuota, montoCuota, porcentaje, montoAmortizacion, garantiaSgr.new_garantiaid, api, credencialesCliente);

                                                if(resultadoCuota == "ERROR")
                                                {
                                                    errorCuota = true;
                                                }
                                            }

                                            string final = resultado;
                                            if (garantiaYCuotas.new_fechadenegociacion != null && !errorCuota)
                                            {
                                                Monetizar(garantiaYCuotas, garantiaSgr.new_garantiaid, api, credencialesCliente);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return Ok("Monetizacion finalizada");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/buscargarantiamonetizada")]
        public async Task<IActionResult> BuscarGarantiaMonetizada([FromBody] BuscarGarantiaMonetizada garantia)
        {
            try
            {
                #region Validaciones y Credenciales
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

                Credenciales credencialesCliente = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == garantia.cliente);
                #endregion

                ApiDynamics api = new();
                JArray resultadoGarantiaYCuotas = BuscarGarantiasYCuotasMonetizadas(garantia.garantiaid, api, credencialesCliente);

                if (resultadoGarantiaYCuotas.Count > 0)
                {
                    Casfog_Sindicadas.Garantia garantiaYCuotas = ArmarGarantiaYCuotas(resultadoGarantiaYCuotas);

                    if (garantiaYCuotas != null)
                    {
                        if (garantiaYCuotas.operacionSindicada.Count > 0)
                        {
                            JArray garantiasSGR = BuscarGarantiasEnSGR(garantia.garantiaid, api, credenciales);
                            //JArray garantiasSGR = BuscarGarantiasEnSGR("4387f141-4b38-ed11-9db0-000d3ac16f71", api, credencialesCliente);
                            if (garantiasSGR.Count > 0)
                            {
                                GarantiaSGR garantiaSgr = ArmarGarantiaSGR(garantiasSGR);
                                if (garantiaSgr.new_garantiaid != null)
                                {
                                    string resultado = string.Empty;
                                    //Limite Particular

                                    string resultadolimiteparticular = CrearLimite(garantia.socioid, garantiaYCuotas.cuenta.name, 12, garantiaYCuotas.new_monto, garantiaYCuotas.new_fechadeorigen, garantiaYCuotas.new_fechadevencimiento,
                                        garantiaYCuotas.new_nroexpedientetad, api, credenciales, garantiaYCuotas.new_monto_formateado);
                                    //Ver logica de validacion
                                    if (garantiaYCuotas.cuotas != null && garantiaYCuotas.cuotas.Count > 0)
                                    {
                                        decimal porcentaje = 0;
                                        decimal montoTotal = 0;
                                        decimal montoTotalAmortizacion = 0;
                                        decimal montoTotalGarantia = 0;
                                        OperacionSindicadaVinculada op = garantiaYCuotas.operacionSindicada.FirstOrDefault(x => x.new_credencialapi == credenciales.cliente);
                                        if (op != null && op.new_porcentaje != 0)
                                        {
                                            porcentaje = op.new_porcentaje;
                                            porcentaje = Math.Truncate(porcentaje * 10000) / 10000;
                                        }

                                        porcentaje = porcentaje / 100;

                                        if (op != null && op.new_importeenpesos != 0)
                                        {
                                            montoTotalGarantia = op.new_importeenpesos;
                                        }

                                        List<Cuota> listaCuotas = garantiaYCuotas.cuotas.OrderBy(x => x.new_numero).ToList();
                                        if (listaCuotas.Count > 0 && garantiaSgr.cuotas.Count == 0)
                                        {
                                            decimal montoCuota;
                                            decimal montoAmortizacion;
                                            foreach (var cuota in listaCuotas)
                                            {
                                                montoCuota = 0;
                                                montoAmortizacion = 0;
                                                montoCuota = cuota.new_montocuota * porcentaje;
                                                montoCuota = Math.Truncate(montoCuota * 10000) / 10000;
                                                montoAmortizacion = cuota.new_amortizacion * porcentaje;
                                                montoAmortizacion = Math.Truncate(montoAmortizacion * 10000) / 10000;
                                                montoTotal += montoCuota;
                                                montoTotalAmortizacion += montoAmortizacion;

                                                if (cuota == listaCuotas.Last())
                                                {
                                                    if (montoTotalAmortizacion != montoTotalGarantia)
                                                    {
                                                        if (montoTotalAmortizacion > montoTotalGarantia)
                                                        {
                                                            decimal montoExcedente = montoTotalAmortizacion - montoTotalGarantia;
                                                            montoAmortizacion = montoAmortizacion - montoExcedente;
                                                        }
                                                        else
                                                        {
                                                            decimal montoExcedente = montoTotalGarantia - montoTotalAmortizacion;
                                                            montoAmortizacion = montoAmortizacion + montoExcedente;
                                                        }
                                                    }
                                                }

                                                CrearCuota(cuota, montoCuota, porcentaje, montoAmortizacion, garantiaSgr.new_garantiaid, api, credenciales);
                                            }
                                        }
                                    }
                                    if (garantiaYCuotas.new_fechadenegociacion != null)
                                    {
                                        Monetizar(garantiaYCuotas, garantiaSgr.new_garantiaid, api, credenciales);
                                    }
                                }
                            }
                        }
                    }
                }

                return Ok("Monetizacion finalizada");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/reprocesargarantia")]
        public async Task<IActionResult> ReprocesarGarantia([FromBody] GarantiaReprocesada garantia)
        {
            try
            {
                #region Validaciones
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

                Credenciales credencialesCliente = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == garantia.cliente);
                if (credencialesCliente == null)
                {
                    return BadRequest("No existen credenciales para el cliente de sgroneclick.");
                }
                cliente = credencialesCliente.cliente;
                #endregion
                ApiDynamics api = new();
                string resultado = string.Empty;

                JArray garantiasSGR = BuscarGarantiasEnSGR(garantia.garantiaid, api, credencialesCliente);
                
                if (garantiasSGR.Count > 0)
                {
                    resultado = "Garantia Encontrada - ";
                    GarantiaSGR garantiaSgr = ArmarGarantiaSGR(garantiasSGR);

                    //decimal porcentaje = 0;
                    decimal montoAvaladoSGR = 0;

                    //if (garantia.porcentaje != 0)
                    //{
                    //    porcentaje = garantia.porcentaje;
                    //    porcentaje = Math.Truncate(porcentaje * 10000) / 10000;
                    //}

                    //porcentaje = porcentaje / 100;

                    montoAvaladoSGR = garantia.montoDeLaGarantia;

                    if (garantiaSgr.new_garantiaid != null && montoAvaladoSGR != 0)
                    {
                        resultado += "Actualizando Garantia - ";
                        resultado += ActualizarGarantia(garantiaSgr.new_garantiaid, montoAvaladoSGR, api, credencialesCliente);
                    }
                }
                else
                {
                    resultado = "Garantia no encontrada";
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/crearmensajemasivo")]
        public async Task<IActionResult> CrearMensajeColaAzureMasivo([FromBody] MensajeAzureMasivo mensaje)
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
                List<OPYSGR> OPYSGR = new();
                JArray OPXGarantia = await BuscarOperacionesSindicadas(mensaje.garantia_id, api, credenciales);
                if(OPXGarantia.Count > 0)
                {
                    OPYSGR = ArmarOPYSGR(OPXGarantia);
                }
                else
                {
                    return BadRequest("No se encontraron Operaciones Sindicadas para esa garantia");
                }


                for (int i = 0; i < OPYSGR.Count; i++)
                {
                    Credenciales credencialesCliente = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == OPYSGR[i].new_credencialapi);
                    if (credencialesCliente == null)
                        continue;

                    string cuitSGR = await BuscarCUITSgr(api, credencialesCliente);

                    MensajeColaWebJOBSindicadas mensajeCWJ = new()
                    {
                        credenciales = credenciales,
                        credencialesCliente = credencialesCliente,
                        cuitSGR = cuitSGR,
                        garantia_id = mensaje.garantia_id,
                        operacion_id = OPYSGR[i].new_operacionsindicadaid,
                        importeAvalado = OPYSGR[i].new_importeenpesos
                    };

                    string mensajeCWJSTR = JsonConvert.SerializeObject(mensajeCWJ);
                    string conexionAzureStorage = configuration["StorageConnectionString"];

                    QueueClient queue = new(conexionAzureStorage,
                        "apisindicadas", new QueueClientOptions
                        {
                            MessageEncoding = QueueMessageEncoding.Base64,
                        });

                    var respuesta = await queue.SendMessageAsync(mensajeCWJSTR);
                }

                return Ok($"Mensajes creados con exito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/crearmensaje")]
        public async Task<IActionResult> CrearMensajeColaAzure([FromBody] MensajeAzure mensaje)
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
                List<OPYSGR> OPYSGR = new();
                JArray OPXGarantia = await BuscarOperacionesSindicadas(mensaje.garantia_id, api, credenciales);
                if (OPXGarantia.Count > 0)
                {
                    OPYSGR = ArmarOPYSGR(OPXGarantia);
                }
                else
                {
                    return BadRequest("No se encontraron Operaciones Sindicadas para esa garantia. Asegurese que este adherida.");
                }

                OPYSGR opSeleccionada = OPYSGR.FirstOrDefault(x => x.new_operacionsindicadaid == mensaje.operacion_id);
                if (opSeleccionada == null)
                    return BadRequest("No se encontraro la operacion sindicada.");

                Credenciales credencialesCliente = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == opSeleccionada.new_credencialapi);
                if (credencialesCliente == null)
                    return BadRequest("No se encontraron las credenciales del cliente en la base de datos. Revise que el nombre de la conexion exista en la sgr");

                string cuitSGR = await BuscarCUITSgr(api, credencialesCliente);

                MensajeColaWebJOBSindicadas mensajeCWJ = new()
                {
                    credenciales = credenciales,
                    credencialesCliente = credencialesCliente,
                    cuitSGR = cuitSGR,
                    garantia_id = mensaje.garantia_id,
                    operacion_id = opSeleccionada.new_operacionsindicadaid,
                    importeAvalado = opSeleccionada.new_importeenpesos
                };

                string mensajeCWJSTR = JsonConvert.SerializeObject(mensajeCWJ);
                string conexionAzureStorage = configuration["StorageConnectionString"];

                QueueClient queue = new(conexionAzureStorage,
                    "apisindicadas", new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });

                var respuesta = await queue.SendMessageAsync(mensajeCWJSTR);

                return Ok($"Mensajes creados con exito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/crearmensajedocumentacionsindicada")]
        public async Task<IActionResult> CrearMensajeDocumentacionColaAzure([FromBody] MensajeAzure mensaje)
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
                List<OPYSGR> OPYSGR = new();
                JArray OPXGarantia = await BuscarOperacionesSindicadas(mensaje.garantia_id, api, credenciales);
                if (OPXGarantia.Count > 0)
                {
                    OPYSGR = ArmarOPYSGR(OPXGarantia);
                }
                else
                {
                    return BadRequest("No se encontraron Operaciones Sindicadas para esa garantia. Asegurese que este adherida.");
                }

                OPYSGR opSeleccionada = OPYSGR.FirstOrDefault(x => x.new_operacionsindicadaid == mensaje.operacion_id);
                if (opSeleccionada == null)
                    return BadRequest("No se encontraro la operacion sindicada.");

                Credenciales credencialesCliente = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == opSeleccionada.new_credencialapi);
                if (credencialesCliente == null)
                    return BadRequest("No se encontraron las credenciales del cliente en la base de datos. Revise que el nombre de la conexion exista en la sgr");

                string cuitSGR = await BuscarCUITSgr(api, credencialesCliente);

                MensajeColaWebJOBSindicadas mensajeCWJ = new()
                {
                    credenciales = credenciales,
                    credencialesCliente = credencialesCliente,
                    cuitSGR = cuitSGR,
                    garantia_id = mensaje.garantia_id,
                    operacion_id = opSeleccionada.new_operacionsindicadaid,
                    importeAvalado = opSeleccionada.new_importeenpesos
                };

                string mensajeCWJSTR = JsonConvert.SerializeObject(mensajeCWJ);
                string conexionAzureStorage = configuration["StorageConnectionString"];

                QueueClient queue = new(conexionAzureStorage,
                    "documentosindicadas", new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });

                var respuesta = await queue.SendMessageAsync(mensajeCWJSTR);

                return Ok($"Mensajes creados con exito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //OBSOLETOS
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/monetizargarantiamasivo")]
        public async Task<IActionResult> MonetizarGarantiaMasivo([FromBody] GarantiaMonetizada garantia)
        {
            try
            {
                #region Validaciones
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

                ApiDynamics api = new();
                JArray resultadoGarantiaYCuotas = BuscarGarantiasYCuotas(garantia.garantiaid, api, credenciales);

                if (resultadoGarantiaYCuotas.Count > 0)
                {
                    Casfog_Sindicadas.Garantia garantiaYCuotas = ArmarGarantiaYCuotas(resultadoGarantiaYCuotas);

                    if (garantiaYCuotas != null)
                    {
                        if (garantiaYCuotas.operacionSindicada.Count > 0)
                        {
                            foreach (var op in garantiaYCuotas.operacionSindicada)
                            {
                                if (op.new_clientesgroneclick == true)
                                {
                                    Credenciales credencialesCliente = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == op.new_credencialapi);
                                    JArray garantiasSGR = BuscarGarantiasEnSGR(garantia.garantiaid, api, credencialesCliente);
                                    if (garantiasSGR.Count > 0)
                                    {
                                        GarantiaSGR garantiaSgr = ArmarGarantiaSGR(garantiasSGR);

                                        if (garantiaSgr.new_garantiaid != null)
                                        {
                                            if (garantiaYCuotas.cuotas != null && garantiaYCuotas.cuotas.Count > 0)
                                            {
                                                decimal porcentaje = op.new_porcentaje;
                                                foreach (var cuota in garantiaYCuotas.cuotas)
                                                {
                                                    //string resultadoCuota = CrearCuota(cuota, porcentaje, garantiaSgr.new_garantiaid, api, credencialesCliente);
                                                }
                                            }

                                            if (garantiaYCuotas.new_fechadenegociacion != null)
                                            {
                                                string resultadoMonetizacion = Monetizar(garantiaYCuotas, garantiaSgr.new_garantiaid, api, credencialesCliente);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/casfog/generardesembolso")]
        public async Task<IActionResult> GenerarDesembolso([FromBody] Desembolso garantia)
        {
            try
            {
                #region Validaciones
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

                ApiDynamics api = new();
                JArray resultadoGarantiaYCuotas = BuscarGarantiasYCuotas(garantia.garantiaid, api, credenciales);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //############NUEVA VERSION METODOS SINDICADAS
        #region MetodosSIndicadasV2
        public static async Task<JArray> BuscarGarantiaYSocioV2(string garantia_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_garantias";

                fetchXML = "<entity name='new_garantia'>" +
                            "<attribute name='new_tipodeoperacion'/> " +
                            "<attribute name='new_tipodegarantias'/> " +
                            "<attribute name='new_socioparticipe'/>" +
                            "<attribute name='new_saldovigente'/> " +
                            "<attribute name='new_ndeordendelagarantiaotorgada'/> " +
                            "<attribute name='new_monto'/> " +
                            "<attribute name='new_fechadevencimiento'/> " +
                            "<attribute name='new_fechadeorigen'/> " +
                            "<attribute name='new_fechadenegociacion'/> " +
                            "<attribute name='statuscode' /> " +
                            "<attribute name='transactioncurrencyid'/> " +
                            "<attribute name='new_codigocvba'/> " +
                            "<attribute name='new_acreedor'/> " +
                            "<attribute name='new_garantiaid'/> " +
                            "<attribute name='new_saldocreditoprincipal' /> " +
                            "<attribute name='new_saldocuotasoperativo'/> " +
                            "<attribute name='new_saldocuotasvigentes'/> " +
                            "<attribute name='new_saldodelaamortizacion'/> " +
                            "<attribute name='new_cantidadcuotasafrontadas'/> " +
                            "<attribute name='new_desembolsoanterior'/> " +
                            "<attribute name='new_condesembolsosparciales'/> " +
                            "<attribute name='new_fechaemisindelcheque'/> " +
                            "<attribute name='new_sistemadeamortizacion'/> " +
                            "<attribute name='new_periodicidadpagos'/> " +
                            "<attribute name='new_fechaemisindelcheque'/> " +
                            "<attribute name='new_tasa'/> " +
                            "<attribute name='new_puntosporcentuales'/> " +
                            "<attribute name='new_nroexpedientetad'/> " +
                            "<attribute name='new_superatasabadlar'/> " +
                            "<attribute name='new_tasabadlar'/> " +
                            "<attribute name='new_tasabarancaria'/> " +
                            "<attribute name='new_observaciones'/> " +
                            "<filter type='and'>" +
                                $"<condition attribute='new_garantiaid' operator='eq' value='{garantia_id}' />" +
                            "</filter>" +
                            "<link-entity name='new_acreedor' from='new_acreedorid' to='new_acreedor' link-type='outer' alias='acreedor'>" +
                                "<attribute name='new_name'/> " +
                                "<attribute name='new_cuit'/> " +
                                "<attribute name='new_tipodeacreedor'/> " +
                            "</link-entity>" +
                            "<link-entity name='transactioncurrency' from='transactioncurrencyid' to='transactioncurrencyid' link-type='outer' alias='divisa'>" +
                                "<attribute name='isocurrencycode'/> " +
                            "</link-entity>" +
                            "<link-entity name='account' from='accountid' to='new_socioparticipe' link-type='outer' alias='cuenta'>" +
                                "<attribute name='accountid'/> " +
                                "<attribute name='name'/>" +
                                "<attribute name='primarycontactid'/> " +
                                "<attribute name='new_nmerodedocumento'/> " +
                                "<attribute name='new_personeria'/> " +
                                "<attribute name='new_rol'/> " +
                                "<attribute name='new_tipodedocumentoid'/> " +
                                "<attribute name='new_productoservicio'/> " +
                                "<attribute name='new_tiposocietario'/> " +
                                "<attribute name='new_condicionimpositiva'/> " +
                                "<attribute name='emailaddress1'/> " +
                                "<attribute name='statuscode'/> " +
                                "<attribute name='new_actividadafip'/> " +
                                "<attribute name='new_facturacionultimoanio'/> " +
                                "<attribute name='new_fechadealta'/> " +
                                "<attribute name='new_onboarding'/> " +
                                "<attribute name='new_essoloalyc'/> " +
                                "<attribute name='new_estadodelsocio'/> " +
                                "<attribute name='telephone2'/> " +
                                "<attribute name='address1_line1'/> " +
                                "<attribute name='new_direccion1numero'/> " +
                                "<attribute name='address1_name'/> " +
                                "<attribute name='new_direccion1depto'/> " +
                                "<attribute name='address1_postalcode'/> " +
                                "<attribute name='address1_county'/> " +
                                "<attribute name='new_localidad'/> " +
                                "<attribute name='new_calificacion'/> " +
                                "<attribute name='new_estadodeactividad'/> " +
                                "<attribute name='new_tipodeasociacion'/> " +
                                "<link-entity name='contact' from='contactid' to='new_contactodenotificaciones' link-type='outer' alias='contacto'>" +
                                    "<attribute name='firstname'/> " +
                                    "<attribute name='lastname'/> " +
                                    "<attribute name='new_cuitcuil'/> " +
                                    "<attribute name='contactid'/> " +
                                "</link-entity>" +
                                "<link-entity name='contact' from='contactid' to='new_contactofirmante' link-type='outer' alias='contactoFirmante'>" +
                                    "<attribute name='firstname'/> " +
                                    "<attribute name='lastname'/> " +
                                    "<attribute name='new_cuitcuil'/> " +
                                    "<attribute name='contactid'/> " +
                                "</link-entity>" +
                                "<link-entity name='new_documentacionporcuenta' from='new_cuentaid' to='accountid' link-type='outer' alias='documentacion'>" +
                                        "<attribute name='new_documentacionporcuentaid'/> " +
                                        "<attribute name='new_cuentaid'/> " +
                                        "<attribute name='new_documentoid'/> " +
                                        "<attribute name='statuscode'/> " +
                                        "<attribute name='new_vinculocompartido'/> " +
                                        "<attribute name='new_fechadevencimiento'/> " +
                                        "<link-entity name='new_documentacion' from='new_documentacionid' to='new_documentoid' link-type='outer' alias='documento'>" +
                                            "<attribute name='new_codigo'/> " +
                                            "<attribute name='new_documentacionid'/> " +
                                            "<attribute name='new_urlplantilla'/> " +
                                            "<attribute name='new_tipodefiador'/> " +
                                            "<attribute name='new_personeria'/> " +
                                            "<attribute name='new_grupoeconomico'/> " +
                                            "<attribute name='new_fiador'/> " +
                                            "<attribute name='new_name'/> " +
                                            "<attribute name='new_estadodelsocio'/> " +
                                            "<attribute name='new_descripcion'/> " +
                                            "<attribute name='new_condicionimpositiva'/> " +
                                        "</link-entity>" +
                                "</link-entity>" +
                                "<link-entity name='new_certificadopyme' from='new_socioparticipe' to='accountid' link-type='outer' alias='certificado'>" +
                                        "<attribute name='new_certificadopymeid'/> " +
                                        "<attribute name='new_numeroderegistro'/> " +
                                        "<attribute name='new_fechadeemision'/> " +
                                        "<attribute name='new_categoria'/> " +
                                        "<attribute name='new_sectoreconomico'/> " +
                                        "<attribute name='new_vigenciadesde'/> " +
                                        "<attribute name='new_vigenciahasta'/> " +
                                        "<attribute name='statecode'/> " +
                                "</link-entity>" +
                                "<link-entity name='new_actividadafip' from='new_actividadafipid' to='new_actividadafip' link-type='outer' alias='actividadAfip'>" +
                                    "<attribute name='new_codigo'/> " +
                                "</link-entity>" +
                                "<link-entity name='new_tipodedocumento' from='new_tipodedocumentoid' to='new_tipodedocumentoid' link-type='outer' alias='tipoDocumento'>" +
                                    "<attribute name='new_codigo'/> " +
                                    "<attribute name='new_tipodedocumentoid'/> " +
                                "</link-entity>" +
                                "<link-entity name='new_condicionpyme' from='new_condicionpymeid' to='new_condicionpyme' link-type='outer' alias='condicionPyme'>" +
                                    "<attribute name='new_codigo'/> " +
                                "</link-entity>" +
                                "<link-entity name='new_categoracertificadopyme' from='new_categoracertificadopymeid' to='new_categoria' link-type='outer' alias='categoria'>" +
                                    "<attribute name='new_codigo'/> " +
                                    "<attribute name='new_name'/> " +
                                "</link-entity>" +
                                "<link-entity name='new_pais' from='new_paisid' to='new_pais' link-type='outer' alias='pais'>" +
                                    "<attribute name='new_codpais'/> " +
                                "</link-entity>" +
                                "<link-entity name='new_provincia' from='new_provinciaid' to='new_provincia' link-type='outer' alias='provincia'>" +
                                    "<attribute name='new_codprovincia'/> " +
                                "</link-entity>" +
                            "</link-entity>" +
                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }
        public static async Task<JArray> BuscarRelacionesPorSocioV2(string cuit, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_participacionaccionarias";

                fetchXML = "<entity name='new_participacionaccionaria'>" +
                                "<attribute name='new_participacionaccionariaid'/> " +
                                "<attribute name='new_name'/>" +
                                "<attribute name='createdon'/> " +
                                "<attribute name='new_tipoderelacion'/> " +
                                "<attribute name='new_porcentajedeparticipacion'/> " +
                                "<attribute name='new_observaciones'/> " +
                                "<attribute name='new_cuentacontactovinculado'/> " +
                                "<attribute name='new_porcentajebeneficiario'/> " +
                                "<filter type='and'>" +
                                $"<condition attribute='new_cuentaid' operator='eq' value='{cuit}' />" +
                                "</filter>" +
                                "<link-entity name='contact' from='contactid' to='new_cuentacontactovinculado' link-type='outer' alias='contacto'>" +
                                        "<attribute name='firstname'/> " +
                                        "<attribute name='lastname'/> " +
                                        "<attribute name='new_cuitcuil'/> " +
                                        "<attribute name='contactid'/> " +
                                "</link-entity>" +
                                "<link-entity name='account' from='accountid' to='new_cuentacontactovinculado' link-type='outer' alias='cuenta'>" +
                                        "<attribute name='accountid'/> " +
                                        "<attribute name='name'/> " +
                                        "<attribute name='new_nmerodedocumento'/> " +
                                        "<attribute name='new_tipodedocumentoid'/> " +
                                        "<attribute name='new_personeria'/> " +
                                        "<attribute name='new_rol'/> " +
                                        "<attribute name='emailaddress1'/> " +
                                "</link-entity>" +
                        "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }
        public static async Task<JArray> BuscarTipoDeDocumentoV2(string codigo, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_tipodedocumentos";

                fetchXML = "<entity name='new_tipodedocumento'>" +
                                                           "<attribute name='new_tipodedocumentoid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<attribute name='new_codigo'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codigo' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarActividadAFIPV2(int codigo, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_actividadafips";

                fetchXML = "<entity name='new_actividadafip'>" +
                                                           "<attribute name='new_actividadafipid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codigo' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarCategoriaCertificadoPymeV2(string nombre, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_categoracertificadopymes";

                fetchXML = "<entity name='new_categoracertificadopyme'>" +
                                                           "<attribute name='new_categoracertificadopymeid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_name' operator='eq' value='{nombre}' />" +
                                                           "</filter>" +
                                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarCondicionPymeV2(int codigo, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_condicionpymes";

                fetchXML = "<entity name='new_condicionpyme'>" +
                                                           "<attribute name='new_condicionpymeid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codigo' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarProvinciaV2(string codigo, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_provincias";

                fetchXML = "<entity name='new_provincia'>" +
                                                           "<attribute name='new_provinciaid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codprovincia' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarPaisV2(string codigo, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_paises";

                fetchXML = "<entity name='new_pais'>" +
                                                           "<attribute name='new_paisid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codpais' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarDocumentacionCASFOGV2(ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_documentacions";

                fetchXML = "<entity name='new_documentacion'>" +
                                                           "<attribute name='new_documentacionid'/> " +
                                                           "<attribute name='new_codigocasfog'/> " +
                                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }
        public static JArray BuscarOperacionesSindicadas2(string garantia_id, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_operacionsindicadas";

                fetchXML = "<fetch mapping='logical'>" +
                                        "<entity name='new_operacionsindicada'>" +
                                                           "<attribute name='new_operacionsindicadaid'/> " +
                                                           "<attribute name='new_garantiamonetizada'/> " +
                                                           "<attribute name='new_importeenpesos'/> " +
                                                           "<filter type='and'>" +
                                                           $"<condition attribute='new_garantia' operator='eq' value='{garantia_id}' />" +
                                                           "</filter>" +
                                                            "<link-entity name='new_sgr' from='new_sgrid' to='new_sgr' link-type='inner' alias='sgr'>" +
                                                                "<attribute name='new_credencialapi'/> " +
                                                                 "<filter type='and'>" +
                                                                 "<condition attribute='new_clientesgroneclick' operator='eq' value='1' />" +
                                                                 "</filter>" +
                                                           "</link-entity>" +
                                                "</entity>" +
                                         "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static GarantiaV2 ArmarGarantiaV2(JToken cuentaJT)
        {
            GarantiaV2 garantia = new();
            CuentaV2 cuenta = new();
            Contacto contactoNotificaciones = new();
            ContactoFirmante firmante = new();
            List<Documentacion> listaDocumentos = new();
            List<CertificadoPymeV2> listaCertificados = new();
            TipoDocumentoVinculado tipoDocumento = new();
            ActividadAFIPVinculado actividadAFIP = new();
            CondicionPymeVinculado condicionPyme = new();
            CategoriaVinculado categoria = new();
            AcreedorVinculado acreedor = new();
            DivisaVinculada divisa = new();
            ProvinciaVinculada provincia = new();
            PaisVinculado pais = new();

            garantia = JsonConvert.DeserializeObject<GarantiaV2>(cuentaJT.First.ToString());
            acreedor = JsonConvert.DeserializeObject<AcreedorVinculado>(cuentaJT.First.ToString());
            divisa = JsonConvert.DeserializeObject<DivisaVinculada>(cuentaJT.First.ToString());
            cuenta = JsonConvert.DeserializeObject<CuentaV2>(cuentaJT.First.ToString());
            contactoNotificaciones = JsonConvert.DeserializeObject<Contacto>(cuentaJT.First.ToString());
            firmante = JsonConvert.DeserializeObject<ContactoFirmante>(cuentaJT.First.ToString());
            listaDocumentos = JsonConvert.DeserializeObject<List<Documentacion>>(cuentaJT.ToString());
            listaCertificados = JsonConvert.DeserializeObject<List<CertificadoPymeV2>>(cuentaJT.ToString());
            listaDocumentos = listaDocumentos.GroupBy(x => x.new_documentacionporcuentaid).Select(g => g.First()).ToList();
            listaDocumentos.RemoveAll(x => x.new_documentacionporcuentaid == null);
            listaCertificados = listaCertificados.GroupBy(x => x.new_certificadopymeid).Select(g => g.First()).ToList();
            listaCertificados.RemoveAll(x => x.new_certificadopymeid == null);

            tipoDocumento = JsonConvert.DeserializeObject<TipoDocumentoVinculado>(cuentaJT.First.ToString());
            actividadAFIP = JsonConvert.DeserializeObject<ActividadAFIPVinculado>(cuentaJT.First.ToString());
            condicionPyme = JsonConvert.DeserializeObject<CondicionPymeVinculado>(cuentaJT.First.ToString());
            categoria = JsonConvert.DeserializeObject<CategoriaVinculado>(cuentaJT.First.ToString());
            provincia = JsonConvert.DeserializeObject<ProvinciaVinculada>(cuentaJT.First.ToString());
            pais = JsonConvert.DeserializeObject<PaisVinculado>(cuentaJT.First.ToString());

            garantia.acreedor = acreedor;
            garantia.divisa = divisa;
            cuenta.contactoNotificaciones = contactoNotificaciones.contactid != null ? contactoNotificaciones : null;
            cuenta.firmante = firmante.contactid != null ? firmante : null;
            cuenta.documentos = listaDocumentos.Count > 0 ? listaDocumentos : null;
            cuenta.certificados = listaCertificados.Count > 0 ? listaCertificados : null;
            cuenta.tipoDocumento = tipoDocumento.new_codigo != null ? tipoDocumento : null;
            cuenta.actividadAFIP = actividadAFIP.new_codigo != 0 ? actividadAFIP : null;
            cuenta.condicionPyme = condicionPyme.new_codigo != 0 ? condicionPyme : null;
            cuenta.categoria = categoria.new_name != null ? categoria : null;
            cuenta.provincia = provincia.new_codprovincia != null ? provincia : null;
            cuenta.pais = pais.new_codpais != null ? pais : null;
            garantia.cuenta = cuenta;

            return garantia;
        }
        public static async Task<Casfog_Sindicadas.Socio> VerificarSocioV2(string cuit, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                Casfog_Sindicadas.Socio socio = new();
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "accounts";

                fetchXML = "<entity name='account'>" +
                            "<attribute name='accountid'/> " +
                            "<attribute name='name'/> " +
                            "<attribute name='new_estadodelsocio'/> " +
                            "<attribute name='new_tipodeasociacion'/> " +
                            "<filter type='and'>" +
                            $"<condition attribute='new_nmerodedocumento' operator='eq' value='{cuit}' />" +
                                "</filter>" +
                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                if (respuesta.Count > 0)
                    socio = JsonConvert.DeserializeObject<Casfog_Sindicadas.Socio>(respuesta.First.ToString());

                return socio;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<string> CrearContactoV2(Contacto contacto, string nombreSocio, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string contact_id = string.Empty;

                JObject contact = new();

                if (!string.IsNullOrEmpty(contacto.firstname))
                    contact.Add("firstname", contacto.firstname);

                if (!string.IsNullOrEmpty(contacto.lastname))
                    contact.Add("lastname", contacto.lastname);

                if (contacto.new_cuitcuil != 0)
                    contact.Add("new_cuitcuil", Convert.ToDecimal(contacto.new_cuitcuil));

                ResponseAPI responseAPI = await api.CreateRecord("contacts", contact, credenciales);
                if (responseAPI.ok)
                    contact_id = responseAPI.descripcion;
                else
                    throw new Exception(responseAPI.descripcion);

                return contact_id;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear contacto de notificaciones para le socio {nombreSocio} - {ex.Message}");
                throw;
            }
        }
        public static async Task<string> CrearContactoV2(ContactoFirmante contacto, string nombreSocio, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string contact_id = string.Empty;

                JObject contact = new();

                if (!string.IsNullOrEmpty(contacto.firstname))
                    contact.Add("firstname", contacto.firstname);

                if (!string.IsNullOrEmpty(contacto.lastname))
                    contact.Add("lastname", contacto.lastname);

                if (contacto.new_cuitcuil != 0)
                    contact.Add("new_cuitcuil", Convert.ToDecimal(contacto.new_cuitcuil));

                ResponseAPI responseAPI = await api.CreateRecord("contacts", contact, credenciales);
                if (responseAPI.ok)
                    contact_id = responseAPI.descripcion;
                else
                    throw new Exception(responseAPI.descripcion);

                return contact_id;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear contacto firmante para el socio {nombreSocio} - {ex.Message}");
                throw;
            }
        }
        public static async Task<string> CrearSocioV2(CuentaV2 socio, ApiDynamicsV2 api, Credenciales credenciales, Excepciones excepcion, string? tipoDocumento, string? actividadAFIP,
           string? condicionPyme, string? categoria, string? provincia, string? pais, string? cuitSGR)
        {
            try
            {
                string accountid = string.Empty;
                JObject cuenta = new()
                {
                    //General
                    { "new_estadodelsocio", 100000000 }, //Activo
                    { "new_creadaporapicasfog", true },
                    { "new_calificacion", 100000001 }, //Aprobada
                    { "new_estadodeactividad", 100000000 } //Completa
                };

                if (!string.IsNullOrEmpty(socio.name))
                    cuenta.Add("name", socio.name.Replace(".", ""));

                if (!string.IsNullOrEmpty(socio.new_nmerodedocumento))
                    cuenta.Add("new_nmerodedocumento", socio.new_nmerodedocumento);

                if (!string.IsNullOrEmpty(socio.emailaddress1))
                    cuenta.Add("emailaddress1", socio.emailaddress1);

                if (socio.new_personeria != 0)
                    cuenta.Add("new_personeria", socio.new_personeria);

                if (socio.new_rol != 0)
                    cuenta.Add("new_rol", socio.new_rol);

                if (!string.IsNullOrEmpty(tipoDocumento))
                    cuenta.Add("new_TipodedocumentoId@odata.bind", "/new_tipodedocumentos(" + tipoDocumento + ")");

                if (!string.IsNullOrEmpty(socio.new_productoservicio))
                    cuenta.Add("new_productoservicio", socio.new_productoservicio);

                if (socio.new_tiposocietario != 0)
                    cuenta.Add("new_tiposocietario", socio.new_tiposocietario);

                if (socio.new_condicionimpositiva != 0)
                    cuenta.Add("new_condicionimpositiva", socio.new_condicionimpositiva);

                if (socio.new_inscripcionganancias != 0)
                    cuenta.Add("new_inscripcionganancias", socio.new_inscripcionganancias);

                if (!string.IsNullOrEmpty(actividadAFIP))
                    cuenta.Add("new_ActividadAFIP@odata.bind", "/new_actividadafips(" + actividadAFIP + ")");

                if (!string.IsNullOrEmpty(condicionPyme))
                    cuenta.Add("new_CondicionPyme@odata.bind", "/new_condicionpymes(" + condicionPyme + ")");

                if (!string.IsNullOrEmpty(categoria))
                    cuenta.Add("new_Categoria@odata.bind", "/new_categoracertificadopymes(" + categoria + ")");

                if (socio.new_facturacionultimoanio != 0)
                    cuenta.Add("new_facturacionultimoanio", socio.new_facturacionultimoanio);

                if (socio.new_essoloalyc != null)
                    cuenta.Add("new_essoloalyc", socio.new_essoloalyc);
                //Direccion
                if (socio.telephone2 != null)
                    cuenta.Add("telephone2", socio.telephone2);

                if (socio.address1_postalcode != null)
                    cuenta.Add("address1_postalcode", socio.address1_postalcode);

                if (socio.address1_line1 != null)
                    cuenta.Add("address1_line1", socio.address1_line1);

                if (socio.new_localidad != null)
                    cuenta.Add("new_localidad", socio.new_localidad);

                if (socio.new_direccion1numero != null)
                    cuenta.Add("new_direccion1numero", socio.new_direccion1numero);

                if (socio.address1_county != null)
                    cuenta.Add("address1_county", socio.address1_county);

                if (socio.address1_name != null)
                    cuenta.Add("address1_name", socio.address1_name);

                if (provincia != string.Empty)
                    cuenta.Add("new_Provincia@odata.bind", "/new_provincias(" + provincia + ")");

                if (pais != string.Empty)
                    cuenta.Add("new_Pais@odata.bind", "/new_paises(" + pais + ")");

                if (socio.new_nuevapyme != false)
                    cuenta.Add("new_nuevapyme", socio.new_nuevapyme);

                //if (socio.new_tipodeasociacion != 0)
                //    cuenta.Add("new_tipodeasociacion", socio.new_tipodeasociacion);

                ResponseAPI responseAPI = await api.CreateRecord("accounts", cuenta, credenciales);
                if (responseAPI.ok)
                    accountid = responseAPI.descripcion;
                else
                    throw new Exception(responseAPI.descripcion);

                await excepcion.CrearExcepcionHWAV2($"Socio {socio.name} creado.", credenciales.url, cuitSGR, $"Socio {socio.name} creado.", "WebAPI Sindicadas");
                //logger.LogInformation($"Socio {socio.name} creado.");

                return accountid;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear socio {socio.name} - {ex.Message}");
                throw;
            }
        }
        public static async Task<string> CrearSocioV2(CuentaVinculada socio, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string accountid = string.Empty;
                JObject cuenta = new();

                if (socio.name != null)
                    cuenta.Add("name", socio.name);

                if (socio.new_nmerodedocumento != null)
                    cuenta.Add("new_nmerodedocumento", socio.new_nmerodedocumento);

                if (socio.new_personeria != null)
                    cuenta.Add("new_personeria", socio.new_personeria);

                if (socio.new_rol != null)
                    cuenta.Add("new_rol", socio.new_rol);

                if (socio.new_tipodedocumentoid != null)
                    cuenta.Add("new_TipodedocumentoId@odata.bind", "/new_tipodedocumentos(" + socio.new_tipodedocumentoid + ")");

                ResponseAPI responseAPI = await api.CreateRecord("accounts", cuenta, credenciales);
                if (responseAPI.ok)
                    accountid = responseAPI.descripcion;
                else
                    throw new Exception(responseAPI.descripcion);

                return accountid;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear cuenta del socio {socio.name} de la relacion - {ex.Message}");
                throw;
            }
        }
        public static async Task CrearCertificadosV2(List<CertificadoPymeV2> certificados, string socio_id, ApiDynamicsV2 api, Credenciales credenciales,
           string? condicionpyme_id, string? categoria_id, string nombreSocio, Excepciones excepcion, Credenciales credencialesCliente, string cuitSGR)
        {
            if (certificados != null && certificados.Count > 0)
            {
                foreach (var certificado in certificados)
                {
                    try
                    {
                        JObject Certificado = new()
                        {
                            { "new_aprobacion1", 100000000 }
                        };

                        if (!string.IsNullOrEmpty(socio_id))
                            Certificado.Add("new_SocioParticipe@odata.bind", "/accounts(" + socio_id + ")");

                        if (certificado.new_numeroderegistro != 0)
                            Certificado.Add("new_numeroderegistro", certificado.new_numeroderegistro);

                        if (!string.IsNullOrEmpty(certificado.new_fechadeemision))
                            Certificado.Add("new_fechadeemision", certificado.new_fechadeemision);

                        if (!string.IsNullOrEmpty(certificado.new_vigenciadesde))
                            Certificado.Add("new_vigenciadesde", certificado.new_vigenciadesde);

                        if (!string.IsNullOrEmpty(certificado.new_vigenciahasta))
                        {
                            Certificado.Add("new_vigenciahasta", certificado.new_vigenciahasta);
                            if (DateTime.Parse(certificado.new_vigenciahasta) >= DateTime.Now)
                                Certificado.Add("statuscode", 1); //Aprobado
                        }

                        if (!string.IsNullOrEmpty(condicionpyme_id))
                            Certificado.Add("new_SectorEconomico@odata.bind", "/new_condicionpymes(" + condicionpyme_id + ")");

                        if (!string.IsNullOrEmpty(categoria_id))
                            Certificado.Add("new_Categoria@odata.bind", "/new_categoracertificadopymes(" + categoria_id + ")");

                        ResponseAPI responseAPI = await api.CreateRecord("new_certificadopymes", Certificado, credenciales);
                        if (!responseAPI.ok)
                            throw new Exception(responseAPI.descripcion);
                    }
                    catch (Exception ex)
                    {
                        await excepcion.CrearExcepcionHWAV2($"Error al crear certificado pyme para el socio {nombreSocio}", credencialesCliente.url, cuitSGR,
                            $"Error al crear certificado pyme - {ex.Message}", "WebAPI Sindicadas");
                    }
                }
            }
            else
            {
                //logger.LogInformation($"No hay certificados pymes para el socio {nombreSocio}");
            }
        }
        public static async Task ValidarCertificadoPymeCrearOAprobar(string socio_id, string condicionpyme_id, string categoria_id, List<CertificadoPymeV2> listaCertificadosPymes,
            string nombreSocio, ApiDynamicsV2 api, Credenciales credenciales, Excepciones excepcion, string cuitSGR)
        {
            try
            {
                ////BUSCAMOS CERTIFICADOS PYMES EN EL SOCIO DE LA SGR
                JArray CertificadosPymes = await BuscarCertificadosPymesPorSocio(socio_id, api, credenciales);
                if (CertificadosPymes.Count > 0)
                {
                    List<CertificadosPymesV2> listaCertificados = ArmarCertificadosPymes(CertificadosPymes);
                    ////COMPROBAMOS QUE HAYA ALGUN CERTIFICADO CON ESTADO APROBADO DE LO CONTRARIO SE APRUBA O SE CREA SI FALTA ALGUNO
                    if (listaCertificados.FirstOrDefault(x => x.statuscode == 1) == null)
                    {
                        ////SE BUSCA EL CERTIFICADO PYME VIGENTE Y SE APRUEBA
                        CertificadosPymesV2 certificado = listaCertificados?.FirstOrDefault(x => x?.new_vigenciahasta >= DateTime.Now);
                        if (certificado != null)
                        {
                            JObject _certificado = new()
                            {
                                { "statuscode", 1 },
                                { "new_aprobacion1", 100000000 }
                            };

                            ResponseAPI responseAPI = await api.UpdateRecord("new_certificadopymes", certificado.new_certificadopymeid, _certificado, credenciales);
                            if (!responseAPI.ok)
                                throw new Exception(responseAPI.descripcion);
                        }
                        else
                        {
                            ////SE BUSCA CERTIFICADO PYME FALTANTE
                            List<CertificadoPymeV2> certificadosPymesSinCrear = new();
                            foreach (var _certificado in listaCertificadosPymes)
                            {
                                if (listaCertificados?.FirstOrDefault(x => x.new_numeroderegistro == _certificado.new_numeroderegistro) == null)
                                    certificadosPymesSinCrear.Add(_certificado);
                            }
                            ////SE CREAN CERTIFICADOS PYMES FALTANTES
                            if (certificadosPymesSinCrear.Count > 0)
                                await CrearCertificadosV2(certificadosPymesSinCrear, socio_id, api, credenciales, condicionpyme_id, categoria_id, nombreSocio,
                                    excepcion, credenciales, cuitSGR);
                        }
                    }
                }
                else
                {
                    ////SE CREAN CERTIFICADOS PYMES
                    await CrearCertificadosV2(listaCertificadosPymes, socio_id, api, credenciales, condicionpyme_id, categoria_id, nombreSocio,
                        excepcion, credenciales, cuitSGR);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<string> VerificarContactoV2(decimal cuitcuil, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string contact_id = string.Empty;
                JArray respuesta = new();
                string fetchXML = string.Empty;
                string[] cuit = cuitcuil.ToString().Split(',');

                api.EntityName = "contacts";

                fetchXML = "<entity name='contact'>" +
                                        "<attribute name='contactid'/> " +
                                        "<filter type='and'>" +
                                        $"<condition attribute='new_cuitcuil' operator='eq' value='{cuit[0]}' />" +
                                            "</filter>" +
                            "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                if (respuesta.Count == 0)
                {
                    return contact_id;
                }
                else
                {
                    Contact contacto = JsonConvert.DeserializeObject<Contact>(respuesta.First.ToString());
                    contact_id = contacto?.contactid.ToString();
                }

                return contact_id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task CrearRelacionV2(Relacion relacionVinculacion, string socio_id, string nombreSocio, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JObject relacion = new();

                if (!string.IsNullOrEmpty(socio_id))
                    relacion.Add("new_CuentaId@odata.bind", "/accounts(" + socio_id + ")");

                if (relacionVinculacion.cuentaVinculada != null)
                {
                    string cuenta_id = string.Empty;
                    Casfog_Sindicadas.Socio socioVerificar = await VerificarSocioV2(relacionVinculacion.cuentaVinculada.new_nmerodedocumento, api, credenciales);
                    if (string.IsNullOrEmpty(socioVerificar?.accountid))
                    {
                        string resultadoSocio = await CrearSocioV2(relacionVinculacion.cuentaVinculada, api, credenciales);
                        if (!string.IsNullOrEmpty(resultadoSocio))
                            cuenta_id = resultadoSocio;
                    }
                    else
                    {
                        cuenta_id = socioVerificar.accountid;
                    }

                    relacion.Add("new_CuentaContactoVinculado_account@odata.bind", "/accounts(" + cuenta_id + ")");
                }

                if (relacionVinculacion.contactoVinculado != null)
                {
                    string contact_id = string.Empty;
                    string verificaContacto = await VerificarContactoV2(relacionVinculacion.contactoVinculado.new_cuitcuil, api, credenciales);
                    if (string.IsNullOrEmpty(verificaContacto))
                    {
                        string resultadoContacto = await CrearContactoV2(relacionVinculacion.contactoVinculado, nombreSocio, api, credenciales);
                        if (!string.IsNullOrEmpty(resultadoContacto))
                        {
                            contact_id = resultadoContacto;
                        }
                    }
                    else
                    {
                        contact_id = verificaContacto;
                    }

                    relacion.Add("new_CuentaContactoVinculado_contact@odata.bind", "/contacts(" + contact_id + ")");
                }

                if (!string.IsNullOrEmpty(relacionVinculacion.new_tipoderelacion))
                    relacion.Add("new_tipoderelacion", Convert.ToInt32(relacionVinculacion.new_tipoderelacion));

                if (relacionVinculacion.new_porcentajedeparticipacion > 0)
                    relacion.Add("new_porcentajedeparticipacion", relacionVinculacion.new_porcentajedeparticipacion);

                if (relacionVinculacion.new_porcentajebeneficiario > 0)
                    relacion.Add("new_porcentajebeneficiario", relacionVinculacion.new_porcentajebeneficiario);

                if (!string.IsNullOrEmpty(relacionVinculacion.new_observaciones))
                    relacion.Add("new_observaciones", relacionVinculacion.new_observaciones);

                ResponseAPI responseAPI = await api.CreateRecord("new_participacionaccionarias", relacion, credenciales);
                if (!responseAPI.ok)
                    throw new Exception(responseAPI.descripcion);
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear relacion de vinculacion para el socio {nombreSocio} - {ex.Message}");
                throw;
            }
        }
        public static List<DocumentoCasfog> ArmarDocumentacion(JToken documentacionJT)
        {
            return JsonConvert.DeserializeObject<List<DocumentoCasfog>>(documentacionJT.ToString());
        }
        public static async Task<string> CrearDocumento(Documentacion documentacion, string nombreSocio, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string documento_id = string.Empty;
                JObject documento = new();

                if (documentacion.new_estadodelsocio != 0)
                    documento.Add("new_estadodelsocio", documentacion.new_estadodelsocio);

                if (!string.IsNullOrEmpty(documentacion.new_requeridoa))
                    documento.Add("new_requeridoa", documentacion.new_requeridoa);

                if (documentacion.new_personeria != 0)
                    documento.Add("new_personeria", documentacion.new_personeria);

                if (!string.IsNullOrEmpty(documentacion.new_grupoeconomico))
                    documento.Add("new_grupoeconomico", documentacion.new_grupoeconomico);

                if (!string.IsNullOrEmpty(documentacion.new_fiador))
                    documento.Add("new_fiador", documentacion.new_fiador);

                if (!string.IsNullOrEmpty(documentacion.new_descripcion))
                    documento.Add("new_descripcion", documentacion.new_descripcion);

                if (!string.IsNullOrEmpty(documentacion.new_codigo))
                    documento.Add("new_codigocasfog", documentacion.new_codigo);

                if (!string.IsNullOrEmpty(documentacion.new_condicionimpositiva))
                    documento.Add("new_condicionimpositiva", documentacion.new_condicionimpositiva);

                if (!string.IsNullOrEmpty(documentacion.new_urlplantilla))
                    documento.Add("new_urlplantilla", documentacion.new_urlplantilla);

                if (documentacion.new_tipodefiador != 0)
                    documento.Add("new_tipodefiador", documentacion.new_tipodefiador);

                if (!string.IsNullOrEmpty(documentacion.new_name_documento))
                    documento.Add("new_name", documentacion.new_name_documento);

                ResponseAPI responseAPI = await api.CreateRecord("new_documentacions", documento, credenciales);
                if (responseAPI.ok)
                    documento_id = responseAPI.descripcion;
                else
                    throw new Exception(responseAPI.descripcion);

                return documento_id;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear documentacion para el socio {nombreSocio} - {ex.Message}");
                throw;
            }
        }
        public static async Task CrearDocumentacionPorCuenta(Documentacion documentacion, string nombreSocio, string socio_id, string documento_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JObject documento = new();

                if (!string.IsNullOrEmpty(documentacion.new_name))
                    documento.Add("new_name", documentacion.new_name);

                if (!string.IsNullOrEmpty(documento_id))
                    documento.Add("new_DocumentoId@odata.bind", "/new_documentacions(" + documento_id + ")");

                if (!string.IsNullOrEmpty(socio_id))
                    documento.Add("new_CuentaId@odata.bind", "/accounts(" + socio_id + ")");

                if (!string.IsNullOrEmpty(documentacion.statuscode))
                    documento.Add("statuscode", Convert.ToInt32(documentacion.statuscode));

                if (!string.IsNullOrEmpty(documentacion.new_vinculocompartido))
                    documento.Add("new_vinculocompartido", documentacion.new_vinculocompartido);

                if (!string.IsNullOrEmpty(documentacion.new_fechadevencimiento))
                    documento.Add("new_fechadevencimiento", documentacion.new_fechadevencimiento);

                ResponseAPI responseAPI = await api.CreateRecord("new_documentacionporcuentas", documento, credenciales);
                if (!responseAPI.ok)
                    throw new Exception(responseAPI.descripcion);
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear documentacion por cuenta para el socio {nombreSocio} - {ex.Message}");
                throw;
            }
        }
        public static async Task<string> CrearSocioRelacionesDocumentosCertificados(GarantiaV2 garantia, ApiDynamicsV2 api, Credenciales credenciales,
            Credenciales credencialesCliente, string cuitSGR, Excepciones excepcion)
        {
            try
            {
                string socio_id = string.Empty;
                string tipoDocumento_id = string.Empty;
                string actividadAfip_id = string.Empty;
                string condicionPyme_id = string.Empty;
                string categoria_id = string.Empty;
                string provincia_id = string.Empty;
                string pais_id = string.Empty;

                //Buscamos las relaciones de vinculacion de la pyme en casfog.
                JArray relacionesPorSocio = await BuscarRelacionesPorSocioV2(garantia.cuenta.accountid, api, credenciales);
                List<Relacion> listaRelaciones = ArmarRelaciones(relacionesPorSocio);
                if (listaRelaciones.Count > 0)
                {
                    if (garantia.cuenta != null)
                    {
                        garantia.cuenta.relaciones = listaRelaciones;
                    }
                }
                ////Buscamos el tipo de documento por codigo en la sgr
                if (garantia?.cuenta?.tipoDocumento != null && garantia.cuenta.tipoDocumento.new_codigo != null)
                {
                    JArray TiposDocumento = await BuscarTipoDeDocumentoV2(garantia.cuenta.tipoDocumento.new_codigo, api, credencialesCliente);
                    if (TiposDocumento.Count > 0)
                        tipoDocumento_id = ObtenerTipoDocumentoID(TiposDocumento);
                }
                ////Buscamos Actividad AFIP por codigo en la sgr
                if (garantia?.cuenta?.actividadAFIP != null && garantia.cuenta.actividadAFIP.new_codigo != 0)
                {
                    JArray ActividadesAFIP = await BuscarActividadAFIPV2(garantia.cuenta.actividadAFIP.new_codigo, api, credencialesCliente);
                    if (ActividadesAFIP.Count > 0)
                        actividadAfip_id = ObtenerActividadAfipID(ActividadesAFIP);
                }
                ////Buscamos Categoria Certificado Pyme por codigo en la sgr
                if (garantia?.cuenta?.categoria != null && garantia.cuenta.categoria.new_name != null)
                {
                    JArray Categorias = await BuscarCategoriaCertificadoPymeV2(garantia.cuenta.categoria.new_name, api, credencialesCliente);
                    if (Categorias.Count > 0)
                        categoria_id = ObtenerCategoriaID(Categorias);
                }
                ////Buscamos Condicion Certificado Pyme por codigo en la sgr
                if (garantia?.cuenta?.condicionPyme != null && garantia.cuenta.condicionPyme.new_codigo != 0)
                {
                    JArray CondicionesPyme = await BuscarCondicionPymeV2(garantia.cuenta.condicionPyme.new_codigo, api, credencialesCliente);
                    if (CondicionesPyme.Count > 0)
                        condicionPyme_id = ObtenerCondicionPymeID(CondicionesPyme);
                }
                ////Buscamos Provincia por codigo en la sgr
                if (garantia?.cuenta?.provincia != null && garantia.cuenta.provincia.new_codprovincia != null)
                {
                    JArray Provincias = await BuscarProvinciaV2(garantia.cuenta.provincia.new_codprovincia, api, credencialesCliente);
                    if (Provincias.Count > 0)
                        provincia_id = ObtenerProvinciaID(Provincias);
                }
                ////Buscamos Pais por codigo en la sgr
                if (garantia?.cuenta?.pais != null && garantia.cuenta.pais.new_codpais != null)
                {
                    JArray Paises = await BuscarPaisV2(garantia.cuenta.pais.new_codpais, api, credencialesCliente);
                    if (Paises.Count > 0)
                        pais_id = ObtenerPaisID(Paises);
                }

                //Creamos el socio.
                string resultadoSocio = await CrearSocioV2(garantia?.cuenta, api, credencialesCliente, excepcion, tipoDocumento_id, actividadAfip_id,
                       condicionPyme_id, categoria_id, provincia_id, pais_id, cuitSGR);

                //Si fallo la creacion se termina el proceso
                if (string.IsNullOrEmpty(resultadoSocio))
                    return socio_id;

                string firmante_id = string.Empty;
                string contactoNotificaciones_id = string.Empty;
                string resultadoContactoNotificaciones = string.Empty;
                string verificarFirmante = string.Empty;
                socio_id = resultadoSocio;

                ////Se Crean Certificados Pymes
                if (garantia?.cuenta?.certificados != null && garantia.cuenta.certificados.Count > 0)
                    await ValidarCertificadoPymeCrearOAprobar(resultadoSocio, condicionPyme_id, categoria_id, garantia.cuenta.certificados, garantia?.cuenta?.name,
                        api, credencialesCliente, excepcion, cuitSGR);

                ////Verificamos si el contacto de notificaciones existe como contacto en el sistema de la sgr.
                if (garantia?.cuenta.contactoNotificaciones != null)
                    resultadoContactoNotificaciones = await VerificarContactoV2(garantia.cuenta.contactoNotificaciones.new_cuitcuil, api, credencialesCliente);

                ////Verificamos si el contacto firmante existe como contacto en el sistema de la sgr.
                if (garantia?.cuenta.firmante != null)
                    verificarFirmante = await VerificarContactoV2(garantia.cuenta.firmante.new_cuitcuil, api, credencialesCliente);

                ////Si no existe se crea contacto de notificaciones en el sistema de la sgr.
                if (!string.IsNullOrEmpty(resultadoContactoNotificaciones) && garantia?.cuenta.contactoNotificaciones != null)
                    contactoNotificaciones_id = await CrearContactoV2(garantia.cuenta.contactoNotificaciones, garantia?.cuenta?.name, api, credencialesCliente);
                else
                    contactoNotificaciones_id = resultadoContactoNotificaciones;

                ////Crea contacto firmante en el sistema de la sgr.
                if (!string.IsNullOrEmpty(verificarFirmante) && garantia?.cuenta.firmante != null)
                    firmante_id = await CrearContactoV2(garantia.cuenta.firmante, garantia?.cuenta?.name, api, credencialesCliente);
                else
                    firmante_id = verificarFirmante;

                ////Asociar los contactos a la pyme en el sistema de la sgr.
                if (!string.IsNullOrEmpty(firmante_id) || !string.IsNullOrEmpty(contactoNotificaciones_id))
                    await AsociarContactoASocio(socio_id, firmante_id, contactoNotificaciones_id, garantia?.cuenta?.name, api, credencialesCliente);

                ////Crear Relaciones de Vinculacion en el sistema de la sgr.
                if (garantia?.cuenta.relaciones != null && garantia.cuenta.relaciones.Count > 0)
                {
                    foreach (var relacion in garantia.cuenta.relaciones)
                    {
                        await CrearRelacionV2(relacion, socio_id, garantia?.cuenta?.name, api, credencialesCliente);
                    }
                }
                ////Crear documentacion por cuenta
                if (garantia?.cuenta.documentos != null && garantia.cuenta.documentos.Count > 0)
                {
                    //Buscamos los documentos en la SGR
                    List<DocumentoCasfog> listaDocumentosDeCASFOG = new();
                    JArray documentos = await BuscarDocumentacionCASFOGV2(api, credencialesCliente);
                    if (documentos.Count > 0)
                        listaDocumentosDeCASFOG = ArmarDocumentacion(documentos);

                    foreach (var documento in garantia.cuenta.documentos)
                    {
                        string documento_id = string.Empty;
                        //Ver de matchear documentos por codigo de casfog, de lo contrario lo creamos
                        if (listaDocumentosDeCASFOG.FirstOrDefault(x => x.new_codigocasfog == documento.new_codigo) != null)
                        {
                            documento_id = listaDocumentosDeCASFOG.FirstOrDefault(x => x.new_codigocasfog == documento.new_codigo).new_documentacionid;
                        }
                        else if (documento.new_documentacionid_documento != null)
                        {
                            documento_id = await CrearDocumento(documento, garantia?.cuenta?.name, api, credencialesCliente);
                        }
                        //Crear documentacion por cuenta en el sistema de la sgr.
                        await CrearDocumentacionPorCuenta(documento, garantia?.cuenta?.name, socio_id, documento_id, api, credencialesCliente);
                    }
                }

                return socio_id;
            }
            catch (Exception ex)
            {
                await excepcion.CrearExcepcionHWAV2($"Excepcion en metodo CrearSocioRelacionesDocumentosCertificados para el socio {garantia?.cuenta?.name}",
                     credencialesCliente.url, cuitSGR, ex.Message, "CrearSocioRelacionesDocumentosCertificados");
                throw;
            }
        }
        #endregion
        //############NUEVA VERSION METODOS SINDICADAS
        public static JArray BuscarGarantiaYSocio(string garantia_id, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_garantias";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_garantia'>" +
                                                           "<attribute name='new_tipodeoperacion'/> " +
                                                           "<attribute name='new_tipodegarantias'/> " +
                                                           "<attribute name='new_socioparticipe'/>" +
                                                           "<attribute name='new_saldovigente'/> " +
                                                           "<attribute name='new_ndeordendelagarantiaotorgada'/> " +
                                                           "<attribute name='new_monto'/> " +
                                                           "<attribute name='new_fechadevencimiento'/> " +
                                                           "<attribute name='new_fechadeorigen'/> " +
                                                           "<attribute name='new_fechadenegociacion'/> " +
                                                           "<attribute name='statuscode' /> " +
                                                           "<attribute name='transactioncurrencyid'/> " +
                                                           "<attribute name='new_codigocvba'/> " +
                                                           "<attribute name='new_acreedor'/> " +
                                                           "<attribute name='new_garantiaid'/> " +
                                                           "<attribute name='new_saldocreditoprincipal' /> " +
                                                           "<attribute name='new_saldocuotasoperativo'/> " +
                                                           "<attribute name='new_saldocuotasvigentes'/> " +
                                                           "<attribute name='new_saldodelaamortizacion'/> " +
                                                           "<attribute name='new_cantidadcuotasafrontadas'/> " +
                                                           "<attribute name='new_desembolsoanterior'/> " +
                                                           "<attribute name='new_condesembolsosparciales'/> " +
                                                           "<attribute name='new_fechaemisindelcheque'/> " +
                                                           "<attribute name='new_sistemadeamortizacion'/> " +
                                                           "<attribute name='new_periodicidadpagos'/> " +
                                                           "<attribute name='new_fechaemisindelcheque'/> " +
                                                           "<attribute name='new_tasa'/> " +
                                                           "<attribute name='new_puntosporcentuales'/> " +
                                                           "<attribute name='new_nroexpedientetad'/> " +
                                                           "<attribute name='new_superatasabadlar'/> " +
                                                           "<attribute name='new_tasabadlar'/> " +
                                                           "<attribute name='new_tasabarancaria'/> " +
                                                           "<attribute name='new_observaciones'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_garantiaid' operator='eq' value='{garantia_id}' />" +
                                                           "</filter>" +
                                                           "<link-entity name='new_acreedor' from='new_acreedorid' to='new_acreedor' link-type='outer' alias='acreedor'>" +
                                                                "<attribute name='new_name'/> " + 
                                                                "<attribute name='new_cuit'/> " +
                                                                "<attribute name='new_tipodeacreedor'/> " +
                                                           "</link-entity>" +
                                                            "<link-entity name='transactioncurrency' from='transactioncurrencyid' to='transactioncurrencyid' link-type='outer' alias='divisa'>" +
                                                                "<attribute name='isocurrencycode'/> " +
                                                           "</link-entity>" +
                                                           "<link-entity name='account' from='accountid' to='new_socioparticipe' link-type='outer' alias='cuenta'>" +
                                                               "<attribute name='accountid'/> " +
                                                               "<attribute name='name'/>" +
                                                               "<attribute name='primarycontactid'/> " +
                                                               "<attribute name='new_nmerodedocumento'/> " +
                                                               "<attribute name='new_personeria'/> " +
                                                               "<attribute name='new_rol'/> " +
                                                               "<attribute name='new_tipodedocumentoid'/> " +
                                                               "<attribute name='new_productoservicio'/> " +
                                                               "<attribute name='new_tiposocietario'/> " +
                                                               "<attribute name='new_condicionimpositiva'/> " +
                                                               "<attribute name='emailaddress1'/> " +
                                                               "<attribute name='statuscode'/> " +
                                                               "<attribute name='new_actividadafip'/> " +
                                                               "<attribute name='new_facturacionultimoanio'/> " +
                                                               "<attribute name='new_fechadealta'/> " +
                                                               "<attribute name='new_onboarding'/> " +
                                                               "<attribute name='new_essoloalyc'/> " +
                                                               "<attribute name='new_estadodelsocio'/> " +
                                                                //"<attribute name='new_inscripcionganancias'/> " +
                                                                "<attribute name='telephone2'/> " +
                                                               "<attribute name='address1_line1'/> " +
                                                               "<attribute name='new_direccion1numero'/> " +
                                                               "<attribute name='address1_name'/> " +
                                                               "<attribute name='new_direccion1depto'/> " +
                                                               "<attribute name='address1_postalcode'/> " +
                                                               "<attribute name='address1_county'/> " +
                                                               "<attribute name='new_localidad'/> " +
                                                               "<attribute name='new_calificacion'/> " +
                                                                "<attribute name='new_estadodeactividad'/> " +
                                                               //"<attribute name='new_nuevapyme'/> " +
                                                               "<link-entity name='contact' from='contactid' to='new_contactodenotificaciones' link-type='outer' alias='contacto'>" +
                                                                    "<attribute name='firstname'/> " +
                                                                    "<attribute name='lastname'/> " +
                                                                    "<attribute name='new_cuitcuil'/> " +
                                                                    "<attribute name='contactid'/> " +
                                                               "</link-entity>" +
                                                               "<link-entity name='contact' from='contactid' to='new_contactofirmante' link-type='outer' alias='contactoFirmante'>" +
                                                                    "<attribute name='firstname'/> " +
                                                                    "<attribute name='lastname'/> " +
                                                                    "<attribute name='new_cuitcuil'/> " +
                                                                    "<attribute name='contactid'/> " +
                                                               "</link-entity>" +
                                                               "<link-entity name='new_documentacionporcuenta' from='new_cuentaid' to='accountid' link-type='outer' alias='documentacion'>" +
                                                                     "<attribute name='new_documentacionporcuentaid'/> " +
                                                                     "<attribute name='new_cuentaid'/> " +
                                                                     "<attribute name='new_documentoid'/> " +
                                                                     "<attribute name='statuscode'/> " +
                                                                     "<attribute name='new_vinculocompartido'/> " +
                                                                     "<attribute name='new_fechadevencimiento'/> " +
                                                                      "<link-entity name='new_documentacion' from='new_documentacionid' to='new_documentoid' link-type='outer' alias='documento'>" +
                                                                          "<attribute name='new_codigo'/> " +
                                                                          "<attribute name='new_documentacionid'/> " +
                                                                          "<attribute name='new_urlplantilla'/> " +
                                                                          "<attribute name='new_tipodefiador'/> " +
                                                                          "<attribute name='new_personeria'/> " +
                                                                          "<attribute name='new_grupoeconomico'/> " +
                                                                          "<attribute name='new_fiador'/> " +
                                                                          "<attribute name='new_name'/> " +
                                                                          "<attribute name='new_estadodelsocio'/> " +
                                                                          "<attribute name='new_descripcion'/> " +
                                                                          "<attribute name='new_condicionimpositiva'/> " +
                                                                      "</link-entity>" +
                                                               "</link-entity>" +
                                                               "<link-entity name='new_certificadopyme' from='new_socioparticipe' to='accountid' link-type='outer' alias='certificado'>" +
                                                                     "<attribute name='new_certificadopymeid'/> " +
                                                                     "<attribute name='new_numeroderegistro'/> " +
                                                                     "<attribute name='new_fechadeemision'/> " +
                                                                     "<attribute name='new_categoria'/> " +
                                                                     "<attribute name='new_sectoreconomico'/> " +
                                                                     "<attribute name='new_vigenciadesde'/> " +
                                                                     "<attribute name='new_vigenciahasta'/> " +
                                                                     "<attribute name='statecode'/> " +
                                                               "</link-entity>" +
                                                               "<link-entity name='new_actividadafip' from='new_actividadafipid' to='new_actividadafip' link-type='outer' alias='actividadAfip'>" +
                                                                    "<attribute name='new_codigo'/> " +
                                                               "</link-entity>" +
                                                               "<link-entity name='new_tipodedocumento' from='new_tipodedocumentoid' to='new_tipodedocumentoid' link-type='outer' alias='tipoDocumento'>" +
                                                                    "<attribute name='new_codigo'/> " +
                                                                    "<attribute name='new_tipodedocumentoid'/> " +
                                                               "</link-entity>" +
                                                               "<link-entity name='new_condicionpyme' from='new_condicionpymeid' to='new_condicionpyme' link-type='outer' alias='condicionPyme'>" +
                                                                    "<attribute name='new_codigo'/> " +
                                                               "</link-entity>" +
                                                               "<link-entity name='new_categoracertificadopyme' from='new_categoracertificadopymeid' to='new_categoria' link-type='outer' alias='categoria'>" +
                                                                    "<attribute name='new_codigo'/> " +
                                                                     "<attribute name='new_name'/> " +
                                                               "</link-entity>" +
                                                               "<link-entity name='new_pais' from='new_paisid' to='new_pais' link-type='outer' alias='pais'>" +
                                                                    "<attribute name='new_codpais'/> " +
                                                               "</link-entity>" +
                                                               "<link-entity name='new_provincia' from='new_provinciaid' to='new_provincia' link-type='outer' alias='provincia'>" +
                                                                    "<attribute name='new_codprovincia'/> " +
                                                               "</link-entity>" +
                                                            "</link-entity>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarSocioyDocumentos(string cuit, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "accounts";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='account'>" +
                                                           "<attribute name='accountid'/> " +
                                                           "<attribute name='name'/>" +
                                                           "<attribute name='primarycontactid'/> " +
                                                           "<attribute name='new_nmerodedocumento'/> " +
                                                           "<attribute name='new_personeria'/> " +
                                                           "<attribute name='new_rol'/> " +
                                                           "<attribute name='new_tipodedocumentoid'/> " +
                                                           "<attribute name='new_productoservicio'/> " +
                                                           "<attribute name='new_tiposocietario'/> " +
                                                           "<attribute name='new_condicionimpositiva'/> " +
                                                           "<attribute name='emailaddress1'/> " +
                                                           "<attribute name='statuscode'/> " +
                                                           "<attribute name='new_actividadafip'/> " +
                                                           "<attribute name='new_facturacionultimoanio'/> " +
                                                           "<attribute name='new_fechadealta'/> " +
                                                           "<attribute name='new_onboarding'/> " +
                                                           "<attribute name='new_essoloalyc'/> " +
                                                           "<attribute name='new_estadodelsocio'/> " +
                                                           "<attribute name='new_inscripcionganancias'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_nmerodedocumento' operator='eq' value='{cuit}' />" +
                                                           "</filter>" +
                                                           "<link-entity name='contact' from='contactid' to='new_contactodenotificaciones' link-type='outer' alias='contacto'>" +
                                                                    "<attribute name='firstname'/> " +
                                                                    "<attribute name='lastname'/> " +
                                                                    "<attribute name='new_cuitcuil'/> " +
                                                                    "<attribute name='contactid'/> " +
                                                           "</link-entity>" +
                                                           "<link-entity name='contact' from='contactid' to='new_contactofirmante' link-type='outer' alias='contactoFirmante'>" +
                                                                    "<attribute name='firstname'/> " +
                                                                    "<attribute name='lastname'/> " +
                                                                    "<attribute name='new_cuitcuil'/> " +
                                                                    "<attribute name='contactid'/> " +
                                                           "</link-entity>" +
                                                           "<link-entity name='new_documentacionporcuenta' from='new_cuentaid' to='accountid' link-type='outer' alias='documentacion'>" +
                                                                    "<attribute name='new_documentacionporcuentaid'/> " +
                                                                     "<attribute name='new_cuentaid'/> " +
                                                                     "<attribute name='new_documentoid'/> " +
                                                                     "<attribute name='statuscode'/> " +
                                                                     "<attribute name='new_vinculocompartido'/> " +
                                                                     "<attribute name='new_fechadevencimiento'/> " +
                                                           "</link-entity>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarRelacionesPorSocio(string cuit, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_participacionaccionarias";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_participacionaccionaria'>" +
                                                           "<attribute name='new_participacionaccionariaid'/> " +
                                                           "<attribute name='new_name'/>" +
                                                           "<attribute name='createdon'/> " +
                                                           "<attribute name='new_tipoderelacion'/> " +
                                                           "<attribute name='new_porcentajedeparticipacion'/> " +
                                                           "<attribute name='new_observaciones'/> " +
                                                           "<attribute name='new_cuentacontactovinculado'/> " +
                                                           "<attribute name='new_porcentajebeneficiario'/> " +
                                                           "<filter type='and'>" +
                                                           $"<condition attribute='new_cuentaid' operator='eq' value='{cuit}' />" +
                                                           "</filter>" +
                                                           "<link-entity name='contact' from='contactid' to='new_cuentacontactovinculado' link-type='outer' alias='contacto'>" +
                                                                    "<attribute name='firstname'/> " +
                                                                    "<attribute name='lastname'/> " +
                                                                    "<attribute name='new_cuitcuil'/> " +
                                                                    "<attribute name='contactid'/> " +
                                                           "</link-entity>" +
                                                           "<link-entity name='account' from='accountid' to='new_cuentacontactovinculado' link-type='outer' alias='cuenta'>" +
                                                                    "<attribute name='name'/> " +
                                                                    "<attribute name='new_nmerodedocumento'/> " +
                                                                    "<attribute name='new_tipodedocumentoid'/> " +
                                                                    "<attribute name='new_personeria'/> " +
                                                                    "<attribute name='new_rol'/> " +
                                                                    "<attribute name='emailaddress1'/> " +
                                                           "</link-entity>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarGarantiasYCuotas(string garantia_id, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_garantias";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_garantia'>" +
                                                           "<attribute name='new_tipodeoperacion'/> " +
                                                           "<attribute name='new_tipodegarantias'/> " +
                                                           "<attribute name='new_socioparticipe'/>" +
                                                           "<attribute name='new_saldovigente'/> " +
                                                           "<attribute name='new_ndeordendelagarantiaotorgada'/> " +
                                                           "<attribute name='new_monto'/> " +
                                                           "<attribute name='new_fechadevencimiento'/> " +
                                                           "<attribute name='new_fechadeorigen'/> " +
                                                           "<attribute name='new_fechadenegociacion'/> " +
                                                           "<attribute name='statuscode' /> " +
                                                           "<attribute name='transactioncurrencyid'/> " +
                                                           "<attribute name='new_codigocvba'/> " +
                                                           "<attribute name='new_acreedor'/> " +
                                                           "<attribute name='new_garantiaid'/> " +
                                                           "<attribute name='new_saldocreditoprincipal' /> " +
                                                           "<attribute name='new_saldocuotasoperativo'/> " +
                                                           "<attribute name='new_saldocuotasvigentes'/> " +
                                                           "<attribute name='new_saldodelaamortizacion'/> " +
                                                           "<attribute name='new_cantidadcuotasafrontadas'/> " +
                                                           "<attribute name='new_dictamendelaval'/> " +
                                                           "<attribute name='new_montogarantia'/> " +
                                                           "<filter type='and'>" +
                                                           $"<condition attribute='new_garantiaid' operator='eq' value='{garantia_id}' />" +
                                                           "</filter>" +
                                                           "<link-entity name='new_prestamos' from='new_garantia' to='new_garantiaid' link-type='outer' alias='cuota'>" +
                                                                    "<attribute name ='new_prestamosid'/> " +
                                                                    "<attribute name='new_numero'/> " +
                                                                    "<attribute name='new_montocuota'/> " +
                                                                    "<attribute name='statuscode'/> " +
                                                                    "<attribute name='new_fechadevencimiento'/> " +
                                                                    "<attribute name='new_fechaestimadapago'/> " +
                                                                    "<attribute name='new_interesperiodo'/> " +
                                                                    "<attribute name='new_montooperativo'/> " +
                                                                    "<attribute name='new_montovigente'/> " +
                                                                    "<attribute name='new_amortizacion'/> " +
                                                           "</link-entity>" +
                                                           "<link-entity name='account' from='accountid' to='new_socioparticipe' link-type='outer' alias='socio'>" +
                                                                    "<attribute name='new_nmerodedocumento'/> " +
                                                           "</link-entity>" +
                                                           "<link-entity name='new_operacionsindicada' from='new_garantia' to='new_garantiaid' link-type='outer' alias='operacion'>" +
                                                                    "<attribute name='new_operacionsindicadaid'/> " +
                                                                    "<attribute name='new_name'/> " +
                                                                    "<attribute name='new_socioparticipe'/> " +
                                                                    "<attribute name='new_monto'/> " +
                                                                    "<attribute name='new_garantia'/> " +
                                                                    "<attribute name='new_porcentaje'/> " +
                                                                     "<attribute name='new_importeenpesos'/> " +
                                                                //"<link-entity name='new_sgr' from='new_sgrid' to='new_sgr' link-type='outer' alias='sgr'>" +
                                                                //"<attribute name='new_clientesgroneclick'/> " +
                                                                //"<attribute name='new_credencialapi'/> " +
                                                                //"<attribute name='new_name' />" +
                                                                // "</link-entity>" +
                                                                "</link-entity>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarGarantiasYCuotasMonetizadas(string garantia_id, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_garantias";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_garantia'>" +
                                                           "<attribute name='new_tipodeoperacion'/> " +
                                                           "<attribute name='new_tipodegarantias'/> " +
                                                           "<attribute name='new_socioparticipe'/>" +
                                                           "<attribute name='new_saldovigente'/> " +
                                                           "<attribute name='new_ndeordendelagarantiaotorgada'/> " +
                                                           "<attribute name='new_monto'/> " +
                                                           "<attribute name='new_fechadevencimiento'/> " +
                                                           "<attribute name='new_fechadeorigen'/> " +
                                                           "<attribute name='new_fechadenegociacion'/> " +
                                                           "<attribute name='statuscode' /> " +
                                                           "<attribute name='transactioncurrencyid'/> " +
                                                           "<attribute name='new_codigocvba'/> " +
                                                           "<attribute name='new_acreedor'/> " +
                                                           "<attribute name='new_garantiaid'/> " +
                                                           "<attribute name='new_saldocreditoprincipal' /> " +
                                                           "<attribute name='new_saldocuotasoperativo'/> " +
                                                           "<attribute name='new_saldocuotasvigentes'/> " +
                                                           "<attribute name='new_saldodelaamortizacion'/> " +
                                                           "<attribute name='new_cantidadcuotasafrontadas'/> " +
                                                           "<attribute name='new_dictamendelaval'/> " +
                                                           "<attribute name='new_montogarantia'/> " +
                                                           "<filter type='and'>" +
                                                           $"<condition attribute='new_garantiaid' operator='eq' value='{garantia_id}' />" +
                                                           "<condition attribute='new_garantiaid' operator='not-null' />" +
                                                           "</filter>" +
                                                           "<link-entity name='new_prestamos' from='new_garantia' to='new_garantiaid' link-type='outer' alias='cuota'>" +
                                                                    "<attribute name ='new_prestamosid'/> " +
                                                                    "<attribute name='new_numero'/> " +
                                                                    "<attribute name='new_montocuota'/> " +
                                                                    "<attribute name='statuscode'/> " +
                                                                    "<attribute name='new_fechadevencimiento'/> " +
                                                                    "<attribute name='new_fechaestimadapago'/> " +
                                                                    "<attribute name='new_interesperiodo'/> " +
                                                                    "<attribute name='new_montooperativo'/> " +
                                                                    "<attribute name='new_montovigente'/> " +
                                                                    "<attribute name='new_amortizacion'/> " +
                                                           "</link-entity>" +
                                                           "<link-entity name='account' from='accountid' to='new_socioparticipe' link-type='outer' alias='cuenta'>" +
                                                                    "<attribute name='new_nmerodedocumento'/> " +
                                                                    "<attribute name='name'/> " +
                                                           "</link-entity>" +
                                                           "<link-entity name='new_operacionsindicada' from='new_garantia' to='new_garantiaid' link-type='outer' alias='operacion'>" +
                                                                    "<attribute name='new_operacionsindicadaid'/> " +
                                                                    "<attribute name='new_name'/> " +
                                                                    "<attribute name='new_socioparticipe'/> " +
                                                                    "<attribute name='new_monto'/> " +
                                                                    "<attribute name='new_garantia'/> " +
                                                                    "<attribute name='new_porcentaje'/> " +
                                                                    "<attribute name='new_importeenpesos'/> " +
                                                                    "<link-entity name='new_sgr' from='new_sgrid' to='new_sgr' link-type='outer' alias='sgr'>" +
                                                                        "<attribute name='new_credencialapi'/> " +
                                                                    "</link-entity>" +
                                                            "</link-entity>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarTipoDeDocumento(string codigo, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_tipodedocumentos";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_tipodedocumento'>" +
                                                           "<attribute name='new_tipodedocumentoid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<attribute name='new_codigo'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codigo' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarActividadAFIP(int codigo, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_actividadafips";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_actividadafip'>" +
                                                           "<attribute name='new_actividadafipid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codigo' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarCondicionPyme(int codigo, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_condicionpymes";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_condicionpyme'>" +
                                                           "<attribute name='new_condicionpymeid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codigo' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarCategoriaCertificadoPyme(string codigo, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_categoracertificadopymes";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_categoracertificadopyme'>" +
                                                           "<attribute name='new_categoracertificadopymeid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codigo' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarAcreedor(string cuit, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_acreedors";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_acreedor'>" +
                                                           "<attribute name='new_acreedorid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_cuit' operator='eq' value='{cuit}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarDivisa(string codigo, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "transactioncurrencies";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='transactioncurrency'>" +
                                                           "<attribute name='transactioncurrencyid'/> " +
                                                           "<attribute name='isocurrencycode'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='isocurrencycode' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarProvincia(string codigo, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_provincias";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_provincia'>" +
                                                           "<attribute name='new_provinciaid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codprovincia' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarPais(string codigo, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_paises";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_pais'>" +
                                                           "<attribute name='new_paisid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codpais' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarGarantiasEnSGR(string garantia_id, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_garantias";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_garantia'>" +
                                                           "<attribute name='new_garantiaid'/> " +
                                                           "<attribute name='new_fechadenegociacion'/> " +
                                                           "<filter type='and'>" +
                                                           $"<condition attribute='new_casfoggarantiaid' operator='eq' value='{garantia_id}' />" +
                                                           "</filter>" +
                                                            "<link-entity name='new_prestamos' from='new_garantia' to='new_garantiaid' link-type='outer' alias='cuota'>" +
                                                                    "<attribute name ='new_prestamosid'/> " +
                                                                    "<attribute name='new_numero'/> " +
                                                                    "<attribute name='new_montocuota'/> " +
                                                                    "<attribute name='statuscode'/> " +
                                                                    "<attribute name='new_fechadevencimiento'/> " +
                                                                    "<attribute name='new_fechaestimadapago'/> " +
                                                                    "<attribute name='new_interesperiodo'/> " +
                                                                    "<attribute name='new_montooperativo'/> " +
                                                                    "<attribute name='new_montovigente'/> " +
                                                                    "<attribute name='new_amortizacion'/> " +
                                                                    "<filter type='and'>" +
                                                                    "<condition attribute='statecode' operator='eq' value='0' />" + //Activo
                                                                    "</filter>" +
                                                           "</link-entity>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {
                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }
                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarDocumentacion(string codigo, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_documentacions";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_documentacion'>" +
                                                           "<attribute name='new_documentacionid'/> " +
                                                           "<filter type='and'>" +
                                                           $"<condition attribute='new_codigo' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarDocumentacionCASFOG(string codigo, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_documentacions";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_documentacion'>" +
                                                           "<attribute name='new_documentacionid'/> " +
                                                           "<filter type='and'>" +
                                                           $"<condition attribute='new_codigocasfog' operator='eq' value='{codigo}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarLimitePorLineaGeneral(string cuenta_id, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_productoses";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_productos'>" +
                                                           "<attribute name='new_productosid'/> " +
                                                           "<attribute name='new_limitecomercialdivisa'/> " +
                                                           "<attribute name='new_vigenciahasta'/> " +
                                                           "<attribute name='new_lineatipodeoperacion'/> " +
                                                           "<filter type='and'>" +
                                                                "<condition attribute='new_lineatipodeoperacion' operator='in' >" +
                                                                "<value>12</value>" + //publicas
                                                                "<value>100000000</value>" +//General 
                                                                "</condition>" +
                                                                $"<condition attribute='new_cuenta' operator='eq' value='{cuenta_id}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarTareas(ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "tasks";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='task'>" +
                                                           "<attribute name='regardingobjectid'/> " +
                                                           "<filter type='and'>" +
                                                                "<condition attribute='regardingobjectid' operator='eq' value='{3671AD95-B6B6-EC11-983F-000D3AC08CE7}' />" + //General
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static JArray BuscarOperacionSindicada(string operacion_id, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_operacionsindicadas";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_operacionsindicada'>" +
                                                           "<attribute name='new_operacionsindicadaid'/> " +
                                                           "<attribute name='new_garantiamonetizada'/> " +
                                                           "<filter type='and'>" +
                                                           $"<condition attribute='new_operacionsindicadaid' operator='eq' value='{operacion_id}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarOperacionesSindicadas(string garantia_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_operacionsindicadas";

                fetchXML = "<entity name='new_operacionsindicada'>" +
                                                           "<attribute name='new_operacionsindicadaid'/> " +
                                                           "<attribute name='new_garantiamonetizada'/> " +
                                                           "<attribute name='new_importeenpesos'/> " +
                                                           "<filter type='and'>" +
                                                           $"<condition attribute='new_garantia' operator='eq' value='{garantia_id}' />" +
                                                           "<condition attribute='statuscode' operator='eq' value='1' />" +
                                                           "</filter>" +
                                                            "<link-entity name='new_sgr' from='new_sgrid' to='new_sgr' link-type='inner' alias='sgr'>" +
                                                                 "<attribute name='new_credencialapi'/> " +
                                                                 "<filter type='and'>" +
                                                                 "<condition attribute='new_clientesgroneclick' operator='eq' value='1' />" +
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
        public static async Task<string> BuscarCUITSgr(ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string cuitSGR = string.Empty;
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "businessunits";

                fetchXML = "<entity name='businessunit'>" +
                                                           "<attribute name='new_cuitdesgr'/> " +
                                                           "<filter type='and'>" +
                                                                "<condition attribute='parentbusinessunitid' operator='null' />" +
                                                           "</filter>" +
                                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                    if (respuesta.Count > 0)
                    {
                        UnidadDeNegocioSindicada Un = JsonConvert.DeserializeObject<UnidadDeNegocioSindicada>(respuesta.First().ToString());
                        if (Un.new_cuitdesgr != null)
                            cuitSGR = Un.new_cuitdesgr;
                    }
                }

                return cuitSGR;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static Casfog_Sindicadas.Socio ArmarSocio(JToken cuentaJT, JToken relacionesVinculacion)
        {
            Casfog_Sindicadas.Socio socio = new();
            Cuenta cuenta = new();
            Contacto contactoNotificaciones = new();
            ContactoFirmante firmante = new();
            List<Documentacion> listaDocumentos = new();
            List<Relacion> listaRelaciones = new();

            cuenta = JsonConvert.DeserializeObject<Cuenta>(cuentaJT.First.ToString());
            contactoNotificaciones = JsonConvert.DeserializeObject<Contacto>(cuentaJT.First.ToString());
            firmante = JsonConvert.DeserializeObject<ContactoFirmante>(cuentaJT.First.ToString());
            listaDocumentos = JsonConvert.DeserializeObject<List<Documentacion>>(cuentaJT.ToString());
            listaDocumentos = listaDocumentos.GroupBy(g => g.new_documentacionporcuentaid).Select(s => s.First()).ToList();
            listaDocumentos.RemoveAll(x => x.new_documentacionporcuentaid == null);

            foreach (var relacionVinculacion in relacionesVinculacion.Children())
            {
                Relacion relacion = JsonConvert.DeserializeObject<Relacion>(relacionVinculacion.ToString());
                Contacto contactoVinculado = JsonConvert.DeserializeObject<Contacto>(relacionVinculacion.ToString());
                CuentaVinculada cuentaVinculada = JsonConvert.DeserializeObject<CuentaVinculada>(relacionVinculacion.ToString());

                if (contactoVinculado.contactid != null)
                    relacion.contactoVinculado = contactoVinculado;

                if (cuentaVinculada.accountid != null)
                    relacion.cuentaVinculada = cuentaVinculada;
               
                listaRelaciones.Add(relacion);
            }

            return socio;
        }
        public static Casfog_Sindicadas.Garantia ArmarGarantia(JToken cuentaJT)
        {
            Casfog_Sindicadas.Garantia garantia = new();
            Cuenta cuenta = new();
            Contacto contactoNotificaciones = new();
            ContactoFirmante firmante = new();
            List<Documentacion> listaDocumentos = new();
            List<CertificadoPyme> listaCertificados = new();
            TipoDocumentoVinculado tipoDocumento = new();
            ActividadAFIPVinculado actividadAFIP = new();
            CondicionPymeVinculado condicionPyme = new();
            CategoriaVinculado categoria = new();
            AcreedorVinculado acreedor = new();
            DivisaVinculada divisa = new();
            ProvinciaVinculada provincia = new();
            PaisVinculado pais = new();

            garantia = JsonConvert.DeserializeObject<Casfog_Sindicadas.Garantia>(cuentaJT.First.ToString());
            acreedor = JsonConvert.DeserializeObject<AcreedorVinculado>(cuentaJT.First.ToString());
            divisa = JsonConvert.DeserializeObject<DivisaVinculada>(cuentaJT.First.ToString());
            cuenta = JsonConvert.DeserializeObject<Cuenta>(cuentaJT.First.ToString());
            contactoNotificaciones = JsonConvert.DeserializeObject<Contacto>(cuentaJT.First.ToString());
            firmante = JsonConvert.DeserializeObject<ContactoFirmante>(cuentaJT.First.ToString());
            listaDocumentos = JsonConvert.DeserializeObject<List<Documentacion>>(cuentaJT.ToString());
            listaCertificados = JsonConvert.DeserializeObject<List<CertificadoPyme>>(cuentaJT.ToString());
            listaDocumentos = listaDocumentos.GroupBy(x => x.new_documentacionporcuentaid).Select(g => g.First()).ToList();
            listaDocumentos.RemoveAll(x => x.new_documentacionporcuentaid == null);
            listaCertificados = listaCertificados.GroupBy(x => x.new_certificadopymeid).Select(g => g.First()).ToList();
            listaCertificados.RemoveAll(x => x.new_certificadopymeid == null);

            tipoDocumento = JsonConvert.DeserializeObject<TipoDocumentoVinculado>(cuentaJT.First.ToString());
            actividadAFIP = JsonConvert.DeserializeObject<ActividadAFIPVinculado>(cuentaJT.First.ToString());
            condicionPyme = JsonConvert.DeserializeObject<CondicionPymeVinculado>(cuentaJT.First.ToString());
            categoria = JsonConvert.DeserializeObject<CategoriaVinculado>(cuentaJT.First.ToString());
            provincia = JsonConvert.DeserializeObject<ProvinciaVinculada>(cuentaJT.First.ToString());
            pais = JsonConvert.DeserializeObject<PaisVinculado>(cuentaJT.First.ToString());

            garantia.acreedor = acreedor;
            garantia.divisa = divisa;
            cuenta.contactoNotificaciones = contactoNotificaciones.contactid != null ? contactoNotificaciones : null;
            cuenta.firmante = firmante.contactid != null ? firmante : null;
            cuenta.documentos = listaDocumentos.Count > 0 ? listaDocumentos  : null;
            cuenta.certificados = listaCertificados.Count > 0 ? listaCertificados : null;
            cuenta.tipoDocumento = tipoDocumento.new_codigo != null ? tipoDocumento: null;
            cuenta.actividadAFIP = actividadAFIP.new_codigo != 0 ? actividadAFIP : null;
            cuenta.condicionPyme = condicionPyme.new_codigo != 0 ? condicionPyme : null;
            cuenta.categoria = categoria.new_name != null ? categoria : null;
            cuenta.provincia = provincia.new_codprovincia != null ? provincia : null;
            cuenta.pais = pais.new_codpais != null ? pais : null;
            garantia.cuenta = cuenta;

            return garantia;
        }
        public static List<Relacion> ArmarRelaciones(JToken relacionesVinculacion)
        {
            List<Relacion> listaRelaciones = new();

            foreach (var relacionVinculacion in relacionesVinculacion.Children())
            {
                Relacion relacion = JsonConvert.DeserializeObject<Relacion>(relacionVinculacion.ToString());
                Contacto contactoVinculado = JsonConvert.DeserializeObject<Contacto>(relacionVinculacion.ToString());
                CuentaVinculada cuentaVinculada = JsonConvert.DeserializeObject<CuentaVinculada>(relacionVinculacion.ToString());

                if (contactoVinculado.contactid != null)
                    relacion.contactoVinculado = contactoVinculado;

                if (cuentaVinculada.accountid != null)
                    relacion.cuentaVinculada = cuentaVinculada;

                listaRelaciones.Add(relacion);
            }

            return listaRelaciones;
        }
        public static Casfog_Sindicadas.Garantia ArmarGarantiaYCuotas(JToken garantiaJT)
        {
            Casfog_Sindicadas.Garantia garantia;
            Cuenta cuenta = new();
            SocioGarantia socioGarantia;
            List<Cuota> listaCuotas;
            List<OperacionSindicadaVinculada> listaOperaciones;

            garantia = JsonConvert.DeserializeObject<Casfog_Sindicadas.Garantia>(garantiaJT.First.ToString());
            cuenta = JsonConvert.DeserializeObject<Cuenta>(garantiaJT.First.ToString());
            socioGarantia = JsonConvert.DeserializeObject<SocioGarantia>(garantiaJT.First.ToString());
            listaCuotas = JsonConvert.DeserializeObject<List<Cuota>>(garantiaJT.ToString());
            listaCuotas = listaCuotas.GroupBy(x => x.new_prestamosid).Select(g => g.First()).ToList();
            listaCuotas.RemoveAll(x => x.new_prestamosid == null);
            listaOperaciones = JsonConvert.DeserializeObject<List<OperacionSindicadaVinculada>>(garantiaJT.ToString());
            listaOperaciones = listaOperaciones.GroupBy(x => x.new_operacionsindicadaid).Select(g => g.First()).ToList();
            listaOperaciones.RemoveAll(x => x.new_operacionsindicadaid == null);

            garantia.cuenta = cuenta;
            garantia.cuotas = listaCuotas;
            garantia.operacionSindicada = listaOperaciones;
            garantia.socioGarantia = socioGarantia;

            return garantia;
        }
        public static GarantiaSGR ArmarGarantiaSGR(JToken garantiaJT)
        {
            GarantiaSGR garantia = new();
            List<Cuota> listaCuotas;
            garantia = JsonConvert.DeserializeObject<GarantiaSGR>(garantiaJT.First.ToString());
            listaCuotas = JsonConvert.DeserializeObject<List<Cuota>>(garantiaJT.ToString());
            listaCuotas = listaCuotas.GroupBy(x => x.new_prestamosid).Select(g => g.First()).ToList();
            listaCuotas.RemoveAll(x => x.new_prestamosid == null);
            garantia.cuotas = listaCuotas;
            return garantia;
        }
        public static OP ArmarOP(JToken opJT)
        {
            OP operacionSindicada = new();

            operacionSindicada = JsonConvert.DeserializeObject<OP>(opJT.First.ToString());

            return operacionSindicada;
        }
        public static List<Limite> ArmarLimite(JToken limiteJT)
        {
            List<Limite> limitaLimites;

            limitaLimites = JsonConvert.DeserializeObject<List<Limite>>(limiteJT.ToString());

            return limitaLimites;
        }
        public static List<OPYSGR> ArmarOPYSGR(JToken op)
        {
            List<OPYSGR> OpYSgr;

            OpYSgr = JsonConvert.DeserializeObject<List<OPYSGR>>(op.ToString());

            return OpYSgr;
        }
        public static string VerificarSocio(string cuit, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string socio_id = string.Empty;
                JArray respuesta = null;
                Casfog_Sindicadas.Socio socio = new();
                string fetchXML = string.Empty;

                api.EntityName = "accounts";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='account'>" +
                                                           "<attribute name='accountid'/> " +
                                                           "<filter type='and'>" +
                                                            $"<condition attribute='new_nmerodedocumento' operator='eq' value='{cuit}' />" +
                                                             "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                if (respuesta.Count == 0)
                {
                    return socio_id;
                }
                else
                {
                    socio = JsonConvert.DeserializeObject<Casfog_Sindicadas.Socio>(respuesta.First.ToString());
                    socio_id = socio.accountid;
                }

                return socio_id;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static string VerificarContacto(decimal cuitcuil, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string contact_id = string.Empty;
                JArray respuesta = null;
                Contact contacto = new();
                string fetchXML = string.Empty;
                string[] cuit = cuitcuil.ToString().Split(',');

                api.EntityName = "contacts";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='contact'>" +
                                                           "<attribute name='contactid'/> " +
                                                           "<filter type='and'>" +
                                                            $"<condition attribute='new_cuitcuil' operator='eq' value='{cuit[0]}' />" +
                                                             "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                if (respuesta.Count == 0)
                {
                    return contact_id;
                }
                else
                {
                    contacto = JsonConvert.DeserializeObject<Contact>(respuesta.First.ToString());
                    contact_id = contacto.contactid.ToString();
                }

                return contact_id;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static string CrearContacto(Contacto contacto, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string contact_id = string.Empty;

                JObject contact = new JObject();

                if (contacto.firstname != null)
                    contact.Add("firstname", contacto.firstname);

                if (contacto.lastname != null)
                    contact.Add("lastname", contacto.lastname);

                if (contacto.new_cuitcuil != 0)
                    contact.Add("new_cuitcuil", Convert.ToDecimal(contacto.new_cuitcuil));

                contact_id = api.CreateRecord("contacts", contact, credenciales);

                return contact_id;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static string CrearContacto(ContactoFirmante contacto, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                string contact_id = string.Empty;

                JObject contact = new JObject();

                if (contacto.firstname != null)
                    contact.Add("firstname", contacto.firstname);

                if (contacto.lastname != null)
                    contact.Add("lastname", contacto.lastname);

                if (contacto.new_cuitcuil != 0)
                    contact.Add("new_cuitcuil", Convert.ToDecimal(contacto.new_cuitcuil));

                contact_id = api.CreateRecord("contacts", contact, credenciales);

                return contact_id;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void CrearRelacion(Relacion relacionVinculacion, string cuit, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                JObject relacion = new JObject();

                if (cuit != null)
                    relacion.Add("new_CuentaId@odata.bind", "/accounts(" + cuit + ")");

                if (relacionVinculacion.cuentaVinculada != null)
                {
                    string cuenta_id = string.Empty;
                    string verificaCuenta = VerificarSocio(relacionVinculacion.cuentaVinculada.new_nmerodedocumento, api, credenciales);
                    if (verificaCuenta == string.Empty)
                    {
                        string resultadoSocio = CrearSocio(relacionVinculacion.cuentaVinculada, api, credenciales);
                        if (resultadoSocio != string.Empty)
                        {
                            cuenta_id = resultadoSocio;
                        }
                    }
                    else
                    {
                        cuenta_id = verificaCuenta;
                    }

                    relacion.Add("new_CuentaContactoVinculado_account@odata.bind", "/accounts(" + cuenta_id + ")");
                }

                if (relacionVinculacion.contactoVinculado != null)
                {
                    string contact_id = string.Empty;
                    string verificaContacto = VerificarContacto(relacionVinculacion.contactoVinculado.new_cuitcuil, api, credenciales);
                    if (verificaContacto == string.Empty)
                    {
                        string resultadoContacto = CrearContacto(relacionVinculacion.contactoVinculado, api, credenciales);
                        if (resultadoContacto != string.Empty)
                        {
                            contact_id = resultadoContacto;
                        }
                    }
                    else
                    {
                        contact_id = verificaContacto;
                    }

                    relacion.Add("new_CuentaContactoVinculado_contact@odata.bind", "/contacts(" + contact_id + ")");
                }

                if (relacionVinculacion.new_tipoderelacion != null)
                    relacion.Add("new_tipoderelacion", Convert.ToInt32(relacionVinculacion.new_tipoderelacion));

                if (relacionVinculacion.new_porcentajedeparticipacion > 0)
                    relacion.Add("new_porcentajedeparticipacion", relacionVinculacion.new_porcentajedeparticipacion);

                if (relacionVinculacion.new_porcentajebeneficiario > 0)
                    relacion.Add("new_porcentajebeneficiario", relacionVinculacion.new_porcentajebeneficiario);

                if (relacionVinculacion.new_observaciones != null)
                    relacion.Add("new_observaciones", relacionVinculacion.new_observaciones);

                api.CreateRecord("new_participacionaccionarias", relacion, credenciales);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void CrearDocumentacionPorCuenta(Documentacion documentacion, string socio_id, string documento_id, ApiDynamics api, Credenciales credenciales)
        {
            JObject documento = new JObject();

            if (documentacion.new_name != null)
                documento.Add("new_name", documentacion.new_name);

            if (documento_id != null && documento_id != string.Empty)
                documento.Add("new_DocumentoId@odata.bind", "/new_documentacions(" + documento_id + ")");

            if (socio_id != null)
                documento.Add("new_CuentaId@odata.bind", "/accounts(" + socio_id + ")");

            if (documentacion.statuscode != null)
                documento.Add("statuscode", Convert.ToInt32(documentacion.statuscode));

            if (documentacion.new_vinculocompartido != null)
                documento.Add("new_vinculocompartido", documentacion.new_vinculocompartido);

            if (documentacion.new_fechadevencimiento != null)
                documento.Add("new_fechadevencimiento", documentacion.new_fechadevencimiento);

            api.CreateRecord("new_documentacionporcuentas", documento, credenciales);
        }
        public static string CrearDocumento(Documentacion documentacion, ApiDynamics api, Credenciales credenciales)
        {
            JObject documento = new JObject();

            if (documentacion.new_estadodelsocio != 0)
                documento.Add("new_estadodelsocio", documentacion.new_estadodelsocio);

            if (documentacion.new_requeridoa != null)
                documento.Add("new_requeridoa", documentacion.new_requeridoa);

            if (documentacion.new_personeria != 0)
                documento.Add("new_personeria", documentacion.new_personeria);

            if (documentacion.new_grupoeconomico != null)
                documento.Add("new_grupoeconomico", documentacion.new_grupoeconomico);

            if (documentacion.new_fiador != null)
                documento.Add("new_fiador", documentacion.new_fiador);

            if (documentacion.new_descripcion != null)
                documento.Add("new_descripcion", documentacion.new_descripcion);

            if (documentacion.new_codigo != null)
                documento.Add("new_codigocasfog", documentacion.new_codigo);

            if (documentacion.new_condicionimpositiva != null)
                documento.Add("new_condicionimpositiva", documentacion.new_condicionimpositiva);

            if (documentacion.new_urlplantilla != null)
                documento.Add("new_urlplantilla", documentacion.new_urlplantilla);

            if (documentacion.new_tipodefiador != 0)
                documento.Add("new_tipodefiador", documentacion.new_tipodefiador);

            if (documentacion.new_name_documento != null)
                documento.Add("new_name", documentacion.new_name_documento);

            return api.CreateRecord("new_documentacions", documento, credenciales);
        }
        public static string CrearGarantia(Casfog_Sindicadas.Garantia garantia, OperacionSindicada op, string socio_id, string casfogG_id, ApiDynamics api, Credenciales credenciales,
            string acreedor_id = null, string divisa_id = null, string desembolsoAnterior_id = null)
        {
            decimal montoAvaladorSGR = 0;

            montoAvaladorSGR = op.importeAvaladoSgr;

            JObject aval = new();

            if (garantia.new_fechadevencimiento != null)
                aval.Add("new_fechadevencimiento", garantia.new_fechadevencimiento);

            if (garantia.new_tipodeoperacion != 0)
                aval.Add("new_tipodeoperacion", garantia.new_tipodeoperacion);

            if (socio_id != string.Empty)
                aval.Add("new_SocioParticipe@odata.bind", "/accounts(" + socio_id + ")");

            if (acreedor_id != string.Empty)
                aval.Add("new_Acreedor@odata.bind", "/new_acreedors(" + acreedor_id + ")");

            if (divisa_id != string.Empty)
                aval.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + divisa_id + ")");

            if (garantia.new_condesembolsosparciales == true)
                aval.Add("new_condesembolsosparciales", garantia.new_condesembolsosparciales);

            if (desembolsoAnterior_id != string.Empty && desembolsoAnterior_id != null)
                aval.Add("new_DesembolsoAnterior@odata.bind", "/new_garantias(" + desembolsoAnterior_id + ")");

            aval.Add("statuscode", 100000004); //En Cartera

            if (garantia.new_tipodegarantias != 0)
                aval.Add("new_tipodegarantias", garantia.new_tipodegarantias);

            if (montoAvaladorSGR != 0)
                aval.Add("new_monto", montoAvaladorSGR);

            if (garantia.new_fechadeorigen != null)
                aval.Add("new_fechadeorigen",  DateTime.Parse(garantia.new_fechadeorigen).ToString("yyyy-MM-dd"));

            if (garantia.new_dictamendelaval != 0)
                aval.Add("new_dictamendelaval", garantia.new_dictamendelaval);

            if (casfogG_id != string.Empty)
                aval.Add("new_casfoggarantiaid", casfogG_id);

            if (garantia.new_fechaemisindelcheque != null)
                aval.Add("new_fechaemisindelcheque", garantia.new_fechaemisindelcheque);

            if (garantia.new_sistemadeamortizacion != 0)
                aval.Add("new_sistemadeamortizacion", garantia.new_sistemadeamortizacion);

            if (garantia.new_tasa != 0)
                aval.Add("new_tasa", garantia.new_tasa);

            if (garantia.new_superatasabadlar == true)
                aval.Add("new_superatasabadlar", garantia.new_superatasabadlar);

            if (garantia.new_tasabadlar != 0)
                aval.Add("new_tasabadlar", garantia.new_tasabadlar);

            if (garantia.new_tasabarancaria != 0)
                aval.Add("new_tasabarancaria", garantia.new_tasabarancaria);

            if (garantia.new_puntosporcentuales != 0)
                aval.Add("new_puntosporcentuales", garantia.new_puntosporcentuales);

            if (garantia.new_periodicidadpagos != 0)
                aval.Add("new_periodicidadpagos", garantia.new_periodicidadpagos);

            if (!string.IsNullOrEmpty(garantia.new_observaciones))
                aval.Add("new_observaciones", garantia.new_observaciones);

            return api.CreateRecord("new_garantias", aval, credenciales);
        }
        public static string CrearSocio(Cuenta socio, ApiDynamics api, Credenciales credenciales, string tipoDocumento = null, string actividadAFIP = null,
            string condicionPyme = null, string categoria = null, string provincia = null, string pais = null)
        {
            JObject cuenta = new JObject();
            //General
            cuenta.Add("new_estadodelsocio", 100000000);//Activo
            cuenta.Add("new_creadaporapicasfog", true);

            if (socio.name != null)
                cuenta.Add("name", socio.name.Replace(".", ""));

            if (socio.new_nmerodedocumento != null)
                cuenta.Add("new_nmerodedocumento", socio.new_nmerodedocumento);

            if (socio.emailaddress1 != null)
                cuenta.Add("emailaddress1", socio.emailaddress1);

            if (socio.new_personeria != 0)
                cuenta.Add("new_personeria", socio.new_personeria);

            if (socio.new_rol != 0)
                cuenta.Add("new_rol", socio.new_rol);

            if (tipoDocumento != string.Empty)
                cuenta.Add("new_TipodedocumentoId@odata.bind", "/new_tipodedocumentos(" + tipoDocumento + ")");

            if (socio.new_productoservicio != null)
                cuenta.Add("new_productoservicio", socio.new_productoservicio);

            if (socio.new_tiposocietario != 0)
                cuenta.Add("new_tiposocietario", socio.new_tiposocietario);

            if (socio.new_condicionimpositiva != 0)
                cuenta.Add("new_condicionimpositiva", socio.new_condicionimpositiva);

            if (socio.new_inscripcionganancias != 0)
                cuenta.Add("new_inscripcionganancias", socio.new_inscripcionganancias);

            if (actividadAFIP != string.Empty)
                cuenta.Add("new_ActividadAFIP@odata.bind", "/new_actividadafips(" + actividadAFIP + ")");

            if (condicionPyme != string.Empty)
                cuenta.Add("new_CondicionPyme@odata.bind", "/new_condicionpymes(" + condicionPyme + ")");

            if (categoria != string.Empty)
                cuenta.Add("new_Categoria@odata.bind", "/new_categoracertificadopymes(" + categoria + ")");

            if (socio.new_facturacionultimoanio != 0)
                cuenta.Add("new_facturacionultimoanio", socio.new_facturacionultimoanio);

            //if (socio.new_fechadealta != null)
            //    cuenta.Add("new_fechadealta", socio.new_fechadealta);

            //if (socio.new_onboarding != null)
            //    cuenta.Add("new_onboarding", socio.new_onboarding);

            if (socio.new_essoloalyc != null)
                cuenta.Add("new_essoloalyc", socio.new_essoloalyc);

            //Direccion
            if (socio.telephone2 != null)
                cuenta.Add("telephone2", socio.telephone2);

            if (socio.address1_postalcode != null)
                cuenta.Add("address1_postalcode", socio.address1_postalcode);

            if (socio.address1_line1 != null)
                cuenta.Add("address1_line1", socio.address1_line1);

            if (socio.new_localidad != null)
                cuenta.Add("new_localidad", socio.new_localidad);

            if (socio.new_direccion1numero != null)
                cuenta.Add("new_direccion1numero", socio.new_direccion1numero);

            if (socio.address1_county != null)
                cuenta.Add("address1_county", socio.address1_county);

            if (socio.address1_name != null)
                cuenta.Add("address1_name", socio.address1_name);

            if (provincia != string.Empty)
                cuenta.Add("new_Provincia@odata.bind", "/new_provincias(" + provincia + ")");

            if (pais != string.Empty)
                cuenta.Add("new_Pais@odata.bind", "/new_paises(" + pais + ")");

            if (socio.new_nuevapyme != false)
                cuenta.Add("new_nuevapyme", socio.new_nuevapyme);

            cuenta.Add("new_calificacion", 100000001); //Aprobada

            cuenta.Add("new_estadodeactividad", 100000000); //Completa

            //if (socio.new_calificacion != 0)
            //    cuenta.Add("new_calificacion", socio.new_calificacion);

            //if (socio.new_estadodeactividad != 0)
            //    cuenta.Add("new_estadodeactividad", socio.new_estadodeactividad);

            return api.CreateRecord("accounts", cuenta, credenciales);
        }
        public static string CrearSocio(CuentaVinculada socio, ApiDynamics api, Credenciales credenciales)
        {

            JObject cuenta = new JObject();

            if (socio.name != null)
                cuenta.Add("name", socio.name);

            if (socio.new_nmerodedocumento != null)
                cuenta.Add("new_nmerodedocumento", socio.new_nmerodedocumento);

            if (socio.new_personeria != null)
                cuenta.Add("new_personeria", socio.new_personeria);

            if (socio.new_rol != null)
                cuenta.Add("new_rol", socio.new_rol);

            if (socio.new_tipodedocumentoid != null)
                cuenta.Add("new_TipodedocumentoId@odata.bind", "/new_tipodedocumentos(" + socio.new_tipodedocumentoid + ")");

            return api.CreateRecord("accounts", cuenta, credenciales);
        }
        public static string CrearCuota(Cuota planCuota, decimal montoCuota, decimal porcentaje, decimal amortizacion, string garantiaid, ApiDynamics api, Credenciales credenciales)
        {

            JObject cuota = new JObject();

            if (planCuota.new_interesperiodo != 0)
            {
                decimal interes = planCuota.new_interesperiodo * porcentaje;
                cuota.Add("new_interesperiodo", interes);
            }

            if (planCuota.new_numero != 0)
                cuota.Add("new_numero", planCuota.new_numero);

            if (montoCuota != 0)
            {
                //decimal monto = planCuota.new_montocuota * porcentaje;
                cuota.Add("new_montocuota", montoCuota);
            }
            else if (planCuota.new_montocuota == 0)
            {
                cuota.Add("new_montocuota", 0);
            }

            if (planCuota.new_fechadevencimiento != null)
                cuota.Add("new_fechadevencimiento", planCuota.new_fechadevencimiento);

            if (planCuota.new_fechaestimadapago != null)
                cuota.Add("new_fechaestimadapago", planCuota.new_fechaestimadapago);

            if (planCuota.new_montooperativo != 0)
                cuota.Add("new_montooperativo", planCuota.new_montooperativo);

            if (planCuota.new_montovigente != 0)
                cuota.Add("new_montovigente", planCuota.new_montovigente);

            //if (planCuota.statuscode != 0)
            //    cuota.Add("statuscode", planCuota.statuscode);

            if (garantiaid != string.Empty)
                cuota.Add("new_Garantia@odata.bind", "/new_garantias(" + garantiaid + ")");

            if (amortizacion > 0 || amortizacion == 0)
                cuota.Add("new_amortizacion", amortizacion);

            return api.CreateRecord("new_prestamoses", cuota, credenciales);
        }
        public static string CrearLimite(string socio_id, string nombreSocio, int tipoDeOperacion, decimal montoGarantia, string fechaCarga, string fechaVencimiento,
            string nroExpedienteTAD, ApiDynamics api, Credenciales credenciales, string divisa_id = null, string montoGarantiaF = null, bool desembolsoInexistente = false)
        {
            string resultado = string.Empty;
            bool validacion = true;

            JArray limites = BuscarLimitePorLineaGeneral(socio_id, api, credenciales);
            if (limites.Count > 0)
            {
                List<Limite> listaLimites = ArmarLimite(limites);

                if (listaLimites.Count > 0)
                {
                    DateTime fechaHoy = DateTime.Now;
                    listaLimites.ForEach(lim =>
                    {
                        if (lim.new_lineatipodeoperacion == tipoDeOperacion && lim.new_vigenciahasta >= fechaHoy)
                        {
                            validacion = false;

                            if (lim.new_limitecomercialdivisa != 0 && lim.new_limitecomercialdivisa < montoGarantia)
                            {
                                //string asunto = $"El límite {lim.new_lineatipodeoperacion_value} de {nombreSocio} es inferior.";
                                //string descripcion = $"Para poder generar la garantía pública debes subir el límite {lim.new_lineatipodeoperacion_value} a {montoGarantiaF}";
                                //string resultadoTarea = CrearTarea(asunto, socio_id, descripcion, api, credenciales);
                                CrearModificacionLimiteComercial(lim.new_productosid, divisa_id, montoGarantia, lim.new_limitecomercialdivisa, desembolsoInexistente, credenciales);
                                //Generar Actividad
                            }
                        }
                    });
                   
                }
            }

            if (validacion)
            {
                JObject limite = new JObject();

                limite.Add("new_Cuenta@odata.bind", "/accounts(" + socio_id + ")");
                limite.Add("new_lineatipodeoperacion", tipoDeOperacion);
                limite.Add("new_fechadealta", DateTime.Now.ToString("yyyy-MM-dd"));

                if (montoGarantia != 0)
                    limite.Add("new_limitecomercialdivisa", montoGarantia);
                if (fechaCarga != null)
                {
                    limite.Add("new_fechadeaprobacionlimite", fechaCarga);
                    limite.Add("new_fechadeaprobacionporconsejo", fechaCarga);
                    limite.Add("new_fechadeinstrumentacion", fechaCarga);
                }
                if (fechaVencimiento != null)
                    limite.Add("new_vigenciahasta", fechaVencimiento);
                if (nroExpedienteTAD != null)
                    limite.Add("new_observaciones", nroExpedienteTAD);

                resultado = api.CreateRecord("new_productoses", limite, credenciales);
            }

            return resultado;
        }
        public static string CrearTarea(string asunto, string socio_id, string descripcion, ApiDynamics api, Credenciales credenciales)
        {
            JObject Tarea = new JObject();

            Tarea.Add("subject", asunto);
            Tarea.Add("regardingobjectid_account@odata.bind", "/accounts(" + socio_id + ")");
            Tarea.Add("description", descripcion);

            return api.CreateRecord("tasks", Tarea, credenciales);
        }
        public static string Monetizar(Casfog_Sindicadas.Garantia garantia, string garantiaid, ApiDynamics api, Credenciales credenciales)
        {
            JObject aval = new();

            if (garantia.new_fechadenegociacion != null)
                aval.Add("new_fechadenegociacion", DateTime.Parse(garantia.new_fechadenegociacion).ToString("yyyy-MM-dd"));

            return api.UpdateRecord("new_garantias", garantiaid, aval, credenciales);
        }
        public static string ActualizarGarantia(string garantiaid, decimal importeAvaladoSGR, ApiDynamics api, Credenciales credenciales)
        {
            JObject aval = new JObject();

            if (importeAvaladoSGR != 0)
                aval.Add("new_monto", importeAvaladoSGR);

            return api.UpdateRecord("new_garantias", garantiaid, aval, credenciales);
        }
        public static void CrearCertificados(List<CertificadoPyme> certificados, string socio_id, ApiDynamics api, Credenciales credenciales,
            string condicionpyme_id = null, string categoria_id = null)
        {
            if (certificados.Count > 0)
            {
                foreach (var certificado in certificados)
                {
                    JObject Certificado = new()
                    {
                        { "new_aprobacion1", 100000000 }
                    };

                    if (socio_id != string.Empty)
                        Certificado.Add("new_SocioParticipe@odata.bind", "/accounts(" + socio_id + ")");

                    if (certificado.new_numeroderegistro != 0)
                        Certificado.Add("new_numeroderegistro", certificado.new_numeroderegistro);

                    if (certificado.new_fechadeemision != null)
                        Certificado.Add("new_fechadeemision", certificado.new_fechadeemision);

                    if (certificado.new_vigenciadesde != null)
                        Certificado.Add("new_vigenciadesde", certificado.new_vigenciadesde);

                    if (certificado.new_vigenciahasta != null)
                        Certificado.Add("new_vigenciahasta", certificado.new_vigenciahasta);

                    if (condicionpyme_id != string.Empty)
                        Certificado.Add("new_SectorEconomico@odata.bind", "/new_condicionpymes(" + condicionpyme_id + ")");

                    if (categoria_id != string.Empty)
                        Certificado.Add("new_Categoria@odata.bind", "/new_categoracertificadopymes(" + categoria_id + ")");

                    api.CreateRecord("new_certificadopymes", Certificado, credenciales);
                }
            }
        }

        public static void CrearModificacionLimiteComercial(string limite_id, string divisa_id, decimal monto, decimal montoAnterior, bool desembolsoInexistente, Credenciales credenciales)
        {
            ApiDynamics api = new();
            decimal montoLimite = 0;
            if (!desembolsoInexistente)
            {
                montoLimite = monto - montoAnterior;
            }
            else
            {
                montoLimite = monto;
            }

            if (montoLimite < 0)
                montoLimite *= -1;

            if (montoLimite > 0 || montoLimite == 0)
                montoLimite += 1;

            if (montoLimite > 0)
            {
                JObject modificacion = new()
                {
                    { "new_modificacionde", 100000000 }, //Limite Comercial
                    { "new_importeincremental", montoLimite },
                    { "new_Limiteporlinea@odata.bind", "/new_productoses(" + limite_id + ")" }
                };

                if (divisa_id != string.Empty)
                    modificacion.Add("new_divisa@odata.bind", "/transactioncurrencies(" + divisa_id + ")");

                api.CreateRecord("new_modificaciondelimitecomercials", modificacion, credenciales);
            }
            else
            {
                new Excepciones(credenciales.cliente, "El monto de la modificación del limite es inválido " + montoLimite);
            }
        }
        public static string CrearAcreedor(AcreedorVinculado acreedor, ApiDynamics api, Credenciales credenciales)
        {
            JObject Acreedor = new JObject();

            if (acreedor.new_name != null)
                Acreedor.Add("new_name", acreedor.new_name);
            if (acreedor.new_cuit != null)
                Acreedor.Add("new_cuit", acreedor.new_cuit);
            if (acreedor.new_tipodeacreedor >= 0)
                Acreedor.Add("new_tipodeacreedor", acreedor.new_tipodeacreedor);

            return api.CreateRecord("new_acreedors", Acreedor, credenciales);
        }
        public static string EnviarAMonetizarGarantiaEnSGR(string operacion_id, ApiDynamics api, Credenciales credenciales)
        {
            string resultado = string.Empty;

            JArray operacion = BuscarOperacionSindicada(operacion_id, api, credenciales);

            if(operacion.Count > 0)
            {
                OP op = ArmarOP(operacion);
                if(!op.new_garantiamonetizada)
                {
                    JObject opSindicada = new() 
                    {
                        { "new_garantiamonetizada", true }
                    };

                    resultado = api.UpdateRecord("new_operacionsindicadas", operacion_id, opSindicada, credenciales);
                }
            }

            return resultado;
        }
        public static string ObtenerTipoDocumentoID(JToken tipoDocumentoJT)
        {
            string tipoDocumento_id = string.Empty;

            TipoDocumento tipoDocumento = JsonConvert.DeserializeObject<TipoDocumento>(tipoDocumentoJT.First().ToString());
            if (tipoDocumento.new_tipodedocumentoid != null)
                tipoDocumento_id = tipoDocumento.new_tipodedocumentoid;

            return tipoDocumento_id;
        }
        public static string ObtenerActividadAfipID(JToken actividadAfipJT)
        {
            string actividadAfip_id = string.Empty;

            ActividadAFIP actividadAfip = JsonConvert.DeserializeObject<ActividadAFIP>(actividadAfipJT.First().ToString());
            if (actividadAfip.new_actividadafipid != null)
                actividadAfip_id = actividadAfip.new_actividadafipid;

            return actividadAfip_id;
        }
        public static string ObtenerCondicionPymeID(JToken condicionPymeJT)
        {
            string condicionPyme_id = string.Empty;

            CondicionPyme condicionPyme = JsonConvert.DeserializeObject<CondicionPyme>(condicionPymeJT.First().ToString());
            if (condicionPyme.new_condicionpymeid != null)
                condicionPyme_id = condicionPyme.new_condicionpymeid;

            return condicionPyme_id;
        }
        public static string ObtenerCategoriaID(JToken categoriaJT)
        {
            string categoria_id = string.Empty;

            Categoria categoria = JsonConvert.DeserializeObject<Categoria>(categoriaJT.First().ToString());
            if (categoria.new_categoracertificadopymeid != null)
                categoria_id = categoria.new_categoracertificadopymeid;

            return categoria_id;
        }
        public static string ObtenerAcreedorID(JToken acreedorJT)
        {
            string acreedor_id = string.Empty;

            Acreedor acreedor = JsonConvert.DeserializeObject<Acreedor>(acreedorJT.First().ToString());
            if (acreedor.new_acreedorid != null)
                acreedor_id = acreedor.new_acreedorid;

            return acreedor_id;
        }
        public static string ObtenerDivisaID(JToken divisaJT)
        {
            string divisa_id = string.Empty;

            Divisa divisa = JsonConvert.DeserializeObject<Divisa>(divisaJT.First().ToString());
            if (divisa.transactioncurrencyid != null)
                divisa_id = divisa.transactioncurrencyid;

            return divisa_id;
        }
        public static string ObtenerProvinciaID(JToken provinciaJT)
        {
            string provincia_id = string.Empty;

            Provincia provincia = JsonConvert.DeserializeObject<Provincia>(provinciaJT.First().ToString());
            if (provincia.new_provinciaid != null)
                provincia_id = provincia.new_provinciaid;

            return provincia_id;
        }
        public static string ObtenerPaisID(JToken paisJT)
        {
            string pais_id = string.Empty;

            Pais pais = JsonConvert.DeserializeObject<Pais>(paisJT.First().ToString());

            if (pais.new_paisid != null)
                pais_id = pais.new_paisid;

            return pais_id;
        }
        public static string ObtenerDocumentoID(JToken documentoJT)
        {
            string documento_id = string.Empty;

            Documento documento = JsonConvert.DeserializeObject<Documento>(documentoJT.First().ToString());

            if (documento.new_documentacionid != null)
                documento_id = documento.new_documentacionid;

            return documento_id;
        }
        public static async Task<JArray> BuscarCertificadosPymesPorSocio(string socio_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_certificadopymes";

                fetchXML = "<entity name='new_certificadopyme'>" +
                                                           "<attribute name='new_certificadopymeid'/> " +
                                                           "<attribute name='new_vigenciahasta'/> " +
                                                           "<attribute name='statuscode'/> " +
                                                           "<attribute name='new_numeroderegistro'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_socioparticipe' operator='eq' value='{socio_id}' />" +
                                                           "</filter>" +
                                                "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static List<CertificadosPymesV2> ArmarCertificadosPymes(JToken certificadosPymes)
        {
            return JsonConvert.DeserializeObject<List<CertificadosPymesV2>>(certificadosPymes.ToString());
        }
        public static async Task AsociarContactoASocio(string socio_id, string? firmante_id, string? contactoNotificaciones_id, string nombreSocio, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JObject actualizarCuenta = new();

                if (!string.IsNullOrEmpty(contactoNotificaciones_id))
                    actualizarCuenta.Add("new_ContactodeNotificaciones@odata.bind", "/contacts(" + contactoNotificaciones_id + ")");

                if (!string.IsNullOrEmpty(firmante_id))
                    actualizarCuenta.Add("new_ContactoFirmante@odata.bind", "/contacts(" + firmante_id + ")");

                ResponseAPI responseAPI = await api.UpdateRecord("accounts", socio_id, actualizarCuenta, credenciales);
                if (!responseAPI.ok)
                    throw new Exception(responseAPI.descripcion);
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al asociar contacto al socio {nombreSocio} - {ex.Message}");
                throw;
            }
        }
    }
}
