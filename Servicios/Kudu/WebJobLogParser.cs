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
        private static readonly Regex HeaderRegex = new(
            @"^\[(?<ts>\d{2}/\d{2}/\d{4} \d{2}:\d{2}:\d{2}) > (?<run>[0-9a-fA-F]+): (?<lvl>[A-Z ]+)\] (?<msg>.*)$",
            RegexOptions.Compiled);

        private const string KUDU_TRUNC_MARKER = "Reached maximum allowed output lines";

        private static readonly string[] NoisePhrases =
        {
            "Sin Mensajes en Cola",
            "Run script 'run.cmd'",
            "Status changed to Running",
            "Status changed to Success",
            "Status changed to PendingRestart",
            "Process went down",
            "waiting for 60 seconds"
        };

        private static readonly string[] CycleBoundaryPhrases =
        {
            "Run script 'run.cmd'",
            "Status changed to Running",
            "Status changed to PendingRestart",
            "Process went down",
            "waiting for 60 seconds"
        };

        private static readonly Regex SensitiveLineRegex = new(
            @"(^|\b)OAuth\s*:|" +
            @"AuthType\s*=\s*ClientSecret|" +
            @"ClientSecret\s*=|" +
            @"ClientId\s*=|" +
            @"LoginPrompt\s*=|" +
            @"authority\s*=|" +
            @"resource\s*=|" +
            @"tenant\s*=|" +
            @"\burl\s*=\s*https?://|" +
            @"Password\s*=|" +
            @"SharedAccessKey\s*=|" +
            @"AccountKey\s*=|" +
            @"Authorization:\s*Bearer\s+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static List<WebJobLogEntry> Parse(string rawLogText)
        {
            var lines = (rawLogText ?? "")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');

            var result = new List<WebJobLogEntry>(lines.Length);
            WebJobLogEntry? current = null;
            var idx = 0;

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
                        Index = idx++,
                        Timestamp = ts,
                        RunId = m.Groups["run"].Value,
                        Level = (m.Groups["lvl"].Value ?? "UNKNOWN").Trim(),
                        Message = m.Groups["msg"].Value ?? "",
                        Raw = line,
                        IsCurrentExecution = false
                    };
                }
                else
                {
                    if (current == null)
                    {
                        current = new WebJobLogEntry
                        {
                            Index = idx++,
                            Timestamp = null,
                            RunId = null,
                            Level = "UNKNOWN",
                            Message = line,
                            Raw = line,
                            IsCurrentExecution = false
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

        public static bool DetectTruncatedByKudu(string rawText)
        {
            return (rawText ?? "")
                .IndexOf(KUDU_TRUNC_MARKER, StringComparison.OrdinalIgnoreCase) >= 0;
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
        private static bool IsKuduTruncationMarker(WebJobLogEntry e)
        {
            return !string.IsNullOrWhiteSpace(e.Message) &&
                   e.Message.IndexOf(KUDU_TRUNC_MARKER, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsSensitive(WebJobLogEntry e)
        {
            if (string.IsNullOrWhiteSpace(e.Message)) return false;
            if (IsKuduTruncationMarker(e)) return false;
            return SensitiveLineRegex.IsMatch(e.Message);
        }

        private static bool IsNoise(WebJobLogEntry e)
        {
            if (IsKuduTruncationMarker(e))
                return false;

            if (IsSensitive(e))
                return true;

            if (!string.IsNullOrWhiteSpace(e.Level) &&
                e.Level.IndexOf("SYS", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (string.IsNullOrWhiteSpace(e.Message))
                return true;

            if (ContainsAny(e.Message, NoisePhrases))
                return true;

            return false;
        }

        private static bool IsCycleBoundary(WebJobLogEntry e)
        {
            return ContainsAny(e.Message, CycleBoundaryPhrases);
        }

        public static (bool hasActive, int startIndex, int endIndex) FindLastActiveRange(List<WebJobLogEntry> lastRunEntries)
        {
            if (lastRunEntries == null || lastRunEntries.Count == 0)
                return (false, 0, -1);

            var lastUsefulIdx = -1;
            for (int i = lastRunEntries.Count - 1; i >= 0; i--)
            {
                if (!IsNoise(lastRunEntries[i]))
                {
                    lastUsefulIdx = i;
                    break;
                }
            }

            if (lastUsefulIdx < 0)
                return (false, 0, -1);

            var lastUsefulIndexValue = lastRunEntries[lastUsefulIdx].Index;

            var endIndexValue = lastRunEntries.Max(e => e.Index);
            for (int i = lastUsefulIdx + 1; i < lastRunEntries.Count; i++)
            {
                if (IsCycleBoundary(lastRunEntries[i]))
                {
                    endIndexValue = lastRunEntries[i].Index - 1;
                    break;
                }
            }

            var startIndexValue = lastRunEntries.Min(e => e.Index);
            for (int i = lastUsefulIdx; i >= 0; i--)
            {
                if (IsCycleBoundary(lastRunEntries[i]))
                {
                    startIndexValue = lastRunEntries[i].Index + 1;
                    break;
                }
            }

            if (startIndexValue > lastUsefulIndexValue || endIndexValue < lastUsefulIndexValue)
                return (false, 0, -1);

            return (true, startIndexValue, endIndexValue);
        }

        public static List<WebJobLogEntry> ExtractRealTrace(
            List<WebJobLogEntry> entries,
            int currentStartIndex,
            int currentEndIndex,
            bool markCurrent)
        {
            if (entries == null || entries.Count == 0)
                return new List<WebJobLogEntry>();

            var result = new List<WebJobLogEntry>(entries.Count);

            foreach (var e in entries)
            {
                if (IsNoise(e))
                    continue;

                result.Add(new WebJobLogEntry
                {
                    Timestamp = e.Timestamp,
                    RunId = e.RunId,
                    Level = e.Level,
                    Message = e.Message,
                    Raw = e.Raw,
                    Index = e.Index,
                    IsCurrentExecution = markCurrent && e.Index >= currentStartIndex && e.Index <= currentEndIndex
                });
            }

            return result;
        }
    }
}
