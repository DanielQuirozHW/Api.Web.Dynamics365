using System;
using System.Collections.Generic;

namespace Api.Web.Dynamics365.Models.Kudu
{
    public class WebJobCurrentExecutionResponse
    {
        public string AppService { get; set; } = "";
        public string WebJobName { get; set; } = "";
        public string JobType { get; set; } = "";

        public bool IsRunningNow { get; set; }
        public string Status { get; set; } = "Unknown";

        public string? ExecutionId { get; set; }
        public DateTime? StartTimeUtc { get; set; }

        public bool TruncatedByKudu { get; set; }

        public List<WebJobLogEntry> Entries { get; set; } = new();
    }
}
