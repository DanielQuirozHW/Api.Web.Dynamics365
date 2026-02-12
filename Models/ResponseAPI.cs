using Newtonsoft.Json.Linq;

namespace Api.Web.Dynamics365.Models
{
    public class ResponseAPI
    {
        public int codigo { get; set; }
        public bool ok { get; set; }
        public string descripcion { get; set; }
        public JArray coleccion { get; set; }
    }
}
