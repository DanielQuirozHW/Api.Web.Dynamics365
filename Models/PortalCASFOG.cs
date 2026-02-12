namespace Api.Web.Dynamics365.Models
{
    public class PortalCASFOG
    {
        public class GarantiaCasfog
        {
            public string new_garantiaid { get; set; }
            public int new_tipodeoperacion { get; set; }
            public string new_fechadeorigen { get; set; }
            public string new_acreedor { get; set; }
            public string new_serie { get; set; }
            public int statuscode { get; set; }
            public string new_referido { get; set; }
            public string new_fechaemisindelcheque { get; set; }
            public string new_oficialdecuentas { get; set; }
            public string new_fechadenegociacion { get; set; }
            public int new_sistemadeamortizacion { get; set; }
            public int new_tasa { get; set; }
            public decimal new_puntosporcentuales { get; set; }
            public int new_periodicidadpagos { get; set; }
            public int new_dictamendelaval { get; set; }
            public string new_creditoaprobado { get; set; }
            public string new_codigo { get; set; }
            public int new_tipodegarantias { get; set; }
            public string new_nroexpedientetad { get; set; }
            public int new_plazodias { get; set; }
            public string new_fechadevencimiento { get; set; }
            public decimal new_montocomprometidodelaval { get; set; }
            public string new_determinadaenasamblea { get; set; }
            public decimal new_monto { get; set; }
            public string new_socioparticipe { get; set; }
            public string new_nmerodeserie { get; set; }
            public int new_numerodeprestamo { get; set; }
            //Desembolso
            public string new_DesembolsoAnterior { get; set; }
            public string new_condesembolsosparciales { get; set; }
        }

        public class OperacionSindicadaCasfog
        {
            public string new_operacionsindicadaid { get; set; }
            public int statuscode { get; set; }
            public string new_motivoderechazo { get; set; }
        }

        public class Pyme
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
            public string new_fechaltimaconsulta { get; set; }
            public string new_respuestanosis { get; set; }
            public int new_calificacion { get; set; }
            public string emailaddress1 { get; set; }
        }

        public class DocumentacionPorCuenta
        {
            public string new_documentacionporcuentaid { get; set; }
            public int statuscode { get; set; }
            public string new_fechadevencimiento { get; set; }
            public string new_visibleenportal { get; set; }
        }

        public class SGR
        {
            //new_sgr
            public string new_sgrid { get; set; }
            public string new_fechadeiniciodeactividades { get; set; }
            public string new_fechadeasociacinencasfog { get; set; }
            public string new_fechainscripcinantebcra { get; set; }
            public string new_nombredelacalificadora { get; set; }
            public string new_calificacin { get; set; }
            public string new_fechaultimacalificacion { get; set; }
            public string new_cuitcalificadora { get; set; }
        }

        public class EstructuraSGR
        {
            //new_estructurasgr
            public string new_estructurasgrid { get; set; }
            public string new_sgr { get; set; }
            public string new_contacto { get; set; }
            public int new_rol { get; set; }
            public string new_cargo { get; set; }
            public string new_name { get; set; }
            public string new_correoelectronico { get; set; }
            public string new_numerodedocumento { get; set; }
            public decimal new_porcentaje { get; set; }
        }

        public class DocumentacionPorSGR
        {
            //new_documentacionporsgr
            public string new_sgr { get; set; }
            public string new_documentacion { get; set; }
            public string new_fechadevencimiento { get; set; }
        }

        public class IndicadorMensualSGR
        {
            //new_indicadoresmensualessgr
            public string new_fechahasta { get; set; }
            public string new_indicadoresmensualessgrid { get; set; }
            public string new_sgr { get; set; }
            //INDICADORES MENSUALES(Fuente SGR)
            public decimal new_saldonetodegarantiasvigentes { get; set; }
            public decimal new_solvencia { get; set; }
            public decimal new_fondoderiesgointegrado { get; set; }
            public decimal new_fondoderiesgodisponible { get; set; }
            public decimal new_fondoderiesgocontingente { get; set; }
            public decimal new_fondoderiesgoavalordemercado { get; set; }

            //INDICADORES MENSUALES COMPLEMENTARIOS(fuente SGR/FONDOS)

            //RIESGO MERCADO DE CAPITALES POR ENTIDAD DE GARANTÍA
            public decimal new_porcentajeriesgopropio { get; set; }
            public decimal new_porcentajeriesgoterceros { get; set; }

            //COMPOSICION CONTRAGARANTIAS SEGUN PYMES CON GARANTIAS VIGENTES
            public decimal new_porcentajeprenda { get; set; }
            public decimal new_porcentajehipoteca { get; set; }
            public decimal new_porcentajefianza { get; set; }
            public decimal new_porcentajeotras { get; set; }

            //Garantías Vigentes(riesgo vivo) CNV por Tipo de Acreedor
            public string new_entidadesfinancierascnv { get; set; }
            public decimal new_garantiascomercialescnv { get; set; }
            public decimal new_garantastecnicascnv { get; set; }
            public decimal new_mercadodecapitalescnv { get; set; }

            //Garantías Vigentes(riesgo vivo) CNV por tipo de instrumento del Mercado de Capitales
            public decimal new_chequedepagodiferidocnv { get; set; }
            public decimal new_pagarbursatilcnv { get; set; }
            public decimal new_valoresdecortoplazocnv { get; set; }
            public decimal new_obligacionesnegociablescnv { get; set; }

            //Garantías Vigentes(riesgo vivo) CNV por tipo de instrumento del Mercado de Capitales
            public decimal new_garantasvigentesrvenpymesensituacion1 { get; set; }
            public decimal new_garantasvigentesrvenpymesensituacion2 { get; set; }
            public decimal new_garantasvigentesrvenpymesensituacion3 { get; set; }
            public decimal new_garantasvigentesrvenpymesensituacion4 { get; set; }
            public decimal new_garantasvigentesrvenpymesensituacion5 { get; set; }

        }

        public class IndicadorMensualSocioYLibradores
        {
            //new_indicadormensualsocioylibradores
            public string new_indicadormensualsocioylibradoresid { get; set; }
            public string new_indicadormensualsgr { get; set; }
            public string new_name { get; set; }
            public string new_librador { get; set; }
            public decimal new_porcentajelibrador { get; set; }
            public string new_socioparticipetercero { get; set; }
            public decimal new_porcentajesocioparticipetercero { get; set; }
            public string new_socioprotector { get; set; }
            public decimal new_porcentajesocioprotector { get; set; }
            public string new_fecha { get; set; }
        }

        public class DocumentacionSGR
        {
            public string sgr_id { get; set; }
            public string documentacion_id { get; set; }
            public string fechaVencimiento { get; set; }
        }

        public class AdjuntoGarantia
        {
            public string serie { get; set; }
            public string garantia { get; set; }
            public int tipoTemplate { get; set; }
        }

        public class AdjuntoGarantiaActualizar
        {
            public string adjuntoGarantiaId { get; set; }
            public string notaid { get; set; }
            public bool visiblePortal { get; set; }
        }

        public class DocumentoTemplate
        {
            public string new_documentacionid { get; set; }
        }
    }
}
