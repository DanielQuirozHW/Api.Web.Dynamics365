using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class PortalSocioParticipe
    {
        public class RelacionDeVinculacion
        {
            public string new_participacionaccionariaid { get; set; }
            public string accountid { get; set; }
            public int new_tipoderelacion { get; set; }
            public decimal new_porcentajedeparticipacion { get; set; }
            public string new_observaciones { get; set; }
            public decimal new_porcentajebeneficiario { get; set; }
            public string new_cargo { get; set; }
            public string new_relacion { get; set; }
            public CuentaRelacionada cuenta { get; set; }
            public ContactoRelacionado contacto { get; set; }
            public string esFirmante { get; set; }
        }

        public class RelacionDeVinculacionEliminar
        {
            public string new_participacionaccionariaid { get; set; }
        }
        public class CuentaRelacionada
        {
            public string accountid { get; set; }
            public string name { get; set; }
            public string new_nmerodedocumento { get; set; }
            public string emailaddress1 { get; set; }
            //public string new_tipodedocumentoid { get; set; }
        }
        public class ContactoRelacionado
        {
            public string contactid { get; set; }
            public string firstname { get; set; }
            public string lastname { get; set; }
            public decimal new_cuitcuil { get; set; }
            public int new_nrodedocumento { get; set; }
            public string emailaddress1 { get; set; }
            public string birthdate { get; set; }
            public string new_lugardenacimiento { get; set; }
            public int familystatuscode { get; set; }
            public string spousesname { get; set; }
            public string new_profesionoficioactividad { get; set; }
            public string new_correoelectrnicopararecibirestadodecuenta { get; set; }
            public string Telephone1 { get; set; }
        }
        public class SociedadDeBolsa
        {
            public string new_sociedaddebolsaporsocioid { get; set; }
            
            public string new_socio { get; set; }
            
            public string new_sociedaddebolsa { get; set; }
            
            public int new_cuentacomitente { get; set; }
        }
        public class MiCuenta
        {
            public string accountid { get; set; }
            public string telephone2 { get; set; }
            public string address1_line1 { get; set; }
            public string new_direccion1numero { get; set; }
            public string address1_name { get; set; }
            public string new_direccion1depto { get; set; }
            public string new_provincia { get; set; }
            public string new_localidad { get; set; }
            public string address1_county { get; set; }
            public string address1_postalcode { get; set; }
            public int new_inscripcionganancias { get; set; }
            public string new_pais { get; set; }
            public string new_firmante { get; set; }
            public int new_estadodeactividad { get; set; }
            public int new_estadodelsocio { get; set; }
            public string new_contactodenotificaciones { get; set; }
            public string new_condiciondeinscripcionanteafip { get; set; }
        }

        public class Operaciones
        {
            public string new_operacionid { get; set; }
            public string new_socioparticipe { get; set; }
            public int new_tipooperacin { get; set; }
            public int new_tipodecheque { get; set; }
            public string new_destinodefondo { get; set; }
            public string new_acreedor { get; set; }
        }

        public class Garantia
        {
            public string new_garantiaid { get; set; }
            public string new_socioparticipe { get; set; }
            public int new_tipodeoperacion { get; set; }
            public int new_formatodelcheque { get; set; }
            public string new_fechadeorigen { get; set; }
            public string new_acreedor { get; set; }
            public int statuscode { get; set; }
            public decimal new_monto { get; set; }
            public int new_tipochpd { get; set; }
            public string new_libradorcheque { get; set; }
            public string new_operacion { get; set; }
            public string new_fechadevencimiento { get; set; }
            public string new_numerodecheque { get; set; }
            public string new_fechadepago { get; set; }
            public Librador librador { get; set; }
            public int new_tasa { get; set; }
            public int new_plazodias { get; set; }
            public int new_periodogracia { get; set; } 
            public int new_sistemadeamortizacion { get; set; }
            public int new_periodicidadpagos { get; set; }
            public string new_observaciones { get; set; }
            public decimal new_puntosporcentuales { get; set; }
            public string transactioncurrencyid { get; set; }
        }

        public class Librador
        {
            public string new_libradorid { get; set; }
            public string new_cuitlibrador { get; set; }
            public string new_name { get; set; }
        }

        public class ActividadTarea
        {
            [Required]
            public string activityid { get; set; }
        }

        public class DocumentacionAdjunta
        {
            public string DocumentacionPorCuentaId { get; set; }
            public string NombreDocumento { get; set; }
            public string NombreSocio { get; set; }
            public List<IFormFile> Archivos { get; set; }
            public string? teamid { get; set; }
        }

        public class UsuariosDeEquipo
        {
            public string systemuserid { get; set; }
            public string fullname { get; set; }
        }
    }
}
