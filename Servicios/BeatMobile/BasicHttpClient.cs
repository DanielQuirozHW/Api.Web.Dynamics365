using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static Console.SGR.API.ActualizarCuentas.CommonValues;

namespace Console.SGR.API.ActualizarCuentas
{
 /// <summary>
    /// Clase base para implementancion de clientes http
    /// </summary>
    public abstract class BasicHttpClient
    {
        /// <summary>
        /// Timeout en Milisegundos
        /// </summary>
        public virtual int? GlobalTimeout { get; set; }

        /// <summary>
        /// ServerCertificateValidationCallback
        /// Permite aceptar o no los certificados del servidor 
        /// </summary>
        protected virtual bool AcceptCertificateValidationCallback { get; } = true;

        /// <summary>
        /// UseToken
        /// Permite indicar si utiliza Token
        /// </summary>
        protected virtual bool UseToken { get; set; } = false;

        /// <summary>
        /// Content-Type, por defecto JSON
        /// </summary>
        protected virtual string ContentType { get; } = CommonValues.CONTENT_TYPE_JSON;

        /// <summary>
        /// Indica si se utiliza o no autenticacion user/pw
        /// Hacer override a Username/Password en caso de tener autenticacion
        /// </summary>
        protected abstract bool UseBasicAuthentication { get; }

        /// <summary>
        /// Indica el nombre de usuario en caso de tener autenticacion
        /// </summary>
        protected virtual string Username { get; }

        /// <summary>
        /// Indica el password de acceso en caso de tener autenticacion
        /// </summary>
        protected virtual string Password { get; }

        /// <summary>
        /// Token Key
        /// </summary>
        protected virtual string TokenKEY { get; set; }

        /// <summary>
        /// Token Key
        /// </summary>
        protected virtual string TokenValue { get; set; }

        /// <summary>
        /// URl del Serv
        /// </summary>
        protected string URL { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public BasicHttpClient(string pUrl)
        {
            URL = pUrl;
            this.Configure();
        }

        #region POST
        /// <summary>
        /// Ejecuta un POST Http
        /// </summary>
        /// <typeparam name="Req">Type del Request</typeparam>
        /// <typeparam name="Res">Type del Response</typeparam>
        /// <param name="Request">Params</param>
        public virtual Res ExecutePOST<Req, Res>(Req Request, string action)
        where Res : class, new()
        {
            Res ResEN = null;

            try
            {
                var httpWebRequest = this.BuildHttpWebRequest(action, MethodType.POST);

                var jsonReq = JsonConvert.SerializeObject(Request);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonReq);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream newStream = httpResponse.GetResponseStream();
                StreamReader sr = new StreamReader(newStream);
                var result = sr.ReadToEnd();
                ResEN = JsonConvert.DeserializeObject<Res>(result);
                httpResponse.Dispose();
                httpWebRequest = null;

                //dtoAnexo.Json = jsonReq.ToString();
                //dtoAnexo.EscribirJSONenAnexo(dtoAnexo, service);
            }
            catch (Exception e)
            {
                //Excepcion excepcion = new Excepcion();

                ////excepcion.ErrorPersionalizado = "[" + dtoAnexo.NombreAnexo + "] - No se pudo completar la Publicación del Anexo - ";
                //excepcion.ErrorExcepcion = "Excepción: " + e.ToString();
                //excepcion.PublicarExepcion(dtoAnexo, service);
                Environment.Exit(0);
            }

            return ResEN;
        }

        public virtual Res ExecutePOST<Res>(string jsonReq, string action)
        where Res : class, new()
        {
            Res ResEN = null;

            try
            {
                var httpWebRequest = this.BuildHttpWebRequest(action, MethodType.POST);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonReq);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream newStream = httpResponse.GetResponseStream();
                StreamReader sr = new StreamReader(newStream);
                var result = sr.ReadToEnd();
                ResEN = JsonConvert.DeserializeObject<Res>(result);
                httpResponse.Dispose();
                httpWebRequest = null;

                //dtoAnexo.Json = jsonReq.ToString();
                //dtoAnexo.EscribirJSONenAnexo(dtoAnexo, service);
            }
            catch (Exception e)
            {
                //Excepcion excepcion = new Excepcion();

               // excepcion.ErrorPersionalizado = "[" + dtoAnexo.NombreAnexo + "] - No se pudo completar la Publicación del Anexo - ";
                //excepcion.ErrorExcepcion = "Excepción: " + e.ToString();
                //excepcion.PublicarExepcion(dtoAnexo, service);
                Environment.Exit(0);
            }

            return ResEN;
        }

        public virtual string ExecutePOSTReturnString<Req>(Req Request, string action)
        {
            var httpWebRequest = this.BuildHttpWebRequest(action, MethodType.POST);

            AddActionToHttpWebRequestPostBuild(httpWebRequest);

            var jsonReq = JsonConvert.SerializeObject(Request);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonReq);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream newStream = httpResponse.GetResponseStream();
            StreamReader sr = new StreamReader(newStream);
            var result = sr.ReadToEnd();
            httpResponse.Dispose();
            httpWebRequest = null;

            return result;
        }
        #endregion

        #region GET
        /// <summary>
        /// Ejecuta un GET Http
        /// </summary>
        /// <typeparam name="Req">Type del Request</typeparam>
        /// <typeparam name="Res">Type del Response</typeparam>
        /// <param name="Request">Params</param>
        public virtual Res ExecuteGET<Req, Res>(Req Request, string action)
        where Res : class, new()
        {
            var Properties = Request.GetType().GetProperties();

            // -- Agregamos a queryString los valores
            StringBuilder queryStringBuilder = new StringBuilder();
            foreach (var prop in Properties)
            {
                queryStringBuilder.Append(prop.GetValue(Request).ToString());
            }

            var fullActionWithParams = action + queryStringBuilder;
            var httpWebRequest = this.BuildHttpWebRequest(fullActionWithParams, MethodType.GET);

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream newStream = httpResponse.GetResponseStream();
            StreamReader sr = new StreamReader(newStream);
            var result = sr.ReadToEnd();
            var ResEN = JsonConvert.DeserializeObject<Res>(result);
            httpResponse.Dispose();
            httpWebRequest = null;

            return ResEN;
        }

        /// <summary>
        /// Ejecuta un GET Http - Cotizacion Dolar
        /// </summary>
        /// <typeparam name="Req">Type del Request</typeparam>
        /// <typeparam name="Res">Type del Response</typeparam>
        /// <param name="Request">Params</param>
        public virtual Res ExecuteGETCotizacionDolar<Req, Res>(Req Request, string action)
        where Res : class, new()
        {
            Res ResEN = null;

            try
            {
                var Properties = Request.GetType().GetProperties();

                // -- Agregamos a queryString los valores
                StringBuilder queryStringBuilder = new StringBuilder();
                foreach (var prop in Properties)
                {
                    queryStringBuilder.Append(prop.GetValue(Request).ToString());
                }

                var fullActionWithParams = action + queryStringBuilder;
                var httpWebRequest = this.BuildHttpWebRequest(fullActionWithParams, MethodType.GET);

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream newStream = httpResponse.GetResponseStream();
                StreamReader sr = new StreamReader(newStream);
                var result = sr.ReadToEnd();
                ResEN = JsonConvert.DeserializeObject<Res>(result);
                httpResponse.Dispose();
                httpWebRequest = null;
            }
            catch (Exception e)
            {
                //Excepcion excepcion = new Excepcion();
                //System.Console.WriteLine("No se pudo obtener datos del CUIT");
                ////excepcion.ErrorPersionalizado = "[" + dtoAnexo.NombreAnexo + "] - No se pudo obtener Cotización del Dolar - ";
                //excepcion.ErrorExcepcion = "Excepción: " + e.ToString();
                //excepcion.PublicarExepcion(dtoAnexo, service);
                Environment.Exit(0);
            }

            return ResEN;
        }

        public virtual Res ExecuteGETAOperationStatus<Req, Res>(Req Request, string action)
        where Res : class, new()
        {
            Res ResEN = null;

            try
            {
                var Properties = Request.GetType().GetProperties();

                // -- Agregamos a queryString los valores
                StringBuilder queryStringBuilder = new StringBuilder();
                foreach (var prop in Properties)
                {
                    queryStringBuilder.Append(prop.GetValue(Request).ToString());
                }

                var fullActionWithParams = action + queryStringBuilder;
                var httpWebRequest = this.BuildHttpWebRequest(fullActionWithParams, MethodType.GET);

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream newStream = httpResponse.GetResponseStream();
                StreamReader sr = new StreamReader(newStream);
                var result = sr.ReadToEnd();
                ResEN = JsonConvert.DeserializeObject<Res>(result);
                httpResponse.Dispose();
                httpWebRequest = null;
            }
            catch (Exception e)
            {
                //Excepcion excepcion = new Excepcion();

                ////excepcion.ErrorPersionalizado = "[" + dtoAnexo.NombreAnexo + "] - No se pudo obtener el Estado de la Publicación del Anexo - ";
                //excepcion.ErrorExcepcion = "Excepción: " + e.ToString();
                //excepcion.PublicarExepcion(dtoAnexo, service);
                Environment.Exit(0);
            }

            return ResEN;
        }

        /// <summary>
        /// Ejecuta un GET Http (retorna string)
        /// </summary>
        /// <typeparam name="Req">Type del Request</typeparam>
        public virtual string ExecuteGET<Req>(Req Request, string action)
        {
            var Properties = Request.GetType().GetProperties();

            // -- Agregamos a queryString los valores
            StringBuilder queryStringBuilder = new StringBuilder("?");
            foreach (var prop in Properties)
            {
                queryStringBuilder.Append($"{prop.Name}={prop.GetValue(Request).ToString()}&");
            }

            var fullActionWithParams = action + queryStringBuilder;
            var httpWebRequest = this.BuildHttpWebRequest(fullActionWithParams, MethodType.GET);

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream newStream = httpResponse.GetResponseStream();
            StreamReader sr = new StreamReader(newStream);
            var result = sr.ReadToEnd();

            httpResponse.Dispose();
            httpWebRequest = null;

            return result;
        }

        /// <summary>
        /// Ejecuta un GET Http (Sin parametros)
        /// </summary>
        /// <typeparam name="Req">Type del Request</typeparam>
        /// <typeparam name="Res">Type del Response</typeparam>
        /// <param name="Request">Params</param>
        public virtual Res ExecuteGET<Res>(string action)
        where Res : class, new()
        {
            var fullActionWithParams = action;
            var httpWebRequest = this.BuildHttpWebRequest(fullActionWithParams, MethodType.GET);

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream newStream = httpResponse.GetResponseStream();
            StreamReader sr = new StreamReader(newStream);
            var result = sr.ReadToEnd();
            var ResEN = JsonConvert.DeserializeObject<Res>(result);
            httpResponse.Dispose();
            httpWebRequest = null;

            return ResEN;
        }
        #endregion

        /// <summary>
        /// Configura el contexto http
        /// </summary>
        private void Configure()
        {
            if (AcceptCertificateValidationCallback)
                ServicePointManager.ServerCertificateValidationCallback = (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
        }

        /// <summary>
        /// Retorna un HttpWebRequest
        /// </summary>
        private HttpWebRequest BuildHttpWebRequest(string action, MethodType method)
        {
            string fullUrl = string.Format("{0}{1}", this.URL, action);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(fullUrl);
            httpWebRequest.ContentType = this.ContentType;
            httpWebRequest.Method = method.ToString();

            if (this.GlobalTimeout.HasValue)
                httpWebRequest.Timeout = GlobalTimeout.Value;

            // -- Agrega Header de autenticacion en caso de requerirlo
            if (this.UseBasicAuthentication)
            {
                string authInfo = string.Format("{0}:{1}", this.Username, this.Password);
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                httpWebRequest.Headers.Add("Authorization", authInfo);
            }

            // -- Uso de Token
            if (this.UseToken)
            {
                this.AddTokenValues(httpWebRequest.Headers);
            }

            // -- Agregar otro tipo de headers
            this.AddOtherHeaders(httpWebRequest.Headers);

            return httpWebRequest;
        }

        /// <summary>
        /// Override en caso de requerir agregar otros encabezados especificos
        /// </summary>
        /// <param name="Header">Header del Request Http</param>
        protected virtual void AddOtherHeaders(WebHeaderCollection Header)
        {

        }

        protected virtual void AddActionToHttpWebRequestPostBuild(HttpWebRequest httpWRequest)
        {

        }

        /// <summary>
        /// Agrega el Token
        /// </summary>
        protected virtual void AddTokenValues(WebHeaderCollection Header)
        {
            if (!string.IsNullOrEmpty(this.TokenKEY) && !string.IsNullOrEmpty(this.TokenValue))
                Header.Add(this.TokenKEY, this.TokenValue);
        }

    }

    public static class CommonValues
    {

        /// <summary>
        /// Tipo de contenido JSON
        /// </summary>
        public const string CONTENT_TYPE_JSON = "application/json";

        /// <summary>
        /// Methods HttpRequests
        /// </summary>
        public enum MethodType
        {
            GET,
            POST
        }
    }
}
