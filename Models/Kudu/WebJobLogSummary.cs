using System;
using System.Collections.Generic;

namespace Api.Web.Dynamics365.Models.Kudu
{
    public class WebJobLogSummary
    {
        public string Status { get; set; } = "Unknown"; // Success | Failed | Running | Unknown
        public DateTime? FirstTimestamp { get; set; }
        public DateTime? LastTimestamp { get; set; }
        public int TotalEntries { get; set; }
        public IReadOnlyDictionary<string, int> CountByLevel { get; set; } =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public bool TruncatedByKudu { get; set; }

        public IReadOnlyList<WebJobLogEntry> TopErrors { get; set; } =
            Array.Empty<WebJobLogEntry>();

        public WebJobLogSummary() { }

        public WebJobLogSummary(
            string status,
            DateTime? firstTimestamp,
            DateTime? lastTimestamp,
            int totalEntries,
            IReadOnlyDictionary<string, int> countByLevel,
            bool truncatedByKudu,
            IReadOnlyList<WebJobLogEntry> topErrors)
        {
            Status = status;
            FirstTimestamp = firstTimestamp;
            LastTimestamp = lastTimestamp;
            TotalEntries = totalEntries;
            CountByLevel = countByLevel;
            TruncatedByKudu = truncatedByKudu; //Kudu cortó la traza por maximo de líneas? 
            TopErrors = topErrors;
        }
    }
}
