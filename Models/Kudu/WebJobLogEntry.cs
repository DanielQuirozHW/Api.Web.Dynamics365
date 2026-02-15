using System;
using System.Text.Json.Serialization;

namespace Api.Web.Dynamics365.Models.Kudu
{
    public class WebJobLogEntry
    {
        public DateTime? Timestamp { get; set; }

        [JsonIgnore]
        public string? RunId { get; set; }
        public string Level { get; set; } = "UNKNOWN";
        public string Message { get; set; } = "";
        public bool IsCurrentExecution { get; set; }

        [JsonIgnore]
        public string Raw { get; set; } = "";

        [JsonIgnore]
        public int Index { get; set; }
    }
}
