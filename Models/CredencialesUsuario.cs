using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class CredencialesUsuario
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Password { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Cliente { get; set; }

        public class CredencialesLogin
        {
            [Required(ErrorMessage = "El campo {0} es requerido")]
            [EmailAddress]
            public string Email { get; set; }
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string Password { get; set; }
        }

        public class Admin
        {
            [Required(ErrorMessage = "El campo {0} es requerido")]
            [EmailAddress]
            public string Email { get; set; }
        }
    }
}
