using Newtonsoft.Json;

namespace Api.Web.Dynamics365.Models
{
    public class Signatura
    {
        public string id { get; set; }
        public string status { get; set; }
        public string document { get; set; }
        public string signature_content { get; set; }
        public string[] invite_channel { get; set; }
        public string ip_address { get; set; }
        public string info_device { get; set; }
        public string certificate { get; set; }
        public Validations validations { get; set; }
        public string invalidation_reason { get; set; }
        public string created_date { get; set; }
        public string signed_date { get; set; }

        public class Validations
        {
            public AF AF { get; set; }
            public resultado EM { get; set; }
            public resultado PH { get; set; }
            public resultado FA { get; set; }
        }

        public class AF
        {
            public AFIP value { get; set; }
            public bool validated { get; set; }
        }

        public class AFIP
        {
            public string id { get; set; }
            public string CUIT { get; set; }
            public string nivel { get; set; }
            public string last_name { get; set; }
            public string first_name { get; set; }
            public string tipo_persona { get; set; }
            public string full_name { get; set; }
        }

        public class resultado
        {
            public string value { get; set; }
            public bool validated { get; set; }
        }

        public class Signature
        {
            public string Firmante_id { get; set; }
            public string Firma_id { get; set; }
            public string ApiKey { get; set; }
        }

        public class Invalidar
        {
            public string firmaid { get; set; }
            public string id { get; set; }
            public string invalidation_reason { get; set; }
            public string status { get; set; }
            public string new_signer { get; set; }
            public bool notify { get; set; }
            public string usuarioApi { get; set; }
            public string apiKey { get; set; }
        }

        public class InvalidarFirma
        {
            public string firmaid { get; set; }
            public string invalidation_reason { get; set; }
            public string status { get; set; }
            public string new_signer { get; set; }
            public bool notify { get; set; }
        }

        public class Reenviar
        {
            public string id { get; set; }
            public string usuarioApi { get; set; }
            public string apiKey { get; set; }
        }

        public Signatura()
        {

        }

        public class DocumentacionSignatura
        {
            public string new_documentacionid { get; set; }
            public string new_rolesfirmantes { get; set; }
            public int new_cantidadfirmasrequeridas { get; set; }
        }

        public class FirmanteSignatura
        {
            public string new_participacionaccionariaid { get; set; }
            public string new_name { get; set; }
            [JsonProperty("contacto.contactid")]
            public string contactid { get; set; }
            [JsonProperty("contacto.emailaddress1")]
            public string emailaddress1 { get; set; }
            [JsonProperty("contacto.firstname")]
            public string firstname { get; set; }
            [JsonProperty("contacto.lastname")]
            public string lastname { get; set; }
            [JsonProperty("cuenta.emailaddress1")]
            public string correoCuenta { get; set; }
            [JsonProperty("cuenta.name")]
            public string razonSocial { get; set; }
        }

        public class FirmantesYFirmas
        {
            public List<FirmanteSignatura> firmantes { get; set; }
            public int firmasRequeridas { get; set; }
        }   

        public class CancelDocument
        {
            public string cancel_reason { get; set; }
        }
    }
}
