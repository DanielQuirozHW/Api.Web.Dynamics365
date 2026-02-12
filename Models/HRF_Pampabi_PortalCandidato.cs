using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class HRF_Pampabi_PortalCandidato
    {
        public class Candidato
        {
            [Required(ErrorMessage = "El campo {0} es requerido")]
            [MaxLength(100)]
            public string nombre { get; set; }
            [Required(ErrorMessage = "El campo {0} es requerido")]
            [MaxLength(100)]
            public string apellido { get; set; }
            [MaxLength(100)]
            public string telefono { get; set; }
            [Required(ErrorMessage = "El campo {0} es requerido")]
            [MaxLength(100)]
            public string correo { get; set; }
            [MaxLength(100)]
            public string linkedin { get; set; }
        }

        public class CandidatoActualizacion
        {
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string id { get; set; }
            [MaxLength(100)]
            public string nombre { get; set; }
            [MaxLength(100)]
            public string apellido { get; set; }
            [MaxLength(100)]
            public string telefono { get; set; }
            [MaxLength(100)]
            public string linkedin { get; set; }
            [MaxLength(100)]
            public string documento { get; set; }
            [MaxLength(100)]
            public string tipoDocumento { get; set; }
            [MaxLength(100)]
            public string fechaNacimiento { get; set; }
            [MaxLength(100)]
            public string pais { get; set; }
        }


        public class Postulacion
        {
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string candidato { get; set; }
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string busqueda { get; set; }
        }
    }
}
