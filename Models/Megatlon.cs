using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class Megatlon
    {
        #region Back
        public class MantenimientoPreventivo
        {
            [Required]
            public string new_elementosid { get; set; }
            [Required]
            public int new_medicionmantenimiento { get; set; }
            public string new_observaciones { get; set; }
        }
        public class EstadoMantenimientoPreventivo
        {
            [Required]
            public string new_mantenimientopreventivodiarioid { get; set; }
            [Required]
            public int statuscode { get; set; }
        }
        public class Tarea
        {
            [Required]
            public string activityid { get; set; }
            public string subject { get; set; }
            public string new_accion { get; set; } //new_Accion
            public string new_instalacion { get; set; } //new_Instalacion
            public string new_sede { get; set; } //new_Sede
            public string new_subinstalacion { get; set; } //new_SubInstalacion
            public int new_prioridad { get; set; }
            public int new_origen { get; set; }
            public string description { get; set; }
            public string new_resolucion { get; set; }
            public string scheduledstart { get; set; }
            public int new_porcentajerealizado { get; set; }
            public string scheduledend { get; set; }
        }
        public class CerrarTarea
        {
            [Required]
            public string activityid { get; set; }
            public string new_resolucion { get; set; }
        }
        #endregion
        #region Front
        public class Caso
        {
            [Required]
            public string contactid { get; set; }
            public string titulo { get; set; }
            public string descripcion { get; set; }
            public string asunto { get; set; }
            public string asuntoPrimario { get; set; }
            public string solicitante { get; set; }
            public string puestoSolicitante { get; set; }
            public string tipoCaso { get; set; }
            public string comentarios { get; set; }
            public string sucursal { get; set; }
            public string accountid { get; set; }
            public string instalacionPorSede { get; set; }
            public string equipoDetenido { get; set; }
            public string prioridad { get; set; }
            public string derivar { get; set; }
            public string areaEscalar { get; set; }
            public string equipoRelacionado { get; set; }
            public string origenCaso { get; set; }
            public string rubro { get; set; }
            public string subRubro { get; set; }
            public string cuit { get; set; }
            public string tipoCorporeo { get; set; }
            public string tipoLockers { get; set; }
            public string tipoReglamento { get; set; }
            public string tipoImagen { get; set; }
            public string tipoFolleteria { get; set; }
            public string tipoCarteles { get; set; }
            public string nivelServicio { get; set; }
            public string medidasSalon1 { get; set; }
            public string medidasSalon2 { get; set; }
            public string medidaSpinning { get; set; }
            public string medidaSuperficie { get; set; }
            public string cantidad { get; set; }
            public string soporteMantenimiento { get; set; }
            public string tipoDeSolicitud { get; set; }
        }
        public class AsuntoPorArea
        {
            public string new_asuntoxareaid { get; set; }
            public string new_area { get; set; }
            public string new_asunto{ get; set; }
            public string new_sla { get; set; }
            public string new_derivacion { get; set; }
            [JsonProperty("_new_areaaescalar_value")]
            public string new_areaaescalar { get; set; }
            public string new_requiereautorizacion { get; set; }
            public string new_gerentecomercial { get; set; }
            public string new_gerentedeservicios { get; set; }
            public string new_coordinadordeventas { get; set; }
            public string new_coordinadordeservicios { get; set; }
            public string new_coordinadordepileta { get; set; }
            public string new_administrativo { get; set; }
            public string new_gerenteregional { get; set; }
            public string new_director { get; set; }
            public string new_responsablededarrespuesta { get; set; }
            public string new_enviarnotificacionalcliente { get; set; }
            public string new_tipodepropietarioareaaescalar { get; set; }
            public string new_usuarioaderivarelcaso { get; set; }
            public string new_agrupadorasuntos { get; set; }
            public string new_asuntosagrupacin { get; set; }
            public string new_usuariofijoderivableareaaescalar { get; set; }
        }
        public class CasoTkt
        {
            public string ticketnumber { get; set; }
        }
        public class DocumentoLegal
        {
            [Required]
            public string new_cliente { get; set; }
            [Required]
            public string new_fechaderecepcin { get; set; }
            [Required]
            public int new_descripcindeldocumento { get; set; }
            [Required]
            public string new_sede { get; set; }
            [Required]
            public string new_personaquerecepcion { get; set; }
            public string new_observaciones { get; set; }
        }
        public class BusquedaPersonal
        {
            public string new_puesto { get; set; }
            public int new_tipodepuesto { get; set; }
            public string new_sucursal { get; set; }
            public string new_area { get; set; }
            public string new_reportaraa { get; set; }
            public string new_jornadalaboral { get; set; }
            public string new_descripciongeneraldelpuesto { get; set; }
            public string new_reemplazaraa { get; set; }
            public int new_tipodebusqueda { get; set; }
            public string new_autorizadopor { get; set; }
            public string new_personasacargosino { get; set; }
            public string new_contactocreador { get; set; }
            public string new_justificacindelabsqueda { get; set; }
            public int new_expertise { get; set; }
            public string new_edadestimada { get; set; }
            public int new_preferenciadegenero { get; set; }
            public string new_motivodelgenero { get; set; }
            public int new_nivelmnimodeeducacin { get; set; }
            public int new_estadodeniveldeeducacin { get; set; }
            public string new_experiencia { get; set; }
            public string new_competenciascaractersticasdepersonalidad { get; set; }
            public string new_fechadeingreso { get; set; }
            public string new_reemplazode { get; set; }
            public string new_reportaa { get; set; }
        }
        public class AprobarBusquedaPersonal
        {
            [Required]
            public string new_busquedadepersonalid { get; set; }
            [Required]
            public string new_contactoaprobador { get; set; }
            [Required]
            public int new_aprobacion { get; set; }
            [Required]
            public string new_observacionesaprobador { get; set; }
        }
        public class EvaluacionPeriodoPrueba
        {
            public string evaluaciones { get; set; }
            public string evaluacionid { get; set; }
            public string comentarios30 { get; set; }
            public string comentarios60 { get; set; }
            public string comentarios80 { get; set; }
            public string fechaIngreso { get; set; }
            public string esReferido { get; set; }
            public string induccion { get; set; }
            public string periodoPrueba { get; set; }
            public string cuit { get; set; }
        }
        public class ItemEvaluacionPeriodoPrueba
        {
            public string id { get; set; }
            public string valor { get; set; }
            public string valor2 { get; set; }
            public string valor3 { get; set; }
        }
        public class PostulacionCandidato
        {
            [Required]
            public string new_candidatoporbusquedaid { get; set; }
            public string new_observacionessedesareas { get; set; }
            public int statuscode { get; set; }
        }

        public class UsuarioMegatlon
        {
            public string contactid { get; set; }
            public string new_idusuariointranet { get; set; }
            public string emailaddress1 { get; set; }
            
        }
        #endregion
    }
}
