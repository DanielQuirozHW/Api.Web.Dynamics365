using System;
using System.Net;
using System.Text;

namespace Console.SGR.API.ActualizarCuentas.Api
{   
        public class BeatMobileHttpClient : BasicHttpClient
        {
            /// <summary>
            /// Indica si se debe habilitar o no el token de authorizacion (X ej en Login no se utiliza)
            /// </summary>
            public bool EnableTokenAuthorizationToRequest { get; set; } = false;

            /// <summary>
            /// Valor del Token en caso de tenerlo
            /// </summary>
            public string TokenAuthorizationValue { get; set; }

            public BeatMobileHttpClient(string Url) : base(Url)
            {

            }

            protected override bool UseBasicAuthentication
            {
                get
                {
                    return false;
                }
            }

            protected override void AddOtherHeaders(WebHeaderCollection Header)
            {
                if (EnableTokenAuthorizationToRequest)
                {
                    Header.Set("Authorization", $"Bearer {TokenAuthorizationValue}");
                }
            }

            protected override void AddActionToHttpWebRequestPostBuild(HttpWebRequest httpWRequest)
            {
                if (EnableTokenAuthorizationToRequest)
                {
                    httpWRequest.KeepAlive = true;
                    httpWRequest.PreAuthenticate = true;
                }
            }
        }
}
