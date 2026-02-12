using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class ChatIA
    {
        [Required]
        public string Consulta { get; set; }
        [Required]
        public string Respuesta { get; set; }
        [Required]
        public string Contactid { get; set; }
        [Required]
        public string Accountid { get; set; }
        public int? Segmento { get; set; }
        public string PaginaRespuesta { get; set; }
    }
}
