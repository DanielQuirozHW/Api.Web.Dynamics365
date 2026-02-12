using Newtonsoft.Json;

namespace Api.Web.Dynamics365.Models
{
    public class SgrOneClick
    {
        public class ConsultarCuit
        {
            public string email { get; set; }
            public string password { get; set; }
            public string cuit { get; set; }
            public string cuenta_id { get; set; }
            public bool aprobarCertificadoPyme { get; set; }
            public string cuitSGR { get; set; }
        }
        public class CrearMensajePyme
        {
            public string email { get; set; }
            public string password { get; set; }
            public string cuit { get; set; }
            public string cuitSGR { get; set; }
            public bool novedad { get; set; }
            public DateTime fechaBusqueda { get; set; }
        }
        public class Certificado
        {
            public string new_certificadopymeid { get; set; }
            public int new_numeroderegistro { get; set; }
            public int statecode { get; set; }
            public string new_vigenciahasta { get; set; }
            public string new_vigenciadesde { get; set; }
            public int statuscode{ get; set; }
            [JsonProperty("sgr.new_sgrid")]
            public string new_sgrid { get; set; }
        }
        public class CertificadoAsociado
        {
            [JsonProperty("certificado.new_certificadopymeid")]
            public string new_certificadopymeid { get; set; }
            [JsonProperty("certificado.new_numeroderegistro")]
            public int new_numeroderegistro { get; set; }
            [JsonProperty("certificado.statecode")]
            public int statecode { get; set; }
            [JsonProperty("certificado.new_vigenciahasta")]
            public string new_vigenciahasta { get; set; }
            [JsonProperty("certificado.new_vigenciadesde")]
            public string new_vigenciadesde { get; set; }
            [JsonProperty("certificado.statuscode")]
            public int statuscode { get; set; }
            public string accountid { get; set; }
        }
        public class CuentaPorSGRAsociado
        {
            [JsonProperty("cuentaporsgr.new_name")]
            public string new_name { get; set; }
            [JsonProperty("cuentaporsgr.new_sgr@OData.Community.Display.V1.FormattedValue")]
            public string new_sgr { get; set; }
            [JsonProperty("cuentaporsgr.new_cuentasporsgrid")]
            public string new_cuentasporsgrid { get; set; }
            [JsonProperty("cuentaporsgr.statuscode")]
            public string statuscode { get; set; }
            [JsonProperty("cuentaporsgr.new_rol")]
            public string new_rol { get; set; }
            [JsonProperty("cuentaporsgr.new_saldobrutogaratiasvigentes")]
            public string new_saldobrutogaratiasvigentes { get; set; }
            [JsonProperty("cuentaporsgr.new_saldodeudaporgtiasabonada")]
            public string new_saldodeudaporgtiasabonada { get; set; }
            [JsonProperty("cuentaporsgr.new_cantidadgtiasenmora")]
            public string new_cantidadgtiasenmora { get; set; }
            [JsonProperty("cuentaporsgr.new_situaciondeladueda")]
            public string new_situaciondeladueda { get; set; }
            [JsonProperty("cuentaporsgr.new_diasdeatraso")]
            public string new_diasdeatraso { get; set; }
            public string accountid { get; set; }
            public bool existeCASFOG { get; set; }
        }
        public class CategoriaCertificado
        {
            public string new_categoracertificadopymeid { get; set; }
            public string new_name { get; set; }
        }
        public class CondicionPymeSOC
        {
            public string new_condicionpymeid { get; set; }
            public string new_name { get; set; }
            public int statecode { get; set; }
            public string new_codigo { get; set; }
        }
        public class SGRSOC
        {
            public string new_sgrid { get; set; }
            public string new_name { get; set; }
        }

        public class SGRHWA
        {
            public string accountid { get; set; }
            public string name { get; set; }
        }
        public class CuentasCertificadosPymes
        {
            public string accountid { get; set; }
            public string name { get; set; }
            public string new_nmerodedocumento { get; set; }
            public string new_cantidadtotalgaratiasotorgadassepyme { get; set; }
            public string new_montototalgaratiasotorgadassepyme { get; set; }
            public string new_cantidadgarantiasvigentessepyme { get; set; }
            public string new_saldobrutogaratiasvigentessepyme { get; set; }
            public List<CertificadoAsociado> Certificados { get; set; }
            public List<CuentaPorSGRAsociado> CuentasPorSGR { get; set; }
        }
        public class CuentaPorSGR
        {
            public string new_name { get; set; }
            [JsonProperty("new_sgr@OData.Community.Display.V1.FormattedValue")]
            public string new_sgr { get; set; }
            public string new_cuentasporsgrid { get; set; }
            public string statuscode { get; set; }
            public string new_rol { get; set; }
            public string new_saldobrutogaratiasvigentes { get; set; }
            public string new_saldodeudaporgtiasabonada { get; set; }
            public string new_cantidadgtiasenmora { get; set; }
            public string new_situaciondeladueda { get; set; }
            public string new_diasdeatraso { get; set; }
            [JsonProperty("sgr.new_sgrid")]
            public string new_sgrid { get; set; }
        }

        public class MensajeColaWebJOB
        {
            public Credenciales credenciales { get; set; }
            public string email { get; set; }
            public string password { get; set; }
            public bool aprobarCertificadoPyme { get; set; }
            public string cuitSGR { get; set; }
        }
        public class MensajeColaWebJOBPyme
        {
            public Credenciales credenciales { get; set; }
            public string email { get; set; }
            public string password { get; set; }
            public string cuitSGR { get; set; }
            public bool novedad { get; set; }
            public DateTime fechaBusqueda { get; set; }
        }
    }
}
