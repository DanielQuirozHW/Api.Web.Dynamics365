using System;
using System.Text.Json.Serialization;

namespace Api.Web.Dynamics365.Models.Kudu
{
    public class WebJobLogEntry
    {
        public DateTime? Timestamp { get; set; }
        public string? RunId { get; set; }
        public string Level { get; set; } = "UNKNOWN";
        public string Message { get; set; } = "";

        [JsonIgnore]
        public string Raw { get; set; } = "";
    }
}
