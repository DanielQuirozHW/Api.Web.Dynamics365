using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Aspose.Pdf.Operators;
using Azure.Storage.Queues;
using Console.SGR.API.ActualizarCuentas.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System.Globalization;
using System.Net;
using System.Web;
using System.Xml.Linq;
using static Api.Web.Dynamics365.Controllers.SgrOneClickController;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;
using static Api.Web.Dynamics365.Models.PortalCASFOG;
using static Api.Web.Dynamics365.Models.SgrOneClick;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class SgrOneClickController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration configuration;

        public SgrOneClickController(ApplicationDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/sgroneclick/consultarcuit")]
        public async Task<IActionResult> ConsultarCuit([FromBody] ConsultarCuit consulta)
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
            ApiDynamicsV2 api = new();
            string sgrhwa_id = string.Empty;
            try
            {
                ConexionApi conexionApi = new();
                SgrApiCUITRequest requestCuit = new();
                ApiDynamics apiF = new();
                List<CategoriaCertificado> listaCategoriaCertificados = new();
                List<CondicionPymeSOC> listaCondicionesPymes = new();
                List<SGRSOC> listaSGRs = new();
                List<CuentasCertificadosPymes> listaCuentasYCertificadosPymes = new();
                CuentasCertificadosPymes cuentaCP = new();

                BeatMobileHttpClient ClientHttp = conexionApi.Login(consulta.email, consulta.password);
                requestCuit.CUIT = Convert.ToInt64(consulta.cuit);
                var responseCuit = ClientHttp.ExecuteGETCotizacionDolar<SgrApiCUITRequest, SgrApiCUITResponseContainer>(requestCuit, "central_deudores/");

                if (responseCuit.success == true)
                {
                    JArray CuentasYCertificados = await BuscarTodasCuentasYCertificadosPorCuenta(consulta.cuenta_id, api, credenciales);
                    if (CuentasYCertificados.Count > 0)
                    {
                        listaCuentasYCertificadosPymes = ArmarCuentaYCertificados(CuentasYCertificados);
                        cuentaCP = listaCuentasYCertificadosPymes[0];
                    }   

                    JArray TodasCategorias = await BuscarTodasCategorias(api, credenciales);
                    if (TodasCategorias.Count > 0)
                        listaCategoriaCertificados = ArmarTodasCategoriaCertifcado(TodasCategorias);

                    JArray TodasCondiciones = await BuscarTodasCondicionesPymes(api, credenciales);
                    if (TodasCondiciones.Count > 0)
                        listaCondicionesPymes = ArmarTodasCondicionesPymes(TodasCondiciones);

                    JArray TodasSGRs = await BuscarSGR(api, credenciales);
                    if (TodasSGRs.Count > 0)
                        listaSGRs = ArmarSGRs(TodasSGRs);

                    await UpdateCuenta(consulta.cuenta_id, responseCuit.data.informacion_de_garantias_de_la_pyme.metricas, api, credenciales);
                    await ActualYCrearCertificadosPyme(cuentaCP, responseCuit.data.certificados_pymes, listaCategoriaCertificados, listaCondicionesPymes,
                                api, credenciales, consulta.aprobarCertificadoPyme);
                    await ActualYCrearCuentasporSGR(cuentaCP, responseCuit.data.informacion_de_garantias_de_la_pyme.data, listaSGRs,
                        responseCuit.data.general, api, credenciales);
                }
                else
                {
                    Credenciales credencialesHWA = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == "Hwapplications");
                    if (string.IsNullOrEmpty(sgrhwa_id)){
                        JArray sgrsHWA = await BuscarSGRHWA(consulta.cuitSGR, api, credencialesHWA);
                        if (sgrsHWA.Count > 0)
                            sgrhwa_id = ObtenerSGRHWID(sgrsHWA);
                    }
                    Excepciones excepciones = new();
                    excepciones.CrearExcepcionHWA($"API error en CUIT:  {consulta.cuit}", credenciales.cliente, sgrhwa_id, responseCuit?.data?.error?.message, "Consultar CUIT", credencialesHWA);
                    return BadRequest(responseCuit.data.error.message);
                }

                return Ok("Consulta exitosa");
            }
            catch (Exception ex)
            {
                Credenciales credencialesHWA = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == "Hwapplications");
                if (string.IsNullOrEmpty(sgrhwa_id)){
                    JArray sgrsHWA = await BuscarSGRHWA(consulta.cuitSGR, api, credencialesHWA);
                    if (sgrsHWA.Count > 0)
                        sgrhwa_id = ObtenerSGRHWID(sgrsHWA);
                }
                Excepciones excepciones = new();
                excepciones.CrearExcepcionHWA($"API error en CUIT:  {consulta.cuit}", credenciales.cliente, sgrhwa_id, ex.Message, "Consultar CUIT", credencialesHWA);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/sgroneclick/consultarcuitmasivo")]
        public async Task<IActionResult> ConsultarCuitMasivo([FromBody] ConsultarCuit consulta)
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
            ApiDynamicsV2 api = new();
            string sgrhwa_id = string.Empty;
            string errores_Cuit = string.Empty;

            try
            {
                //ConexionApi conexionApi = new();
                //SgrApiCUITRequest requestCuit = new();
                //List<CuentasCertificadosPymes> listaCuentasYCertificadosPymes = new();
                //List<CategoriaCertificado> listaCategoriaCertificados = new();
                //List<CondicionPymeSOC> listaCondicionesPymes = new();
                //List<SGR> listaSGRs = new();

                //JArray CuentasYCertificados = await BuscarTodasCuentasYCertificados(api, credenciales);

                //if (CuentasYCertificados.Count > 0)
                //{
                //    listaCuentasYCertificadosPymes = ArmarCuentaYCertificados(CuentasYCertificados);

                //    JArray TodasCategorias = await BuscarTodasCategorias(api, credenciales);
                //    if (TodasCategorias.Count > 0)
                //        listaCategoriaCertificados = ArmarTodasCategoriaCertifcado(TodasCategorias);

                //    JArray TodasCondiciones = await BuscarTodasCondicionesPymes(api, credenciales);
                //    if (TodasCondiciones.Count > 0)
                //        listaCondicionesPymes = ArmarTodasCondicionesPymes(TodasCondiciones);

                //    JArray TodasSGRs = await BuscarSGR(api, credenciales);
                //    if (TodasSGRs.Count > 0)
                //        listaSGRs = ArmarSGRs(TodasSGRs);

                //    BeatMobileHttpClient ClientHttp = conexionApi.Login(consulta.email, consulta.password);

                //    listaCuentasYCertificadosPymes.ForEach(cuentaCP =>
                //    {
                //        requestCuit.CUIT = Convert.ToInt64(cuentaCP.new_nmerodedocumento);
                //        var responseCuit = ClientHttp.ExecuteGETCotizacionDolar<SgrApiCUITRequest, SgrApiCUITResponseContainer>(requestCuit, "central_deudores/");

                //        if (responseCuit.success == true)
                //        {
                //            UpdateCuenta(cuentaCP.accountid, responseCuit.data.informacion_de_garantias_de_la_pyme.metricas, api, credenciales);
                //            ActualYCrearCertificadosPyme(cuentaCP, responseCuit.data.certificados_pymes, listaCategoriaCertificados, listaCondicionesPymes,
                //                api, credenciales, consulta.aprobarCertificadoPyme);
                //            ActualYCrearCuentasporSGR(cuentaCP, responseCuit.data.informacion_de_garantias_de_la_pyme.data, listaSGRs,
                //                responseCuit.data.general, api, credenciales);
                //        }
                //        else
                //        {
                //            errores_Cuit += $"- {cuentaCP.new_nmerodedocumento}";
                //        }
                //    });

                //    if (!string.IsNullOrEmpty(errores_Cuit))
                //    {
                //        Credenciales credencialesHWA = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == "Hwapplications");
                //        if (string.IsNullOrEmpty(sgrhwa_id))
                //        {
                //            JArray sgrsHWA = await BuscarSGRHWA(consulta.cuitSGR, api, credencialesHWA);
                //            if (sgrsHWA.Count > 0)
                //                sgrhwa_id = ObtenerSGRHWID(sgrsHWA);
                //        }
                //        Excepciones excepciones = new();
                //        excepciones.CrearExcepcionHWA($"API error en CUIT masivo:  {consulta.cuit}", credenciales.cliente, sgrhwa_id, $"CUITS {errores_Cuit}", "Consultar CUIT Masivo", credencialesHWA);
                //        return BadRequest("Errores al consultar cuit masivo");
                //    }
                //}
                //else
                //{
                //    return Ok("No se encontraron cuentas.");
                //}

                    return Ok("Consulta exitosa");
            }
            catch (Exception ex)
            {
                Credenciales credencialesHWA = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == "Hwapplications");
                if (string.IsNullOrEmpty(sgrhwa_id))
                {
                    JArray sgrsHWA = await BuscarSGRHWA(consulta.cuitSGR, api, credencialesHWA);
                    if (sgrsHWA.Count > 0)
                        sgrhwa_id = ObtenerSGRHWID(sgrsHWA);
                }
                Excepciones excepciones = new();
                excepciones.CrearExcepcionHWA($"API error en CUIT:  {consulta.cuit}", credenciales.cliente, sgrhwa_id, ex.Message, "Consultar CUIT Masivo", credencialesHWA);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/sgroneclick/crearcola")]
        public async Task<IActionResult> CrearColaAzure([FromBody] ConsultarCuit consulta)
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
                MensajeColaWebJOB mensajeCWJ = new()
                {
                    credenciales = credenciales,
                    email = consulta?.email,
                    password = consulta?.password,
                    aprobarCertificadoPyme = consulta.aprobarCertificadoPyme,
                    cuitSGR = consulta?.cuitSGR
                };

                string mensajeCWJSTR = JsonConvert.SerializeObject(mensajeCWJ);
                string conexionAzureStorage = configuration["StorageConnectionString"];

                QueueClient queue = new(conexionAzureStorage,
                    "cuitmasivo", new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });

                var respuesta = await queue.SendMessageAsync(mensajeCWJSTR);

                return Ok($"Mensaje creado con exito: {respuesta.Value.MessageId}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/sgroneclick/crearmensajefinancimiento")]
        public async Task<IActionResult> CrearColaAzureFinanciamientoPyme([FromBody] CrearMensajePyme consulta)
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
                MensajeColaWebJOBPyme mensajeCWJ = new()
                {
                    credenciales = credenciales,
                    email = consulta?.email,
                    password = consulta?.password,
                    cuitSGR = consulta?.cuitSGR,
                    novedad = consulta.novedad,
                    fechaBusqueda = consulta.fechaBusqueda
                };

                string mensajeCWJSTR = JsonConvert.SerializeObject(mensajeCWJ);
                string conexionAzureStorage = configuration["StorageConnectionString"];

                QueueClient queue = new(conexionAzureStorage,
                    "financiamientopyme", new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });

                var respuesta = await queue.SendMessageAsync(mensajeCWJSTR);

                return Ok($"Mensaje creado con exito: {respuesta.Value.MessageId}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/sgroneclick/crearmensajefinancimientobdadv")]
        public async Task<IActionResult> CrearColaAzureFinanciamientoPymebdadv([FromBody] CrearMensajePyme consulta)
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
                MensajeColaWebJOBPyme mensajeCWJ = new()
                {
                    credenciales = credenciales,
                    email = consulta?.email,
                    password = consulta?.password,
                    cuitSGR = consulta?.cuitSGR,
                    novedad = consulta.novedad,
                    fechaBusqueda = consulta.fechaBusqueda
                };

                string mensajeCWJSTR = JsonConvert.SerializeObject(mensajeCWJ);
                string conexionAzureStorage = configuration["StorageConnectionString"];

                QueueClient queue = new(conexionAzureStorage,
                    "financiamientopymebdadv", new QueueClientOptions
                    {
                        MessageEncoding = QueueMessageEncoding.Base64
                    });

                var respuesta = await queue.SendMessageAsync(mensajeCWJSTR);

                return Ok($"Mensaje creado con exito: {respuesta.Value.MessageId}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #region Cuit
        public class SgrApiCUITRequest
        {
            public long CUIT { get; set; }
        }
        public class SgrApiCUITResponseContainer
        {
            public bool success { get; set; }
            public Data data { get; set; } = new Data();
        }
        public class Data
        {
            public General general { get; set; } = new General();
            public InformGarantiasPyme informacion_de_garantias_de_la_pyme { get; set; } = new InformGarantiasPyme();
            public IList<CertificPymes> certificados_pymes { get; set; }
            public Error error { get; set; }
        }
        public class Error
        {
            public string message { get; set; }
        }
        #endregion
        #region General
        public class General
        {
            public IList<Socio> socio_protector { get; set; }
            public IList<Socio> socio_participe { get; set; }
            public IList<Socio> cuit_de_terceros { get; set; }
        }
        public class Socio
        {
            public string business_name { get; set; }
            public string cuit { get; set; }
        }
        #endregion
        #region Informe garantias Pyme
        public class InformGarantiasPyme
        {
            public Metricas metricas { get; set; } = new Metricas();
            public IList<Data2> data { get; set; }
        }
        public class Metricas
        {
            public string cantidad_total_garantias_otorgadas { get; set; }
            public string monto_total_garantias_otorgadas { get; set; }
            public string cantidad_total_garantias_vigentes { get; set; }
            public string saldo_bruto_garantias_vigentes { get; set; }
        }
        public class Data2
        {
            public string SGR { get; set; }
            public string SALDO_BRUTO_DE_GTIAS_VIGENTE { get; set; }
            public string SALDO_DEUDA_POR_GTIAS_ABONADAS { get; set; }
            public string CANTIDAD_GTIAS_EN_MORA { get; set; }
            public string SITUACION_DE_LA_DEUDA { get; set; }
            public string DIAS_ATRASO { get; set; }
        }
        #endregion
        #region CertificadoPyme
        public class CertificPymes
        {
            public string NUMERO { get; set; }
            public string FECHA_REGISTRO { get; set; }
            public string FECHA_VENCIMIENTO { get; set; }
            public string SECTOR { get; set; }
            public string CATEGORIA { get; set; }
        }
        #endregion
        #region Metodos
        public static async Task UpdateCuenta(string cuenta_id, Metricas metricas, ApiDynamicsV2 api, Credenciales credenciales)
        {
            JObject matriz = new()
            {
                { "new_cantidadtotalgaratiasotorgadassepyme", Convert.ToInt32(metricas.cantidad_total_garantias_otorgadas)},
                { "new_montototalgaratiasotorgadassepyme", Convert.ToDecimal(metricas.monto_total_garantias_otorgadas)},
                { "new_cantidadgarantiasvigentessepyme", Convert.ToInt32(metricas.cantidad_total_garantias_vigentes)},
                { "new_saldobrutogaratiasvigentessepyme", Convert.ToDecimal(metricas.saldo_bruto_garantias_vigentes)},
            };

            ResponseAPI response = await api.UpdateRecord("accounts", cuenta_id, matriz, credenciales);
        }
        public static async Task ActualYCrearCertificadosPyme(CuentasCertificadosPymes cuentaCP, IList<CertificPymes> CertificadosCasfog, List<CategoriaCertificado> CategoriasCertificads,
            List<CondicionPymeSOC> CondicionesPymes, ApiDynamicsV2 api,  Credenciales credenciales, bool aprobarCertificado)
        {
            CertificadosCasfog = CertificadosCasfog.GroupBy(o => o.NUMERO).Select(g => g.First()).ToList();

            foreach (CertificPymes certificado in CertificadosCasfog)
            {
                bool existeCertificado = false;
                if (cuentaCP?.Certificados?.FindAll(x => x.new_numeroderegistro == Convert.ToInt32(certificado.NUMERO)).Count > 0)
                    existeCertificado = true;

                if (!existeCertificado)
                {
                    await CrearCertificados(certificado, cuentaCP.accountid, CategoriasCertificads, CondicionesPymes, api, credenciales, aprobarCertificado);
                }
            };
        }
        public static async void ActualYCrearCertificadosPymePorCuenta(CuentasCertificadosPymes cuentaCP, IList<CertificPymes> CertificadosCasfog, List<CategoriaCertificado> CategoriasCertificads,
           List<CondicionPymeSOC> CondicionesPymes, ApiDynamics apiF, ApiDynamicsV2 api, Credenciales credenciales, bool aprobarCertificado)
        {
            CertificadosCasfog = CertificadosCasfog.GroupBy(o => o.NUMERO).Select(g => g.First()).ToList();

            for (int i = 0; i < CertificadosCasfog.Count; i++)
            {
                bool existeCertificado = false;
                if (cuentaCP?.Certificados?.FindAll(x => x.new_numeroderegistro == Convert.ToInt32(CertificadosCasfog[i].NUMERO)).Count > 0)
                    existeCertificado = true;

                if (!existeCertificado)
                {
                    await CrearCertificados(CertificadosCasfog[i], cuentaCP.accountid, CategoriasCertificads, CondicionesPymes, api, credenciales, aprobarCertificado);
                }
            }
        }
        public static async Task ActualYCrearCuentasporSGR(CuentasCertificadosPymes cuentaCP, IList<Data2> datosCuentSGR,  List<SGRSOC> SGRs, General general, ApiDynamicsV2 api, Credenciales credenciales)
        {
            List<CuentaPorSGRAsociado> listaSGRs = new();
            int socioTercero = 100000001;
            int socioParticie = 100000004;
            int socioProtector = 100000003;

            foreach (Socio socio in general.cuit_de_terceros)
            {
                await BuscarCrearOActualizarCuentaPorSGR(socio, datosCuentSGR, socioTercero, cuentaCP, SGRs, api, credenciales);
            }

            foreach (Socio socio in general.socio_participe)
            {
                await BuscarCrearOActualizarCuentaPorSGR(socio, datosCuentSGR, socioParticie, cuentaCP, SGRs, api, credenciales);
            }

            foreach (Socio socio in general.socio_protector)
            {
                await BuscarCrearOActualizarCuentaPorSGR(socio, null, socioProtector, cuentaCP, SGRs, api, credenciales);
            }

            listaSGRs = cuentaCP.CuentasPorSGR.FindAll(x => x.existeCASFOG == false);

            if (listaSGRs.Count > 0)
            {
                foreach (CuentaPorSGRAsociado cuentaSGR in listaSGRs)
                {
                    await InactivarCuentaPorSGR(cuentaSGR.new_cuentasporsgrid, api, credenciales);
                }
            }
        }
        public static List<Certificado> ArmarCertificadosPymes(JToken certificadoJT)
        {
            return JsonConvert.DeserializeObject<List<Certificado>>(certificadoJT.ToString()); 
        }
        public static List<CuentaPorSGR> ArmarCuentasPorSGR(JToken cuentaPorSGRJT)
        {
            return JsonConvert.DeserializeObject<List<CuentaPorSGR>>(cuentaPorSGRJT.ToString());
        }
        public static async Task<JArray> BuscarCondicionPyme(string nombreSector, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                int cod = 0;
                switch (nombreSector)
                {
                    case "AGROPECUARIO": cod = 1; break;
                    case "COMERCIO": cod = 2; break;
                    case "CONSTRUCCIÓN":
                    case "CONSTRUCCIóN":
                    case "CONSTRUCCION":
                        cod = 3; break;
                    case "SERVICIOS": cod = 4; break;
                    case "INDUSTRIA": cod = 5; break;
                    case "MINERíA": cod = 6; break;
                }

                api.EntityName = "new_condicionpymes";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_condicionpyme'>" +
                                                           "<attribute name='new_condicionpymeid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           //"<attribute name='new_numeroderegistro'/> " +
                                                            "<attribute name='statecode'/> " +
                                                            //"<attribute name='new_sectoreconomico'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_codigo' operator='eq' value='{cod}' />" +
                                                                "<condition attribute='statecode' operator='eq' value='0' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

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
        public static async Task<JArray> BuscarCategoria(string nombreCateg, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                string tramo = "";
                int cod = 0;
                switch (nombreCateg)
                {
                    case "MICRO": tramo = "MICRO"; cod = 1; break;
                    case "PEQ": tramo = "PEQUEÑA EMPRESA"; cod = 2; break;
                    case "TRAMO1": tramo = "MEDIANA TRAMO 1"; cod = 3; break;
                    case "TRAMO2": tramo = "MEDIANA TRAMO 2"; cod = 4; break;
                }

                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_categoracertificadopymes";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_categoracertificadopyme'>" +
                                                           "<attribute name='new_categoracertificadopymeid'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_name' operator='eq' value='{tramo}' />" +
                                                                "<condition attribute='statecode' operator='eq' value='0' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

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
        public static async Task<JArray> BuscarCertificadoPyme(string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_certificadopymes";

                fetchXML = "<entity name='new_certificadopyme'>" +
                                                           "<attribute name='new_name'/> " +
                                                           "<attribute name='new_vigenciahasta'/> " +
                                                           "<attribute name='new_vigenciadesde'/> " +
                                                           "<attribute name='statuscode'/> " +
                                                           "<attribute name='new_certificadopymeid'/> " +
                                                           "<attribute name='new_numeroderegistro'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_socioparticipe' operator='eq' value='{cuenta_id}' />" +
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
        public static async Task<JArray> BuscarTodasCuentasYCertificados(ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "accounts";

                fetchXML = "<entity name='account'>" +
                                                           "<attribute name='name'/> " +
                                                           "<attribute name='accountid'/> " +
                                                           "<attribute name='new_nmerodedocumento'/> " +
                                                           "<attribute name='new_cantidadtotalgaratiasotorgadassepyme'/> " +
                                                           "<attribute name='new_montototalgaratiasotorgadassepyme'/> " +
                                                           "<attribute name='new_cantidadgarantiasvigentessepyme'/> " +
                                                           "<attribute name='new_saldobrutogaratiasvigentessepyme'/> " +
                                                           "<filter type='and'>" +
                                                                "<condition attribute='statecode' operator='eq' value='0' />" +
                                                                "<condition attribute='new_nmerodedocumento' operator='not-null'  />" +
                                                                "<condition attribute='new_rol' operator='in'>" +
                                                                    "<value>100000001</value>" +
                                                                    "<value>100000004</value>" +
                                                                "</condition>" +
                                                           "</filter>" +
                                                           "<link-entity name='new_certificadopyme' from='new_socioparticipe' to='accountid' link-type='outer' alias='certificado'>" +
                                                                "<attribute name='new_certificadopymeid'/> " +
                                                                "<attribute name='new_numeroderegistro'/> " +
                                                                "<attribute name='statecode'/> " +
                                                                "<attribute name='new_vigenciahasta'/> " +
                                                                "<attribute name='new_vigenciadesde'/> " +
                                                                "<attribute name='statuscode'/> " +
                                                                "<attribute name='statuscode'/> " +
                                                            "</link-entity>" +
                                                            "<link-entity name='new_cuentasporsgr' from='new_socio' to='accountid' link-type='outer' alias='cuentaporsgr'>" +
                                                                "<attribute name='new_name'/> " +
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
                                                                    "<condition attribute='statecode' operator='eq' value='0' />" +
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
        public static async Task<JArray> BuscarTodasCuentasYCertificadosPorCuenta(string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "accounts";

                fetchXML = "<entity name='account'>" +
                                                           "<attribute name='accountid'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='accountid' operator='eq' value='{cuenta_id}' />" +
                                                           "</filter>" +
                                                           "<link-entity name='new_certificadopyme' from='new_socioparticipe' to='accountid' link-type='outer' alias='certificado'>" +
                                                                "<attribute name='new_certificadopymeid'/> " +
                                                                "<attribute name='new_numeroderegistro'/> " +
                                                                "<attribute name='statecode'/> " +
                                                                "<attribute name='new_vigenciahasta'/> " +
                                                                "<attribute name='new_vigenciadesde'/> " +
                                                                "<attribute name='statuscode'/> " +
                                                                "<attribute name='statuscode'/> " +
                                                            "</link-entity>" +
                                                            "<link-entity name='new_cuentasporsgr' from='new_socio' to='accountid' link-type='outer' alias='cuentaporsgr'>" +
                                                                "<attribute name='new_name'/> " +
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
                                                                    "<condition attribute='statecode' operator='eq' value='0' />" +
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
        public static async Task<JArray> BuscarTodasCategorias(ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_categoracertificadopymes";

                fetchXML = "<entity name='new_categoracertificadopyme'>" +
                                                           "<attribute name='new_categoracertificadopymeid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<filter type='and'>" +
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
        public static async Task<JArray> BuscarTodasCondicionesPymes(ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_condicionpymes";

                fetchXML = "<entity name='new_condicionpyme'>" +
                                                           "<attribute name='new_condicionpymeid'/> " +
                                                           "<attribute name='new_name'/> " +
                                                           "<attribute name='statecode'/> " +
                                                           "<attribute name='new_codigo'/> " +
                                                           "<filter type='and'>" +
                                                                    "<condition attribute='statecode' operator='eq' value='0' />" +
                                                                "</filter>" +
                                                "</entity>" ;

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
        public static async Task<JArray> BuscarCuentasPorSGR(string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_cuentasporsgrs";

                fetchXML =  "<entity name='new_cuentasporsgr'>" +
                                                           "<attribute name='new_name'/> " +
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
                                                "</entity>" +
                                            "</fetch>";

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
        public static async Task<JArray> BuscarSGR(ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_sgrs";

                fetchXML = "<entity name='new_sgr'>" +
                                                           "<attribute name='new_name'/> " +
                                                           "<attribute name='new_sgrid'/> " +
                                                           "<filter type='and'>" +
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
        public static async Task<JArray> BuscarSGRHWA(string cuit, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "accounts";

                fetchXML = "<entity name='account'>" +
                                                           "<attribute name='accountid'/> " +
                                                           "<filter type='and'>" +
                                                                    $"<condition attribute='new_nmerodedocumento' operator='eq' value='{cuit}' />" +
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
        public static List<CuentasCertificadosPymes> ArmarCuentaYCertificados(JToken cuentaJT)
        {
            List<CuentasCertificadosPymes> cuentas = new();
            List<CuentasCertificadosPymes> TodasCuentas = JsonConvert.DeserializeObject<List<CuentasCertificadosPymes>>(cuentaJT.ToString());
            TodasCuentas = TodasCuentas.GroupBy(x => x.accountid).Select(acc => acc.First()).ToList();
            List<CertificadoAsociado> listaTodosCertificados = JsonConvert.DeserializeObject<List<CertificadoAsociado>>(cuentaJT.ToString());
            List<CuentaPorSGRAsociado> listaTodasCuentasPorSGR = JsonConvert.DeserializeObject<List<CuentaPorSGRAsociado>>(cuentaJT.ToString());

            TodasCuentas.ForEach(cuenta =>
            {
                CuentasCertificadosPymes cuenta_iteracion = cuenta;
                List<CertificadoAsociado> listaCertificados = listaTodosCertificados.FindAll(x => x.accountid == cuenta_iteracion.accountid).ToList();
                listaCertificados = listaCertificados.GroupBy(x => x.new_certificadopymeid).Select(g => g.First()).ToList();
                listaCertificados.RemoveAll(x => x.new_certificadopymeid == null);
                List<CuentaPorSGRAsociado> listaCuentasPorSGR = listaTodasCuentasPorSGR.FindAll(x => x.accountid == cuenta_iteracion.accountid).ToList();
                listaCuentasPorSGR = listaCuentasPorSGR.GroupBy(x => x.new_cuentasporsgrid).Select(g => g.First()).ToList();
                listaCuentasPorSGR.RemoveAll(x => x.new_cuentasporsgrid == null);
                cuenta_iteracion.Certificados = listaCertificados;
                cuenta_iteracion.CuentasPorSGR = listaCuentasPorSGR;
                cuentas.Add(cuenta_iteracion);
            });

            return cuentas;
        }
        public static List<CategoriaCertificado> ArmarTodasCategoriaCertifcado (JToken categoriasJT)
        {
            return JsonConvert.DeserializeObject<List<CategoriaCertificado>>(categoriasJT.ToString());
        }
        public static List<SGRSOC> ArmarSGRs(JToken SGRsJT)
        {
            return JsonConvert.DeserializeObject<List<SGRSOC>>(SGRsJT.ToString());
        }
        public static List<CondicionPymeSOC> ArmarTodasCondicionesPymes(JToken condicionesJT)
        {
            return JsonConvert.DeserializeObject<List<CondicionPymeSOC>>(condicionesJT.ToString());
        }
        public static async Task ActualizarCertificadoPyme(CertificadoAsociado certificadoD365, CertificPymes certificadoCasfog, List<CondicionPymeSOC> CondicionesPymes,
            ApiDynamicsV2 api, Credenciales credenciales)
        {
            CondicionPymeSOC condicionP = CondicionesPymes.FirstOrDefault(x => x.new_name.Trim() == certificadoCasfog.SECTOR.Trim());
            
            if (condicionP.new_condicionpymeid?.Length > 0)
            {    
                JObject certificadoPyme = new()
                 {
                    { "new_SectorEconomico@odata.bind", "/new_condicionpymes(" + condicionP.new_condicionpymeid + ")" },
                 };

                await api.UpdateRecord("new_certificadopymes", certificadoD365.new_certificadopymeid, certificadoPyme, credenciales);
            }
        }
        public static async Task ActualizarCertificadoPymePorCuenta(Certificado certificadoD365, CertificPymes certificadoCasfog, List<CondicionPymeSOC> CondicionesPymes,
           ApiDynamicsV2 api, Credenciales credenciales)
        {
            CondicionPymeSOC condicionP = CondicionesPymes.FirstOrDefault(x => x.new_name.Trim() == certificadoCasfog.SECTOR.Trim());

            if (condicionP.new_condicionpymeid?.Length > 0)
            {
                JObject certificadoPyme = new()
                 {
                    { "new_SectorEconomico@odata.bind", "/new_condicionpymes(" + condicionP.new_condicionpymeid + ")" },
                 };

                await api.UpdateRecord("new_certificadopymes", certificadoD365.new_certificadopymeid, certificadoPyme, credenciales);
            }
        }
        public static async Task CrearCertificados(CertificPymes certificado, string socio_id, List<CategoriaCertificado> CategoriasCertificads,
            List<CondicionPymeSOC> CondicionesPymes, ApiDynamicsV2 api, Credenciales credenciales,
            bool aprobarCertificado =  false)
        {
            Dictionary<string, string> categorias = new()
            {
                {"TRAMO1", "MEDIANA TRAMO 1"},
                {"TRAMO2", "MEDIANA TRAMO 2"},
                {"PEQ", "PEQUEÑA EMPRESA"},
                {"MICRO", "MICRO"}
            };

            string inputFormat = "yyyy-MM-dd";
            JObject Certificado = new()
            {
                { "new_SocioParticipe@odata.bind", "/accounts(" + socio_id + ")" },
                { "new_numeroderegistro", certificado.NUMERO },
                { "new_fechadeemision", DateTime.Parse(certificado.FECHA_REGISTRO).ToString("yyyy-MM-dd")},
                { "new_vigenciahasta",  DateTime.Parse(certificado.FECHA_VENCIMIENTO).ToString("yyyy-MM-dd")},
                { "new_vigenciadesde",  DateTime.Parse(certificado.FECHA_REGISTRO).ToString("yyyy-MM-dd")},
            };

            if (aprobarCertificado)
                Certificado.Add("new_aprobacion1", 100000000);

            bool Vencer = false;
            if (DateTime.ParseExact(certificado.FECHA_VENCIMIENTO, inputFormat, CultureInfo.InvariantCulture) <= DateTime.Today)
            {
                Vencer = true;
            }
            else
            {
                if (aprobarCertificado)
                    Certificado.Add("statuscode", 1); //APROBADO
            }

            string categoriaCertificadoPyme = string.Empty;

            if (categorias.TryGetValue(certificado.CATEGORIA, out string resultadoCategoria))
            {
                categoriaCertificadoPyme = resultadoCategoria;
            }

            CondicionPymeSOC condicionP = CondicionesPymes.FirstOrDefault(x => x.new_name.ToUpper().Trim() == certificado.SECTOR.ToUpper().Trim());
            CategoriaCertificado categoriaCertificado = CategoriasCertificads.FirstOrDefault(x => x.new_name.Trim() == categoriaCertificadoPyme);

            if (condicionP?.new_condicionpymeid?.Length > 0)
                Certificado.Add("new_SectorEconomico@odata.bind", "/new_condicionpymes(" + condicionP.new_condicionpymeid + ")");

            if (categoriaCertificado?.new_categoracertificadopymeid?.Length > 0)
                Certificado.Add("new_Categoria@odata.bind", "/new_categoracertificadopymes(" + categoriaCertificado.new_categoracertificadopymeid + ")");

            ResponseAPI response  = await api.CreateRecord("new_certificadopymes", Certificado, credenciales);

            if (Vencer && response.ok)
            {
                JObject CertVencimient = new()
                {
                    { "statecode", 1 },
                    { "statuscode", 2 }
                };

               await api.UpdateRecord("new_certificadopymes", response.descripcion, CertVencimient, credenciales);
            }
        }
        public static async Task ActualizarCuentaPorSGR(string cuentaPorSGR_id, Data2 datoCuentaSGR, ApiDynamicsV2 api, Credenciales credenciales)
        {
            JObject CuentaSGR = new();

            if (datoCuentaSGR.SALDO_BRUTO_DE_GTIAS_VIGENTE != "-")
                CuentaSGR.Add("new_saldobrutogaratiasvigentes", Convert.ToDecimal(datoCuentaSGR.SALDO_BRUTO_DE_GTIAS_VIGENTE));
            if (datoCuentaSGR.SALDO_DEUDA_POR_GTIAS_ABONADAS != "-")
                CuentaSGR.Add("new_saldodeudaporgtiasabonada", Convert.ToDecimal(datoCuentaSGR.SALDO_DEUDA_POR_GTIAS_ABONADAS));
            if (datoCuentaSGR.CANTIDAD_GTIAS_EN_MORA != "-")
                CuentaSGR.Add("new_cantidadgtiasenmora", Convert.ToInt32(datoCuentaSGR.CANTIDAD_GTIAS_EN_MORA));
            if (datoCuentaSGR.SITUACION_DE_LA_DEUDA != "-")
                CuentaSGR.Add("new_situaciondeladueda", Convert.ToInt32(datoCuentaSGR.SITUACION_DE_LA_DEUDA) <= 10 ? Convert.ToInt32(datoCuentaSGR.SITUACION_DE_LA_DEUDA) : 10);
            if (datoCuentaSGR.DIAS_ATRASO != "-")
                CuentaSGR.Add("new_diasdeatraso", Convert.ToInt32(datoCuentaSGR.DIAS_ATRASO));

            await api.UpdateRecord("new_cuentasporsgrs", cuentaPorSGR_id, CuentaSGR, credenciales);
        }
        public static async Task CrearCuentaPorSGR(string cuenta_id, string sgr_id, int rol, Data2 datoCuentaSGR, ApiDynamicsV2 api, Credenciales credenciales)
        {
            JObject CuentaSGR = new()
            {
                 { "new_Socio@odata.bind", "/accounts(" + cuenta_id + ")" },
                 { "new_rol", rol},
            };

            if (!string.IsNullOrEmpty(sgr_id))
                CuentaSGR.Add("new_SGR@odata.bind", $"/new_sgrs({sgr_id})");
            if (datoCuentaSGR?.SALDO_BRUTO_DE_GTIAS_VIGENTE != null && datoCuentaSGR?.SALDO_BRUTO_DE_GTIAS_VIGENTE != "-")
                CuentaSGR.Add("new_saldobrutogaratiasvigentes", Convert.ToDecimal(datoCuentaSGR.SALDO_BRUTO_DE_GTIAS_VIGENTE));
            if (datoCuentaSGR?.SALDO_DEUDA_POR_GTIAS_ABONADAS != null && datoCuentaSGR?.SALDO_DEUDA_POR_GTIAS_ABONADAS != "-")
                CuentaSGR.Add("new_saldodeudaporgtiasabonada", Convert.ToDecimal(datoCuentaSGR.SALDO_DEUDA_POR_GTIAS_ABONADAS));
            if (datoCuentaSGR?.CANTIDAD_GTIAS_EN_MORA != null && datoCuentaSGR?.CANTIDAD_GTIAS_EN_MORA != "-")
                CuentaSGR.Add("new_cantidadgtiasenmora", Convert.ToInt32(datoCuentaSGR.CANTIDAD_GTIAS_EN_MORA));
            if (datoCuentaSGR?.SITUACION_DE_LA_DEUDA != null && datoCuentaSGR?.SITUACION_DE_LA_DEUDA != "-")
                CuentaSGR.Add("new_situaciondeladueda", Convert.ToInt32(datoCuentaSGR.SITUACION_DE_LA_DEUDA) <= 10 ? Convert.ToInt32(datoCuentaSGR.SITUACION_DE_LA_DEUDA) : 10);
            if (datoCuentaSGR?.DIAS_ATRASO != null && datoCuentaSGR?.DIAS_ATRASO != "-")
                CuentaSGR.Add("new_diasdeatraso", Convert.ToInt32(datoCuentaSGR.DIAS_ATRASO));

            await api.CreateRecord("new_cuentasporsgrs", CuentaSGR, credenciales);
        }
        public static async Task InactivarCuentaPorSGR(string cuenta_id,  ApiDynamicsV2 api, Credenciales credenciales)
        {
            JObject cuenta_SGR = new()
            {
                { "statecode", 1 },
            };

           await api.UpdateRecord("new_cuentasporsgrs", cuenta_id, cuenta_SGR, credenciales);
        }
        public static string ObtenerCategoriaID(JToken categoriaJT)
        {
            string categoria_id = string.Empty;

            CategoriaCertificado categoria = JsonConvert.DeserializeObject<CategoriaCertificado>(categoriaJT.First().ToString());
            if (categoria.new_categoracertificadopymeid != null)
                categoria_id = categoria.new_categoracertificadopymeid;

            return categoria_id;
        }
        public static string ObtenerCondicionPymeID(JToken condicionPymeJT)
        {
            string condicionPyme_id = string.Empty;

            CondicionPymeSOC condicionPyme = JsonConvert.DeserializeObject<CondicionPymeSOC>(condicionPymeJT.First().ToString());
            if (condicionPyme.new_condicionpymeid != null)
                condicionPyme_id = condicionPyme.new_condicionpymeid;

            return condicionPyme_id;
        }
        public static string ObtenerSGRID(JToken sgrJT)
        {
            string sgr_id = string.Empty;

            SGR sgr = JsonConvert.DeserializeObject<SGR>(sgrJT.First().ToString());
            if (sgr.new_sgrid != null)
                sgr_id = sgr.new_sgrid;

            return sgr_id;
        }
        public static string ObtenerSGRHWID(JToken sgrJT)
        {
            string sgr_id = string.Empty;

            SGRHWA sgr = JsonConvert.DeserializeObject<SGRHWA>(sgrJT.First().ToString());
            if (sgr.accountid != null)
                sgr_id = sgr.accountid;

            return sgr_id;
        }
        private static int GetRol(General general, string nombreSGR)
        {
            int Rol = 0;
            bool tercero = true;
            foreach (Socio socio in general.socio_participe)
            {
                if (socio.business_name == nombreSGR)
                {
                    Rol = 100000004; //Socio participe
                    tercero = false;
                }
            }
            foreach (Socio socio in general.socio_protector)
            {
                if (socio.business_name == nombreSGR)
                {
                    Rol = 100000003; //Socio protector
                    tercero = false;
                }
            }
            if (tercero == true)
            {
                Rol = 100000001;
            }
            return Rol;
        }
        public static void CrearExcepcionHWA()
        {

        }
        public static async Task BuscarCrearOActualizarCuentaPorSGR(Socio socio, IList<Data2> datosCuentSGR, int rol, CuentasCertificadosPymes cuentaCP, 
            List<SGRSOC> SGRs, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                Data2 dataCuentaSGR = datosCuentSGR?.FirstOrDefault(x => x.SGR == socio.business_name);
                if (dataCuentaSGR != null)
                {
                    CuentaPorSGRAsociado CuentaSGR = cuentaCP.CuentasPorSGR.FirstOrDefault(x => x.new_sgr != null && x.new_sgr.Trim() == dataCuentaSGR.SGR.Trim());

                    if (CuentaSGR?.new_sgr?.Length > 0 && Convert.ToInt32(CuentaSGR.new_rol) == rol) //TERCERO
                    {
                        await ActualizarCuentaPorSGR(CuentaSGR.new_sgr, dataCuentaSGR, api, credenciales);
                        CuentaSGR.existeCASFOG = true;
                    }
                    else
                    {
                        SGRSOC sgr = SGRs.FirstOrDefault(x => x.new_name.Trim() == dataCuentaSGR.SGR.Trim());
                        await CrearCuentaPorSGR(cuentaCP.accountid, sgr?.new_sgrid, rol, dataCuentaSGR, api, credenciales);
                    }
                }
                else
                {
                    CuentaPorSGRAsociado CuentaSGR = cuentaCP.CuentasPorSGR.FirstOrDefault(x => x.new_sgr != null && x?.new_sgr.Trim() == socio?.business_name.Trim());

                    if (CuentaSGR == null || (CuentaSGR?.new_sgr?.Length > 0 && Convert.ToInt32(CuentaSGR.new_rol) != rol)) //TERCERO
                    {
                        SGRSOC sgr = SGRs.FirstOrDefault(x => x.new_name.Trim() == socio.business_name.Trim());
                        await CrearCuentaPorSGR(cuentaCP.accountid, sgr?.new_sgrid, rol, null, api, credenciales);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
        #region MetodosQueues
        static async Task InsertMessageAsync(QueueClient theQueue, string newMessage)
        {
            var respuesta = await theQueue.CreateIfNotExistsAsync();

            var respuesta2 = await theQueue.SendMessageAsync(newMessage);

            string resp2 = respuesta2.Value.ToString();
        }
        #endregion
    }
}
