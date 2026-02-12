using System.ComponentModel.DataAnnotations;
using static Api.Web.Dynamics365.Models.PortalSocioParticipe;

namespace Api.Web.Dynamics365.Models
{
    public class PortalSocioProtector
    {
        public class Aportes
        {
            [Required]
            public decimal new_Montointegrado { get; set; }
            [Required]
            public string new_Fechadelaporte { get; set; }
            [Required]
            public string new_Cuenta { get; set; }
            [Required]
            public int Statuscode { get; set; }
            public string new_Comentarios {  get; set; }

        }
    }
}
