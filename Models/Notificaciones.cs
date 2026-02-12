namespace Api.Web.Dynamics365.Models
{
    public class Notificaciones
    {
        public string notification_id { get; set; }
        public string document_id { get; set; }
        public string signature_id { get; set; }
        public string timestamp_id { get; set; }
        public string notification_action { get; set; }
        public string new_status { get; set; }
    }
}
