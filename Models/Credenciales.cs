using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class Credenciales
    {
        public int id { get; set; }
        public string conexion { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string url { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string clientid { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string clientsecret { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string tenantid { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string cliente { get; set; }

        public class ActualizarCredenciales
        {
            public string url { get; set; }
            public string clientid { get; set; }
            public string clientsecret { get; set; }
            public string tenantid { get; set; }
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string cliente { get; set; }
        }
    }
}
