using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Console.SGR.API.ActualizarCuentas.Api
{
    class ConexionApi
    {

        public BeatMobileHttpClient Login(string email, string password)
        {
            BeatMobileHttpClient ClientHttp = null;

            try
            {
                // -- Fundamental para que se pueda llevar adelante la ejecucion contra servicio web
                ServicePointManager.Expect100Continue = false;

                // -- 1 Creas la clase BeatMobileHttpClient heredando de la que te pase antes (esta en este proyecto tmb)
                //ClientHttp = new BeatMobileHttpClient("http://sgr.beatmobile.com.ar/api/v1/");
                //LL - 03:57 p. m. 10/10/2019
                //Se cambio la URL a Producción | Presentacion Anexos 09 (15-10)
                //ClientHttp = new BeatMobileHttpClient("http://sgr.beatmobile.com.ar/api/v1.1/");
                //
                ClientHttp = new BeatMobileHttpClient("https://sgr.casfog.com.ar/api/v1.1/");


                // -- 2 Tenes que crear clases que representan el request y el response
                // -- 3 Completas valores de Request
                SgrApiLoginRequest Request = new SgrApiLoginRequest();
                Request.email = email;
                Request.password = password;

                // -- 4 Ejecutamos y como generico le pasamos el tipo del Request y del Response(estructura identica a documentancion)
                var Response = ClientHttp.ExecutePOST<SgrApiLoginRequest, SgrApiModelContainer>(Request, "login");

                // -- Una vez que estamos autenticados le decimos al servicio que use el token
                ClientHttp.EnableTokenAuthorizationToRequest = true;
                ClientHttp.TokenAuthorizationValue = Response.success.token;
            }
            catch (Exception e)
            {
                //Excepcion excepcion = new Excepcion();
                //System.Console.WriteLine("Problemas de Autenticación con la API");
                ////excepcion.ErrorPersionalizado = "[" + dtoAnexo.NombreAnexo + "] - Problemas de Autenticación con la API - ";
                //excepcion.ErrorExcepcion = "Excepción: " + e.ToString();
                //excepcion.PublicarExepcion(dtoAnexo, service);
                Environment.Exit(0);
            }

            return ClientHttp;
        }

        #region Estructura REQUEST
        public class SgrApiLoginRequest
        {
            public string email { get; set; }
            public string password { get; set; }
        }
        #endregion

        #region Estructura RESPONSE
        public class SgrApiModelContainer
        {
            public SgrApiLoginModelResponse success { get; set; } = new SgrApiLoginModelResponse();
        }

        public class SgrApiLoginModelResponse
        {
            public string token { get; set; }
            public SgrApiUserResponse user { get; set; }
        }

        public class SgrApiUserResponse
        {
            public string name { get; set; }
            public string email { get; set; }
        }
        #endregion
    }
}
