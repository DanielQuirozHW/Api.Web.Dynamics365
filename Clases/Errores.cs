namespace Api.Web.Dynamics365.Clases
{
    public class Errores
    {
        public Error error { get; set; }

        public class Error
        {
            public string code { get; set; }
            public string message { get; set; }
        }
    }
}
