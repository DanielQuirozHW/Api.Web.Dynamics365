using System.Collections.Generic;

namespace Api.Web.Dynamics365.Models.Kudu
{
    public class WebJobLogParsedResponse
    {
        public string AppService { get; set; } = "";
        public string WebJobName { get; set; } = "";
        public string JobType { get; set; } = "";
        public WebJobLogSummary Summary { get; set; } = new WebJobLogSummary();
        public IReadOnlyList<WebJobLogEntry> Entries { get; set; } = new List<WebJobLogEntry>();

        public WebJobLogParsedResponse() { }

        public WebJobLogParsedResponse(
            string appService,
            string webJobName,
            string jobType,
            WebJobLogSummary summary,
            IReadOnlyList<WebJobLogEntry> entries)
        {
            AppService = appService;
            WebJobName = webJobName;
            JobType = jobType;
            Summary = summary;
            Entries = entries;
        }
    }
}
