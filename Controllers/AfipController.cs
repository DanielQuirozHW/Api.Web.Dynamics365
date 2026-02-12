using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Api.Web.Dynamics365.Servicios.AFIP;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using ServiceReference1;
using System.Net;
using System.Security;
using static Api.Web.Dynamics365.Models.Afip;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class AfipController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IConfiguration configuration;

        public AfipController(ApplicationDbContext context, IConfiguration configuration) 
        {
            this.context = context;
            this.configuration = configuration;
        }
        
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/afip/consultarafip")] 
        public async Task<IActionResult> ConsultarAFIP([FromBody] ConsultaAFIP consulta)
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
                ApiDynamicsV2 api = new();
                ParametrosAFIP parametro = new();
                //Stream strRutaCertSigner;
                byte[] strRutaCertSigner;
                ComprobanteDeVenta comprobante = new();
                CrmManager crm = new(api);
                AfipManager afip = new(crm);
                ResultadoAFIP resultadoComprobante = new();
                int TipoComprobante = 0;
                TipoComprobante = Convert.ToInt32(consulta.tipoDeComprobante);

                JArray parametrosAfip = await BuscarParametrosAFIP(consulta.parametrosWebService, api, credenciales);
                if(parametrosAfip.Count > 0)
                    parametro = ArmarParametros(parametrosAfip);
                else
                    return BadRequest("No se encontraron Parametros AFIP");

                JArray notas = await BuscarNota(parametro.new_parametrosafipid, api, credenciales);
                if (notas.Count > 0)
                    strRutaCertSigner = ArmarStreamNota(notas);
                else
                    return BadRequest("No se encontro certificado");

                SecureString strPasswordSecureString = new();
                strPasswordSecureString.AppendChar('C');
                strPasswordSecureString.AppendChar('r');
                strPasswordSecureString.AppendChar('m');
                strPasswordSecureString.AppendChar('.');
                strPasswordSecureString.AppendChar('2');
                strPasswordSecureString.AppendChar('0');
                strPasswordSecureString.AppendChar('1');
                strPasswordSecureString.AppendChar('9');
                strPasswordSecureString.AppendChar('$');
                strPasswordSecureString.MakeReadOnly();

                FEAuthRequest token = null;

                token = await afip.ObtenerLoginTicket(parametro.new_name, parametro.new_urlafiplogin, strRutaCertSigner, strPasswordSecureString, 
                    parametro.new_token, parametro.new_sign, parametro.new_cuitcontribuyente);

                List<DTOFeDetReq> listDTOFeDetReq = new();
                List<DTOAlicIva> listDTOAlicIva = new();
                List<DTOTributos> listDTOTributos = new();

                if (Convert.ToBoolean(consulta.consultarAutorizado))
                {
                    //Factura A
                    await afip.ConsultarNrosAutorizados(token, parametro.new_nropuntoventa, 1, parametro.new_parametrosafipid, credenciales);

                    ///NC A
                    await afip.ConsultarNrosAutorizados(token, parametro.new_nropuntoventa, 3, parametro.new_parametrosafipid, credenciales);

                    //ND A
                    await afip.ConsultarNrosAutorizados(token, parametro.new_nropuntoventa, 2, parametro.new_parametrosafipid, credenciales);

                    //Factura B
                    await afip.ConsultarNrosAutorizados(token, parametro.new_nropuntoventa, 6, parametro.new_parametrosafipid, credenciales);

                    //ND B
                    await afip.ConsultarNrosAutorizados(token, parametro.new_nropuntoventa, 7, parametro.new_parametrosafipid, credenciales);

                    //NC B
                    await afip.ConsultarNrosAutorizados(token, parametro.new_nropuntoventa, 8, parametro.new_parametrosafipid, credenciales);
                }

                switch (TipoComprobante)
                {
                    case 10:
                        //FACTURA DE CRÉDITO ELECTRÓNICA MiPyMEs (FCE) A
                        TipoComprobante = 201;
                        break;
                    case 11:
                        //NOTA DE DEBITO ELECTRÓNICA MiPyMEs (FCE) A
                        TipoComprobante = 202;
                        break;
                    case 12:
                        //NOTA DE CREDITO ELECTRÓNICA MiPyMEs(FCE) A
                        TipoComprobante = 203;
                        break;
                    case 13:
                        //FACTURA DE CRÉDITO ELECTRÓNICA MiPyMEs (FCE) B
                        TipoComprobante = 206;
                        break;
                    case 14:
                        //NOTA DE DEBITO ELECTRÓNICA MiPyMEs (FCE) B
                        TipoComprobante = 207;
                        break;
                    case 15:
                        //NOTA DE CREDITO ELECTRÓNICA MiPyMEs(FCE) B
                        TipoComprobante = 208;
                        break;
                }

                if (consulta.tipoDeComprobante.Equals(0)) return BadRequest();

                if(consulta.comprobante != null)
                {
                    JArray ComprobantesDeVenta = await BuscarComprobanteConItemsYPersepciones(consulta.comprobante, api, credenciales);
                    if (ComprobantesDeVenta.Count > 0)
                        comprobante = ArmarComprobanteDeVenta(ComprobantesDeVenta);

                    if (comprobante.itemsDeComprobante.Count > 0)
                    {
                        listDTOAlicIva = crm.SetDTOAlicIva(comprobante);
                    }

                    if (comprobante.percepciones.Count > 0)
                    {
                        listDTOTributos = crm.SetDTOTributos(comprobante.percepciones);
                    }

                    listDTOFeDetReq.Add(await crm.SetDTOFeDetReq(comprobante, parametro, listDTOAlicIva, listDTOTributos, credenciales));

                    resultadoComprobante = await afip.AutorizarComprobante(listDTOFeDetReq, token, listDTOFeDetReq[0].PuntoVenta, listDTOFeDetReq[0].TipoComprobante, credenciales);
                }
                else
                {
                    listDTOFeDetReq = await ProcesarPorTipo(TipoComprobante, crm, parametro, api, credenciales);
                    if(listDTOFeDetReq.Count > 0)
                    {
                       resultadoComprobante =  await afip.AutorizarComprobante(listDTOFeDetReq, token, listDTOFeDetReq[0].PuntoVenta, listDTOFeDetReq[0].TipoComprobante, credenciales);
                    }
                }

                string resultJSON = JsonConvert.SerializeObject(resultadoComprobante);
                
                return Ok(resultJSON);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region Metodos
        public static async Task<JArray> BuscarParametrosAFIP(string parametro_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_parametrosafips";

                fetchXML = "<entity name='new_parametrosafip'>" +
                                            "<attribute name='new_name'/> " +
                                            "<attribute name='new_codigomoneda'/> " +
                                            "<attribute name='new_concepto'/> " +
                                            "<attribute name='new_cuitcontribuyente'/> " +
                                            "<attribute name='new_fechaexpiraciontoken'/> " +
                                            "<attribute name='new_fechaserviciodesde'/> " +
                                            "<attribute name='new_fechaserviciohasta'/> " +
                                            "<attribute name='new_nropuntoventa'/> " +
                                            "<attribute name='new_sign'/> " +
                                            "<attribute name='new_unidaddemedida'/> " +
                                            "<attribute name='new_token'/> " +
                                             "<attribute name='new_urlafiplogin'/> " +
                                             "<filter type='and'>" +
                                                    $"<condition attribute='new_parametrosafipid' operator='eq' value='{parametro_id}' />" +
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
        public static ParametrosAFIP ArmarParametros(JToken parametrosAfipJT)
        {
            return JsonConvert.DeserializeObject<ParametrosAFIP>(parametrosAfipJT.First().ToString());
        }
        public static async Task<JArray> BuscarNota(string parametro_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "annotations";

                fetchXML = "<entity name='annotation'>" +
                                            "<attribute name='subject'/> " +
                                            "<attribute name='objectid'/> " +
                                            "<attribute name='filename'/> " +
                                            "<attribute name='documentbody'/> " +
                                            "<attribute name='mimetype'/> " +
                                            "<order attribute ='createdon' descending ='true' />" +
                                            "<filter type='and'>" +
                                                    $"<condition attribute='objectid' operator='eq' value='{parametro_id}' />" +
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
        public static byte[] ArmarStreamNota(JToken notaAfipJT)
        {
            NotaAFIP nota = JsonConvert.DeserializeObject<NotaAFIP>(notaAfipJT.First().ToString());
            byte[] content = Convert.FromBase64String(nota.documentbody);
            return content;
            //return new MemoryStream(content);
        }
        public static async Task<JArray> BuscarComprobanteConItemsYPersepciones(string comprobante_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_comprobantedeventas";

                fetchXML = "<entity name='new_comprobantedeventa'>" +
                                                           "<attribute name='new_comprobantedeventaid'/> " +
                                                           "<attribute name='new_cliente'/> " +
                                                           "<attribute name='new_conceptoiva'/> " +
                                                           "<attribute name='new_conceptopercepciones'/> " +
                                                           "<attribute name='new_condiciondepago'/> " +
                                                           "<attribute name='new_fecha'/> " +
                                                           "<attribute name='new_impuestosinternos'/> " +
                                                           "<attribute name='new_nrocomprobante'/> " +
                                                           "<attribute name='new_percepcioniibb'/> " +
                                                           "<attribute name='new_puntodeventa'/> " +
                                                           "<attribute name='new_retencioniva'/> " +
                                                           "<attribute name='new_tipo'/> " +
                                                           "<attribute name='new_tipodecomprobante'/> " +
                                                           "<attribute name='new_estadoafip'/> " +
                                                           "<attribute name='new_total'/> " +
                                                           "<attribute name='new_totalbruto'/> " +
                                                           "<attribute name='new_totalimpuestos'/> " +
                                                           "<attribute name='new_totaliva'/> " +
                                                           "<attribute name='new_totalnogravado'/> " +
                                                           "<attribute name='new_totalpercepciones'/> " +
                                                           "<attribute name='new_vencimientodelcobro'/> " +
                                                           "<attribute name='new_comprobanteasociado'/> " +
                                                           "<attribute name='new_totalexento'/> " +
                                                           "<filter type='and'>" +
                                                                "<condition attribute='statecode' operator='eq' value='0' />" +
                                                                $"<condition attribute='new_comprobantedeventaid' operator='eq' value='{comprobante_id}'/>" +
                                                           "</filter>" +
                                                           "<link-entity name='new_itemdecomprobantedeventa' from='new_comprobantedeventa' to='new_comprobantedeventaid' link-type='outer' alias='item'>" +
                                                                "<attribute name='new_itemdecomprobantedeventaid'/> " +
                                                                "<attribute name='new_alicuotaiva'/> " +
                                                                "<attribute name='new_cantidad'/> " +
                                                                "<attribute name='new_comprobantedeventa'/> " +
                                                                "<attribute name='new_importe'/> " +
                                                                "<attribute name='new_nogravado'/> " +
                                                                "<attribute name='new_iva'/> " +
                                                                "<attribute name='new_precio'/> " +
                                                                "<attribute name='new_total'/> " +
                                                                "<attribute name='new_totalimportenogravado'/> " +
                                                                "<attribute name='new_totalsinimpuestos'/> " +
                                                                "<filter type='and'>" +
                                                                    "<condition attribute='statecode' operator='eq' value='0' />" +
                                                                "</filter>" +
                                                            "</link-entity>" +
                                                            "<link-entity name='new_percepcionporcomprobantedeventa' from='new_comprobantedeventa' to='new_comprobantedeventaid' link-type='outer' alias='percepciones'>" +
                                                                "<attribute name='new_percepcionporcomprobantedeventaid'/> " +
                                                                "<attribute name='new_baseimponible'/> " +
                                                                "<attribute name='new_importe'/> " +
                                                                "<attribute name='new_percepcion'/> " +
                                                                "<attribute name='new_observaciones'/> " +
                                                                "<filter type='and'>" +
                                                                    "<condition attribute='statecode' operator='eq' value='0' />" +
                                                                "</filter>" +
                                                                "<link-entity name='new_percepcion' from='new_percepcionid' to='new_percepcion' link-type='outer' alias='percepcion'>" +
                                                                     "<attribute name='new_tipodepercepcion'/> " +
                                                                 "</link-entity>" +
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
        public static ComprobanteDeVenta ArmarComprobanteDeVenta(JToken comprobanteJT)
        {
            ComprobanteDeVenta comprobanteDeVenta = JsonConvert.DeserializeObject<ComprobanteDeVenta>(comprobanteJT.First().ToString());
            List<ItemDeComprobante> listaItems = JsonConvert.DeserializeObject<List<ItemDeComprobante>>(comprobanteJT.ToString());
            listaItems = listaItems.GroupBy(x => x.new_itemdecomprobantedeventaid).Select(g => g.First()).ToList();
            listaItems.RemoveAll(x => x.new_itemdecomprobantedeventaid == null);
            List<Percepciones> listaPercepciones = JsonConvert.DeserializeObject<List<Percepciones>>(comprobanteJT.ToString());
            listaPercepciones = listaPercepciones.GroupBy(x => x.new_percepcionporcomprobantedeventaid).Select(g => g.First()).ToList();
            listaPercepciones.RemoveAll(x => x.new_percepcionporcomprobantedeventaid == null);
            comprobanteDeVenta.itemsDeComprobante = listaItems;
            comprobanteDeVenta.percepciones = listaPercepciones;
            return comprobanteDeVenta;
        }
        public static List<ComprobanteDeVenta> ArmarComprobantes(JToken comprobanteJT)
        {
            return JsonConvert.DeserializeObject<List<ComprobanteDeVenta>>(comprobanteJT.ToString());
        }
        public static async Task<List<DTOFeDetReq>> ProcesarPorTipo(int TipoCompte, CrmManager crm, ParametrosAFIP ParametroWsAfip, ApiDynamicsV2 api, Credenciales credenciales)
        {
            List<DTOFeDetReq> listDTOFeDetReq = new();
            List<DTOAlicIva> listDTOAlicIva = new();
            List<DTOTributos> listDTOTributos = new();
            List<ComprobanteDeVenta> listaComprobantes = new();

            JArray ComprobantesDeVenta = await BuscarComprobantes(TipoCompte, api, credenciales);
            if (ComprobantesDeVenta.Count > 0)
                listaComprobantes = ArmarComprobantes(ComprobantesDeVenta);

            foreach (ComprobanteDeVenta item in listaComprobantes)
            {
                ComprobanteDeVenta comprobante = new();
                JArray ComprobantesDeVentas = await BuscarComprobanteConItemsYPersepciones(item.new_comprobantedeventaid, api, credenciales);
                if (ComprobantesDeVentas.Count > 0)
                    comprobante = ArmarComprobanteDeVenta(ComprobantesDeVenta);
                //    System.Console.WriteLine("Nro Comprobante Venta: " + item.Attributes["new_name"].ToString());

                //    System.Console.WriteLine("Recuperando Comprobante...");
                //    Entity comprobante = crm.GetComprobante(item.Id);

                //    System.Console.WriteLine("Recuperando Detalles De Comprobante...");
                //    EntityCollection detalles = crm.GetDetalles(comprobante.Id);

                //    System.Console.WriteLine("Recuperando Percepciones Comprobante...");
                //    EntityCollection percepciones = crm.GetPercepciones(comprobante.Id);

                //    System.Console.WriteLine("Generando DTOFeDetReq Comprobante...");

                if (comprobante.itemsDeComprobante.Count > 0)
                {
                    listDTOAlicIva = crm.SetDTOAlicIva(comprobante);
                }

                if (comprobante.percepciones.Count > 0)
                {
                    listDTOTributos = crm.SetDTOTributos(comprobante.percepciones);
                }

                DTOFeDetReq det = await crm.SetDTOFeDetReq(comprobante, ParametroWsAfip, listDTOAlicIva, listDTOTributos, credenciales);
                listDTOFeDetReq.Add(det);
            }

            return listDTOFeDetReq;

        }
        public static async Task<JArray> BuscarComprobantes(int Tipo, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_comprobantedeventas";

                fetchXML = "<entity name='new_comprobantedeventa'>" +
                                                           "<attribute name='new_comprobantedeventaid'/> " +
                                                           "<attribute name='new_cliente'/> " +
                                                           "<attribute name='new_conceptoiva'/> " +
                                                           "<attribute name='new_conceptopercepciones'/> " +
                                                           "<attribute name='new_condiciondepago'/> " +
                                                           "<attribute name='new_fecha'/> " +
                                                           "<attribute name='new_impuestosinternos'/> " +
                                                           "<attribute name='new_nrocomprobante'/> " +
                                                           "<attribute name='new_percepcioniibb'/> " +
                                                           "<attribute name='new_puntodeventa'/> " +
                                                           "<attribute name='new_retencioniva'/> " +
                                                           "<attribute name='new_tipo'/> " +
                                                           "<attribute name='new_tipodecomprobante'/> " +
                                                           "<attribute name='new_estadoafip'/> " +
                                                           "<attribute name='new_total'/> " +
                                                           "<attribute name='new_totalbruto'/> " +
                                                           "<attribute name='new_totalimpuestos'/> " +
                                                           "<attribute name='new_totaliva'/> " +
                                                           "<attribute name='new_totalnogravado'/> " +
                                                           "<attribute name='new_totalpercepciones'/> " +
                                                           "<attribute name='new_vencimientodelcobro'/> " +
                                                           "<attribute name='new_comprobanteasociado'/> " +
                                                           "<attribute name='new_totalexento'/> " +
                                                           "<filter type='and'>" +
                                                                "<condition attribute='new_nrocae' operator='not-null' />" +
                                                                $"<condition attribute='new_tipo' operator='qe' value={Tipo}/>" +
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
    }
}
