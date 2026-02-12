using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Api.Web.Dynamics365.Models
{
    public class Lufe
    {
        public class Entidad
        {
            public long cuit { get; set; }
            public string nombre { get; set; }
            public string forma_juridica { get; set; }
            public int actividad_principal { get; set; }
            public string personeria { get; set; }
            public string fecha_contrato_social { get; set; }
            public Impuesto[] impuestos { get; set; }
            public Actividad[] actividades { get; set; }
            public ContactoLufe[] contactos { get; set; }
            public Autoridad[] autoridad { get; set; }
            public DocumentoLufe[] documentos { get; set; }
            public DocumentosEntidad[] todosDocumentos { get; set; }
            public CertificadoPymeLufe certificado_pyme { get; set; }
        }
        public class Impuesto
        {
            public string estado { get; set; }
            public string origen { get; set; }
            public string periodo_vigencia { get; set; }
            public string fecha_actualizacion { get; set; }
            public string codigo_caracterizacion { get; set; }
            public string identificacion_estado_vigente { get; set; }
        }
        public class Actividad
        {
            public string orden { get; set; }
            public string codigo { get; set; }
            public string estado { get; set; }
            public string origen { get; set; }
            public string vigente { get; set; }
            public string nomenclador { get; set; }
            public string periodo_vigencia { get; set; }
            public string fecha_actualizacion { get; set; }
        }
        public class ContactoLufe
        {
            public string nombre { get; set; }
            public string tipo { get; set; }
            public string telefono { get; set; }
            public string email { get; set; }
        }
        public class CertificadoPymeLufe
        {
            public string categoria { get; set; }
            public string desde { get; set; }
            public string fecha_emision { get; set; }
            public string hasta { get; set; }
            public int nro_registro { get; set; }
            public string sector { get; set; }
            public int transaccion { get; set; }
        }
        public class DeudaEntidad
        {
            public bool tiene_deuda { get; set; }
            public Deudas deudas { get; set; }
        }
        public class Deudas
        {
            public Deuda[] deuda { get; set; }
        }
        public class Deuda
        {
            public int impuesto { get; set; }
            public string periodo_fiscal { get; set; }
        }
        public class Autoridades
        {
            public Autoridad[] autoridad { get; set; }
        }
        public class Autoridad
        {
            public long cuit { get; set; }
            public string? denominacion { get; set; }
            public int? es_accionista { get; set; }
            public decimal? porc_accionista { get; set; }
            public string? cargo { get; set; }
        }
        public class Indicador
        {
            public decimal? periodo { get; set; }
            public decimal? fechapresentacion { get; set; }
            public decimal? rentabilidad { get; set; }
            public decimal? liquidez_cte { get; set; }
            public decimal? endeudamiento { get; set; }
            public decimal? capital_trabajo { get; set; }
            public decimal? plazo_medio_ctas_a_cobar { get; set; }
            public decimal? rotacion_inventarios { get; set; }
            public decimal? plazo_medio_ctas_a_pagar { get; set; }
            public decimal? compras_totales_insumos { get; set; }
            public decimal? vtas_mensuales_prom { get; set; }
            public decimal? inmovilizacion_bienes_de_uso { get; set; }
            public decimal? productividad_bs_de_uso_afectados_exportacion { get; set; }
            public decimal? incidencia_amortizaciones_bs_uso_sobre_costos { get; set; }
            public decimal? solvencia { get; set; }
            public decimal? endeudamiento_diferido { get; set; }
            public decimal? liquidez_acida { get; set; }
            public decimal? ebitda { get; set; }
            public decimal? retorno_activo_total { get; set; }
            public decimal? retorno_patrimonio_neto { get; set; }
            public decimal? utilidad_neta_patrimonio_neto { get; set; }
            public decimal? utilidad_bruta_costos { get; set; }
            public decimal? ebitda_vtas { get; set; }
            public decimal? ebit { get; set; }
        }
        public class IndicadorPostBalance
        {
            public JObject ventas { get; set; }
            public JObject compras { get; set; }
        }
        public class TodosDocumentosLufe
        {
            DocumentosEntidad[] documentosLufe;
        }
        public class DocumentosEntidad
        {
            public string periodo { get; set; }
            public DocumentoLufe[] documentos { get; set; }
        }
        public class DocumentoLufe
        {
            public string nombre { get; set; }
            public string url { get; set; }
        }
        public class OnboardingLufe
        {
            public string email { get; set; }
            public string cuit { get; set; }
            public string tipoDocumento { get; set; }
            public string accountid { get; set; }
        }
        public class UnidadDeNegocioLufe
        {
            public string new_apikeylufe { get; set; }
        }
        public class DocumentacionPorCuentaLufe
        {
            [JsonProperty("_new_documentoid_value")]
            public string new_documentoid { get; set; }
            public string new_documentacionporcuentaid { get; set;}
        }
        public class ConsultaLufe
        {
            public string Cuit { get; set; }
            public string Socio_id { get; set; }
            public string ApiKey{ get; set; }
        }
        public class IndicadorSOC 
        { 
            public string new_indicadoresid { get; set; }
            public string new_periodo { get; set; }
            public string new_fechapresentacion { get; set; }
            public string new_ventas_pb { get; set; }
            public string new_compras_pb { get; set; }
        }
    }
}
