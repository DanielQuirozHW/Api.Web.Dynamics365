namespace Api.Web.Dynamics365.Models
{
    public class Onboarding
    {
        public string personeria { get; set; }
        public string razonSocial { get; set; }
        public string cuit { get; set; }
        public string cuitcuil { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public string nombreContacto { get; set; }
        public string apellido { get; set; }
        public string tipoDocumento { get; set; }
        public string productoServicio { get; set; }
        public string actividadAFIP { get; set; }
        public string monto { get; set; }
        public string tipoRelacion { get; set; }
        public string tipoSocietario { get; set; }
        public string condicionImpositiva { get; set; }
        public string cantidadMujeres { get; set; }
        public string empleadas { get; set; }
        public string discapacitados { get; set; }
        public string otro { get; set; }
        public string sectorEconomico { get; set; }
        public string inicioActividad { get; set; }
        public string resena { get; set; }
        public string emailNotificaciones { get; set; }
        public string invitacion { get; set; }
        public string cuitReferidor { get; set; }
        public string facturacion { get; set; }
        public string montoGarantia { get; set; }
        public string nroExpediente { get; set; }
        public string creditoAprobado { get; set; }
        public string serie { get; set; }
        public string creadaPorApiLufe { get; set; }
        public string calle { get; set; }
        public string numero { get; set; }
        public string piso { get; set; }
        public string departamento { get; set; }
        public string codigoPostal { get; set; }
        public string municipio { get; set; }
        public string localidad { get; set; }
        public string provincia { get; set; }
        public string pais { get; set; }
        public string destinoLineaDeCredito { get; set; }
        public string lineaDeCredito { get; set; }
        public string observaciones { get; set; }
    }

    public class DocumentacionPorCuenta
    {
        public string documentacion_id { get; set; }
        public string socio_id { get; set; }
        public string documentacionporcuenta_id { get; set; }
        public string solicitud_id { get; set; }
        public string cliente { get; set; }
    }

    public class DocumentacionPorCuentaONB
    {
        public string new_documentacionporcuentaid { get; set; }
    }

    public class DocumentacionPorCuentaOnboarding
    {
        public string new_documentoid { get; set; }
    }

    public class Accionistas
    {
        public string personeria { get; set; }
        public string cuitcuil { get; set; }
        public string razonSocial { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string tipoRelacion { get; set; }
        public bool relacionDirecta { get; set; }
        public string porcentaje { get; set; }
        public string descripcion { get; set; }
        public string uid { get; set; }
        public string tipoRelacionAccionista { get; set; }  
    }

    public class Account
    {
        public Guid accountid { get; set; }
    }

    public class Contact
    {
        public Guid contactid { get; set; }
    }

    public class GarantiaPublica
    {
        public string socio_id { get; set; }
        public string serie_id { get; set; }
        public string monto { get; set; }
    }

    public class SerieOnboarding
    {
        public int new_tasa { get; set; }
        public int new_sistemadeamortizacion { get; set; }
        public decimal new_porcentajeavaladodelaserie { get; set; }
        public int new_periodicidadpagos { get; set; }
        public decimal new_intersptosporcentuales { get; set; }
    }

    public class ContactoOnboardingCasfog
    {
        public string contactid { get; set; }
        public string accountid { get; set; }
        public string firstname { get; set; }
        public decimal new_cuitcuil { get; set; }
        public string emailaddress1 { get; set; }
        public string new_nrodedocumento { get; set; }
        public string address1_line1 { get; set; }
        public string telephone1 { get; set; }
        

    }
}
