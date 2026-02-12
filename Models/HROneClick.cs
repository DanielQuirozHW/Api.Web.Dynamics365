namespace Api.Web.Dynamics365.Models
{
    public class HROneClick
    {
        public class EmpleadoHR
        {
            //General
            public string new_empleadoid { get; set; }
            public string new_empresa { get; set; }
            public string new_nombredepila { get; set; }
            public string new_telefonomovil { get; set; }
            public string new_apellidos { get; set; }
            public string new_telefonoparticular { get; set; }
            public int new_numerolegajo { get; set; }
            public string new_extenciontelefonica { get; set; }
            public int new_tipodocumento { get; set; }
            public string new_correoelectronico { get; set; }
            public string new_nrodocumento { get; set; }
            public int new_tipodeincorporacion { get; set; }
            public string new_cuitcuil { get; set; }
            public bool new_sexo { get; set; }
            public int new_estadocivil { get; set; }
            //Cargo Actual
            public string new_puesto { get; set; }
            public string new_reportaaid { get; set; }
            public string new_unidadfuncional { get; set; }
            public string new_fechainiciocargo { get; set; }
            public string new_categoria { get; set; }
            //Antiguedad
            public string new_fechaingreso { get; set; }
            public string new_fechavacaciones { get; set; }
            public string new_fechadejubilacion { get; set; }
            public string new_fechadebaja { get; set; }
            public int new_motivodebaja { get; set; }
            public decimal new_salariobrutovigente { get; set; }
            public string new_convenio { get; set; }
            //Horario Laboral
            public bool new_turnorotativo { get; set; }
            public decimal new_horadesde { get; set; }
            public decimal new_horahasta { get; set; }
            public string new_fechavigenciadesde { get; set; }
            public string new_fechavigenciahasta { get; set; }
            //Datos de Nacimiento
            public string new_paisnacimiento { get; set; }
            public string new_fechanacimiento { get; set; }
            public string new_provincianacimiento { get; set; }
            public int new_edad { get; set; }
            public string new_localidadnacimiento { get; set; }
            //Ultimo Domicilio
            public string new_calle { get; set; }
            public string new_nro { get; set; }
            public string new_piso { get; set; }
            public string new_depto { get; set; }
            public string new_localidad { get; set; }
            public string new_codigopostal { get; set; }
            public string new_sufijo { get; set; }
            public string new_provincia { get; set; }
            public string new_pais { get; set; }
            public bool new_primariocompleto { get; set; }
            public bool new_secundariocompleto { get; set; }
            public bool new_secundarioincompleto { get; set; }
            public bool new_bachiller { get; set; }
            public bool new_tecnico { get; set; }
            public bool new_peritomercantil { get; set; }
        }
        public class EvaluacionHR
        {
            public string new_evaluacionid { get; set; }
            public string new_fechadevencimiento { get; set; }
            public int new_tipodeevaluacion { get; set; }
            public string new_evaluador { get; set; }
            public string new_evaluado { get; set; }
            public int statuscode { get; set; }
            public string new_periodo { get; set; }
            public int new_analisisglobaldecompetencias1 { get; set; }
            public int new_analisisglobaldecompetencias { get; set; }
            public int new_puntajeanalisisglobal { get; set; }
            public string new_elecciondeevaluadores { get; set; }
        }
        public class RequerimientoDeCapacitacionHR
        {
            public string new_requerimientodecapacitacionid { get; set; }
            public string new_name { get; set; }
            public string new_curso { get; set; }
            public string new_fecharequerimiento { get; set; }
            public string new_fechapretendida { get; set; }
            public string new_solicitadopor { get; set; }
            //public string ownerid { get; set; }
            public int new_duracionendias { get; set; }
            public decimal new_horasenclase { get; set; }
            public int statuscode { get; set; }
            public string new_motivodelaprioridad { get; set; }
            public string new_observaciones { get; set; }
            //public string new_eventoid { get; set; }
            public int new_prioridad { get; set; }
            public string new_aprobador1 { get; set; }
            public string new_aprobador2 { get; set; }
            public int new_aprueba1 { get; set; }
            public int new_aprueba2 { get; set; }
        }

        public class PlanDeCapacitacionHR
        {
            public string new_plandecapacitacionid { get; set; }
            public string new_name { get; set; }
            public string new_periodo { get; set; }
            public decimal new_presupuestoasignado { get; set; }
        }

        public class Curso
        {
            public string new_cursoid { get; set; }
            public string new_name { get; set; }
            public bool new_requiereeficacia { get; set; }
            public int new_accion { get; set; }
            public decimal new_duracion { get; set; }
            public int statuscode { get; set; }
            public bool new_elearning { get; set; }
            public bool new_interna { get; set; }
            public bool new_incompany { get; set; }
            public bool new_externa { get; set; }
            public string new_url { get; set; }
            public string new_objetivo { get; set; }
            public string new_contenido { get; set; }
        }

        public class Encuesta
        {
            public string new_encuestaid { get; set; }
            public string new_fecha { get; set; }
            public string new_fechavencimiento { get; set; }
            public string new_encuestado { get; set; }
            public int statuscode { get; set; }
            public string new_name { get; set; }
            public string new_template { get; set; }
            public string new_introduccion { get; set; }
            public string new_comentarios { get; set; }
            //public decimal new_puntajeidealencuesta { get; set; }
            //public decimal new_satisfacciondelaencuesta { get; set; }
        }

        public class EvaluacionDocenteHR
        {
            public string new_evaluacionid { get; set; }
            public int new_valoracionfinal { get; set; }
            public string new_cualitativo { get; set; }
        }

        public class EvaluacionPGDHROC
        {
            //new_evaluaciondepgds
            public string new_evaluaciondepgdid { get; set; }
            public int new_estadodelaautoevaluacin { get; set; }
            public int new_estadodelaevaluacindellder { get; set; }
            public string new_ciclo { get; set; }
            public string new_lder { get; set; }
            public string new_evaluado { get; set; }
            public int statuscode { get; set; }
            //Definicion de objetivos
            public string new_fechainicioautoevaluacion { get; set; }
            public string new_fechavencimientoautoevaluacin { get; set; }
            public bool new_autoevaluacion { get; set; }
            public string new_comentariosyobservaciones { get; set; }
            //Resultados y Observaciones
            public string new_comentariosyobservacionesdeautoevaluacion { get; set; }
            //Mi Proposito
            public string new_miproposito { get; set; }
            public bool new_elcolaboradorhacambiadosupropsito { get; set; }
            public string new_nuevoproposito { get; set; }
            public string new_comentariosyobervacionesdesuproposito { get; set; }
            ////Mi Aspiracional de Carrera
            public int new_interesendesarrolloprox6meses { get; set; }
            public int new_interesendesarrolloprximos12meses { get; set; }
            public string new_puestoaspiracionalprximos6meses { get; set; } //Puesto
            public string new_posicinaspiracionalprximos6meses { get; set; } //Posicion
            public string new_unidadorganizativaaspiracionalprximos6mes { get; set; } //Unidad organigrama 
            public string new_puestoaspiracionalprximos12meses { get; set; } //Puesto
            public string new_posicinaspiracionalprximos12meses { get; set; } //Posicion
            public string new_unidadorganizativaaspiracionalprximos12me { get; set; } //Unidad organigrama 
            public string new_comentariosyobservacionesdemiproposito { get; set; }
            ////Gestion y seguimiento
            public string new_fechainicioevaluacindellider { get; set; }
            public string new_fechavencimientoevaluacindellider { get; set; }
            //Resultados y Observaciones
            public string new_comentariosyobservacionesdelaevaluacion { get; set; }
            //Aspiracional de Carrera
            public string new_comentariosyobservacionesaspeval { get; set; }
            //Evaluacion y Feedback
            public string new_fechainiciofeedback { get; set; }  
            public string new_fechavencimientofeedback { get; set; }
            public string new_fechayhoradelencuentrodefeedback { get; set; }
            public int new_estadodelencuentrodefeedback { get; set; }
            //Feedback
            public int new_scoreglobal { get; set; }
            public string new_comentariosyobservacionesdelfeedback { get; set; }
            public string new_comentariosyobservacionesdelfeedbacklider { get; set; }
        }

        public class GestiónDeObjetivosHROC
        {
            //new_objetivodeevaluacion
            public string new_name { get; set; }
            public string new_objetivodeevaluacionid { get; set; }
            public string new_evaluaciondepgd { get; set; }
            public string new_objetivo { get; set; } //Maestro de objetivos
            public int new_tipodeobjetivo { get; set; }
            public string new_perspectivadenegocio { get; set; } //Perspectiva del negocio
            public string new_plazo { get; set; }
            public int new_ponderacionlider { get; set; }
            public string new_fuentedemedicion { get; set; }
            public string new_piso { get; set; } 
            public string new_target { get; set; }
            public string new_techo { get; set; }
        }

        public class ItemDePGDHROC
        {
            //new_itemdeevaluaciondedesempeo
            public string new_itemdeevaluaciondedesempeoid { get; set; }
            public string new_evaluaciondepgd { get; set; }
            public int new_tipodeitemdeevaluacion { get; set; }
            public string new_competencia { get; set; } //Competencia
            public int new_valoracin { get; set; }
            public int new_valoraciondellider { get; set; }
            public int new_tipodeinstancia { get; set; }
            public string new_plandesucesin { get; set; }
        }

        public class MetaPrioritariaHROC
        {
            //new_metaprioritaria
            public string new_metaprioritariaid { get; set; }
            public string new_evaluacionpgd { get; set; }
            public string new_name { get; set; }
            public string new_accion { get; set; }
            public string new_evidencia { get; set; }
            public string new_fechadesde { get; set; } 
            public string new_fechahasta { get; set; }
        }

        public class ParticipantePorEventoHROC
        {
            //new_participanteporeventodecapacitacion
            public string new_participanteporeventodecapacitacionid { get; set; }
            public int statuscode { get; set; }
        }

        public class RequerimientoDePersonalHROC
        {
            //new_solicituddecandidatos
            public string new_solicituddecandidatoid { get; set; }
            public string new_empleadosolicitante { get; set; }
            public string new_cliente { get; set; }
            public int new_prioridad { get; set; }
            public string new_puesto { get; set; }
            public string new_perfil { get; set; }
            public string new_proyectos { get; set; }
            public int new_vacante { get; set; }
            public int new_cantidaddehorasmensuales { get; set; }
            public int new_modalidaddecontratacin { get; set; }
            public int new_duracindelacontratacin { get; set; }
            public int new_jornadadetrabajo { get; set; }
            public string new_fechaidealdeinicio { get; set; }
            public int statuscode { get; set; }
            public string new_descripcionproyecto { get; set; }
            public string new_requerimientodelperfilacontratar { get; set; }
            public string new_condicinespecialesdesegurodeaccidente { get; set; }
            public string new_beneficioadicional { get; set; }
            public string new_comentariosgenerales { get; set; }
            public string new_solicituddepuestonuevo { get; set; }
            public string new_empleadoaprobador1 { get; set; }
            public int new_aprobador1 { get; set; }
        }
    }
}
