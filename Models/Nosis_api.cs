using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class Nosis_api
    {
        public resultado Resultado { get; set; }
        public datos Datos { get; set; }
        public infoDni InfoDni { get; set; }
        public Nosis_api()
        {

        }

        public class resultado
        {
            public string Estado { get; set; }
            public string Novedad { get; set; }
            public string Tiempo { get; set; }
            public string FechaRecepcion { get; set; }
            public string Transaccion { get; set; }
            public string Referencia { get; set; }
            public string Servidor { get; set; }
            public string Version { get; set; }

            public resultado()
            {
                Estado = string.Empty;
                Novedad = string.Empty;
                Tiempo = string.Empty;
                FechaRecepcion = string.Empty;
                Transaccion = string.Empty;
                Referencia = string.Empty;
                Servidor = string.Empty;
                Version = string.Empty;
            }

        }

        public class datos
        {
            public variables[] Variables { get; set; }
        }

        public class variables
        {
            public string Nombre { get; set; }
            public string Valor { get; set; }
            public string Descripcion { get; set; }
            public string Tipo { get; set; }
            public string FechaAct { get; set; }

            public variables()
            {
                Nombre = string.Empty;
                Valor = string.Empty;
                Descripcion = string.Empty;
                Tipo = string.Empty;
                FechaAct = string.Empty;
            }
        }

        public class infoDni
        {
            public string Origen { get; set; }
            public string FechaAct { get; set; }
            public string Vigente { get; set; }
            public string Documento { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Sexo { get; set; }
            public string FechaNacimiento { get; set; }
            public string Version { get; set; }
            public string FechaVencimiento { get; set; }
            public string FechaEmision { get; set; }
            public string Fallecido { get; set; }
            public string Nacionalidad { get; set; }
            public string PaisNacimiento { get; set; }
            public string DomCalle { get; set; }
            public string DomNumero { get; set; }
            public string DomPiso { get; set; }
            public string DomDto { get; set; }
            public string DomCodPostal { get; set; }
            public string DomBarrio { get; set; }
            public string DomLocalidad { get; set; }
            public string DomProvincia { get; set; }
            public string DomPais { get; set; }

            public infoDni()
            {

            }
        }

        public class consultaDocumento
        {
            [Required]
            public string usuario { get; set; }
            [Required]
            public string token { get; set; }
            [Required]
            public string grupo { get; set; }
            [Required]
            public string documento { get; set; }
        }

        public class consultaDatosNosis
        {
            [Required]
            public string usuario { get; set; }
            [Required]
            public string token { get; set; }
            [Required]
            public string grupo { get; set; }
            [Required]
            public string documento { get; set; }
            //[Required]
            public string cuenta_id { get; set; }
        }

        public class respuestaDocumento
        {
            public variables CI_Vig_Detalle_PorEntidad { get; set; }
            public variables CI_Vig_PeorSit { get; set; }
            public variables CI_Vig_Total_Monto { get; set; }
        }
    }
}
