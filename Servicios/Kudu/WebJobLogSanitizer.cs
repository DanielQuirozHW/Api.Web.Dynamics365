using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Api.Web.Dynamics365.Servicios.Kudu
{
    public static class WebJobLogSanitizer
    {
        // - secrets típicos
        // - connection string params
        // - bearer tokens
        private static readonly (Regex rx, string repl)[] Redactions =
        {
            (new Regex(@"ClientSecret\s*=\s*[^;,\r\n]+", RegexOptions.IgnoreCase|RegexOptions.Compiled), "ClientSecret=***"),
            (new Regex(@"Password\s*=\s*[^;,\r\n]+", RegexOptions.IgnoreCase|RegexOptions.Compiled), "Password=***"),
            (new Regex(@"SharedAccessKey\s*=\s*[^;,\r\n]+", RegexOptions.IgnoreCase|RegexOptions.Compiled), "SharedAccessKey=***"),
            (new Regex(@"AccountKey\s*=\s*[^;,\r\n]+", RegexOptions.IgnoreCase|RegexOptions.Compiled), "AccountKey=***"),

            (new Regex(@"Authorization:\s*Bearer\s+[A-Za-z0-9\-\._~\+\/]+=*", RegexOptions.IgnoreCase|RegexOptions.Compiled), "Authorization: Bearer ***"),
        };

        public static string FixMojibake(string s)
        {
            // Heurística: si no hay caracteres sospechosos, no tocamos
            // Agrego '�' (U+FFFD) porque en tu ejemplo aparece como Par�metros
            if (!s.Contains("ï¿½") && !s.Contains("Ã") && !s.Contains("Â") && !s.Contains("�"))
                return s;

            try
            {
                // Estrategia simple: muchas veces viene como Latin1 mal interpretado.
                var bytes = Encoding.Latin1.GetBytes(s);
                var utf8 = Encoding.UTF8.GetString(bytes);

                // Si empeora (mete más �), devolvemos original.
                if (utf8.Count(c => c == '�') > s.Count(c => c == '�'))
                    return s;

                return utf8;
            }
            catch
            {
                return s;
            }
        }

        public static string Redact(string input)
        {
            var s = input ?? "";
            foreach (var (rx, repl) in Redactions)
                s = rx.Replace(s, repl);
            return s;
        }

        public static string Sanitize(string input)
        {
            var fixedText = FixMojibake(input ?? "");
            return Redact(fixedText);
        }
    }
}
