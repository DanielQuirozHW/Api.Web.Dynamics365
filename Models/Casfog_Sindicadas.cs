using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.Web.Dynamics365.Models
{
    public class Casfog_Sindicadas
    {
        public class OperacionSindicada
        {
            public string id { get; set; }
            public string cliente { get; set; }
            public string garantia_id { get; set; }
            public string sgr { get; set; }
            public decimal porcentaje { get; set; }
            public decimal montoTotalDeLaGarantia{ get; set; }
            public decimal importeAvaladoSgr { get; set; }
        }

        public class OperacionSindicadaLider
        {
            public string id { get; set; }
            public string cliente { get; set; }
            public string garantia_id { get; set; }
            public string sgr { get; set; }
            public string cuitSgr { get; set; }
            public string entornoExcepciones { get; set; }
            //public decimal porcentaje { get; set; }
            //public decimal montoTotalDeLaGarantia { get; set; }
            //public decimal importeAvaladoSgr { get; set; }

        }

        public class MensajeAzureMasivo
        {
            public string garantia_id { get; set; }
        }

        public class MensajeAzure
        {
            public string operacion_id { get; set; }
            public string garantia_id { get; set; }
        }

        public class UnidadDeNegocioSindicada
        {
            public string new_cuitdesgr { get; set; }
        }

        public class OPYSGR
        {
            public string new_operacionsindicadaid { get; set; }
            public decimal new_importeenpesos { get; set; }
            [JsonProperty("sgr.new_credencialapi")]
            public string new_credencialapi { get; set; }
        }

        public class MensajeColaWebJOBSindicadas
        {
            public Credenciales credenciales { get; set; }
            public Credenciales credencialesCliente { get; set; }
            public string cuitSGR { get; set; }
            public string garantia_id { get; set; }
            public string operacion_id { get; set; }
            public decimal importeAvalado { get; set; }
        }

        public class Contacto
        {
            [JsonProperty("contacto.contactid")]
            public string contactid { get; set; }
            [JsonProperty("contacto.firstname")]
            public string firstname { get; set; }
            [JsonProperty("contacto.lastname")]
            public string lastname { get; set; }
            [JsonProperty("contacto.new_cuitcuil")]
            public decimal new_cuitcuil { get; set; }
            [JsonProperty("contacto.emailaddress1")]
            public string emailaddress1 { get; set; }
            [JsonProperty("contacto.address1_country")]
            public string address1_country { get; set; }
            [JsonProperty("contacto.new_nrodedocumento")]
            public string new_nrodedocumento { get; set; }
            [JsonProperty("contacto.statecode")]
            public string statecode { get; set; }
            [JsonProperty("contacto._parentcustomerid_value")]
            public string _parentcustomerid_value { get; set; }
            [JsonProperty("contacto.new_fechaultimavalidacionidentidadrenaper")]
            public string new_fechaultimavalidacionidentidadrenaper { get; set; }
            [JsonProperty("contacto.new_resultadoultimavalidacionidentidadrenaper")]
            public string new_resultadoultimavalidacionidentidadrenaper { get; set; }
        }
        public class Cuenta
        {
            [JsonProperty("cuenta.accountid")]
            public string accountid { get; set; }
            [JsonProperty("cuenta.name")]
            public string name { get; set; }
            [JsonProperty("cuenta.new_nmerodedocumento")]
            public string new_nmerodedocumento { get; set; }
            [JsonProperty("cuenta.emailaddress1")]
            public string emailaddress1 { get; set; }
            [JsonProperty("cuenta.new_personeria")]
            public int new_personeria { get; set; }
            [JsonProperty("cuenta.new_rol")]
            public int new_rol { get; set; }
            [JsonProperty("cuenta.new_tipodedocumentoid")]
            public string new_tipodedocumentoid { get; set; }
            [JsonProperty("cuenta.new_productoservicio")]
            public string new_productoservicio { get; set; }
            [JsonProperty("cuenta.new_tiposocietario")]
            public int new_tiposocietario { get; set; }
            [JsonProperty("cuenta.new_condicionimpositiva")]
            public int new_condicionimpositiva { get; set; }
            [JsonProperty("cuenta.statuscode")]
            public int statuscode { get; set; }
            [JsonProperty("cuenta.new_estadodelsocio")]
            public int new_estadodelsocio { get; set; }
            [JsonProperty("cuenta.new_inscripcionganancias")]
            public int new_inscripcionganancias { get; set; }
            [JsonProperty("cuenta.new_actividadafip")]
            public string new_actividadafip { get; set; }
            [JsonProperty("cuenta.new_facturacionultimoanio")]
            public decimal new_facturacionultimoanio { get; set; }
            [JsonProperty("cuenta.new_fechadealta")]
            public string new_fechadealta { get; set; }
            [JsonProperty("cuenta.new_onboarding")]
            public string new_onboarding { get; set; }
            [JsonProperty("cuenta.new_essoloalyc")]
            public string new_essoloalyc { get; set; }
            public ContactoFirmante firmante { get; set; }
            public Contacto contactoNotificaciones { get; set; }
            public List<Relacion> relaciones { get; set; }
            public List<Documentacion> documentos { get; set; }
            public List<CertificadoPyme> certificados { get; set; }
            public TipoDocumentoVinculado tipoDocumento { get; set; }
            public ActividadAFIPVinculado actividadAFIP { get; set; }
            public CondicionPymeVinculado condicionPyme { get; set; }
            public CategoriaVinculado categoria { get; set; }
            public ProvinciaVinculada provincia { get; set; }
            public PaisVinculado pais { get; set; }
            [JsonProperty("cuenta.telephone2")]
            public string telephone2 { get; set; }
            [JsonProperty("cuenta.address1_postalcode")]
            public string address1_postalcode { get; set; } //cp
            [JsonProperty("cuenta.address1_line1")]
            public string address1_line1 { get; set; }
            [JsonProperty("cuenta.new_localidad")]
            public string new_localidad { get; set; }
            [JsonProperty("cuenta.new_direccion1numero")]
            public string new_direccion1numero { get; set; }
            [JsonProperty("cuenta.address1_county")]
            public string address1_county { get; set; }
            [JsonProperty("cuenta.new_direccion1depto")]
            public string new_direccion1depto { get; set; }
            [JsonProperty("cuenta.address1_name")]
            public string address1_name { get; set; }

            [JsonProperty("cuenta.new_nuevapyme")]
            public bool new_nuevapyme { get; set; }
            [JsonProperty("cuenta.new_calificacion")]
            public int new_calificacion { get; set; }
            [JsonProperty("cuenta.new_estadodeactividad")]
            public int new_estadodeactividad { get; set; }
        }

        public class CuentaV2
        {
            [JsonProperty("cuenta.accountid")]
            public string accountid { get; set; }
            [JsonProperty("cuenta.name")]
            public string name { get; set; }
            [JsonProperty("cuenta.new_nmerodedocumento")]
            public string new_nmerodedocumento { get; set; }
            [JsonProperty("cuenta.emailaddress1")]
            public string emailaddress1 { get; set; }
            [JsonProperty("cuenta.new_personeria")]
            public int new_personeria { get; set; }
            [JsonProperty("cuenta.new_rol")]
            public int new_rol { get; set; }
            [JsonProperty("cuenta.new_tipodedocumentoid")]
            public string new_tipodedocumentoid { get; set; }
            [JsonProperty("cuenta.new_productoservicio")]
            public string new_productoservicio { get; set; }
            [JsonProperty("cuenta.new_tiposocietario")]
            public int new_tiposocietario { get; set; }
            [JsonProperty("cuenta.new_condicionimpositiva")]
            public int new_condicionimpositiva { get; set; }
            [JsonProperty("cuenta.statuscode")]
            public int statuscode { get; set; }
            [JsonProperty("cuenta.new_estadodelsocio")]
            public int new_estadodelsocio { get; set; }
            [JsonProperty("cuenta.new_inscripcionganancias")]
            public int new_inscripcionganancias { get; set; }
            [JsonProperty("cuenta.new_actividadafip")]
            public string new_actividadafip { get; set; }
            [JsonProperty("cuenta.new_facturacionultimoanio")]
            public decimal new_facturacionultimoanio { get; set; }
            [JsonProperty("cuenta.new_fechadealta")]
            public string new_fechadealta { get; set; }
            [JsonProperty("cuenta.new_onboarding")]
            public string new_onboarding { get; set; }
            [JsonProperty("cuenta.new_essoloalyc")]
            public string new_essoloalyc { get; set; }
            public ContactoFirmante firmante { get; set; }
            public Contacto contactoNotificaciones { get; set; }
            public List<Relacion> relaciones { get; set; }
            public List<Documentacion> documentos { get; set; }
            public List<CertificadoPymeV2> certificados { get; set; }
            public TipoDocumentoVinculado tipoDocumento { get; set; }
            public ActividadAFIPVinculado actividadAFIP { get; set; }
            public CondicionPymeVinculado condicionPyme { get; set; }
            public CategoriaVinculado categoria { get; set; }
            public ProvinciaVinculada provincia { get; set; }
            public PaisVinculado pais { get; set; }
            [JsonProperty("cuenta.telephone2")]
            public string telephone2 { get; set; }
            [JsonProperty("cuenta.address1_postalcode")]
            public string address1_postalcode { get; set; } //cp
            [JsonProperty("cuenta.address1_line1")]
            public string address1_line1 { get; set; }
            [JsonProperty("cuenta.new_localidad")]
            public string new_localidad { get; set; }
            [JsonProperty("cuenta.new_direccion1numero")]
            public string new_direccion1numero { get; set; }
            [JsonProperty("cuenta.address1_county")]
            public string address1_county { get; set; }
            [JsonProperty("cuenta.new_direccion1depto")]
            public string new_direccion1depto { get; set; }
            [JsonProperty("cuenta.address1_name")]
            public string address1_name { get; set; }

            [JsonProperty("cuenta.new_nuevapyme")]
            public bool new_nuevapyme { get; set; }
            [JsonProperty("cuenta.new_calificacion")]
            public int new_calificacion { get; set; }
            [JsonProperty("cuenta.new_estadodeactividad")]
            public int new_estadodeactividad { get; set; }
        }
        public class CuentaVinculada
        {
            [JsonProperty("cuenta.accountid")]
            public string accountid { get; set; }
            [JsonProperty("cuenta.name")]
            public string name { get; set; }
            [JsonProperty("cuenta.new_nmerodedocumento")]
            public string new_nmerodedocumento { get; set; }
            [JsonProperty("cuenta.emailaddress1")]
            public string emailaddress1 { get; set; }
            [JsonProperty("cuenta.new_personeria")]
            public string new_personeria { get; set; }
            [JsonProperty("cuenta.new_rol")]
            public string new_rol { get; set; }
            [JsonProperty("cuenta.new_tipodedocumentoid")]
            public string new_tipodedocumentoid { get; set; }
            [JsonProperty("cuenta.telephone2")]
            public string telephone2 { get; set; }
            [JsonProperty("cuenta.address1_postalcode")]
            public string address1_postalcode { get; set; } //cp
            [JsonProperty("cuenta.address1_line1")]
            public string address1_line1 { get; set; }
            [JsonProperty("cuenta.new_localidad")]
            public string new_localidad { get; set; }
            [JsonProperty("cuenta.new_direccion1numero")]
            public string new_direccion1numero { get; set; }
            [JsonProperty("cuenta.address1_county")]
            public string address1_county { get; set; }
            [JsonProperty("cuenta.new_direccion1depto")]//Municipio
            public string new_direccion1depto { get; set; }
            [JsonProperty("cuenta._new_provincia_value")]//depto
            public string _new_provincia_value { get; set; }
            [JsonProperty("cuenta.address1_name")]
            public string address1_name { get; set; }
            [JsonProperty("cuenta._new_pais_value")]
            public string _new_pais_value { get; set; }
            [JsonProperty("cuenta.address1_new_rolname")]
            public string address1_new_rolname { get; set; }
            [JsonProperty("cuenta.address1_telephone1")]
            public string address1_telephone1 { get; set; }
            [JsonProperty("cuenta.new_piso")]
            public string new_piso { get; set; }
            [JsonProperty("cuenta.new_estadodelsociocalyc")]
            public string new_estadodelsociocalyc { get; set; }
            [JsonProperty("cuenta.new_estadodelsocio")]
            public string new_estadodelsocio { get; set; }
            [JsonProperty("cuenta.new_actividadesperadadelacuenta")]
            public string new_actividadesperadadelacuenta { get; set; }
            [JsonProperty("cuenta.new_operaporcuentapropia")]
            public string new_operaporcuentapropia { get; set; }
            [JsonProperty("cuenta.new_montoestimadoainvertir")]
            public string new_montoestimadoainvertir { get; set; }
            [JsonProperty("cuenta.new_propsitodelacuenta")]
            public string new_propsitodelacuenta { get; set; }
            [JsonProperty("cuenta.new_otros")]
            public string new_otros { get; set; }
            [JsonProperty("cuenta.new_mtododeemisin")]
            public string new_mtododeemisin { get; set; }
            [JsonProperty("cuenta.new_fechacontratosocial")]
            public string new_fechacontratosocial { get; set; }
            [JsonProperty("cuenta.new_fechaddeinscripcinregistral")]
            public string new_fechaddeinscripcinregistral { get; set; }
            [JsonProperty("cuenta.new_numerodeinscripcinregistral")]
            public string new_numerodeinscripcinregistral { get; set; }
            [JsonProperty("cuenta.new_productoservicio")]
            public string new_productoservicio { get; set; }
            [JsonProperty("cuenta.new_inscripcionganancias")]
            public string new_inscripcionganancias { get; set; }
            [JsonProperty("cuenta.new_origendelosfondos")]
            public string new_origendelosfondos { get; set; }
            [JsonProperty("cuenta.new_tiposujetoobligado")]
            public string new_tiposujetoobligado { get; set; }
            [JsonProperty("cuenta.new_fechaltimaconsulta")]
            public string new_fechaltimaconsulta { get; set; }
            [JsonProperty("cuenta.new_respuestanosis")]
            public string new_respuestanosis { get; set; }
            [JsonProperty("cuenta.new_referido")]
            public string new_referido { get; set; }
            [JsonProperty("cuenta.new_metododeemision")]
            public string new_metododeemision { get; set; }
            [JsonProperty("cuenta._new_contactodenotificaciones_value")]
            public string _new_contactodenotificaciones_value { get; set; }
        }
        public class Socio
        {
            public string accountid { get; set; }
            //public Cuenta cuenta{ get; set; }
            //public ContactoFirmante firmante { get; set; }
            //public Contacto contactoNotificaciones { get; set; }
            //public List<Relacion> relaciones { get; set; }
            //public List<Documentacion> documentos { get; set; }
        }
        public class ContactoNotificaciones
        {
            [JsonProperty("contactoNotificacione.contactid")]
            public string contactid { get; set; }
            [JsonProperty("contactoNotificacione.firstname")]
            public string firstname { get; set; }
            [JsonProperty("contactoNotificacione.lastname")]
            public string lastname { get; set; }
            [JsonProperty("contactoNotificacione.new_cuitcuil")]
            public string new_cuitcuil { get; set; }
            [JsonProperty("contactoNotificacione.emailaddress1")]
            public string emailaddress1 { get; set; }
            [JsonProperty("contactoNotificacione.address1_country")]
            public string address1_country { get; set; }
            [JsonProperty("contactoNotificacione.new_nrodedocumento")]
            public string new_nrodedocumento { get; set; }
            [JsonProperty("contactoNotificacione.statecode")]
            public string statecode { get; set; }
            [JsonProperty("contactoNotificacione._parentcustomerid_value")]
            public string _parentcustomerid_value { get; set; }
            [JsonProperty("contactoNotificacione.new_fechaultimavalidacionidentidadrenaper")]
            public string new_fechaultimavalidacionidentidadrenaper { get; set; }
            [JsonProperty("contactoNotificacione.new_resultadoultimavalidacionidentidadrenaper")]
            public string new_resultadoultimavalidacionidentidadrenaper { get; set; }
        }
        public class ContactoFirmante
        {
            [JsonProperty("contactoFirmante.contactid")]
            public string contactid { get; set; }
            [JsonProperty("contactoFirmante.firstname")]
            public string firstname { get; set; }
            [JsonProperty("contactoFirmante.lastname")]
            public string lastname { get; set; }
            [JsonProperty("contactoFirmante.new_cuitcuil")]
            public decimal new_cuitcuil { get; set; }
            [JsonProperty("contactoFirmante.emailaddress1")]
            public string emailaddress1 { get; set; }
            [JsonProperty("contactoFirmante.address1_country")]
            public string address1_country { get; set; }
            [JsonProperty("contactoFirmante.new_nrodedocumento")]
            public string new_nrodedocumento { get; set; }
            [JsonProperty("contactoFirmante.statecode")]
            public string statecode { get; set; }
            [JsonProperty("contactoFirmante._parentcustomerid_value")]
            public string _parentcustomerid_value { get; set; }
            [JsonProperty("contactoFirmante.new_fechaultimavalidacionidentidadrenaper")]
            public string new_fechaultimavalidacionidentidadrenaper { get; set; }
            [JsonProperty("contactoFirmante.new_resultadoultimavalidacionidentidadrenaper")]
            public string new_resultadoultimavalidacionidentidadrenaper { get; set; }
        }
        public class Relacion
        {
            public string new_participacionaccionariaid { get; set; }
            public string new_name { get; set; }
            public string new_tipoderelacion { get; set; }
            public CuentaVinculada cuentaVinculada { get; set; }
            public Contacto contactoVinculado { get; set; }
            public decimal new_porcentajedeparticipacion { get; set; }
            public string _new_accionista_value { get; set; }
            public string _new_contacto_value { get; set; }
            public string _new_cuentaid_value { get; set; }
            public string new_cargo { get; set; }
            public decimal new_porcentajebeneficiario { get; set; }
            public string statecode { get; set; }
            public string new_observaciones { get; set; }
        }
        public class ContactoVinculado
        {
            [JsonProperty("contactoVinculado.contactid")]
            public string contactid { get; set; }
            [JsonProperty("contactoVinculado.firstname")]
            public string firstname { get; set; }
            [JsonProperty("contactoVinculado.lastname")]
            public string lastname { get; set; }
            [JsonProperty("contactoVinculado.new_cuitcuil")]
            public string new_cuitcuil { get; set; }
            [JsonProperty("contactoVinculado.emailaddress1")]
            public string emailaddress1 { get; set; }
            [JsonProperty("contactoVinculado.address1_country")]
            public string address1_country { get; set; }
            [JsonProperty("contactoVinculado.new_nrodedocumento")]
            public string new_nrodedocumento { get; set; }
            [JsonProperty("contactoVinculado.statecode")]
            public string statecode { get; set; }
            [JsonProperty("contactoVinculado._parentcustomerid_value")]
            public string _parentcustomerid_value { get; set; }
            [JsonProperty("contactoVinculado.new_fechaultimavalidacionidentidadrenaper")]
            public string new_fechaultimavalidacionidentidadrenaper { get; set; }
            [JsonProperty("contactoVinculado.new_resultadoultimavalidacionidentidadrenaper")]
            public string new_resultadoultimavalidacionidentidadrenaper { get; set; }
        }
        public class GarantiaMonetizada
        {
            public string garantiaid { get; set; }
            public string socioid { get; set; }
            public string operacionid { get; set; }
            public string cliente { get; set; }
        }
        public class BuscarGarantiaMonetizada
        {
            public string garantiaid { get; set; }
            public string socioid { get; set; }
            public string cliente { get; set; }
        }
        public class GarantiaReprocesada
        {
            public string garantiaid { get; set; }
            public decimal montoDeLaGarantia { get; set; }
            public decimal porcentaje { get; set; }
            public string cliente { get; set; }
        }
        public class Garantia
        {
            public string new_garantiaid { get; set; }
            public string new_name { get; set; }
            public string new_fechadevencimiento { get; set; }
            public string new_ndeordendelagarantiaotorgada { get; set; }
            public string new_monto_base { get; set; }
            public string _new_socioparticipe_value { get; set; }
            public int new_tipodeoperacion { get; set; }
            public decimal new_saldocreditoprincipal { get; set; }
            public decimal new_saldocuotasoperativo { get; set; }
            public decimal new_montogarantia { get; set; }
            public decimal new_saldocuotasvigentes { get; set; }
            public decimal new_saldodelaamortizacion { get; set; }
            public int new_cantidadcuotasafrontadas { get; set; }
            public string new_fechadenegociacion { get; set; }
            public int new_tipodegarantias { get; set; }
            public string new_fechadeorigen { get; set; }
            public int statuscode { get; set; }
            public decimal new_monto { get; set; }
            [JsonProperty("new_monto@OData.Community.Display.V1.FormattedValue")]
            public string new_monto_formateado { get; set; }
            public decimal new_montoneto { get; set; }
            public int new_dictamendelaval { get; set; }
            public Cuenta cuenta { get; set; }
            public List<Cuota> cuotas { get; set; }
            public SocioGarantia socioGarantia { get; set; }
            public AcreedorVinculado acreedor { get; set; }
            public DivisaVinculada divisa { get; set; }
            public List<OperacionSindicadaVinculada> operacionSindicada { get; set; }
            public bool new_condesembolsosparciales { get; set; }
            public string new_fechaemisindelcheque { get; set; }
            public int new_sistemadeamortizacion { get; set; }
            public int new_tasa { get; set; }
            public decimal new_puntosporcentuales { get; set; }
            public int new_periodicidadpagos { get; set; }
            public string _new_desembolsoanterior_value { get; set; }
            public string new_nroexpedientetad { get; set; }
            public bool new_superatasabadlar { get; set; }
            public decimal new_tasabadlar { get; set; }
            public decimal new_tasabarancaria { get; set; }
            public string new_observaciones { get; set; }
            public string new_periodogracia { get; set; }
            public string _transactioncurrencyid_value { get; set; }
            public string new_saldovigente { get; set; }
            public string _new_acreedor_value { get; set; }
            public string new_tasadedescuento { get; set; }
            public string createdon { get; set; }
            public string new_importetotalavalado { get; set; }
            public string new_cantgarantastotalesasociadasliquidadas { get; set; }
            public string new_importetotalavaladoliquidado { get; set; }
            public string new_porcentajeavaladodelaserie { get; set; }
            public string _new_nmerodeserie_value { get; set; }
            public string new_ponderacion { get; set; }
            public string new_importeponderacion { get; set; }
            public string new_codigo { get; set; }
            public string new_nroordensepyme { get; set; }
            public string new_reafianzada { get; set; }
            public string new_referido { get; set; }
            public string new_sociosprotector { get; set; }
            public string new_fechadecancelada { get; set; }
            public string new_fechadepago { get; set; }
            public string new_fecharealdepago { get; set; }
            public string new_saldooperativo { get; set; }
            public string new_linea { get; set; }
            public string new_saldovigenteponderado { get; set; }
            public string new_oficialdecuentas { get; set; }
            public string new_numerodeprestamo { get; set; }
            public string new_plazodiasprueba { get; set; }
            public string new_amortizacion { get; set; }
            public string new_numero { get; set; }
            public string new_interesperiodo { get; set; }
            public string new_montocuota { get; set; }
            public string new_ponderacionportipodesocio { get; set; }
            public string new_categoriatipodesocio { get; set; }
            public string new_podenderaciondegarantias { get; set; }
            public string new_cargadasincertificadopyme { get; set; }
        }

        public class GarantiaV2
        {
            public string new_garantiaid { get; set; }
            public string new_name { get; set; }
            public string new_fechadevencimiento { get; set; }
            public string new_ndeordendelagarantiaotorgada { get; set; }
            public string new_monto_base { get; set; }
            public string _new_socioparticipe_value { get; set; }
            public int new_tipodeoperacion { get; set; }
            public decimal new_saldocreditoprincipal { get; set; }
            public decimal new_saldocuotasoperativo { get; set; }
            public decimal new_montogarantia { get; set; }
            public decimal new_saldocuotasvigentes { get; set; }
            public decimal new_saldodelaamortizacion { get; set; }
            public int new_cantidadcuotasafrontadas { get; set; }
            public string new_fechadenegociacion { get; set; }
            public int new_tipodegarantias { get; set; }
            public string new_fechadeorigen { get; set; }
            public int statuscode { get; set; }
            public decimal new_monto { get; set; }
            [JsonProperty("new_monto@OData.Community.Display.V1.FormattedValue")]
            public string new_monto_formateado { get; set; }
            public decimal new_montoneto { get; set; }
            public int new_dictamendelaval { get; set; }
            public CuentaV2 cuenta { get; set; }
            public List<Cuota> cuotas { get; set; }
            public SocioGarantia socioGarantia { get; set; }
            public AcreedorVinculado acreedor { get; set; }
            public DivisaVinculada divisa { get; set; }
            public List<OperacionSindicadaVinculada> operacionSindicada { get; set; }
            public bool new_condesembolsosparciales { get; set; }
            public string new_fechaemisindelcheque { get; set; }
            public int new_sistemadeamortizacion { get; set; }
            public int new_tasa { get; set; }
            public decimal new_puntosporcentuales { get; set; }
            public int new_periodicidadpagos { get; set; }
            public string _new_desembolsoanterior_value { get; set; }
            public string new_nroexpedientetad { get; set; }
            public bool new_superatasabadlar { get; set; }
            public decimal new_tasabadlar { get; set; }
            public decimal new_tasabarancaria { get; set; }
            public string new_observaciones { get; set; }
            public string new_periodogracia { get; set; }
            public string _transactioncurrencyid_value { get; set; }
            public string new_saldovigente { get; set; }
            public string _new_acreedor_value { get; set; }
            public string new_tasadedescuento { get; set; }
            public string createdon { get; set; }
            public string new_importetotalavalado { get; set; }
            public string new_cantgarantastotalesasociadasliquidadas { get; set; }
            public string new_importetotalavaladoliquidado { get; set; }
            public string new_porcentajeavaladodelaserie { get; set; }
            public string _new_nmerodeserie_value { get; set; }
            public string new_ponderacion { get; set; }
            public string new_importeponderacion { get; set; }
            public string new_codigo { get; set; }
            public string new_nroordensepyme { get; set; }
            public string new_reafianzada { get; set; }
            public string new_referido { get; set; }
            public string new_sociosprotector { get; set; }
            public string new_fechadecancelada { get; set; }
            public string new_fechadepago { get; set; }
            public string new_fecharealdepago { get; set; }
            public string new_saldooperativo { get; set; }
            public string new_linea { get; set; }
            public string new_saldovigenteponderado { get; set; }
            public string new_oficialdecuentas { get; set; }
            public string new_numerodeprestamo { get; set; }
            public string new_plazodiasprueba { get; set; }
            public string new_amortizacion { get; set; }
            public string new_numero { get; set; }
            public string new_interesperiodo { get; set; }
            public string new_montocuota { get; set; }
            public string new_ponderacionportipodesocio { get; set; }
            public string new_categoriatipodesocio { get; set; }
            public string new_podenderaciondegarantias { get; set; }
            public string new_cargadasincertificadopyme { get; set; }
        }
        public class Documentacion
        {
            [JsonProperty("documentacion.new_documentacionporcuentaid")] 
            public string new_documentacionporcuentaid { get; set; }
            [JsonProperty("documentacion.new_name")]
            public string new_name { get; set; }
            [JsonProperty("documentacion.new_fechadeldocumento")]
            public string new_fechadeldocumento { get; set; }
            [JsonProperty("documentacion.new_fechadevencimiento")]
            public string new_fechadevencimiento { get; set; }
            [JsonProperty("documentacion.statuscode")]
            public string statuscode { get; set; }
            [JsonProperty("documentacion._new_documentoid_value")]
            public string _new_documentoid_value { get; set; }
            [JsonProperty("documentacion._new_cuentaid_value")]
            public string _new_cuentaid_value { get; set; }
            [JsonProperty("documentacion._new_responsable_value")]
            public string _new_responsable_value { get; set; }
            [JsonProperty("documentacion.new_estado")]
            public string new_estado { get; set; }
            [JsonProperty("documentacion.createdon")]
            public string createdon { get; set; }
            [JsonProperty("documentacion.new_vinculocompartido")]
            public string new_vinculocompartido { get; set; }
            [JsonProperty("documentacion.new_visibleenportal")]
            public string new_visibleenportal { get; set; }
            [JsonProperty("documentacion.new_solicituddealta")]
            public string new_solicituddealta { get; set; }
            //Documento
            [JsonProperty("documento.new_codigo")]
            public string new_codigo { get; set; }
            [JsonProperty("documento.new_urlplantilla")]
            public string new_urlplantilla { get; set; }
            [JsonProperty("documento.new_tipodefiador")]
            public int new_tipodefiador { get; set; }
            [JsonProperty("documento.new_requeridoa")]
            public string new_requeridoa { get; set; }
            [JsonProperty("documento.new_personeria")]
            public int new_personeria { get; set; }
            [JsonProperty("documento.new_grupoeconomico")]
            public string new_grupoeconomico { get; set; }
            [JsonProperty("documento.new_fiador")]
            public string new_fiador { get; set; }
            [JsonProperty("documento.new_estadodelsocio")]
            public int new_estadodelsocio { get; set; }
            [JsonProperty("documento.new_descripcion")]
            public string new_descripcion { get; set; }
            [JsonProperty("documento.new_convertirapdf")]
            public string new_convertirapdf { get; set; }
            [JsonProperty("documento.new_condicionimpositiva")]
            public string new_condicionimpositiva { get; set; }
            [JsonProperty("documento.new_name")]
            public string new_name_documento { get; set; }
            [JsonProperty("documento.new_documentacionid")]
            public string new_documentacionid_documento { get; set; }
        }
        public class Cuota
        {
            [JsonProperty("cuota.new_prestamosid")]
            public string new_prestamosid { get; set; }
            [JsonProperty("cuota.new_numero")]
            public int new_numero { get; set; }
            [JsonProperty("cuota.new_montocuota")]
            [Column(TypeName = "decimal(18,4)")]
            public decimal new_montocuota { get; set; }
            [JsonProperty("cuota.statuscode")]
            public int statuscode { get; set; }
            [JsonProperty("cuota.new_fechadevencimiento")]
            public string new_fechadevencimiento { get; set; }
            [JsonProperty("cuota.new_fechaestimadapago")]
            public string new_fechaestimadapago { get; set; }
            [JsonProperty("cuota.new_interesperiodo")]
            public decimal new_interesperiodo { get; set; }
            [JsonProperty("cuota.new_montooperativo")]
            public decimal new_montooperativo { get; set; }
            [JsonProperty("cuota.new_montovigente")]
            public decimal new_montovigente { get; set; }
            [JsonProperty("cuota.new_amortizacion")]
            public decimal new_amortizacion { get; set; }
        }
        public class SocioGarantia
        {
            [JsonProperty("socio.new_nmerodedocumento")]
            public string new_nmerodedocumento { get; set; }
        }
        public class CertificadoPyme
        {
            [JsonProperty("certificado.new_certificadopymeid")]
            public string new_certificadopymeid { get; set; }
            [JsonProperty("certificado.new_numeroderegistro")]
            public int new_numeroderegistro { get; set; }
            [JsonProperty("certificado.new_fechadeemision")]
            public string new_fechadeemision { get; set; }
            [JsonProperty("certificado.new_categoria")]
            public string new_categoria { get; set; }
            [JsonProperty("certificado.new_sectoreconomico")]
            public string new_sectoreconomico { get; set; }
            [JsonProperty("certificado.new_vigenciadesde")]
            public string new_vigenciadesde { get; set; }
            [JsonProperty("certificado.new_vigenciahasta")]
            public string new_vigenciahasta { get; set; }
            [JsonProperty("certificado.statecode")]
            public string statecode { get; set; }
        }

        public class CertificadosPymes
        {
            public string socioid { get; set; }
            public string condicionpyme_id { get; set; }
            public string categoria_id { get; set; }
            public string cliente { get; set; }


            public int new_numeroderegistro { get; set; }

            public string new_fechadeemision { get; set; }

            public string new_categoria { get; set; }

            public string new_sectoreconomico { get; set; }

            public string new_vigenciadesde { get; set; }

            public string new_vigenciahasta { get; set; }

            public string statecode { get; set; }
        }

        public class CertificadosPymesV2
        {
            public string new_certificadopymeid { get; set; }
            public int new_numeroderegistro { get; set; }
            public string new_fechadeemision { get; set; }
            public string new_categoria { get; set; }
            public string new_sectoreconomico { get; set; }
            public DateTime new_vigenciadesde { get; set; }
            public DateTime new_vigenciahasta { get; set; }
            public int statecode { get; set; }
            public int statuscode { get; set; }
        }

        public class CertificadoPymeV2
        {
            [JsonProperty("certificado.new_certificadopymeid")]
            public string new_certificadopymeid { get; set; }
            [JsonProperty("certificado.new_numeroderegistro")]
            public int new_numeroderegistro { get; set; }
            [JsonProperty("certificado.new_fechadeemision")]
            public string new_fechadeemision { get; set; }
            [JsonProperty("certificado.new_categoria")]
            public string new_categoria { get; set; }
            [JsonProperty("certificado.new_sectoreconomico")]
            public string new_sectoreconomico { get; set; }
            [JsonProperty("certificado.new_vigenciadesde")]
            public string new_vigenciadesde { get; set; }
            [JsonProperty("certificado.new_vigenciahasta")]
            public string new_vigenciahasta { get; set; }
            [JsonProperty("certificado.statecode")]
            public string statecode { get; set; }
        }
        public class TipoDocumentoVinculado 
        {
            [JsonProperty("tipoDocumento.new_codigo@OData.Community.Display.V1.FormattedValue")]
            public string new_codigo { get; set; }
            [JsonProperty("tipoDocumento.new_tipodedocumentoid@OData.Community.Display.V1.FormattedValue")]
            public string new_tipodedocumentoid { get; set; }
        }
        public class ActividadAFIPVinculado
        {
            [JsonProperty("actividadAfip.new_codigo")]
            public int new_codigo { get; set; }
            [JsonProperty("tipoDocumento.new_actividadafipid@OData.Community.Display.V1.FormattedValue")]
            public string new_actividadafipid { get; set; }
        }
        public class CondicionPymeVinculado
        {
            [JsonProperty("condicionPyme.new_codigo")]
            public int new_codigo { get; set; }
            [JsonProperty("tipoDocumento.new_condicionpymeid@OData.Community.Display.V1.FormattedValue")]
            public string new_condicionpymeid { get; set; }
        }
        public class CategoriaVinculado
        {
            [JsonProperty("categoria.new_name")]
            public string new_name { get; set; }
            [JsonProperty("categoria.new_codigo@OData.Community.Display.V1.FormattedValue")]
            public string new_codigo { get; set; }
            [JsonProperty("tipoDocumento.new_categoracertificadopymeid@OData.Community.Display.V1.FormattedValue")]
            public string new_categoracertificadopymeid { get; set; }
        }
        public class TipoDocumento
        {
            public string new_codigo { get; set; }
            public string new_tipodedocumentoid { get; set; }
        }
        public class ActividadAFIP
        {
            public int new_codigo { get; set; }
            public string new_actividadafipid { get; set; }
        }
        public class CondicionPyme
        {
            public int new_codigo { get; set; }
            public string new_condicionpymeid { get; set; }
        }
        public class Categoria
        {
            public int new_codigo { get; set; }
            public string new_categoracertificadopymeid { get; set; }
        }
        public class AcreedorVinculado
        {
            [JsonProperty("acreedor.new_name")]
            public string new_name { get; set; }
            [JsonProperty("acreedor.new_cuit")]
            public string new_cuit { get; set; }
            [JsonProperty("acreedor.new_tipodeacreedor")]
            public int new_tipodeacreedor { get; set; }
        }
        public class Acreedor
        {
            public string new_acreedorid { get; set; }
        }
        public class DivisaVinculada
        {
            [JsonProperty("divisa.isocurrencycode")]
            public string isocurrencycode { get; set; }
        }
        public class Divisa
        {
            public string isocurrencycode { get; set; }
            public string transactioncurrencyid { get; set; }
        }
        public class ProvinciaVinculada
        {
            [JsonProperty("provincia.new_codprovincia")]
            public string new_codprovincia { get; set; }
        }
        public class Provincia
        {
            public string new_provinciaid { get; set; }
            public string new_name { get; set; }
        }
        public class PaisVinculado
        {
            [JsonProperty("pais.new_codpais")]
            public string new_codpais { get; set; }
        }
        public class Pais
        {
            public string new_paisid { get; set; }
            public string new_name { get; set; }
        }
        public class Documento
        {
            public string new_documentacionid { get; set; }
            public string new_name { get; set; }
            public string new_descripcion { get; set; }
            public string new_estadodelsocio { get; set; }
            public string new_fiador { get; set; }
            public string new_grupoeconomico { get; set; }
            public string new_personeria { get; set; }
            public string new_requeridoa { get; set; }
            public string new_urlplantilla { get; set; }
            public string new_codigo { get; set; }
        }

        public class DocumentoCasfog
        {
            public string new_documentacionid { get; set; }
            public string new_name { get; set; }
            public string new_descripcion { get; set; }
            public string new_estadodelsocio { get; set; }
            public string new_fiador { get; set; }
            public string new_grupoeconomico { get; set; }
            public string new_personeria { get; set; }
            public string new_requeridoa { get; set; }
            public string new_urlplantilla { get; set; }
            public string new_codigo { get; set; }
            public string new_codigocasfog { get; set; }
        }
        public class OperacionSindicadaVinculada
        {
            [JsonProperty("operacion.new_operacionsindicadaid")]
            public string new_operacionsindicadaid { get; set; }
            [JsonProperty("operacion.new_name")]
            public string new_name { get; set; }
            [JsonProperty("operacion.new_socioparticipe")]
            public string new_socioparticipe { get; set; }
            [JsonProperty("operacion.new_sgr")]
            public string new_sgr { get; set; }
            [JsonProperty("operacion.new_monto")]
            public string new_monto { get; set; }
            [JsonProperty("operacion.new_garantia")]
            public string new_garantia { get; set; }
            [JsonProperty("operacion.new_porcentaje")]
            public decimal new_porcentaje { get; set; }
            [JsonProperty("sgr.new_clientesgroneclick")]
            public bool new_clientesgroneclick { get; set; }
            [JsonProperty("sgr.new_credencialapi")]
            public string new_credencialapi { get; set; }
            [JsonProperty("operacion.new_importeenpesos")]
            public decimal new_importeenpesos { get; set; }
            public Sgr sgr { get; set; }
        }
        public class OP
        {
            public string new_operacionsindicadaid { get; set; }
            public string new_name { get; set; }
            public string new_socioparticipe { get; set; }
            public string new_sgr { get; set; }
            public decimal new_monto { get; set; }
            public string new_garantia { get; set; }
            public decimal new_porcentaje { get; set; }
            public decimal new_importeenpesos { get; set; }
            public bool new_garantiamonetizada { get; set; }
        }
        public class Sgr
        {
            [JsonProperty("sgr.new_clientesgroneclick")]
            public bool new_clientesgroneclick { get; set; }
            [JsonProperty("sgr.new_credencialapi")]
            public string new_credencialapi { get; set; }
            [JsonProperty("sgr.new_name")]
            public string new_name { get; set; }
        }
        public class GarantiaSGR
        {
            public string new_garantiaid { get; set; }
            public string new_fechadenegociacion { get; set; }
            public List<Cuota> cuotas { get; set; }
        }
        public class Limite
        {
            public string new_productosid { get; set; }
            public decimal new_limitecomercialdivisa { get; set; }
            [JsonProperty("new_lineatipodeoperacion@OData.Community.Display.V1.FormattedValue")]
            public string new_lineatipodeoperacion_value { get; set; }
            public int new_lineatipodeoperacion { get; set; }
            public DateTime new_vigenciahasta { get; set; }
        }
        public class Desembolso
        {
            public string garantiaid { get; set; }
            public string cliente { get; set; }

        }
    }
}
