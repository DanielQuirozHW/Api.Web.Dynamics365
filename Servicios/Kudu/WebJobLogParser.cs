using Api.Web.Dynamics365.Models.Kudu;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Api.Web.Dynamics365.Servicios.Kudu
{
    public static class WebJobLogParser
    {
        // Ej: [01/25/2026 03:08:06 > 79fc8c: SYS INFO] Status changed to Success
        private static readonly Regex HeaderRegex = new(
            @"^\[(?<ts>\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2}) > (?<run>[0-9a-fA-F]+): (?<lvl>[A-Z ]+)\] (?<msg>.*)$",
            RegexOptions.Compiled);

        // Estos son los mensajes “no va / idle / reinicio” que NO querés devolver
        private static readonly string[] NoisePhrases =
        {
            "Sin Mensajes en Cola",                 // “no encontró nada”
            "Run script 'run.cmd'",                 // arranque
            "Status changed to Running",            // sys
            "Status changed to Success",            // sys
            "Status changed to PendingRestart",     // sys
            "Process went down",                    // sys
            "waiting for 60 seconds"                // sys
        };

        public static List<WebJobLogEntry> Parse(string sanitizedLogText)
        {
            var lines = (sanitizedLogText ?? "")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');

            var result = new List<WebJobLogEntry>(lines.Length);
            WebJobLogEntry? current = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine;
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var m = HeaderRegex.Match(line);
                if (m.Success)
                {
                    if (current != null) result.Add(current);

                    DateTime? ts = null;
                    var tsStr = m.Groups["ts"].Value;
                    if (DateTime.TryParseExact(tsStr, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal, out var parsed))
                    {
                        ts = parsed;
                    }

                    current = new WebJobLogEntry
                    {
                        Timestamp = ts,
                        RunId = m.Groups["run"].Value,
                        Level = (m.Groups["lvl"].Value ?? "UNKNOWN").Trim(),
                        Message = m.Groups["msg"].Value ?? "",
                        Raw = line
                    };
                }
                else
                {
                    if (current == null)
                    {
                        current = new WebJobLogEntry
                        {
                            Timestamp = null,
                            RunId = null,
                            Level = "UNKNOWN",
                            Message = line,
                            Raw = line
                        };
                    }
                    else
                    {
                        current.Message += "\n" + line;
                        current.Raw += "\n" + line;
                    }
                }
            }

            if (current != null) result.Add(current);
            return result;
        }

        /// <summary>
        /// “Ejecución actual” para continuous, usando el último RunId encontrado en el log.
        /// Si no hay RunId (formato inesperado), devuelve todo.
        /// </summary>
        public static List<WebJobLogEntry> KeepOnlyLastRun(List<WebJobLogEntry> entries)
        {
            if (entries == null || entries.Count == 0) return new List<WebJobLogEntry>();

            var lastRun = entries.LastOrDefault(e => !string.IsNullOrWhiteSpace(e.RunId))?.RunId;
            if (string.IsNullOrWhiteSpace(lastRun))
                return entries;

            return entries
                .Where(e => string.Equals(e.RunId, lastRun, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static bool DetectTruncatedByKudu(string sanitizedText)
        {
            return (sanitizedText ?? "")
                .IndexOf("Reached maximum allowed output lines", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static List<WebJobLogEntry> TakeLast(List<WebJobLogEntry> entries, int max)
        {
            if (entries == null || entries.Count == 0) return new List<WebJobLogEntry>();
            if (max <= 0) return new List<WebJobLogEntry>();
            if (entries.Count <= max) return entries;

            return entries.Skip(entries.Count - max).ToList();
        }

        private static bool ContainsAny(string? text, IEnumerable<string> phrases)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            foreach (var p in phrases)
            {
                if (text.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static bool IsNoise(WebJobLogEntry e)
        {
            // SYS INFO siempre es “ruido” para tu UI
            if (!string.IsNullOrWhiteSpace(e.Level) &&
                e.Level.IndexOf("SYS", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (string.IsNullOrWhiteSpace(e.Message))
                return true;

            // mensajes de idle / restart
            if (ContainsAny(e.Message, NoisePhrases))
                return true;

            return false;
        }

        private static bool IsCycleBoundary(WebJobLogEntry e)
        {
            // Un “corte” típico del ciclo: arranque o restart del job.
            // Ojo: muchos vienen como SYS INFO, por eso miramos message.
            return ContainsAny(e.Message, new[]
            {
                "Run script 'run.cmd'",
                "Status changed to Running",
                "Status changed to PendingRestart",
                "Process went down",
                "waiting for 60 seconds"
            });
        }

        /// <summary>
        /// ✅ FIX: en continuous el runId puede repetirse por múltiples ciclos (idle/restart).
        /// Esta función devuelve SOLO la última “ventana activa” con mensajes útiles.
        ///
        /// - Si lo último que hay son mensajes tipo "Sin Mensajes en Cola.." + SYS INFO, devuelve vacío.
        /// - Si hay mensajes de negocio, devuelve desde el último “arranque/running” previo a esos mensajes hasta el final,
        ///   filtrando SYS INFO y ruido.
        /// </summary>
        public static List<WebJobLogEntry> KeepOnlyLastActiveWindow(List<WebJobLogEntry> lastRunEntries)
        {
            if (lastRunEntries == null || lastRunEntries.Count == 0)
                return new List<WebJobLogEntry>();

            // 1) Encontrar el último mensaje “útil”
            var lastUsefulIdx = -1;
            for (int i = lastRunEntries.Count - 1; i >= 0; i--)
            {
                if (!IsNoise(lastRunEntries[i]))
                {
                    lastUsefulIdx = i;
                    break;
                }
            }

            // Si no hay nada útil => es el caso “no va / idle”
            if (lastUsefulIdx < 0)
                return new List<WebJobLogEntry>();

            // 2) Buscar hacia atrás el último “boundary” (inicio de ciclo) antes del último útil
            var startIdx = 0;
            for (int i = lastUsefulIdx; i >= 0; i--)
            {
                if (IsCycleBoundary(lastRunEntries[i]))
                {
                    startIdx = i + 1; // empezar después del boundary
                    break;
                }
            }

            // 3) Cortar ventana [startIdx..end] y filtrar ruido
            var window = lastRunEntries
                .Skip(startIdx)
                .Where(e => !IsNoise(e))
                .ToList();

            // 4) Validación final: si lo “útil” era en realidad solo “Sin Mensajes...” (ya lo filtramos),
            // window puede quedar vacío.
            return window;
        }
    }
}
