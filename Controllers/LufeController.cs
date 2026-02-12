using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Api.Web.Dynamics365.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using static Api.Web.Dynamics365.Controllers.SgrOneClickController;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;
using static Api.Web.Dynamics365.Models.Documents;
using static Api.Web.Dynamics365.Models.Lufe;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class LufeController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private ApiLufe APIlufe = new();
        public LufeController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/consultarentidad")]
        public async Task<IActionResult> ObtenerEntidad(string cuit)
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
                Entidad entidad = new();
                string apiKey = string.Empty;
                int anioActual = DateTime.Now.Year + 1;
                List<DocumentosEntidad> listaDocumentosEntidad = new();
                List<DocumentoLufe> listaDocumentosLufe = new();

                JArray UnidadDeNegocio = await BuscarApiKeyLufe(api, credenciales);
                if (UnidadDeNegocio.Count > 0)
                    apiKey = ObtenerApiKey(UnidadDeNegocio);
                var cuitI = Convert.ToInt64(cuit);
                string respuesta = await APIlufe.GetEntidad("Cliente", cuitI, apiKey);
                if (!string.IsNullOrEmpty(respuesta))
                {
                    entidad = JsonConvert.DeserializeObject<Entidad>(respuesta);
                    if(entidad.cuit == 0)
                    {
                        return Ok("No existe la entidad con el CUIT indicado.");
                    }
                }

                for (int anio = anioActual; anio >= 2019; anio--)
                {
                    string respuestaDocumentosPeriodo = await APIlufe.GetDocumentosPorPeriodo("Cliente", cuitI, anio.ToString(), apiKey);
                    if (!string.IsNullOrEmpty(respuestaDocumentosPeriodo))
                    {
                        DocumentosEntidad documentosPeriodo = JsonConvert.DeserializeObject<DocumentosEntidad>(respuestaDocumentosPeriodo);
                        if (documentosPeriodo?.documentos?.Length > 0)
                        {
                            DocumentosEntidad documentosEntidad = new()
                            {
                                periodo = anio.ToString(),
                                documentos = documentosPeriodo.documentos
                            };
                            listaDocumentosEntidad.Add(documentosEntidad);
                            foreach(var doc in documentosPeriodo.documentos)
                            {
                                doc.nombre = doc.nombre + " - " + anio.ToString();
                            }
                            listaDocumentosLufe.AddRange(documentosPeriodo.documentos.ToList());
                            var documentoAcuerdo = documentosPeriodo.documentos.FirstOrDefault(d => d.nombre.Contains("Presentación Única de Balances", StringComparison.OrdinalIgnoreCase));
                            if (documentoAcuerdo != null)
                            {
                                entidad.todosDocumentos = listaDocumentosEntidad.ToArray();
                                entidad.documentos = listaDocumentosLufe.ToArray();
                                break;
                            }
                        }
                    }

                    if(anio == 2019 && listaDocumentosEntidad.Count > 0)
                    {
                        entidad.todosDocumentos = listaDocumentosEntidad.ToArray();
                        entidad.documentos = listaDocumentosLufe.ToArray();
                    }
                }
                string respuestaAutoridades = await APIlufe.GetAutoridades("Cliente", cuitI, apiKey);
                if (!string.IsNullOrEmpty(respuestaAutoridades))
                {
                    Autoridad[] autoridades = JsonConvert.DeserializeObject<Autoridad[]>(respuestaAutoridades);
                    entidad.autoridad = autoridades;
                }
                return Ok(entidad);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/generarentidad")]
        public async Task<IActionResult> GenerarEntidad(OnboardingLufe onboardingLufe)
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
                string cuenta_id = string.Empty;
                string actividadAfip_id = string.Empty;
                string apiKey = string.Empty;
                int anioActual = DateTime.Now.Year + 1;
                List<DocumentosEntidad> listaDocumentosEntidad = new();

                JArray UnidadDeNegocio = await BuscarApiKeyLufe(api, credenciales);
                if (UnidadDeNegocio.Count > 0)
                    apiKey = ObtenerApiKey(UnidadDeNegocio);

                var cuitI = Convert.ToInt64(onboardingLufe.cuit);
                string respuesta = await APIlufe.GetEntidad("Cliente", cuitI, apiKey);

                if (!string.IsNullOrEmpty(respuesta))
                {
                    Entidad entidad = JsonConvert.DeserializeObject<Entidad>(respuesta);

                    if (entidad.cuit == 0)
                    {
                        return Ok("No existe la entidad con el CUIT indicado.");
                    }

                    Casfog_Sindicadas.Socio _socio = await VerificarSocio(onboardingLufe.cuit, api, credenciales);

                    if (_socio == null || _socio.accountid == null)
                    {
                        cuenta_id = await CrearSocio(entidad, api, credenciales);
                    }
                    else
                    {
                        cuenta_id = _socio.accountid;
                    }

                    if (!string.IsNullOrEmpty(cuenta_id))
                    {
                        if (entidad.contactos.Length > 0)
                        {
                            for (int i = 0; i < entidad.contactos.Length; i++)
                            {
                                await CrearContactos(entidad.contactos[i], cuenta_id, api, credenciales);
                            }
                        }

                        ///CREAMOS CERTIFICADO PYME VIGENTE
                        await CrearCertificado(entidad.certificado_pyme, cuenta_id, api, credenciales);

                        string respuestaAutoridades = await APIlufe.GetAutoridades("Cliente", cuitI, apiKey);
                        if (!string.IsNullOrEmpty(respuestaAutoridades))
                        {
                            Autoridad[] autoridades = JsonConvert.DeserializeObject<Autoridad[]>(respuestaAutoridades);
                            if (autoridades.Length > 0)
                            {
                                for (int i = 0; i < autoridades.Length; i++)
                                {
                                    await CrearRelacion(autoridades[i], cuenta_id, api, credenciales);
                                }
                            }
                        }

                        //NUEVO DESARROLLO
                        for (int anio = anioActual; anio >= 2019; anio--)
                        {
                            string respuestaDocumentosPeriodo = await APIlufe.GetDocumentosPorPeriodo("Cliente", cuitI, anio.ToString(), apiKey);
                            if (!string.IsNullOrEmpty(respuestaDocumentosPeriodo))
                            {
                                DocumentosEntidad documentosPeriodo = JsonConvert.DeserializeObject<DocumentosEntidad>(respuestaDocumentosPeriodo);
                                if (documentosPeriodo?.documentos?.Length > 0)
                                {
                                    DocumentosEntidad documentosEntidad = new()
                                    {
                                        periodo = anio.ToString(),
                                        documentos = documentosPeriodo.documentos
                                    };
                                    listaDocumentosEntidad.Add(documentosEntidad);
                                    var documentoAcuerdo = documentosPeriodo.documentos.FirstOrDefault(d => d.nombre.Contains("Presentación Única de Balances", StringComparison.OrdinalIgnoreCase));
                                    if (documentoAcuerdo != null)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        if (listaDocumentosEntidad.Count > 0)
                        {
                            await GenerarTodosDocumentacionAlSocio(listaDocumentosEntidad.ToArray(), cuenta_id, api, credenciales);
                        }

                        string respuestaIndicadores = await APIlufe.GetIndicadores("Cliente", cuitI, apiKey);
                        if (!string.IsNullOrEmpty(respuestaIndicadores))
                        {
                            Indicador Indicador = JsonConvert.DeserializeObject<Indicador>(respuestaIndicadores);
                            await CrearIndicadores(Indicador, cuenta_id, api, credenciales);
                        }

                        string respuestaIndicadoresPostBalance = await APIlufe.GetIndicadoresPostBalance("Cliente", cuitI, apiKey);
                        if (!string.IsNullOrEmpty(respuestaIndicadoresPostBalance))
                        {
                            IndicadorPostBalance Indicador = JsonConvert.DeserializeObject<IndicadorPostBalance>(respuestaIndicadoresPostBalance);
                            await CrearIndicadoresPostBalance(Indicador, cuenta_id, api, credenciales);
                        }

                    };

                    return Ok("Entidad generada con exito.");
                }

                return Ok("No existe la entidad con el CUIT indicado.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/onboardinglufe")]
        public async Task<IActionResult> OnboardingLufe(OnboardingLufe onboardingLufe)
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
                string cuenta_id = string.Empty;
                string actividadAfip_id = string.Empty;
                string apiKey = string.Empty;
                int anioActual = DateTime.Now.Year + 1;
                List<DocumentosEntidad> listaDocumentosEntidad = new();

                JArray UnidadDeNegocio = await BuscarApiKeyLufe(api, credenciales);
                if (UnidadDeNegocio.Count > 0)
                    apiKey = ObtenerApiKey(UnidadDeNegocio);
                var cuitI = Convert.ToInt64(onboardingLufe.cuit);
                string respuesta = await APIlufe.GetEntidad("Cliente", cuitI, apiKey);

                if (!string.IsNullOrEmpty(respuesta))
                {
                    Entidad entidad = JsonConvert.DeserializeObject<Entidad>(respuesta);

                    if (entidad.cuit == 0)
                    {
                        return Ok("No existe la entidad con el CUIT indicado.");
                    }

                    Casfog_Sindicadas.Socio _socio = await VerificarSocio(onboardingLufe.cuit, api, credenciales);

                    if (_socio == null || _socio.accountid == null)
                    {
                        cuenta_id = await CrearSocioOnboarding(entidad, onboardingLufe.email, onboardingLufe.tipoDocumento, api, credenciales);
                    }
                    else
                    {
                        cuenta_id = _socio.accountid;
                    }

                    if (!string.IsNullOrEmpty(cuenta_id))
                    {
                        if (entidad.contactos.Length > 0)
                        {
                            for (int i = 0; i < entidad.contactos.Length; i++)
                            {
                                await CrearContactos(entidad.contactos[i], cuenta_id, api, credenciales);
                            }
                        }

                        ///CREAMOS CERTIFICADO PYME VIGENTE
                        await CrearCertificado(entidad.certificado_pyme, cuenta_id, api, credenciales);

                        string respuestaAutoridades = await APIlufe.GetAutoridades("Cliente", cuitI, apiKey);
                        if (!string.IsNullOrEmpty(respuestaAutoridades))
                        {
                            Autoridad[] autoridades = JsonConvert.DeserializeObject<Autoridad[]>(respuestaAutoridades);
                            if (autoridades.Length > 0)
                            {
                                for (int i = 0; i < autoridades.Length; i++)
                                {
                                    await CrearRelacion(autoridades[i], cuenta_id, api, credenciales);
                                }
                            }
                        }

                        //NUEVO DESARROLLO
                        for (int anio = anioActual; anio >= 2019; anio--)
                        {
                            string respuestaDocumentosPeriodo = await APIlufe.GetDocumentosPorPeriodo("Cliente", cuitI, anio.ToString(), apiKey);
                            if (!string.IsNullOrEmpty(respuestaDocumentosPeriodo))
                            {
                                DocumentosEntidad documentosPeriodo = JsonConvert.DeserializeObject<DocumentosEntidad>(respuestaDocumentosPeriodo);
                                if (documentosPeriodo?.documentos?.Length > 0)
                                {
                                    DocumentosEntidad documentosEntidad = new()
                                    {
                                        periodo = anio.ToString(),
                                        documentos = documentosPeriodo.documentos
                                    };
                                    listaDocumentosEntidad.Add(documentosEntidad);
                                    var documentoAcuerdo = documentosPeriodo.documentos.FirstOrDefault(d => d.nombre.Contains("Presentación Única de Balances", StringComparison.OrdinalIgnoreCase));
                                    if (documentoAcuerdo != null)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        if (listaDocumentosEntidad.Count > 0)
                        {
                            await GenerarTodosDocumentacionAlSocio(listaDocumentosEntidad.ToArray(), cuenta_id, api, credenciales);
                        }

                        string respuestaIndicadores = await APIlufe.GetIndicadores("Cliente", cuitI, apiKey);
                        if (!string.IsNullOrEmpty(respuestaIndicadores))
                        {
                            Indicador Indicador = JsonConvert.DeserializeObject<Indicador>(respuestaIndicadores);
                            await CrearIndicadores(Indicador, cuenta_id, api, credenciales);
                        }

                        string respuestaIndicadoresPostBalance = await APIlufe.GetIndicadoresPostBalance("Cliente", cuitI, apiKey);
                        if (!string.IsNullOrEmpty(respuestaIndicadoresPostBalance))
                        {
                            IndicadorPostBalance Indicador = JsonConvert.DeserializeObject<IndicadorPostBalance>(respuestaIndicadoresPostBalance);
                            await CrearIndicadoresPostBalance(Indicador, cuenta_id, api, credenciales);
                        }
                    };

                    return Ok("Entidad generada con exito");
                }

                return Ok("No existe la entidad con el CUIT indicado.");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/onbdocumentosyautoridadeslufe")]
        public async Task<IActionResult> OnboardingDocsYAut(OnboardingLufe onboardingLufe)
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
                string cuenta_id = string.Empty;
                string actividadAfip_id = string.Empty;
                string apiKey = string.Empty;
                int anioActual = DateTime.Now.Year + 1;
                List<DocumentosEntidad> listaDocumentosEntidad = new();
                JArray UnidadDeNegocio = await BuscarApiKeyLufe(api, credenciales);
                if (UnidadDeNegocio.Count > 0)
                    apiKey = ObtenerApiKey(UnidadDeNegocio);

                var cuitI = Convert.ToInt64(onboardingLufe.cuit);
                string respuesta = await APIlufe.GetEntidad("Cliente", cuitI, apiKey);

                if (!string.IsNullOrEmpty(respuesta))
                {
                    Entidad entidad = JsonConvert.DeserializeObject<Entidad>(respuesta);

                    if (!string.IsNullOrEmpty(onboardingLufe.accountid))
                    {
                        ///CREAMOS CERTIFICADO PYME VIGENTE
                        await CrearCertificado(entidad.certificado_pyme, onboardingLufe.accountid, api, credenciales);

                        string respuestaAutoridades = await APIlufe.GetAutoridades("Cliente", cuitI, apiKey);
                        if (!string.IsNullOrEmpty(respuestaAutoridades))
                        {
                            Autoridad[] autoridades = JsonConvert.DeserializeObject<Autoridad[]>(respuestaAutoridades);
                            if (autoridades.Length > 0)
                            {
                                for (int i = 0; i < autoridades.Length; i++)
                                {
                                    if(autoridades[i].es_accionista == 1)
                                        await CrearRelacion(autoridades[i], onboardingLufe.accountid, api, credenciales);
                                }
                            }
                        }

                        //NUEVO DESARROLLO
                        for (int anio = anioActual; anio >= 2019; anio--)
                        {
                            string respuestaDocumentosPeriodo = await APIlufe.GetDocumentosPorPeriodo("Cliente", cuitI, anio.ToString(), apiKey);
                            if (!string.IsNullOrEmpty(respuestaDocumentosPeriodo))
                            {
                                DocumentosEntidad documentosPeriodo = JsonConvert.DeserializeObject<DocumentosEntidad>(respuestaDocumentosPeriodo);
                                if (documentosPeriodo?.documentos?.Length > 0)
                                {
                                    DocumentosEntidad documentosEntidad = new()
                                    {
                                        periodo = anio.ToString(),
                                        documentos = documentosPeriodo.documentos
                                    };
                                    listaDocumentosEntidad.Add(documentosEntidad);
                                    var documentoAcuerdo = documentosPeriodo.documentos.FirstOrDefault(d => d.nombre.Contains("Presentación Única de Balances", StringComparison.OrdinalIgnoreCase));
                                    if (documentoAcuerdo != null)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        if (listaDocumentosEntidad.Count > 0)
                        {
                            await GenerarTodosDocumentacionAlSocio(listaDocumentosEntidad.ToArray(), onboardingLufe.accountid, api, credenciales);
                        }

                        string respuestaIndicadores = await APIlufe.GetIndicadores("Cliente", cuitI, apiKey);
                        if (!string.IsNullOrEmpty(respuestaIndicadores))
                        {
                            Indicador Indicador = JsonConvert.DeserializeObject<Indicador>(respuestaIndicadores);
                            await CrearIndicadores(Indicador, onboardingLufe.accountid, api, credenciales);
                        }

                        string respuestaIndicadoresPostBalance = await APIlufe.GetIndicadoresPostBalance("Cliente", cuitI, apiKey);
                        if (!string.IsNullOrEmpty(respuestaIndicadoresPostBalance))
                        {
                            IndicadorPostBalance Indicador = JsonConvert.DeserializeObject<IndicadorPostBalance>(respuestaIndicadoresPostBalance);
                            await CrearIndicadoresPostBalance(Indicador, onboardingLufe.accountid, api, credenciales);
                        }
                    };

                    return Ok("Operación exitosa.");
                }

                return Ok("Entidad inexistente");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/consultarautoridades")]
        public async Task<IActionResult> ConsultarAutoridades(string cuit)
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
                string apiKey = string.Empty;
                JArray UnidadDeNegocio = await BuscarApiKeyLufe(api, credenciales);
                if (UnidadDeNegocio.Count > 0)
                    apiKey = ObtenerApiKey(UnidadDeNegocio);
                var cuitI = Convert.ToInt64(cuit);
                string respuesta = await APIlufe.GetAutoridades("Cliente", cuitI, apiKey);

                List<Autoridad> listaAutoridades = JsonConvert.DeserializeObject<List<Autoridad>>(respuesta);

                return Ok(listaAutoridades);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/consultardocumentos")]
        public async Task<IActionResult> ConsultarDocumentos(string cuit, string socio_id)
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
                var cuitI = Convert.ToInt64(cuit);
                ApiDynamicsV2 api = new();
                string apiKey = string.Empty;
                JArray UnidadDeNegocio = await BuscarApiKeyLufe(api, credenciales);
                if (UnidadDeNegocio.Count > 0)
                    apiKey = ObtenerApiKey(UnidadDeNegocio);
                string resp = await APIlufe.GetDocumentos("Cliente", cuitI, apiKey);
                DocumentosEntidad documentos = JsonConvert.DeserializeObject<DocumentosEntidad>(resp);

                List<Documento> listaDocumentosD65 = new();
                List<DocumentacionPorCuentaLufe> listaDocumentosPorCuenta = new();
                JArray documentosD365 = await BuscarDocumentacion(api, credenciales);
                if (documentosD365.Count > 0)
                    listaDocumentosD65 = ArmarDocumentacion(documentosD365);
                JArray documentosPorCuenta = await BuscarDocumentacionPorCuenta(socio_id, api, credenciales);
                if (documentosPorCuenta.Count > 0)
                    listaDocumentosPorCuenta = ArmarDocumentacionPorcuenta(documentosPorCuenta);

                for (int i = 0; i < documentos.documentos.Length; i++)
                {
                    string documento_id = string.Empty;
                    var documento = documentos.documentos[i];
                    string nombreDocumento = $"{documento.nombre} - Lufe";
                    if (listaDocumentosD65.FirstOrDefault(x => x.new_name == nombreDocumento) != null)
                    {
                        documento_id = listaDocumentosD65.FirstOrDefault(x => x.new_name == nombreDocumento).new_documentacionid;
                    }
                    else
                    {
                        documento_id = await CrearDocumento(nombreDocumento, api, credenciales);
                    }

                    if(listaDocumentosPorCuenta.Count == 0 || (listaDocumentosPorCuenta.Count > 0 && 
                        listaDocumentosPorCuenta.FirstOrDefault(x => x.new_documentoid == documento_id) == null))
                    {
                        string documentacionporcuenta_id = await CrearDocumentacionPorCuenta(nombreDocumento, documento_id, socio_id, api, credenciales);

                        byte[] respuestaByte = await APIlufe.GetBase64Document("cliente", documento.url);
                        string respuestaBase64 = System.Convert.ToBase64String(respuestaByte);
                        JObject annotation = new()
                        {
                            { "subject", documento.nombre },
                            { "mimetype",  @"application/pdf" },
                            { "documentbody", respuestaBase64 },
                            { "filename", $"{documento.nombre}.pdf" },
                        };

                        if (!string.IsNullOrEmpty(documentacionporcuenta_id))
                            annotation.Add("objectid_new_documentacionporcuenta@odata.bind", "/new_documentacionporcuentas(" + documentacionporcuenta_id + ")");

                        ResponseAPI notaResponse = await api.CreateRecord("annotations", annotation, credenciales);
                    }
                }

                return Ok("EXITO");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/consultarindicador")]
        public async Task<IActionResult> ConsultarIndicadores(string cuit, string? socio_id)
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
                var cuitI = Convert.ToInt64(cuit);
                ApiDynamicsV2 api = new();
                string apiKey = string.Empty;
                JArray UnidadDeNegocio = await BuscarApiKeyLufe(api, credenciales);
                if (UnidadDeNegocio.Count > 0)
                    apiKey = ObtenerApiKey(UnidadDeNegocio);
                string fechaPresentacion = string.Empty;
                string resp = await APIlufe.GetIndicadores("Cliente", cuitI, apiKey);

                Indicador Indicador = JsonConvert.DeserializeObject<Indicador>(resp);
                if(Indicador.fechapresentacion > 0)
                    fechaPresentacion = DateTime.ParseExact(Indicador.fechapresentacion.ToString(), "yyyyMMdd",
                CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                JObject _Indicador = new()
                {
                    { "new_Cuenta@odata.bind", "/accounts(" + socio_id + ")"},
                    { "new_periodo", Indicador.periodo.ToString() },
                    { "new_rentabilidad", Indicador.rentabilidad },
                    { "new_ebit_vtas", Indicador.ebitda_vtas },
                    { "new_liquidez_cte", Indicador.liquidez_cte }, 
                    { "new_endeudamiento", Indicador.endeudamiento },
                    { "new_capital_trabajo", Indicador.capital_trabajo },
                    { "new_plazo_medio_ctas_a_cobar", Indicador.plazo_medio_ctas_a_cobar },
                    { "new_rotacion_inventarios", Indicador.rotacion_inventarios },
                    //_Indicador.Add("new_plazo_medio_ctas_a_pagar", Indicador.plazo_medio_ctas_a_pagar); //No existe el campo
                    { "new_compras_totales_insumos", Indicador.compras_totales_insumos },
                    { "new_vtas_mensuales_prom", Indicador.vtas_mensuales_prom },
                    { "new_inmovilizacion_bienes_de_uso", Indicador.inmovilizacion_bienes_de_uso },
                    { "new_productividadbsdeusoafectadosexportacion", Indicador.productividad_bs_de_uso_afectados_exportacion },
                    { "new_incidenciaamortizacionesbsusosobrecostos", Indicador.incidencia_amortizaciones_bs_uso_sobre_costos },
                    { "new_solvencia", Indicador.solvencia },
                    { "new_endeudamiento_diferido", Indicador.endeudamiento_diferido },
                    { "new_liquidez_acida", Indicador.liquidez_acida },
                    { "new_ebitda", Indicador.ebitda },
                    { "new_retorno_activo_total", Indicador.retorno_activo_total },
                    { "new_retorno_patrimonio_neto", Indicador.retorno_patrimonio_neto },
                    { "new_utilidad_bruta_costos", Indicador.utilidad_bruta_costos },
                    { "new_ebitda_vtas", Indicador.ebitda_vtas },
                    { "new_ebit", Indicador.ebit }
                };

                if (fechaPresentacion != string.Empty)
                    _Indicador.Add("new_fechapresentacion", fechaPresentacion);

                ResponseAPI response = await api.CreateRecord("new_indicadoreses", _Indicador, credenciales);  

                return Ok(Indicador);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/consultarindicadorpostbalance")]
        public async Task<IActionResult> ConsultarIndicadoresPostBalance(string cuit, string? socio_id)
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
                var cuitI = Convert.ToInt64(cuit);
                ApiDynamicsV2 api = new();
                string apiKey = string.Empty;
                JArray UnidadDeNegocio = await BuscarApiKeyLufe(api, credenciales);
                if (UnidadDeNegocio.Count > 0)
                    apiKey = ObtenerApiKey(UnidadDeNegocio);
                string fechaPresentacion = string.Empty;
                string resp = await APIlufe.GetIndicadoresPostBalance("Cliente", cuitI, apiKey);

                if(!string.IsNullOrEmpty(resp)) 
                {
                    IndicadorPostBalance Indicador = JsonConvert.DeserializeObject<IndicadorPostBalance>(resp);
                    if(Indicador.compras.Count > 0)
                    {
                        Dictionary<string, string> diccionarioCompras = new();
                        foreach (JProperty x in (JToken)Indicador.compras)
                        {
                            string name = x.Name;
                            JToken value = x.Value;
                            diccionarioCompras.Add(name, value.ToString());
                        }

                        foreach (var item in diccionarioCompras)
                        {
                            if (item.Value != "")                                                                                                                                                                                               
                            {
                                JObject _Indicador = new()
                                {                                                                   
                                    { "new_Cuenta@odata.bind", "/accounts(" + socio_id + ")"},
                                    { "new_periodo" , item.Key },
                                    { "new_compras_pb",  Convert.ToDecimal(item.Value) }
                                };

                                //ResponseAPI response = await api.CreateRecord("new_indicadoreses", _Indicador, credenciales);
                            }
                        }
                    }

                    if (Indicador.ventas.Count > 0)
                    {

                        Dictionary<string, string> diccionarioVentas = new();
                        foreach (JProperty x in (JToken)Indicador.ventas)
                        {
                            string name = x.Name;
                            JToken value = x.Value;
                            diccionarioVentas.Add(name, value.ToString());
                        }

                        foreach (var item in diccionarioVentas)
                        {
                            if (item.Value != "")
                            {
                                JObject _Indicador = new()
                                {
                                    { "new_Cuenta@odata.bind", "/accounts(" + socio_id + ")"},
                                    { "new_periodo" , item.Key },
                                    { "new_ventas_pb", Convert.ToDecimal(item.Value) }
                                };

                                //ResponseAPI response = await api.CreateRecord("new_indicadoreses", _Indicador, credenciales);
                            }
                        }
                    }
                }

                return Ok("EXITO");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/consultarindicadores")]
        public async Task<IActionResult> ConsultarIndicadores(ConsultaLufe consulta)
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
                JArray IndicadoresSOC = new();
                var cuitI = Convert.ToInt64(consulta.Cuit);
                IndicadoresSOC = await VerificarIndicador(consulta.Socio_id, api, credenciales);
                List<IndicadorSOC> listaIndicadoresSOC = JsonConvert.DeserializeObject<List<IndicadorSOC>>(IndicadoresSOC.ToString());
                string respuestaIndicadores = await APIlufe.GetIndicadores("Cliente", cuitI, consulta.ApiKey);
                if (!string.IsNullOrEmpty(respuestaIndicadores))
                {
                    Indicador Indicador = JsonConvert.DeserializeObject<Indicador>(respuestaIndicadores);
                    if (listaIndicadoresSOC.Count == 0 || 
                        listaIndicadoresSOC.FindAll(x => x.new_periodo == Indicador.periodo.ToString() && 
                        x.new_fechapresentacion == DateTime.ParseExact(Indicador.fechapresentacion.ToString(), "yyyyMMdd",
                            CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")).Count == 0)
                    {
                        await CrearIndicadores(Indicador, consulta.Socio_id, api, credenciales);
                    }
                }

                string respuestaIndicadoresPostBalance = await APIlufe.GetIndicadoresPostBalance("Cliente", cuitI, consulta.ApiKey);
                if (!string.IsNullOrEmpty(respuestaIndicadoresPostBalance))
                {
                    IndicadorPostBalance Indicador = JsonConvert.DeserializeObject<IndicadorPostBalance>(respuestaIndicadoresPostBalance);
                    await CrearIndicadoresPostBalanceConValidacion(Indicador, consulta.Socio_id, listaIndicadoresSOC, api, credenciales);
                }

                return Ok("Proceso finalizado.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/consultardocumentaciones")]
        public async Task<IActionResult> ConsultarDocumentaciones(ConsultaLufe consulta)
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
                int anioActual = DateTime.Now.Year + 1;
                List<DocumentosEntidad> listaDocumentosEntidad = new();
                var cuitI = Convert.ToInt64(consulta.Cuit);
                //NUEVO DESARROLLO
                for (int anio = anioActual; anio >= 2019; anio--)
                {
                    string respuestaDocumentosPeriodo = await APIlufe.GetDocumentosPorPeriodo("Cliente", cuitI, anio.ToString(), consulta.ApiKey);
                    if (!string.IsNullOrEmpty(respuestaDocumentosPeriodo))
                    {
                        DocumentosEntidad documentosPeriodo = JsonConvert.DeserializeObject<DocumentosEntidad>(respuestaDocumentosPeriodo);
                        if (documentosPeriodo?.documentos?.Length > 0)
                        {
                            DocumentosEntidad documentosEntidad = new()
                            {
                                periodo = anio.ToString(),
                                documentos = documentosPeriodo.documentos
                            };
                            listaDocumentosEntidad.Add(documentosEntidad);
                            var documentoAcuerdo = documentosPeriodo.documentos.FirstOrDefault(d => d.nombre.Contains("Presentación Única de Balances", StringComparison.OrdinalIgnoreCase));
                            if (documentoAcuerdo != null)
                            {
                                break;
                            }
                        }
                    }
                }
                if (listaDocumentosEntidad.Count > 0)
                {
                    await GenerarTodosDocumentacionAlSocio(listaDocumentosEntidad.ToArray(), consulta.Socio_id, api, credenciales);
                }

                return Ok("Proceso finalizado.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/lufe/consultarindicadoresydocumentosonboarding")]
        public async Task<IActionResult> ConsultarIndicadoresYDocumentosOnboarding(ConsultaLufe consulta)
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
                JArray IndicadoresSOC = new();
                var cuitI = Convert.ToInt64(consulta.Cuit);
                string apiKey = string.Empty;
                int anioActual = DateTime.Now.Year + 1;
                List<DocumentosEntidad> listaDocumentosEntidad = new();
                JArray UnidadDeNegocio = await BuscarApiKeyLufe(api, credenciales);
                if (UnidadDeNegocio.Count > 0)
                    apiKey = ObtenerApiKey(UnidadDeNegocio);

                #region Indicadores
                IndicadoresSOC = await VerificarIndicador(consulta.Socio_id, api, credenciales);
                List<IndicadorSOC> listaIndicadoresSOC = JsonConvert.DeserializeObject<List<IndicadorSOC>>(IndicadoresSOC.ToString());
                string respuestaIndicadores = await APIlufe.GetIndicadores("Cliente", cuitI, apiKey);
                if (!string.IsNullOrEmpty(respuestaIndicadores))
                {
                    Indicador Indicador = JsonConvert.DeserializeObject<Indicador>(respuestaIndicadores);
                    if (listaIndicadoresSOC.Count == 0 ||
                        listaIndicadoresSOC.FindAll(x => x.new_periodo == Indicador.periodo.ToString() &&
                        x.new_fechapresentacion == DateTime.ParseExact(Indicador.fechapresentacion.ToString(), "yyyyMMdd",
                            CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")).Count == 0)
                    {
                        await CrearIndicadores(Indicador, consulta.Socio_id, api, credenciales);
                    }
                }

                string respuestaIndicadoresPostBalance = await APIlufe.GetIndicadoresPostBalance("Cliente", cuitI, apiKey);
                if (!string.IsNullOrEmpty(respuestaIndicadoresPostBalance))
                {
                    IndicadorPostBalance Indicador = JsonConvert.DeserializeObject<IndicadorPostBalance>(respuestaIndicadoresPostBalance);
                    await CrearIndicadoresPostBalanceConValidacion(Indicador, consulta.Socio_id, listaIndicadoresSOC, api, credenciales);
                }
                #endregion
                #region Documentos
                //NUEVO DESARROLLO
                for (int anio = anioActual; anio >= 2019; anio--)
                {
                    string respuestaDocumentosPeriodo = await APIlufe.GetDocumentosPorPeriodo("Cliente", cuitI, anio.ToString(), apiKey);
                    if (!string.IsNullOrEmpty(respuestaDocumentosPeriodo))
                    {
                        DocumentosEntidad documentosPeriodo = JsonConvert.DeserializeObject<DocumentosEntidad>(respuestaDocumentosPeriodo);
                        if (documentosPeriodo?.documentos?.Length > 0)
                        {
                            DocumentosEntidad documentosEntidad = new()
                            {
                                periodo = anio.ToString(),
                                documentos = documentosPeriodo.documentos
                            };
                            listaDocumentosEntidad.Add(documentosEntidad);
                            var documentoAcuerdo = documentosPeriodo.documentos.FirstOrDefault(d => d.nombre.Contains("Presentación Única de Balances", StringComparison.OrdinalIgnoreCase));
                            if (documentoAcuerdo != null)
                            {
                                break;
                            }
                        }
                    }
                }
                if (listaDocumentosEntidad.Count > 0)
                {
                    await GenerarTodosDocumentacionAlSocio(listaDocumentosEntidad.ToArray(), consulta.Socio_id, api, credenciales);
                }
                #endregion
                #region Certificado
                string respuesta = await APIlufe.GetEntidad("Cliente", cuitI, apiKey);
                if (!string.IsNullOrEmpty(respuesta))
                {
                    Entidad entidad = JsonConvert.DeserializeObject<Entidad>(respuesta);

                    if (entidad.cuit == 0)
                    {
                        return Ok("No existe la entidad con el CUIT indicado.");
                    }

                    ///CREAMOS CERTIFICADO PYME VIGENTE
                    if(entidad?.certificado_pyme != null)
                    {
                        await CrearCertificado(entidad.certificado_pyme, consulta.Socio_id, api, credenciales);
                    }
                }
                #endregion
                return Ok("Proceso finalizado.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task<string> CrearSocio(Entidad entidad, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                Excepciones excepcion = new();
                string accountid = string.Empty;
                string actividad_afip = string.Empty;

                JObject cuenta = new()
                {
                    //{ "new_estadodelsocio", 100000000 }, //ACTIVO
                    { "new_rol", 100000004 }, //TERCERO
                    { "new_creadaporapilufe", true }
                };

                if (!string.IsNullOrEmpty(entidad.nombre))
                    cuenta.Add("name", entidad.nombre);

                if (entidad.cuit > 0)
                    cuenta.Add("new_nmerodedocumento", entidad.cuit.ToString());

                if (entidad.personeria == "Jurídica")
                    cuenta.Add("new_personeria", 100000000);
                else
                    cuenta.Add("new_personeria", 100000001);

                JArray ActividadesAFIP = await BuscarActividadAFIP(entidad.actividad_principal, api, credenciales);
                if (ActividadesAFIP.Count > 0)
                    actividad_afip = ObtenerActividadAfipID(ActividadesAFIP);

                if (!string.IsNullOrEmpty(actividad_afip))
                    cuenta.Add("new_ActividadAFIP@odata.bind", "/new_actividadafips(" + actividad_afip + ")");

                ResponseAPI responseAPI = await api.CreateRecord("accounts", cuenta, credenciales);
                if (responseAPI.ok)
                    accountid = responseAPI.descripcion;
                else
                    throw new Exception(responseAPI.descripcion);

                return accountid;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear socio {socio.name} - {ex.Message}");
                throw;
            }
        }
        public static async Task<string> CrearSocioOnboarding(Entidad entidad, string correo, string tipoDocumento, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                Excepciones excepcion = new();
                string accountid = string.Empty;
                string actividad_afip = string.Empty;

                JObject cuenta = new()
                {
                    //{ "new_estadodelsocio", 100000000 }, //ACTIVO
                    { "new_rol", 100000004 }, //TERCERO
                    { "new_onboarding", true },
                    { "new_creadaporapilufe", true }
                };

                if (!string.IsNullOrEmpty(entidad.nombre))
                    cuenta.Add("name", entidad.nombre);

                if (!string.IsNullOrEmpty(correo))
                    cuenta.Add("emailaddress1", correo);
                
                if (entidad.cuit > 0)
                    cuenta.Add("new_nmerodedocumento", entidad.cuit.ToString());

                if (entidad.personeria == "Jurídica")
                    cuenta.Add("new_personeria", 100000000);
                else
                    cuenta.Add("new_personeria", 100000001);

                if(!string.IsNullOrEmpty(tipoDocumento))
                    cuenta.Add("new_TipodedocumentoId@odata.bind", "/new_tipodedocumentos(" + tipoDocumento + ")");

                JArray ActividadesAFIP = await BuscarActividadAFIP(entidad.actividad_principal, api, credenciales);
                if (ActividadesAFIP.Count > 0)
                    actividad_afip = ObtenerActividadAfipID(ActividadesAFIP);

                if (!string.IsNullOrEmpty(actividad_afip))
                    cuenta.Add("new_ActividadAFIP@odata.bind", "/new_actividadafips(" + actividad_afip + ")");

                ResponseAPI responseAPI = await api.CreateRecord("accounts", cuenta, credenciales);
                if (responseAPI.ok)
                    accountid = responseAPI.descripcion;
                else
                    throw new Exception(responseAPI.descripcion);

                return accountid;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear socio {socio.name} - {ex.Message}");
                throw;
            }
        }
        public static async Task<string> CrearSocioVinculado(Autoridad autoridad, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                Excepciones excepcion = new();
                string accountid = string.Empty;
                string actividad_afip = string.Empty;

                JObject cuenta = new()
                {
                    { "new_estadodelsocio", 100000000 },
                    { "new_personeria", 100000001 },
                    { "new_rol", 100000005 }, //OTRO
                    { "new_creadaporapilufe", true }
                };

                if (!string.IsNullOrEmpty(autoridad.denominacion))
                    cuenta.Add("name", autoridad.denominacion);

                if (autoridad.cuit > 0)
                    cuenta.Add("new_nmerodedocumento", autoridad.cuit.ToString());

                ResponseAPI responseAPI = await api.CreateRecord("accounts", cuenta, credenciales);
                if (responseAPI.ok)
                    accountid = responseAPI.descripcion;
                else
                    throw new Exception(responseAPI.descripcion);

                return accountid;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear socio {socio.name} - {ex.Message}");
                throw;
            }
        }
        public static async Task<JArray> BuscarActividadAFIP(int codigo, ApiDynamicsV2 api, Credenciales credenciales)
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

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }
        public static async Task<JArray> BuscarCategoriaCertificadoPyme(string codigo, ApiDynamicsV2 api, Credenciales credenciales)
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
                                                                $"<condition attribute='new_name' operator='eq' value='{codigo}' />" +
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
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }
        public static async Task<JArray> BuscarCondicionPyme(string nombre, ApiDynamicsV2 api, Credenciales credenciales)
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
                                                                $"<condition attribute='new_name' operator='eq' value='{nombre}' />" +
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
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }
        public static async Task<JArray> BuscarApiKeyLufe(ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "businessunits";

                fetchXML = "<entity name='businessunit'>" +
                                        "<attribute name='new_apikeylufe'/> " +
                                        "<filter type='and'>" +
                                            $"<condition attribute='parentbusinessunitid' operator='null' />" +
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
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }
        public static async Task<JArray> BuscarDocumentacionPorCuenta(string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_documentacionporcuentas";

                fetchXML = "<entity name='new_documentacionporcuenta'>" +
                                        "<attribute name='new_documentacionporcuentaid'/> " +
                                        "<attribute name='new_documentoid'/> " +
                                        "<filter type='and'>" +
                                            $"<condition attribute='new_cuentaid' operator='eq' value='{cuenta_id}'/>" +
                                            "<condition attribute='statecode' operator='eq' value='0'/>" +
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
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }
        public static async Task<Casfog_Sindicadas.Socio> VerificarSocio(string cuit, ApiDynamicsV2 api, Credenciales credenciales)
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

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                if (respuesta.Count > 0)
                    socio = JsonConvert.DeserializeObject<Casfog_Sindicadas.Socio>(respuesta.First.ToString());

                return socio;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }
        public static string ObtenerActividadAfipID(JToken actividadAfipJT)
        {
            string actividadAfip_id = string.Empty;

            ActividadAFIP actividadAfip = JsonConvert.DeserializeObject<ActividadAFIP>(actividadAfipJT.First().ToString());
            if (actividadAfip.new_actividadafipid != null)
                actividadAfip_id = actividadAfip.new_actividadafipid;

            return actividadAfip_id;
        }
        public static string ObtenerCategoriaID(JToken categoriaJT)
        {
            string categoria_id = string.Empty;

            Categoria categoria = JsonConvert.DeserializeObject<Categoria>(categoriaJT.First().ToString());
            if (categoria.new_categoracertificadopymeid != null)
                categoria_id = categoria.new_categoracertificadopymeid;

            return categoria_id;
        }
        public static string ObtenerApiKey(JToken unidadJT)
        {
            string apiKey = string.Empty;

            UnidadDeNegocioLufe unidad = JsonConvert.DeserializeObject<UnidadDeNegocioLufe>(unidadJT.First().ToString());
            if (!string.IsNullOrEmpty(unidad.new_apikeylufe))
                apiKey = unidad.new_apikeylufe;

            return apiKey;
        }
        public static async Task<JArray> BuscarDocumentacion(ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_documentacions";

                fetchXML = "<entity name='new_documentacion'>" +
                                                           "<attribute name='new_documentacionid'/> " +
                                                           "<attribute name='new_name'/> " +
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
        public static List<Documento> ArmarDocumentacion(JToken documentacionJT)
        {
            return JsonConvert.DeserializeObject<List<Documento>>(documentacionJT.ToString());
        }
        public static List<DocumentacionPorCuentaLufe> ArmarDocumentacionPorcuenta(JToken documentacionJT)
        {
            return JsonConvert.DeserializeObject<List<DocumentacionPorCuentaLufe>>(documentacionJT.ToString());
        }
        public static async Task<JArray> VerificarContacto(string email, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "contacts";

                fetchXML = "<entity name='contact'>" +
                                                           "<attribute name='contactid'/> " +
                                                             "<filter type='and'>" +
                                                                $"<condition attribute='emailaddress1' operator='eq' value='{email}' />" +
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
        public static async Task<string> CrearDocumento(string nombre, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string documento_id = string.Empty;
                JObject documento = new()
                {
                    { "new_name", nombre },
                    {"new_estadodelsocio", 100000005 } //Rechazado }
                };

                ResponseAPI responseAPI = await api.CreateRecord("new_documentacions", documento, credenciales);
                if (responseAPI.ok)
                    documento_id = responseAPI.descripcion;
                else
                    throw new Exception(responseAPI.descripcion);

                return documento_id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<string> CrearDocumentacionPorCuenta(string nombre, string documento_id, string socio_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JObject documento = new();

                if (!string.IsNullOrEmpty(nombre))
                    documento.Add("new_name", nombre);

                if (!string.IsNullOrEmpty(documento_id))
                    documento.Add("new_DocumentoId@odata.bind", "/new_documentacions(" + documento_id + ")");

                if (!string.IsNullOrEmpty(socio_id))
                    documento.Add("new_CuentaId@odata.bind", "/accounts(" + socio_id + ")");

                    documento.Add("statuscode", 100000000);

                ResponseAPI responseAPI = await api.CreateRecord("new_documentacionporcuentas", documento, credenciales);
                if (!responseAPI.ok)
                    throw new Exception(responseAPI.descripcion);

                return responseAPI.descripcion;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<JArray> VerificarRelacionDeVinculacion(string cuenta_id, string cuentaVinculada_id, int tipoRelacion, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_participacionaccionarias";

                fetchXML = "<entity name='new_participacionaccionaria'>" +
                                                           "<attribute name='new_participacionaccionariaid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                             "<filter type='and'>" +
                                                                $"<condition attribute='new_cuentaid' operator='eq' value='{cuenta_id}' />" +
                                                                $"<condition attribute='new_cuentacontactovinculado' operator='eq' uitype='account' value='{cuentaVinculada_id}' />" +
                                                                $"<condition attribute='new_tipoderelacion' operator='eq'    value='{tipoRelacion}' />" +
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
        public static async Task<JArray> VerificarCertificado(int numeroRegistros, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_certificadopymes";

                fetchXML = "<entity name='new_certificadopyme'>" +
                                    "<attribute name='new_certificadopymeid'/> " +
                                        "<filter type='and'>" +
                                        $"<condition attribute='new_numeroderegistro' operator='eq' value='{numeroRegistros}' />" +
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
        public static async Task<JArray> VerificarIndicador(string socio_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "new_indicadoreses";

                fetchXML = "<entity name='new_indicadores'>" +
                                    "<attribute name='new_indicadoresid'/> " +
                                    "<attribute name='new_name'/> " +
                                    "<attribute name='new_periodo'/> " +
                                    "<attribute name='new_fechapresentacion'/> " +
                                    "<attribute name='new_compras_pb'/> " +
                                    "<attribute name='new_ventas_pb'/> " +
                                    "<filter type='and'>" +
                                        $"<condition attribute='new_cuenta' operator='eq' value='{socio_id}' />" +
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
        public static async Task GenerarDocumentacionAlSocio(DocumentosEntidad documentos, string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                ApiLufe APIlufe = new();
                List<Documento> listaDocumentosD65 = new();
                List<DocumentacionPorCuentaLufe> listaDocumentosPorCuenta = new();
                JArray documentosD365 = await BuscarDocumentacion(api, credenciales);
                if (documentosD365.Count > 0)
                    listaDocumentosD65 = ArmarDocumentacion(documentosD365);

                JArray documentosPorCuenta = await BuscarDocumentacionPorCuenta(cuenta_id, api, credenciales);
                if (documentosPorCuenta.Count > 0)
                    listaDocumentosPorCuenta = ArmarDocumentacionPorcuenta(documentosPorCuenta);

                for (int i = 0; i < documentos.documentos.Length; i++)
                {
                    string documento_id = string.Empty;
                    var documento = documentos.documentos[i];
                    string nombreDocumento = $"{documento.nombre} - Lufe - {documentos.periodo}";
                    if (listaDocumentosD65.FirstOrDefault(x => x.new_name == nombreDocumento) != null)
                    {
                        documento_id = listaDocumentosD65.FirstOrDefault(x => x.new_name == nombreDocumento).new_documentacionid;
                    }
                    else
                    {
                        documento_id = await CrearDocumento(nombreDocumento, api, credenciales);
                    }

                    if (listaDocumentosPorCuenta.Count == 0 || (listaDocumentosPorCuenta.Count > 0 &&
                        listaDocumentosPorCuenta.FirstOrDefault(x => x.new_documentoid == documento_id) == null))
                    {
                        string documentacionporcuenta_id = await CrearDocumentacionPorCuenta(nombreDocumento, documento_id, cuenta_id, api, credenciales);

                        byte[] respuestaByte = await APIlufe.GetBase64Document("cliente", documento.url);
                        string respuestaBase64 = System.Convert.ToBase64String(respuestaByte);
                        JObject annotation = new()
                        {
                            { "subject", documento.nombre },
                            { "mimetype",  @"application/pdf" },
                            { "documentbody", respuestaBase64 },
                            { "filename", $"{documento.nombre}.pdf" },
                        };

                        if (!string.IsNullOrEmpty(documentacionporcuenta_id))
                            annotation.Add("objectid_new_documentacionporcuenta@odata.bind", "/new_documentacionporcuentas(" + documentacionporcuenta_id + ")");

                        ResponseAPI notaResponse = await api.CreateRecord("annotations", annotation, credenciales);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task GenerarTodosDocumentacionAlSocio(DocumentosEntidad[] documentosEntidad, string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                ApiLufe APIlufe = new();
                List<Documento> listaDocumentosD65 = new();
                List<DocumentacionPorCuentaLufe> listaDocumentosPorCuenta = new();
                JArray documentosD365 = await BuscarDocumentacion(api, credenciales);
                if (documentosD365.Count > 0)
                    listaDocumentosD65 = ArmarDocumentacion(documentosD365);

                JArray documentosPorCuenta = await BuscarDocumentacionPorCuenta(cuenta_id, api, credenciales);
                if (documentosPorCuenta.Count > 0)
                    listaDocumentosPorCuenta = ArmarDocumentacionPorcuenta(documentosPorCuenta);

                foreach (var documentos in documentosEntidad)
                {
                    string documento_id = string.Empty;
                    foreach (var documento in documentos.documentos)
                    {
                        string nombreDocumento = $"{documento.nombre} - Lufe - {documentos.periodo}";
                        if (listaDocumentosD65.FirstOrDefault(x => x.new_name == nombreDocumento) != null)
                        {
                            documento_id = listaDocumentosD65.FirstOrDefault(x => x.new_name == nombreDocumento).new_documentacionid;
                        }
                        else
                        {
                            documento_id = await CrearDocumento(nombreDocumento, api, credenciales);
                        }

                        if (listaDocumentosPorCuenta.Count == 0 || (listaDocumentosPorCuenta.Count > 0 &&
                            listaDocumentosPorCuenta.FirstOrDefault(x => x.new_documentoid == documento_id) == null))
                        {
                            string documentacionporcuenta_id = await CrearDocumentacionPorCuenta(nombreDocumento, documento_id, cuenta_id, api, credenciales);

                            byte[] respuestaByte = await APIlufe.GetBase64Document("cliente", documento.url);
                            string respuestaBase64 = System.Convert.ToBase64String(respuestaByte);
                            JObject annotation = new()
                            {
                                { "subject", documento.nombre },
                                { "mimetype",  @"application/pdf" },
                                { "documentbody", respuestaBase64 },
                                { "filename", $"{documento.nombre}.pdf" },
                            };

                            if (!string.IsNullOrEmpty(documentacionporcuenta_id))
                                annotation.Add("objectid_new_documentacionporcuenta@odata.bind", "/new_documentacionporcuentas(" + documentacionporcuenta_id + ")");

                            ResponseAPI notaResponse = await api.CreateRecord("annotations", annotation, credenciales);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task CrearContactos(ContactoLufe contacto, string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                if (!string.IsNullOrEmpty(contacto.email))
                {
                    JArray contactoVerificado = await VerificarContacto(contacto.email, api, credenciales);
                    if (contactoVerificado.Count == 0)
                    {
                        JObject _contacto = new()
                        {
                            { "parentcustomerid_account@odata.bind", "/accounts(" + cuenta_id + ")" },
                            { "emailaddress1", contacto.email }
                        };

                        if (!string.IsNullOrEmpty(contacto.nombre) && contacto.nombre.Contains(','))
                        {
                            string[] nombreYapellido = contacto.nombre.Split(',');
                            if (nombreYapellido[0].Length > 0)
                            {
                                _contacto.Add("lastname", nombreYapellido[0].Trim());
                            }
                            if (nombreYapellido[1].Length > 0)
                            {
                                _contacto.Add("firstname", nombreYapellido[1].Trim());
                            }
                        }
                        else if (!string.IsNullOrEmpty(contacto.nombre))
                        {
                            _contacto.Add("firstname", contacto.nombre);
                        }

                        if (!string.IsNullOrEmpty(contacto.tipo))
                            _contacto.Add("jobtitle", contacto.tipo);

                        if (!string.IsNullOrEmpty(contacto.telefono))
                            _contacto.Add("mobilephone", contacto.telefono);

                        ResponseAPI responseApi = await api.CreateRecord("contacts", _contacto, credenciales);
                        if (!responseApi.ok)
                        {
                            throw new Exception(responseApi.descripcion);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task CrearCertificado(CertificadoPymeLufe certificado, string socio_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray certificadoVerificado = await VerificarCertificado(certificado.nro_registro, api, credenciales);

                if(certificadoVerificado.Count == 0)
                {
                    string categoria_id = string.Empty;
                    string condicion_id = string.Empty;

                    Dictionary<string, string> categorias = new()
                    {
                        {"tramo1", "MEDIANA TRAMO 1"},
                        {"tramo2", "MEDIANA TRAMO 2"},
                        {"peq", "PEQUEÑA EMPRESA"},
                        {"micro", "MICRO"}
                    };


                    string categoriaCertificadoPyme = string.Empty;

                    if (categorias.TryGetValue(certificado.categoria, out string resultadoCategoria))
                    {
                        if (!string.IsNullOrEmpty(resultadoCategoria))
                        {
                            categoriaCertificadoPyme = resultadoCategoria;
                        }
                    }

                    if (!string.IsNullOrEmpty(categoriaCertificadoPyme))
                    {
                        JArray Categorias = await BuscarCategoriaCertificadoPyme(categoriaCertificadoPyme, api, credenciales);
                        if (Categorias.Count > 0)
                            categoria_id = ObtenerCategoriaID(Categorias);
                    }

                    JArray condiciones = await BuscarCondicionPyme(certificado.sector, api, credenciales);
                    if (condiciones.Count > 0)
                        condicion_id = ObtenerCondicionPymeID(condiciones);

                    JObject Certificado = new()
                    {
                        { "new_aprobacion1", 100000000 }
                    };

                    if (!string.IsNullOrEmpty(socio_id))
                        Certificado.Add("new_SocioParticipe@odata.bind", "/accounts(" + socio_id + ")");

                    if (certificado.nro_registro > 0)
                        Certificado.Add("new_numeroderegistro", certificado.nro_registro);

                    if (!string.IsNullOrEmpty(certificado.fecha_emision))
                        Certificado.Add("new_fechadeemision", certificado.fecha_emision);

                    if (!string.IsNullOrEmpty(certificado.desde))
                        Certificado.Add("new_vigenciadesde", certificado.desde);

                    if (!string.IsNullOrEmpty(certificado.hasta))
                    {
                        Certificado.Add("new_vigenciahasta", certificado.hasta);
                        if (DateTime.Parse(certificado.hasta) >= DateTime.Now)
                            Certificado.Add("statuscode", 1); //Aprobado
                    }

                    if (!string.IsNullOrEmpty(condicion_id))
                        Certificado.Add("new_SectorEconomico@odata.bind", "/new_condicionpymes(" + condicion_id + ")");

                    if (!string.IsNullOrEmpty(categoria_id))
                        Certificado.Add("new_Categoria@odata.bind", "/new_categoracertificadopymes(" + categoria_id + ")");

                    ResponseAPI responseAPI = await api.CreateRecord("new_certificadopymes", Certificado, credenciales);
                    if (!responseAPI.ok)
                        throw new Exception(responseAPI.descripcion);
                }
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear certificado pyme para el socio {nombreSocio} - {ex.Message}");
            }
        }
        public static async Task CrearRelacion(Autoridad autoridad, string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string resultadoSocio = string.Empty;
                bool cuentaVinculadaExistente = false;
                int tipoRelacion = 0;
                JArray vinculacionExistente = new();
                JObject relacion = new();

                if (!string.IsNullOrEmpty(cuenta_id))
                    relacion.Add("new_CuentaId@odata.bind", "/accounts(" + cuenta_id + ")");

                ///VERIFICAMOS SI EL SOCIO EXISTE, CASO CONTRARIO LO CREAMOS
                Casfog_Sindicadas.Socio socioVerificar = await VerificarSocio(autoridad.cuit.ToString(), api, credenciales);
                if (socioVerificar == null || string.IsNullOrEmpty(socioVerificar?.accountid))
                {
                    resultadoSocio = await CrearSocioVinculado(autoridad, api, credenciales);
                }
                else
                {
                    cuentaVinculadaExistente = true;
                    resultadoSocio = socioVerificar.accountid;
                }

                relacion.Add("new_CuentaContactoVinculado_account@odata.bind", "/accounts(" + resultadoSocio + ")");

                if (autoridad.es_accionista == 1)
                {
                    tipoRelacion = 100000001;
                    relacion.Add("new_tipoderelacion", 100000001); //ACCIONISTA

                    if (autoridad.porc_accionista > 0)
                        relacion.Add("new_porcentajedeparticipacion", autoridad.porc_accionista);
                }
                else
                {
                    tipoRelacion = 100000005;
                    relacion.Add("new_tipoderelacion", 100000005); //OTRA
                }

                if (!string.IsNullOrEmpty(autoridad.cargo))
                    relacion.Add("new_cargo", autoridad.cargo);

                ///VERIFICAMOS SI LA RELACION YA EXISTE
                if (cuentaVinculadaExistente)
                    vinculacionExistente = await VerificarRelacionDeVinculacion(cuenta_id, resultadoSocio, tipoRelacion, api, credenciales);

                if (vinculacionExistente.Count == 0)
                {
                    ResponseAPI responseAPI = await api.CreateRecord("new_participacionaccionarias", relacion, credenciales);
                }
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error al crear relacion de vinculacion para el socio {nombreSocio} - {ex.Message}");
                throw;
            }
        }
        public static async Task CrearIndicadores(Indicador Indicador, string socio_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string fechaPresentacion = string.Empty;

                if (Indicador.fechapresentacion > 0)
                    fechaPresentacion = DateTime.ParseExact(Indicador.fechapresentacion.ToString(), "yyyyMMdd",
                CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                JObject _Indicador = new()
                {
                    { "new_Cuenta@odata.bind", "/accounts(" + socio_id + ")"},
                    { "new_periodo", Indicador.periodo.ToString() },
                    { "new_rentabilidad", Indicador.rentabilidad },
                    { "new_ebit_vtas", Indicador.ebitda_vtas },
                    { "new_liquidez_cte", Indicador.liquidez_cte },
                    { "new_endeudamiento", Indicador.endeudamiento },
                    { "new_capital_trabajo", Indicador.capital_trabajo },
                    { "new_plazo_medio_ctas_a_cobar", Indicador.plazo_medio_ctas_a_cobar },
                    { "new_rotacion_inventarios", Indicador.rotacion_inventarios },
                    //_Indicador.Add("new_plazo_medio_ctas_a_pagar", Indicador.plazo_medio_ctas_a_pagar); //No existe el campo
                    { "new_compras_totales_insumos", Indicador.compras_totales_insumos },
                    { "new_vtas_mensuales_prom", Indicador.vtas_mensuales_prom },
                    { "new_inmovilizacion_bienes_de_uso", Indicador.inmovilizacion_bienes_de_uso },
                    { "new_productividadbsdeusoafectadosexportacion", Indicador.productividad_bs_de_uso_afectados_exportacion },
                    { "new_incidenciaamortizacionesbsusosobrecostos", Indicador.incidencia_amortizaciones_bs_uso_sobre_costos },
                    { "new_solvencia", Indicador.solvencia },
                    { "new_endeudamiento_diferido", Indicador.endeudamiento_diferido },
                    { "new_liquidez_acida", Indicador.liquidez_acida },
                    { "new_ebitda", Indicador.ebitda },
                    { "new_retorno_activo_total", Indicador.retorno_activo_total },
                    { "new_retorno_patrimonio_neto", Indicador.retorno_patrimonio_neto },
                    { "new_utilidad_bruta_costos", Indicador.utilidad_bruta_costos },
                    { "new_ebitda_vtas", Indicador.ebitda_vtas },
                    { "new_ebit", Indicador.ebit }
                };

                if (fechaPresentacion != string.Empty)
                    _Indicador.Add("new_fechapresentacion", fechaPresentacion);

                ResponseAPI response = await api.CreateRecord("new_indicadoreses", _Indicador, credenciales);
                if (!response.ok)
                    throw new Exception(response.descripcion);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task CrearIndicadoresPostBalance(IndicadorPostBalance Indicador, string socio_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                if (Indicador.compras.Count > 0)
                {
                    Dictionary<string, string> diccionarioCompras = new();
                    foreach (JProperty x in (JToken)Indicador.compras)
                    {
                        string name = x.Name;
                        JToken value = x.Value;
                        diccionarioCompras.Add(name, value.ToString());
                    }

                    foreach (var item in diccionarioCompras)
                    {
                        if (item.Value != "")
                        {
                            JObject _Indicador = new()
                            {
                                { "new_Cuenta@odata.bind", "/accounts(" + socio_id + ")"},
                                { "new_periodo" , item.Key },
                                { "new_compras_pb",  Convert.ToDecimal(item.Value)  }
                            };

                            ResponseAPI response = await api.CreateRecord("new_indicadoreses", _Indicador, credenciales);
                        }
                    }
                }

                if (Indicador.ventas.Count > 0)
                {
                    Dictionary<string, string> diccionarioVentas = new();
                    foreach (JProperty x in (JToken)Indicador.ventas)
                    {
                        string name = x.Name;
                        JToken value = x.Value;
                        diccionarioVentas.Add(name, value.ToString());
                    }

                    foreach (var item in diccionarioVentas)
                    {
                        if (item.Value != "")
                        {
                            JObject _Indicador = new()
                            {
                                { "new_Cuenta@odata.bind", "/accounts(" + socio_id + ")"},
                                { "new_periodo" , item.Key },
                                { "new_ventas_pb", Convert.ToDecimal(item.Value) }
                            };

                            ResponseAPI response = await api.CreateRecord("new_indicadoreses", _Indicador, credenciales);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task CrearIndicadoresPostBalanceConValidacion(IndicadorPostBalance Indicador, string socio_id, List<IndicadorSOC> listaIndicadoreSOC,
            ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                if (Indicador.compras.Count > 0)
                {
                    Dictionary<string, string> diccionarioCompras = new();
                    foreach (JProperty x in (JToken)Indicador.compras)
                    {
                        string name = x.Name;
                        JToken value = x.Value;
                        diccionarioCompras.Add(name, value.ToString());
                    }

                    foreach (var item in diccionarioCompras)
                    {
                        if (item.Value != "")
                        {
                            if (listaIndicadoreSOC.Count == 0 || 
                                listaIndicadoreSOC.FindAll(x => x.new_periodo == item.Key && x.new_compras_pb == item.Value).Count == 0)
                            {
                                JObject _Indicador = new()
                                {
                                    { "new_Cuenta@odata.bind", "/accounts(" + socio_id + ")"},
                                    { "new_periodo" , item.Key },
                                    { "new_compras_pb", Convert.ToDecimal(item.Value) }
                                };

                                ResponseAPI response = await api.CreateRecord("new_indicadoreses", _Indicador, credenciales);
                            }
                        }
                    }
                }

                if (Indicador.ventas.Count > 0)
                {
                    Dictionary<string, string> diccionarioVentas = new();
                    foreach (JProperty x in (JToken)Indicador.ventas)
                    {
                        string name = x.Name;
                        JToken value = x.Value;
                        diccionarioVentas.Add(name, value.ToString());
                    }

                    foreach (var item in diccionarioVentas)
                    {
                        if (item.Value != "")
                        {
                            if (listaIndicadoreSOC.Count == 0 || 
                                listaIndicadoreSOC.FindAll(x => x.new_periodo == item.Key && x.new_ventas_pb == item.Value).Count == 0)
                            {
                                JObject _Indicador = new()
                                {
                                    { "new_Cuenta@odata.bind", "/accounts(" + socio_id + ")"},
                                    { "new_periodo" , item.Key },
                                    { "new_ventas_pb", Convert.ToDecimal(item.Value) }
                                };

                                ResponseAPI response = await api.CreateRecord("new_indicadoreses", _Indicador, credenciales);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
