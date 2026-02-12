namespace Api.Web.Dynamics365.Models
{
    public class Converter
    {
        public string nombreArchivo { get; set; }
        public string documentoB64 { get; set; }
        public string tipoDeArchivo { get; set; }
        public string tipoDeArchivoAConvertir { get; set; }
        public string entidadAsociadaID { get; set; }
        public string entidadAsociada { get; set; }
        public string campoEntidadAsociada { get; set; }

        public class ConverterStream
        {
            public string nombreArchivo { get; set; }
            public string documentoB64 { get; set; }
            public string tipoDeArchivo { get; set; }
            public string tipoDeArchivoAConvertir { get; set; }
        }

        public class ResponseStream
        {
            public string stream { get; set; }
        }

    }
}
