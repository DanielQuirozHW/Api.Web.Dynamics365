using Api.Web.Dynamics365.Models.Kudu;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Api.Web.Dynamics365.Servicios.Kudu
{
    public interface IKuduHttpClientFactory
    {
        (bool ok, KuduConfig cfg, string error) TryGetConfig(string appServiceKey);
        HttpClient CreateAuthedClient(KuduConfig cfg);
        string CombineUrl(string baseUrl, string relativeOrAbsolute);
        string NormalizeBaseUrl(string scmBaseUrl);
    }

    public class KuduHttpClientFactory : IKuduHttpClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<KuduOptions> _kuduOptions;

        public KuduHttpClientFactory(IHttpClientFactory httpClientFactory, IOptionsMonitor<KuduOptions> kuduOptions)
        {
            _httpClientFactory = httpClientFactory;
            _kuduOptions = kuduOptions;
        }

        public (bool ok, KuduConfig cfg, string error) TryGetConfig(string appServiceKey)
        {
            var all = _kuduOptions.CurrentValue;

            var match = all.FirstOrDefault(k =>
                string.Equals(k.Key, appServiceKey, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(match.Key))
                return (false, new KuduConfig(), $"No existe configuración Kudu para appService '{appServiceKey}'.");

            var cfg = match.Value;

            if (string.IsNullOrWhiteSpace(cfg.ScmBaseUrl) ||
                string.IsNullOrWhiteSpace(cfg.Username) ||
                string.IsNullOrWhiteSpace(cfg.Password))
            {
                return (false, new KuduConfig(),
                    $"La configuración Kudu para '{match.Key}' está incompleta (ScmBaseUrl/Username/Password).");
            }

            cfg.ScmBaseUrl = NormalizeBaseUrl(cfg.ScmBaseUrl);
            return (true, cfg, "");
        }

        public HttpClient CreateAuthedClient(KuduConfig cfg)
        {
            var client = _httpClientFactory.CreateClient();
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{cfg.Username}:{cfg.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            return client;
        }

        public string CombineUrl(string baseUrl, string relativeOrAbsolute)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
                return baseUrl;

            // si ya viene absoluto (ej output_url de triggered), lo devolvemos tal cual
            if (relativeOrAbsolute.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                relativeOrAbsolute.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return relativeOrAbsolute;

            baseUrl = baseUrl.TrimEnd('/');
            relativeOrAbsolute = relativeOrAbsolute.TrimStart('/');
            return $"{baseUrl}/{relativeOrAbsolute}";
        }

        public string NormalizeBaseUrl(string scmBaseUrl)
        {
            scmBaseUrl = scmBaseUrl.Trim().Replace(":443", "").TrimEnd('/');

            if (!scmBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !scmBaseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                scmBaseUrl = "https://" + scmBaseUrl;
            }
            return scmBaseUrl;
        }
    }
}
