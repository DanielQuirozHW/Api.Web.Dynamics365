namespace Api.Web.Dynamics365.Models
{
    public class Firmante
    {
        public Firmante(string contactid, string correo, string nombre)
        {
            this.contactid = contactid;
            this.correo = correo;
            this.nombre = nombre;
        }

        public string contactid { get; set; }
        public string correo { get; set; }
        public string nombre { get; set; }
    }
}
