using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;

namespace Api.Web.Dynamics365.Clases
{
    public class ApiToken : DelegatingHandler
    {
        private ClientCredential clientCrendential;
        AuthenticationContext authContext;
        private string resource;

        public ApiToken(string clientId, string clientSecret, string contextId, string resource,
            HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.clientCrendential = new ClientCredential(clientId, clientSecret);
            this.authContext = new AuthenticationContext($"https://login.microsoftonline.com/{contextId}");
            this.resource = resource;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
                    authContext.AcquireTokenAsync(resource, clientCrendential).Result.AccessToken);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
