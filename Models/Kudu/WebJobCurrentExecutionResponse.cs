using System;
using System.Collections.Generic;

namespace Api.Web.Dynamics365.Models.Kudu
{
    public class WebJobCurrentExecutionResponse
    {
        public string AppService { get; set; } = "";
        public string WebJobName { get; set; } = "";
        public string JobType { get; set; } = "";         // "continuous" | "triggered"

        public bool IsRunningNow { get; set; }             // lo que te importa
        public string Status { get; set; } = "Unknown";    // Running | Success | Failed | Stopped | Unknown

        // Para UI:
        public string? ExecutionId { get; set; }           // triggered: latest_run.id / continuous: último RunId si existe
        public DateTime? StartTimeUtc { get; set; }
        public DateTime? EndTimeUtc { get; set; }

        // Mantengo esto por compatibilidad, pero no lo fuerzo si no corre
        public bool TruncatedByKudu { get; set; }          // si aparece el mensaje de límite

        // Traza (solo ejecución actual cuando corre)
        public List<WebJobLogEntry> Entries { get; set; } = new();
    }
}
