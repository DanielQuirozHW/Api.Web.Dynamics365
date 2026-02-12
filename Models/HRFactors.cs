using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class HRFactors
    {
        public class Empleado
        {
            [Required]
            public string new_empleadoid { get; set; }
            public string new_nombredepila { get; set; }
            public string new_apellidos { get; set; }
            public int new_numerolegajo { get; set; }
            public int new_tipodocumento { get; set; }
            public string new_nrodocumento { get; set; }
            public string new_cuitcuil { get; set; }
            public string new_correoelectronico { get; set; }
            public bool new_sexo { get; set; }
            public int new_estadocivil { get; set; }
            public string new_telefonomovil { get; set; }
            public string new_telefonoparticular { get; set; }
            public string new_extenciontelefonica { get; set; }
            public int new_tipodeincorporacion { get; set; }
            public string new_fechanacimiento { get; set; }
            public string new_paisnacimiento { get; set; }
            public int new_edad { get; set; }
            public string new_provincianacimiento { get; set; }
            public string new_calle { get; set; }
            public string new_nro { get; set; }
            public string new_piso { get; set; }
            public string new_depto { get; set; }
            public string new_localidad { get; set; }
            public string new_codigopostal { get; set; }
            public string new_provincia { get; set; }
            public string new_pais { get; set; }
            //Educacion
            public bool new_primariocompleto { get; set; }
            public bool new_secundariocompleto { get; set; }
            public bool new_secundarioincompleto { get; set; }
            public bool new_bachiller { get; set; }
            public bool new_tecnico { get; set; }
            public bool new_peritomercantil { get; set; }
        }
        public class UniversidadPorEmpleado
        {
            //new_universidadporcontactos
            public string new_universidadporcontactoId { get; set; }
            public string new_universidad { get; set; } //new_Universidad
            public string new_empleado { get; set; } //new_Empleado
            public string new_carrera { get; set; } //new_Carrera
            public string new_fechadeingreso { get; set; }
            public string new_fechaegreso { get; set; }
            public int new_tipodecarrera { get; set; }
            public int statuscode { get; set; }
        }
        public class IdiomaPorEmpleado
        {
            //new_idiomaporcontactos
            public string new_idiomaporcontactoid { get; set; }
            public string new_idioma { get; set; } //new_Idioma
            public string new_empleado { get; set; } //new_Empleado
            public bool new_habla { get; set; }
            public bool new_lee { get; set; }
            public bool new_escribe { get; set; }
            public int new_nivel { get; set; }
        }
        public class Trayectoria
        {
            public string new_trayectoriaid { get; set; }
            public string new_empleado { get; set; } //new_Empleado
            public string new_empresa { get; set; } //new_Empresa
            public bool new_trayectoriaenlacompania { get; set; }
            public string new_puesto { get; set; } //new_Puesto
            public string new_fechadesde { get; set; }
            public string new_fechahasta { get; set; }
        }
        public class FamiliarDelEmpleado
        {
            //new_familiardeempleados
            public string new_familiardeempleadoid { get; set; }
            public string new_empleado { get; set; }
            public int new_tipodocumento { get; set; }
            public string new_nrodocumento { get; set; }
            public string new_nombredepila { get; set; }
            public string new_apellidos { get; set; }
            public string new_fechanacimiento { get; set; }
            public int new_ocupacion { get; set; }
            public int new_sexo { get; set; }
            public string new_parentesco { get; set; }
        }
        public class Insumo
        {
            //new_insumoparapersonals
            public string new_insumoparapersonalid { get; set; }
            public string new_empleado { get; set; }
            public int new_modelo { get; set; }
            public int new_tipodeinsumo { get; set; }
            public int new_marca { get; set; }
            public int statuscode { get; set; }
            public string new_observaciones { get; set; }
        }
        public class DatosBancarios
        {
            //new_cuentabancarias
            public string new_cuentabancariaid { get; set; }
            public string new_empleado { get; set; }
            public string new_banco { get; set; }
            public int new_tipodecuenta { get; set; }
            public string new_numerocuenta { get; set; }
            public string new_cbu { get; set; }
            public string transactioncurrencyid { get; set; }
        }
        public class CargaHoraria
        {
            //new_cargahorarias
            public string new_cargahorariaid { get; set; }
            public string new_empleado { get; set; }
            public string new_proyecto { get; set; }
            public string new_asignacion { get; set; }
            public string new_fechadecarga { get; set; }
            public decimal new_horas { get; set; }
            public bool new_facturable { get; set; }
            public string new_descripcion { get; set; }
            public string new_cliente { get; set; } //Cuenta
            public int new_devengadoen { get; set; }
            public string new_caso { get; set; }
        }
        public class Asignacion
        {
            public string new_name { get; set; }
            public string new_solucindelacualpartir { get; set; }
            public string new_asignacionid { get; set; }
            public string new_empleado { get; set; }
            public string new_proyecto { get; set; }
            public string new_rolenelproyecto { get; set; }
            public decimal new_tarifa { get; set; }
            public decimal new_cantidadhoras { get; set; }
            public string new_periodo { get; set; } //Periodo
            public int statuscode { get; set; }
            public int new_tipodeasignacion { get; set; }
            public string new_modificadoporempleado { get; set; }
            public string user { get; set; }
            public int new_niveldecriticidad { get;set; }
            public string new_fechaestimadainicio { get; set; }
            public string new_fechaestimadafin { get; set; }
            public int new_tiemporealporestado { get; set; }
            public int? new_naturalezadelaasignacion { get; set; }
        }
        public class Licencias
        {
            public string new_licenciaid { get; set; }
            public string new_empleado { get; set; } //new_Empleado
            public string new_tipodelicencia { get; set; } //new_TipodeLicencia
            public decimal new_cantidadhoraslicencia { get; set; }
            public string new_fechadesde { get; set; }
            public string new_fechahasta { get; set; }
            public int new_horadesde { get; set; }
            public int new_horahasta { get; set; }
            public string new_comentarios { get; set; }
            public string new_vacaciones { get; set; }
            public string new_fechadesolicitud { get; set; }
        }

        public class Comentarios
        {
            public string feedbackid { get; set; }
            public string title { get; set; }
            public string regardingobjectid { get; set; }
            public string comments { get; set; }
            public string createdbycontact { get; set; }
            public string new_empleado { get; set; }
        }

        public class PropuestaYmejora
        {
            public string new_propuestaymejorasid { get; set; }
            public string new_empleado { get; set; }
            public string new_name { get;set; }
            public string new_propuesta { get; set; }
        }

        //DOCENTE Y CODOCENTE
        public class EvaluacionDocente
        {
            public string new_evaluacionid { get; set; }
            public int new_valoracionfinal { get; set; }
            public string new_cualitativo { get; set; }
            public ItemDeLaEvaluacion[] itemsDeLaEvalucion { get; set; }
        }
        public class ItemDeLaEvaluacion
        {
            public string new_preguntadelaevaluacionid { get; set; }
            public int new_respuesta { get; set; }
            public int new_opcionpolivalencia { get; set; }
        }
        public class AsignacionDocente
        {
            //new_aceptaciondedivisions
            public string new_aceptaciondedivisionid { get; set; }
            public int new_acepta { get; set; }
        }
        public class Gestion
        {
            //new_aceptaciondedivisions
            public string new_name { get; set; }
            public string new_empleado { get; set; }
            public int new_tipodeincidencia { get; set; }
            public string new_tema { get; set; }
            public string new_descripcion { get; set; }
        }
        public class PlanificacionUnificada
        {
            public string new_planificacinunificadaid { get; set; }
            public string new_fechadevencimiento { get; set; }
            public bool new_transferenciadeinvestigacion { get; set; }
            public bool new_desarrollodeinvestigacion { get; set; }
            public bool new_extensionpatpic { get; set; }
            public int new_tipodeextension { get; set; }
            public string new_teamteachinginvitado { get; set; }
            public int new_experiencia { get; set; }
            public bool new_didacticas1ernivel { get; set; }
            public string new_tipodidacticas1ernivel { get; set; } //Segundo nivel
            public string new_institutos { get; set; } //Multiseleccion
            public int new_diferenciales { get; set; }
            public string new_categoriateorica { get; set; }
            public string new_innovaciontecnologica { get; set; } //Multiseleccion
            public string new_dimension { get; set; }
            public string new_bibliografabsica { get; set; }
            public string new_bibliografaampliatoria { get; set; }
            public bool new_didcticas2donivel { get; set; }
            public int new_tipodedidcticas1ernivel { get; set; }
        }
        public class IncidenciasDocentes
        {
            public string new_name { get; set; }
            public string new_empleado { get; set; }
            public int new_tipodeincidencia { get; set; }
            public string new_tema { get; set; }
        }
        public class CambioDomicilio
        {
            //new_aceptaciondedivisions
            public string new_name { get; set; }
            public string new_empleado { get; set; }
            public string new_fechasolicitud { get; set; }
            public string new_fechavigencia { get; set; }
            public string new_calle { get; set; }
            public string new_nro { get; set; }
            public string new_depto { get; set; }
            public string new_piso { get; set; }
            public string new_barrio { get; set; }
            public string new_localidad { get; set; }
            public string new_provincia { get; set; }
            public string new_pais { get; set; }
            public string new_fechadevencimiento { get; set; }
            public int new_estadosolicitud { get; set; }
        }
        public class EstadoCivil
        {
            public string new_name { get; set; }
            public string new_empleado { get; set; }
            public string new_fechasolicitud { get; set; }
            public int new_estadocivil { get; set; }
            public int new_estadosolicitud { get; set; }
            public string new_fechadevencimiento { get; set; }
        }
        public class Familiar
        {
            public string new_name { get; set; }
            public string new_empleado { get; set; }
            public string new_fechasolicitud { get; set; }
            public int new_tipodocumento { get; set; }
            public string new_nrodocumento { get; set; }
            public string new_apellidos { get; set; }
            public string new_nombredepila { get; set; }
            public string new_fechanacimiento { get; set; }
            public int new_genero { get; set; }
            public int new_generoautopercibido { get; set; }
            public int new_edad { get; set; }
            public int new_cargadefamilia { get; set; }
            public string new_parentesco { get; set; }
        }
        public class EducacionFormal
        {
            public string new_name { get; set; }
            public string new_empleado { get; set; }
            public string new_titulo { get; set; }
            public string new_universidad { get; set; }
            public int new_tipodecarrera { get; set; }
            public string new_carrerasubdisciplinasconeau { get; set; }
            public string new_fechaingreso { get; set; }
            public string new_fechaegreso { get; set; }
        }
        public class CuentaBancaria
        {
            public string new_name { get; set; }
            public string new_empleado { get; set; }
            public string new_banco { get; set; }
            public int new_tipodecuenta { get; set; }
            public string new_numerocuenta { get; set; }
            public string new_cbu { get; set; }
            public int new_formadepago { get; set; }
        }
        public class Novedad
        {
            //new_novedads
            public string new_empleado { get; set; }
            public string new_tipodenovedaddepago { get; set; }
            public int new_tipodenovedad { get; set; }
            public decimal new_cantidadhoras { get; set; }
            public decimal new_cantidad { get; set; }
            public decimal new_cantidadfinal { get; set; }
            public int new_porcentaje { get; set; }
            public string transactioncurrencyid { get; set; }
            public decimal new_importe { get; set; }
        }
        public class AceptarRechazarNovedad
        {
            //new_novedads
            public string new_novedadId { get; set; }
            public string new_fechadenovedad { get; set; }
            public string new_tipodenovedaddepago { get; set; }
            public int new_informa { get; set; }
            public string transactioncurrencyid { get; set; }
            public int new_aprobar { get; set; }
            public string new_fechadeaprobacin { get; set; }
            public string new_motivoderechazo { get; set; }
        }
        public class Licencia
        {
            //new_licencias
            public string new_empleado { get; set; }
            public string new_tipodelicencia { get; set; }
            public string new_comentarios { get; set; }
            public string new_fechadesde { get; set; }
            //public decimal new_cantidadhoraslicencia { get; set; }
            public int new_diassolicitados { get; set; }
        }
        public class AceptarRechazarLicencia
        {
            //new_licencias
            public string new_licenciaId { get; set; }
            public string new_tipodelicencia { get; set; }
            public bool new_licenciaprolongada { get; set; }
            public int new_diassolicitados { get; set; }
            public string new_fechadesde { get; set; }
            public string new_fechahasta { get; set; }
            public string new_comentarios { get; set; }
            public string new_aprobador1 { get; set; }
            public int new_aprobacionsupervisor { get; set; }
            public string new_validador1 { get; set; }
            public int new_validacion1 { get; set; }
            public string new_validadorrecursoshumanos { get; set; }
            public int new_aprobacion3 { get; set; }
        }
        public class Vacaciones
        {
            //new_licencias
            public string new_tipodelicencia { get; set; }
            public string new_empleado { get; set; }
            public string new_subperiodovacacionaldefinido { get; set; }
            public string new_fechadesolicitud { get; set; }
            public int new_diassolicitados { get; set; }
            public string new_fechadesde { get; set; }
        }
        public class CancelarVacaciones
        {
            //new_licencias
            public string new_licenciaId { get; set; }
            public int new_diassolicitados { get; set; }
            public string new_name{ get; set; }
        }
        public class AceptarRechazarVacaciones
        {
            //new_licencias
            public string new_licenciaId { get; set; }
            public string new_empleado { get; set; }
            public string new_tipodelicencia { get; set; }
            public string new_fechadesolicitud { get; set; }
            public string new_subperiodovacacionaldefinido { get; set; }
            public int new_diassolicitados { get; set; }
            public string new_fechahasta { get; set; }
            public string new_aprobador1 { get; set; }
            public int new_aprobacionsupervisor { get; set; }
            public string new_aprobador2 { get; set; }
            public int new_aprobacion2 { get; set; }
            public string new_aprobador3 { get; set; }
            public int new_aprobacion3 { get; set; }
        }
        public class Objetivo
        {
            //new_licencias
            public string new_objetivodeevaluacionid { get; set; }
            public string new_name { get; set; }
            public string new_empleado { get; set; }
            public string new_evaluaciondepgd { get; set; }
            public string new_objetivo { get; set; }
            public string new_resultadoclave { get; set; }
            public int cr20a_deavance { get; set; }
            public int new_ponderacionlider { get; set; }
            public int cr20a_status { get; set; }
            //public int statuscode { get; set; }
        }
        public class SubperiodoVacacional
        {
            //new_subperiodovacacionalporas
            public string new_subperiodovacacionalporaid { get; set; }
            public string new_unidadorganizativa { get; set; }
            public string new_subperiodovacacionalasociado { get; set; }
            public int new_tamaonomina { get; set; }
            public decimal new_porcentajeacumplirobjetivo { get; set; }
            public decimal new_porcentajelicenciasenelsubperiodo { get; set; }
            public string new_aprobador { get; set; }
            public int new_cantidadlicencias { get; set; }
            public int new_aprobacion { get; set; }
        }
        public class EvaluacionPGD
        {
            //new_evaluaciondepgds
            public string new_evaluaciondepgdid { get; set; }
            public int new_estadodelaautoevaluacin { get; set; }
            public int new_estadodelaevaluacindellder { get; set; }
        }
        public class AreaAcademica
        {
            public string new_empleadoid { get; set; }
            public string new_informacionadicional { get; set; }
            public string new_subdisciplina { get; set; }
            public int new_cantidadtotaldetesis { get; set; }
            public int new_canttesisdoctoralesdirigactualmente { get; set; }
            public int new_canttesismaestriasdirigyconcult { get; set; }
            public int new_cantmaestriasquedirigeactualmente { get; set; }
            public int new_canttesinasytrabfinales { get; set; }
            public int new_canttesinasytrabajosquedirigeactualmente { get; set; }
            public string new_experienciaeneducacinadistancia { get; set; }
            public bool new_conicet { get; set; }
            public bool new_programade { get; set; }
            public int new_nivel { get; set; }
        }
        public class ProyectoDeInvestigacion
        {
            public string new_name { get; set; }
            public string new_plandetrabajo { get; set; }
            public string new_proyectosdeinvestigacinid { get; set; }
            public string new_empleado { get; set; }
            public string new_fechadeinicio { get; set; }
            public string new_fechadefinalizacin { get; set; }
            public string new_institucion { get; set; }
            public string new_institucionevaluadora { get; set; }
            public string new_institucionfinanciadora { get; set; }
            public bool new_experienciaenpi { get; set; }
            public int new_carcterdelaparticipcin { get; set; }
            public string new_division { get; set; }
            public string new_metaria { get; set; }
            public string new_matriz { get; set; }
            public string new_principalesresultados { get; set; }
        }
        public class PublicacionEnRevista
        {
            public string new_publicacionenrevistaid { get; set; }
            public string new_plandetrabajodocente { get; set; }
            public bool new_conarbitraje { get; set; }
            public string new_empleado { get; set; }
            public string new_name { get; set; }
            public string new_autores { get; set; }
            public string new_revista { get; set; }
            public string new_formacindocenteasociadasicorresponde { get; set; }
            public string new_fechadepublicacin { get; set; }
            public string new_volumen { get; set; }
            public int new_paginas { get; set; }
            public string new_sitiowebconinformacin { get; set; }
            public string new_palabrasclave { get; set; }
            public int statuscode { get; set; }
        }
        public class PresentacionACongresos
        {
            public string new_autores { get; set; }
            public string new_name { get; set; }
            public string new_evento { get; set; }
            public string new_lugarderealizacin { get; set; }
            public string new_fechaderealizacin { get; set; }
            public string new_sitiowebconinformacin { get; set; }
            public string new_palabrasclave  { get; set; }
            public string new_empleado { get; set; }
        }
        public class TituloDePropiedad
        {
            public string new_name { get; set; }
            public string new_titular { get; set; }
            public string new_fechasolicitud { get; set; }
            public string new_fechaotorgamiento { get; set; }
            public string new_empleado { get; set; }
        }
        public class DesarrolloNoPasible
        {
            public string new_name { get; set; }
            public string new_descripcion { get; set; }
            public string new_empleado { get; set; }
        }
        public class ReunionCientifico
        {
            public string new_name { get; set; }
            public string new_evento { get; set; }
            public string new_lugar { get; set; }
            public int new_formadeparticipacin { get; set; }
            public string new_fecha { get; set; }
            public string new_empleado { get; set; }
        }
        public class ParticipacionEnComites
        {
            public string new_institucinconvocante { get; set; }
            public int new_tipodeevaluacin { get; set; }
            public string new_lugar { get; set; }
            public string new_fecha { get; set; }
            public string new_empleado { get; set; }
            public string new_name { get; set; }
        }

        public class TrayectoriaDocente
        {
            //new_trayectoriadocentes
            public string new_trayectoriadocenteid { get; set; }
            public string new_plandetrabajodocente { get; set; }
            public string new_empleado { get; set; }
            public int new_tipodetrayectoria { get; set; }
            public string new_institucinacadmica { get; set; } //LK
            public int new_funcion { get; set; }
            public int new_designacin { get; set; }
            public string new_fechadeinicio { get; set; }
            public int new_duracindelcursado { get; set; }
            public string new_disciplina { get; set; } //LK
            public string new_subdisciplina { get; set; } //LK
            public int new_cargo { get; set; }
            public int new_unidadacadmica { get; set; }
            public string new_otrocargofuncin { get; set; }
            public string new_fechadefinalizacin { get; set; }
            public int new_dedicacinsemanal { get; set; }
            public decimal new_canthoras { get; set; }
            public int statuscode { get; set; }
        }

        public class ContactoHWA
        {
            [Required]
            public string firstname { get; set; }
            [Required]
            public string lastname { get; set; }
            public string jobtitle { get; set; }
            [Required]
            public string emailaddress1 { get; set; }
            public string telephone1 { get; set; }
            public string empresa { get; set; }
            public string productoDeInteres { get; set; }
            public string contactoDesde { get; set; }
            public string description { get; set; }
        }

        public class ClientePotencialHWA
        {
            [Required]
            public string firstname { get; set; }
            [Required]
            public string lastname { get; set; }
            public string jobtitle { get; set; }
            [Required]
            public string emailaddress1 { get; set; }
            public string mobilephone { get; set; }
            public string companyname { get; set; }
            public string new_referidodesde { get; set; }
            public string productoDeInteres { get; set; }
            public string contactoDesde { get; set; }
            public string description { get; set; }
        }
        public class ContactoHRCasos
        {
            public string contactid { get; set; }
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string new_cuitcuil { get; set; }
            public string mobilephone { get; set; }
            public string new_fechaultimavalidacionidentidadrenaper { get; set; }
            public string new_resultadoultimavalidacionidentidadrenaper { get; set; }
            public string new_nombrepersonavalidada { get; set; }
            public string new_dnipersonavalidada { get; set; }
            public string jobtitle { get; set; }
            public string emailaddress2 { get; set; }
        }

        public class ParticipantePorEventoHRF
        {
            //new_participanteporeventodecapacitacion
            public string new_participanteporeventodecapacitacionid { get; set; }
            public int statuscode { get; set; }
            public int new_aplica { get; set; }
        }

        //AUSTRAL
        public class PlanificacionActivadadesDocente
        {
            [Required]
            public string new_plandetrabajodocente { get; set; }
            public string new_name { get; set; }
            public string new_planificacindeactividadesdeldocenteid { get; set; }
            public string new_actividadesdocentesplanificada { get; set; }
            public string new_materia { get; set; }
            public string new_carrera { get; set; }
            public decimal new_planificacindepreparacionhorasreloj { get; set; }
            public decimal new_planificacindedictadohorasreloj { get; set; }
            public decimal new_planificacindeevaluacinhorasreloj { get; set; }
            public int new_tipo { get; set; }
        }

        public class PostulacionNombramientoDocente
        {
            public string new_postulacinaconcursodocenteid { get; set; }
            public string new_concursodocente { get; set; }
            public string new_docente { get; set; }
        }

        public class ActividadNota
        {
            //new_asignacions
            public string titulo { get; set; }
            public string descripcion { get; set; }
            public string asignacion_id { get; set; }
        }
        public class Comentario
        {
            //new_asignacions
            public string new_comentarioporasignacionid { get; set; }
            public int new_tipo { get; set; }
            public string new_detalle { get; set; }
            public string new_asignacion { get; set; }
            public string new_modificadoporempleado { get; set; }
            public string new_empleadode { get; set; }
            public string new_empleadopara { get; set; }
            public string new_para { get; set; }
        }
        public class HorasTrabajadas
        {
            //new_horastrabajadases
            public string new_horastrabajadasid { get; set; }
            public decimal new_cantidaddehoras { get; set; }
            public string new_tipodehoras { get; set; }
            public string new_periododeliquidacion { get; set; }
            public string new_fechadecarga { get; set; }
            public string new_empleado { get; set; }
            public string new_obra { get; set; }
        }

        public class AsignacionIntegrantes
        {
            public string new_name { get; set; }
            public string new_solucindelacualpartir { get; set; }
            public string new_asignacionid { get; set; }
            public string new_empleado { get; set; }
            public string new_proyecto { get; set; }
            public string new_rolenelproyecto { get; set; }
            public decimal new_tarifa { get; set; }
            public decimal new_cantidadhoras { get; set; }
            public string new_periodo { get; set; } //Periodo
            public int statuscode { get; set; }
            public int new_tipodeasignacion { get; set; }
            public string new_modificadoporempleado { get; set; }
            public int new_niveldecriticidad { get; set; }
            public string new_fechaestimadainicio { get; set; }
            public string new_fechaestimadafin { get; set; }
            public int new_tiemporealporestado { get; set; }
            public int? new_naturalezadelaasignacion { get; set; }
            public Integrantes[] integrantes { get; set; }
        }

        public class Integrantes
        {
            public string new_integranteporasignacionid { get; set; }
            public string new_integrante { get; set; }
            public string new_rolenasignacion { get; set; }
            public string new_asignacion { get; set; }
            public string new_modificadoporempleado { get; set; }
        }
    }
}
