using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.OpenApi.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using ServiceReference1;
using static Api.Web.Dynamics365.Models.Afip;

namespace Api.Web.Dynamics365.Servicios.AFIP
{
    public class CrmManager
    {
        private readonly ApiDynamicsV2 api;

        public CrmManager(ApiDynamicsV2 api)
        {
            this.api = api;
        }
        public async Task UpdateTokenYSign(string token, string sign, string idParam, long UltimoNroAutorizado, int TipoCompte, Credenciales credenciales)
        {

            JObject parametro = new()
            {
                { "new_token", token },
                { "new_sign", sign }
            };

            //Factura A
            if (TipoCompte.Equals(1)) parametro.Add("new_ultimonroautorizadofactura", int.Parse(UltimoNroAutorizado.ToString()));

            //Nota Debito A
            if (TipoCompte.Equals(2)) parametro.Add("new_ultimonroautorizadond", int.Parse(UltimoNroAutorizado.ToString()));

            //Nota Credito A
            if (TipoCompte.Equals(3)) parametro.Add("new_ultimonroautorizadonc", int.Parse(UltimoNroAutorizado.ToString()));

            //Factura B
            if (TipoCompte.Equals(6)) parametro.Add("new_ultimonroautorizadofacturab", int.Parse(UltimoNroAutorizado.ToString()));

            //Nota Debito B
            if (TipoCompte.Equals(7)) parametro.Add("new_ultimonroautorizadondb", int.Parse(UltimoNroAutorizado.ToString()));

            //Nota Credito B
            if (TipoCompte.Equals(8)) parametro.Add("new_ultimonroautorizadoncb", int.Parse(UltimoNroAutorizado.ToString()));

            await api.UpdateRecord("new_parametrosafips", idParam, parametro, credenciales);
        }
        public List<DTOAlicIva> SetDTOAlicIva(ComprobanteDeVenta comprobante)
        {
            List<DTOAlicIva> retorno = new();

            foreach (ItemDeComprobante item in comprobante.itemsDeComprobante)
            {
                DTOAlicIva detalle = new();
                //19/01/
                //
                //RV agrupar por alicuotas
                double BaseImponible = 0;


                //Universo de Comprobantes tipo B
                if (comprobante.new_tipo.Equals(6)
                    || comprobante.new_tipo.Equals(7)
                    || comprobante.new_tipo.Equals(8)
                    || comprobante.new_tipo.Equals(13)
                    || comprobante.new_tipo.Equals(14)
                    || comprobante.new_tipo.Equals(15))
                {
                    //detalle.BaseImponible = (double)((Money)item.Attributes["new_totalsinimpuestos"]).Value;
                    BaseImponible = item.new_totalsinimpuestos;
                }
                else
                {
                    //detalle.BaseImponible = (double)((Money)item.Attributes["new_importe"]).Value;
                    BaseImponible = item.new_importe;
                }

                //si ya se cargo la alicuota sumar
                if (retorno.Exists(x => x.TipoIva == item.new_alicuotaiva))
                {//sumo

                    //converto a decimal y sumo, si sumo dos doubles me agrega decimales (??)
                    decimal BI = (decimal)retorno.First(x => x.TipoIva == item.new_alicuotaiva).BaseImponible;
                    BI += (decimal)BaseImponible;
                    retorno.First(x => x.TipoIva == item.new_alicuotaiva).BaseImponible = (double)BI;

                    decimal Total = (decimal)retorno.First(x => x.TipoIva == item.new_alicuotaiva).Total;
                    Total += item.new_iva > 0 ? item.new_iva : 0;
                    retorno.First(x => x.TipoIva == item.new_alicuotaiva).Total = (double)Total;

                    //retorno.First(x => x.TipoIva == ((OptionSetValue)item.Attributes["new_alicuotaiva"]).Value).BaseImponible += BaseImponible;
                    //retorno.First(x => x.TipoIva == ((OptionSetValue)item.Attributes["new_alicuotaiva"]).Value).Total += (item.Attributes.Contains("new_iva")) ? (double)((Money)item.Attributes["new_iva"]).Value : 0;
                }
                else
                {//agrego
                    detalle.BaseImponible = BaseImponible;
                    detalle.TipoIva = item.new_alicuotaiva;
                    detalle.Total = item.new_iva > 0 ? Convert.ToDouble(item.new_iva) : 0;
                    retorno.Add(detalle);
                }

            }
            return retorno;
        } 
        public List<DTOTributos> SetDTOTributos(List<Percepciones> listaPercepciones)
        {
            List<DTOTributos> retorno = new();

            foreach (Percepciones item in listaPercepciones)
            {
                DTOTributos detalle = new()
                {
                    BaseImponible = item.new_baseimponible,
                    Tipo = item.new_tipodepercepcion,
                    Total = item.new_importe
                };

                retorno.Add(detalle);
            }

            return retorno;
        }
        public async Task<DTOFeDetReq> SetDTOFeDetReq(ComprobanteDeVenta comprobante, ParametrosAFIP parametroWS, List<DTOAlicIva> Ivas, List<DTOTributos> Tributos, Credenciales credenciales)
        {
            DTOFeDetReq retorno = new()
            {
                IdComprobante = new Guid(comprobante.new_comprobantedeventaid),
                IdParametroWSAfip = new Guid(parametroWS.new_parametrosafipid),
                Concepto = parametroWS.new_concepto,
                Cotizacion = 1,
                FechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Parse(parametroWS.new_fechaserviciodesde).Date.Day).ToString("yyyyMMdd"),
                FechaEmision = DateTime.Parse(comprobante.new_fecha).ToString("yyyyMMdd"),
                FechaHasta = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).ToString("yyyyMMdd"), // ((DateTime)parametroWS.Attributes["new_fechaserviciohasta"]).Date.Day).ToString("yyyyMMdd");
                FechaVtoPago = DateTime.Parse(comprobante.new_vencimientodelcobro).ToString("yyyyMMdd"),
                ImporteExcento = comprobante.new_totalexento,
                ImporteIva = comprobante.new_totaliva,
                ImporteNeto = comprobante.new_totalbruto,
                ImporteNetoNoGravado = comprobante.new_totalnogravado,
                ImporteTotal = comprobante.new_total,
                ImporteTributos = comprobante.new_totalpercepciones > 0 ? comprobante.new_totalpercepciones : 0,
                MonedaId = parametroWS.new_codigomoneda,
                NroComprobante = long.Parse(comprobante.new_nrocomprobante)
            };

            if (comprobante.new_estadoafip > 0)
                retorno.new_estadoafip = comprobante.new_estadoafip;

            if (!string.IsNullOrEmpty(comprobante.new_comprobanteasociado))
            {
                JArray comprobanteAsosciadoA = await BuscarComprobante(comprobante.new_comprobanteasociado, api, credenciales);
                if (comprobanteAsosciadoA.Count > 0)
                {
                    ComprobanteDeVenta comprobanteAsociado = ArmarComprobante(comprobanteAsosciadoA);
                    retorno.CbteAsociado = new ComprobanteAsociado
                    {
                        PtoVenta = comprobanteAsociado.new_puntodeventa,
                        TipoComprobante = short.Parse(comprobanteAsociado.new_tipo.ToString()),
                        NroComprobante = long.Parse(comprobanteAsociado.new_nrocomprobante.ToString())
                    };
                }
            }

            try
            {
                JArray clienteA = await BuscarCliente(comprobante.new_cliente, api, credenciales);
                if (clienteA.Count > 0)
                {
                    Cliente cliente = ArmarCliente(clienteA);
                    retorno.NroDocumento = long.Parse(cliente.new_nmerodedocumento);
                    retorno.TipoDocumento = cliente.new_codigo;
                }
            }
            catch (Exception ex)
            {
                await this.UpdateComprobante(string.Concat("Error en Consulta de cliente ", ex.Message), retorno.NroComprobante.ToString(), retorno.PuntoVenta, retorno.TipoComprobante, credenciales);
            }

            retorno.PuntoVenta = comprobante.new_puntodeventa;
            retorno.TipoComprobante = comprobante.new_tipo;
            retorno.ImporteTotal = comprobante.new_total;

            retorno.percepciones = Tributos;
            retorno.detalles = Ivas;

            return retorno;
        }
        public async Task UpdateComprobante(string CAE, DateTime FechaVtoCAE, long NroCompte, int PtoVenta, int Tipo, Obs[] Observaciones, Credenciales credenciales)
        {
            JArray comprobantesA = await BuscarComprobantePorNroPtoVentaTipo(NroCompte, PtoVenta, Tipo, api, credenciales);
            if (comprobantesA.Count.Equals(0)) return;

            ComprobanteDeVenta comptes = ArmarComprobante(comprobantesA);

            string obs = string.Empty;
            if (Observaciones != null)
            {
                obs = string.Concat("| Nro Comprobante |", " Codigo Observación |", " Descripcion |", "\r\n");
                foreach (Obs item in Observaciones)
                {
                    obs += string.Concat("| ", NroCompte, " | ", item.Code, " | ", item.Msg, "|", "\r\n");
                }
            }

            JObject comprobante = new()
            {
                { "new_nrocae", CAE },
                { "new_fechavencimientocae",  FechaVtoCAE.ToString("yyyy-MM-dd")},
                { "new_motivorechazoafip",  obs},
                { "new_estadoafip", 100000003 }// Aprobado AFIP 
            };

            await api.UpdateRecord("new_comprobantedeventas", comptes.new_comprobantedeventaid, comprobante, credenciales);
        }
        public async Task UpdateComprobante(string errores, string NroCompte, int Tipo, int PtoVenta, Credenciales credenciales)
        {
            JArray comprobantesA = await BuscarComprobantePorNroPtoVentaTipo(long.Parse(NroCompte), PtoVenta, Tipo, api, credenciales);
            if (comprobantesA.Count.Equals(0)) return;

            ComprobanteDeVenta comptes = ArmarComprobante(comprobantesA);


            JObject comprobante = new()
            {
                { "new_motivorechazoafip", errores },
                { "new_estadoafip",  100000001}, //Rechazado
            };

            await api.UpdateRecord("new_comprobantedeventas", comptes.new_comprobantedeventaid, comprobante, credenciales);
        }
        public async Task UpdateComprobantes(string errores, List<DTOFeDetReq> NroComptes, Credenciales credenciales)
        {
            foreach (DTOFeDetReq item in NroComptes)
            {
                JArray comprobantesA = await BuscarComprobantePorNroTipo(item.NroComprobante, item.TipoComprobante, api, credenciales);
                if (comprobantesA.Count.Equals(0)) return;

                ComprobanteDeVenta comptes = ArmarComprobante(comprobantesA);

                JObject comprobante = new()
                {
                    { "new_motivorechazoafip", errores },
                    { "new_estadoafip",  100000001}, //Rechazado
                };

                await api.UpdateRecord("new_comprobantedeventas", comptes.new_comprobantedeventaid, comprobante, credenciales);
            }
        }
        public async Task UpdateComprobante(Obs[] Rechazo, string NroCompte, int PtoVenta, int Tipo, Credenciales credenciales)
        {
            JArray comprobantesA = await BuscarComprobantePorNroPtoVentaTipo(long.Parse(NroCompte), PtoVenta, Tipo, api, credenciales);
            if (comprobantesA.Count.Equals(0)) return;

            ComprobanteDeVenta comptes = ArmarComprobante(comprobantesA);

            //actualizo
            JObject comprobante = new();
            string errores = string.Concat("| Nro Comprobante |", " Codigo Error |", " Descripcion |", "\r\n");
            if (Rechazo != null)
            {
                foreach (Obs item in Rechazo)
                {
                    errores += string.Concat("| ", NroCompte, " | ", item.Code, " | ", item.Msg, "|", "\r\n");
                }

                comprobante.Add("new_motivorechazoafip", errores);
            }
            else
            {
                comprobante.Add("new_motivorechazoafip", string.Empty);
            }

            comprobante.Add("new_estadoafip", 100000001); //Rechazado
            await api.UpdateRecord("new_comprobantedeventas", comptes.new_comprobantedeventaid, comprobante, credenciales);
        }
        public static async Task<JArray> BuscarComprobante(string comprobante_id, ApiDynamicsV2 api, Credenciales credenciales)
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
                                                                $"<condition attribute='new_comprobantedeventaid' operator='eq' value='{comprobante_id}' />" +
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
        public static ComprobanteDeVenta ArmarComprobante(JToken comprobanteJT)
        {
            return JsonConvert.DeserializeObject<ComprobanteDeVenta>(comprobanteJT.First().ToString());
        }
        public static async Task<JArray> BuscarCliente(string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "accounts";

                fetchXML = "<entity name='account'>" +
                                             "<attribute name='new_nmerodedocumento'/> " +
                                             "<filter type='and'>" +
                                                    $"<condition attribute='accountid' operator='eq' value='{cuenta_id}' />" +
                                             "</filter>" +
                                             "<link-entity name='new_tipodedocumento' from='new_tipodedocumentoid' to='new_tipodedocumentoid' link-type='outer' alias='tipoDocumento'>" +
                                                    "<attribute name='new_codigo'/> " +
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
        public static Cliente ArmarCliente(JToken clienteJT)
        {
            return JsonConvert.DeserializeObject<Cliente>(clienteJT.First().ToString());
        }
        public static async Task<JArray> BuscarComprobantePorNroPtoVentaTipo(long NroCompte, int PtoVenta, int Tipo, ApiDynamicsV2 api, Credenciales credenciales)
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
                                                                $"<condition attribute='new_nrocomprobante' operator='eq' value='{NroCompte.ToString().PadLeft(8, '0')}' />" +
                                                                $"<condition attribute='new_tipo' operator='eq' value='{Tipo}'/>" +
                                                                $"<condition attribute='new_puntodeventa' operator='eq' value='{PtoVenta}'/>" +
                                                           "</filter>" +
                                                "</entity>";
                //Tiene que estar activo
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
        public static async Task<JArray> BuscarComprobantePorNroTipo(long NroCompte, int Tipo, ApiDynamicsV2 api, Credenciales credenciales)
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
                                                                $"<condition attribute='new_nrocomprobante' operator='eq' value='{NroCompte.ToString().PadLeft(8, '0')}' />" +
                                                                $"<condition attribute='new_tipo' operator='eq' value='{Tipo}' />" +
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
    }
}
