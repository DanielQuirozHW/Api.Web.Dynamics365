namespace Api.Web.Dynamics365.Models
{
    public class Error
    {
        public string[] file_content { get; set; }
        public string[] fashion { get; set; }
        public string[] validations { get; set; }
        public Dictionary<string, string[]> selected_emails { get; set; }   
    }

    public class Errores
    {
        public string[] property1 { get; set; }
        public string[] property2 { get; set; }
    }
}
