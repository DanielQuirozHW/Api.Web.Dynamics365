using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class Documents
    {
        public string id { get; set; }
        public string title { get; set; }
        public string status { get; set; }


        public class CrearDocumento
        {
            public string DocumentacionPorCuenta_id { get; set; }
            public string DocumentacionPorOperacion_id { get; set; }
            public string Documentacion_id { get; set; }
            public string Socio_id { get; set; }
            public string ApiKey { get; set; }
            public string UnidadDeNegocio { get; set; }
        }

        public class SocioOperacion
        {
            public string name { get; set; }
    }

        public class file
        {
            [Required]
            public string file_content { get; set; } //base64
            [Required]
            [MaxLength(254, ErrorMessage = "El campo title no puede contener mas de 254 caracteres")]
            public string title { get; set; }
            public string[] validations { get; set; }
            [Required]
            public string fashion { get; set; }
            public string[] selected_emails { get; set; }
            //public string[] selected_phones { get; set; }
            //public int certain_amount_value { get; set; }
            //public CustomSignatures[] custom_signatures { get; set; }
            //public string complete_url { get; set; }
            //public bool must_view { get; set; }
            //public bool include_document_url { get; set; }
            public int required_signatures { get; set; }
        }

        //public class CustomSignatures
        //{
        //    public Validations validations { get; set; }
        //    public string[] invite_channel { get; set; } 

        //}

        public class Validation
        {
            public result AF { get; set; }
            public result EM { get; set; }
            public result PH { get; set; }
            public result FA { get; set; }
        }

        public class result
        {
            public string value { get; set; }
            public bool validated { get; set; }
        }

        public class Errores
        {
            public Errors errors { get; set; }
            public string[] non_field_errors { get; set; }
        }

        public class Errors
        {
            public string[] property_name { get; set; }
        }

        public class responseSignature
        {
            public string id { get; set; }
            public string file_content { get; set; }
            public string title { get; set; }
            public string status { get; set; }
            public string tiny_url { get; set; }
            public string required_signatures { get; set; }
            //public string[] validations { get; set; }
            public string fashion { get; set; }
            public string fixed_sign_url { get; set; }
            public string complete_url { get; set; }
            //public string[] selected_emails { get; set; }
            //public string[] selected_phones { get; set; }
            //public int certain_amount_value { get; set; }
            //public string complete_url { get; set; }
            //public bool must_view { get; set; }
            //public bool include_document_url { get; set; }
            //public int required_signatures { get; set; }
            public Signatures[] signatures { get; set; }
        }

        public class DocumentDetail
        {
            public string id { get; set; }
            public string title { get; set; }
            public string file_content { get; set; }
            public string file_name { get; set; }
            public string status { get; set; }
            public string archived { get; set; }
            public string fixed_sign_url { get; set; }
            public string tiny_url { get; set; }
            public string tiny_id { get; set; }
            public string file_hash { get; set; }
            public Settings settings { get; set; }
            public string creation_date { get; set; }
            public string cancel_reason { get; set; }
            public Signatures[] signatures { get; set; }
        }

        public class Settings
        {
            public string required_signatures { get; set; }
            public string fashion { get; set; }
            public string include_document_url { get; set; }
            public string must_view { get; set; }
            public string complete_url { get; set; }
        }

        public class Signatures
        {
            public string id { get; set; }
            public string created_date { get; set; }
            public Validation validations { get; set; }
            public string[] invite_channel { get; set; }
            public string status { get; set; }
            public string url { get; set; }
        }

        public class Cancelacion
        {
            public string Documentoid { get; set; }
            public string Id { get; set; }
            //public string Cancel_reason { get; set; }
            public string ApiKey { get; set; }
        }

        public class CancelacionResponse
        {
            public string id { get; set; }
            public string title { get; set; }
            public string status { get; set; }
            public Signatures[] signatures { get; set; }
            public string cancel_reason { get; set; }
        }

        public class Completar
        {
            public string documentoid { get; set; }
            public string id { get; set; }
            public string nombre { get; set; }
            public string usuarioApi { get; set; }
            public string apiKey { get; set; }
        }

        public class Certificado
        {
            public string Documentoid { get; set; }
            public string Documentofirmaelectronicaid { get; set; }
            public string NombreDocumento { get; set; }
            public string ApiKey { get; set; }
        }

        public class Descargar
        {
            public string Documentoid { get; set; }
            public string Documentofirmaelectronicaid { get; set; }
            public string NombreDocumento { get; set; }
            public string ApiKey { get; set; }
        }

        public class Detalle
        {
            public string documentoid { get; set; }
            public string id { get; set; }
            public string usuarioApi { get; set; }
            public string apiKey { get; set; }
        }

        public class Documentos
        {
            public string usuarioApi { get; set; }
            public string apiKey { get; set; }
        }

        public class DocumentBodyTemplate
        {
            public string documentbody { get; set; }
        }
    }
}
