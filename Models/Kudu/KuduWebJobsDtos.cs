using System;
using System.Text.Json.Serialization;

namespace Api.Web.Dynamics365.Models.Kudu
{
    public class KuduContinuousJobDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class KuduTriggeredJobDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("latest_run")]
        public KuduTriggeredLatestRunDto? LatestRun { get; set; }
    }

    public class KuduTriggeredLatestRunDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("start_time")]
        public DateTime? StartTimeUtc { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime? EndTimeUtc { get; set; }

        [JsonPropertyName("output_url")]
        public string? OutputUrl { get; set; }

        [JsonPropertyName("error_url")]
        public string? ErrorUrl { get; set; }
    }
}
