using Newtonsoft.Json;

namespace Api.Web.Dynamics365.Models
{
    public class Afip
    {
        public class ConsultaAFIP
        {
            public string parametrosWebService { get; set; }
            public string consultarAutorizado { get; set; }
            public string comprobante { get; set; }
            public string tipoDeComprobante { get; set; }
        }
        public class ParametrosAFIP
        {
            public string new_parametrosafipid { get; set; }
            public string new_name { get; set; }
            public string new_codigomoneda { get; set; }
            public int new_concepto { get; set; }
            public string new_cuitcontribuyente { get; set; }
            public string new_fechaexpiraciontoken { get; set; }
            public string new_fechaserviciodesde { get; set; }
            public string new_fechaserviciohasta { get; set; }
            public int new_nropuntoventa { get; set; }
            public string new_sign { get; set; }
            public string new_unidaddemedida { get; set; }
            public string new_token { get; set; }
            public string new_urlafiplogin { get; set; }
        }
        public class NotaAFIP
        {
            public string documentbody { get; set; }
        }
        public class DTOFeDetReq
        {
            public long NroComprobante { get; set; } 
            public Guid IdComprobante { get; set; }
            public Guid IdParametroWSAfip { get; set; }
            public int TipoComprobante { get; set; }
            public int PuntoVenta { get; set; }
            public int Concepto { get; set; }
            public long NroDocumento { get; set; }
            public int TipoDocumento { get; set; }
            public string FechaEmision { get; set; }
            public double ImporteTotal { get; set; }
            public double ImporteNeto { get; set; }
            public double ImporteIva { get; set; }
            public double ImporteNetoNoGravado { get; set; }
            public double ImporteExcento { get; set; }
            public double ImporteTributos { get; set; }
            public string FechaDesde { get; set; }
            public string FechaHasta { get; set; }
            public string FechaVtoPago { get; set; }
            public string MonedaId { get; set; }
            public int Cotizacion { get; set; }
            public int statuscode { get; set; }
            public int new_estadoafip { get; set; }
            public List<DTOAlicIva> detalles { get; set; }
            public List<DTOTributos> percepciones { get; set; }
            public ComprobanteAsociado CbteAsociado { get; set; }
        }
        public class ComprobanteAsociado
        {
            public int PtoVenta { get; set; }
            public long NroComprobante { get; set; }
            public short TipoComprobante { get; set; }
        }
        public class DTOAlicIva
        {
            public double BaseImponible { get; set; }
            public int TipoIva { get; set; }
            public double Total { get; set; }
        }
        public class DTOTributos
        {
            public double BaseImponible { get; set; }
            public int Tipo { get; set; }
            public double Total { get; set; }
            public double Alicuota { get; set; }
        }
        public class ComprobanteDeVenta
        {
            public string new_comprobantedeventaid { get; set; }
            [JsonProperty("_new_cliente_value")]
            public string new_cliente { get; set; }
            public string new_conceptoiva { get; set; }
            public string new_conceptopercepciones { get; set; }
            public string new_condiciondepago { get; set; }
            public string new_fecha { get; set; }
            public string new_impuestosinternos { get; set; }
            public string new_nrocomprobante { get; set; }
            public decimal new_percepcioniibb { get; set; }
            public int new_puntodeventa { get; set; }
            public string new_retencioniva { get; set; }
            public int new_tipo { get; set; }
            public string new_tipodecomprobante { get; set; }
            public int new_estadoafip { get; set; }
            public double new_total { get; set; }
            public double new_totalbruto { get; set; }
            public string new_totalimpuestos { get; set; }
            public double new_totaliva { get; set; }
            public double new_totalnogravado { get; set; }
            public double new_totalpercepciones { get; set; }
            public string new_vencimientodelcobro { get; set; }
            [JsonProperty("_new_comprobanteasociado_value")]
            public string new_comprobanteasociado { get; set; }
            public double new_totalexento { get; set; }
            public List<ItemDeComprobante> itemsDeComprobante { get; set; }
            public List<Percepciones> percepciones { get; set; }
        }
        public class ItemDeComprobante
        {
            [JsonProperty("item.new_itemdecomprobantedeventaid")]
            public string new_itemdecomprobantedeventaid { get; set; }
            [JsonProperty("item.new_alicuotaiva")]
            public int new_alicuotaiva { get; set; }
            [JsonProperty("item.new_cantidad")]
            public string new_cantidad { get; set; }
            [JsonProperty("item.new_comprobantedeventa")]
            public string new_comprobantedeventa { get; set; }
            [JsonProperty("item.new_importe")]
            public double new_importe { get; set; }
            [JsonProperty("item.new_nogravado")]
            public string new_nogravado { get; set; }
            [JsonProperty("item.new_iva")]
            public decimal new_iva { get; set; }
            [JsonProperty("item.new_precio")]
            public string new_precio { get; set; }
            [JsonProperty("item.new_total")]
            public string new_total { get; set; }
            [JsonProperty("item.new_totalimportenogravado")]
            public decimal new_totalimportenogravado { get; set; }
            [JsonProperty("item.new_totalsinimpuestos")]
            public double new_totalsinimpuestos { get; set; }
        }
        public class Percepciones
        {
            [JsonProperty("percepcion.new_percepcionporcomprobantedeventaid")]
            public string new_percepcionporcomprobantedeventaid { get; set; }
            [JsonProperty("percepcion.new_baseimponible")]
            public double new_baseimponible { get; set; }
            [JsonProperty("percepcion.new_importe")]
            public double new_importe { get; set; }
            [JsonProperty("percepcion.new_percepcion")]
            public string new_percepcion { get; set; }
            [JsonProperty("percepcion.new_observaciones")]
            public string new_observaciones { get; set; }
            [JsonProperty("percepcion.new_tipodepercepcion")]
            public int new_tipodepercepcion { get; set; }
        }
        public class Cliente
        {
            public string new_nmerodedocumento { get; set; }
            [JsonProperty("tipoDocumento.new_codigo")]
            public int new_codigo { get; set; }
        }
        public class ResultadoAFIP
        {
            public int codigo { get; set; }
            public string resultado { get; set; }
        }
    }
}