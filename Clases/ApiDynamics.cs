using Api.Web.Dynamics365.Models;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static Api.Web.Dynamics365.Models.Excepcion;

namespace Api.Web.Dynamics365.Clases
{
    public class ApiDynamics
    {
        public string EntityName { get; set; }
        public string Attributes { get; set; }
        public string Filter { get; set; }
        public string FetchXML { get; set; }

        public static string FetchTemplate = "<fetch version='1.0' page='{0}'>'{1}'</fetch>";

        public static string FetchTemplatePaginado = "<fetch version='1.0' page='{0}' paging-cookie='{1}'>'{2}'</fetch>";
        public string CreateRecord(string entityName, JObject entity, Credenciales credenciales)
        {
            HttpMessageHandler messageHandler;
            HttpResponseMessage mesaage;
            Errores excepciones;
            string id = string.Empty;
            
            try
            {
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());

                    using (HttpClient client = new HttpClient(messageHandler))
                    {
                        client.BaseAddress = new Uri(credenciales.url);
                        client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Post, $"api/data/v9.0/{entityName}");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        createRequest.Content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json");
                        mesaage = client.SendAsync(createRequest).Result; 

                        if (mesaage.IsSuccessStatusCode)
                        {
                            string uri = mesaage.Headers.Location.AbsoluteUri;
                            string[] uriSplit = uri.Split('(');
                            id = uriSplit[1].Replace(')', ' ').Trim();
                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = mesaage.Content.ReadAsStringAsync().Result;
                            excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                            if (excepciones != null)
                                error = excepciones.error.message;
                            else
                                error = "error en create";

                            throw new Exception(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Excepciones(credenciales.cliente, "Exepción al crear en la entidad " + entityName + "  : " + ex.Message);
                return "ERROR";
            }

            return id;
        }
        public string NewCreateRecord(string entityName, JObject entity, Credenciales credenciales)
        {
            HttpMessageHandler messageHandler;
            HttpResponseMessage mesaage;
            Errores excepciones;
            string id = string.Empty;

            try
            {
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());

                    using (HttpClient client = new HttpClient(messageHandler))
                    {
                        client.BaseAddress = new Uri(credenciales.url);
                        client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Post, $"api/data/v9.0/{entityName}");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        createRequest.Content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json");
                        mesaage = client.SendAsync(createRequest).Result;

                        if (mesaage.IsSuccessStatusCode)
                        {
                            string uri = mesaage.Headers.Location.AbsoluteUri;
                            string[] uriSplit = uri.Split('(');
                            id = uriSplit[1].Replace(')', ' ').Trim();
                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = mesaage.Content.ReadAsStringAsync().Result;
                            excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                            if (excepciones != null)
                                error = excepciones.error.message;
                            else
                                error = "error en create";

                            throw new Exception(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Excepciones(credenciales.cliente, "Exepción al crear en la entidad " + entityName + "  : " + ex.Message);
                return ex.Message;
            }

            return id;
        }
        public string UpdateRecord(string entityName, string entityId, JObject entity, Credenciales credenciales)
        {
            HttpMessageHandler messageHandler;
            HttpResponseMessage mesaage;
            Errores excepciones;
            string id = string.Empty;

            try
            {
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());

                    using (HttpClient client = new HttpClient(messageHandler))
                    {
                        client.BaseAddress = new Uri(credenciales.url);
                        client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpRequestMessage createRequest = new HttpRequestMessage(new HttpMethod("PATCH"), $"api/data/v9.0/{entityName}({entityId})");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        createRequest.Content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json");
                        mesaage = client.SendAsync(createRequest).Result;

                        if (mesaage.IsSuccessStatusCode)
                        {
                            string uri = mesaage.Headers.Location.AbsoluteUri;
                            string[] uriSplit = uri.Split('(');
                            id = uriSplit[1].Replace(')', ' ').Trim();
                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = mesaage.Content.ReadAsStringAsync().Result;
                            excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                            if (excepciones != null)
                                error = excepciones.error.message;
                            else
                                error = "error en create";

                            throw new Exception(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Excepciones excepciones1 = new Excepciones(credenciales.cliente, "Error ante la actualización dn la entidad: " + EntityName + "| Descripción: " + ex.Message);
                return "ERROR";
            }

            return "EXITO";
        }
        public string DeleteRecord(string entityName, string entityId, Credenciales credenciales)
        {
            HttpMessageHandler messageHandler;
            HttpResponseMessage mesaage;
            Errores excepciones;

            string id = string.Empty;
            try
            {
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());


                    using (HttpClient client = new HttpClient(messageHandler))
                    {
                        client.BaseAddress = new Uri(credenciales.url);
                        client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpRequestMessage createRequest = new HttpRequestMessage(new HttpMethod("DELETE"), $"api/data/v9.0/{entityName}({entityId})");
                        mesaage = client.SendAsync(createRequest).Result;

                        if (mesaage.IsSuccessStatusCode)
                        {
                            id = "Registro eliminado";
                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = mesaage.Content.ReadAsStringAsync().Result;
                            excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                            if (excepciones != null)
                                error = excepciones.error.message;
                            else
                                error = "error en create";

                            throw new Exception(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Excepciones(credenciales.cliente, "Error ante la eliminación dn la entidad: " + EntityName + "| Descripción: " + ex.Message);
                return "ERROR";
            }

            return "EXITO";
        }
        public JArray RetrieveMultipleWithFetch(ApiDynamics api, Credenciales credenciales)
        {
            string consulta = string.Empty;
            JObject respuesta;
            JArray body = null;
            HttpMessageHandler messageHandler;
            Errores excepciones;

            try
            {
                //Obtener connection string para las credenciales de Autenticacion 
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());
                    //Crear mensaje HTTP client para enviar peticion al CRM Web service.  
                    using (HttpClient client = new HttpClient(messageHandler))
                    {
                        //Especificar la direccion Web API del servicio y el periodo de tiempo en que se debe ejecutar.  
                        client.BaseAddress = new Uri(credenciales.url);
                        client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        client.DefaultRequestHeaders.Add("Odata.maxpagesize", "10");
                        client.DefaultRequestHeaders.Add("Odata.nextLink", "true");
                        client.DefaultRequestHeaders.Add("charset", "utf-8");
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        //string fetch = String.Format(FetchTemplate, 1, ' ', api.FetchXML);
                        
                        consulta += api.EntityName;
                        if (api.FetchXML != null) consulta += $"?fetchXml={api.FetchXML}";
                        HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Get, $"api/data/v9.2/{consulta}");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        var response = client.SendAsync(createRequest).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            respuesta = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                            var valor = respuesta["value"];
                            string resultado = JsonConvert.SerializeObject(valor);
                            body = JsonConvert.DeserializeObject<dynamic>(resultado);
                            var nextLink = respuesta["@odata.nextLink"];
                            //var nextLink = respuesta["@Microsoft.Dynamics.CRM.fetchxmlpagingcookie"];
                            while (nextLink != null)
                            {
                                var responseNext = client.GetAsync(nextLink.ToString(),
                                HttpCompletionOption.ResponseHeadersRead).Result;
                                if (responseNext.IsSuccessStatusCode)
                                {
                                    JObject respuestaNext = JObject.Parse(responseNext.Content.ReadAsStringAsync().Result);
                                    var valorNext = respuestaNext["value"];
                                    string resultadoNext = JsonConvert.SerializeObject(valorNext);
                                    dynamic obj = JsonConvert.DeserializeObject<dynamic>(resultadoNext);
                                    body.Merge(obj);
                                    nextLink = respuestaNext["@odata.nextLink"];
                                }
                                else
                                {
                                    throw new Exception(responseNext.ReasonPhrase);
                                }
                            }
                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = response.Content.ReadAsStringAsync().Result;
                            excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                            if (excepciones != null)
                                error = excepciones.error.message;
                            else
                                error = "error en Retrieve";

                            throw new Exception(error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new Excepciones(credenciales.cliente, "Error ante el RetrieveMultipleFetch de la entidad: " + EntityName + "| Descripción: " + ex.Message);
                throw;
            }

            return body;
        }
        public JArray RetrieveMultipleAsync(ApiDynamics api, Credenciales credenciales)
        {
            JObject respuesta;
            string consulta = string.Empty;
            HttpMessageHandler messageHandler;
            Errores excepciones;
            JArray body = null;

            try
            {
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());

                    consulta += api.EntityName;
                    if (api.Filter != null) consulta += "?$filter=" + api.Filter;

                    using (HttpClient client = new HttpClient(messageHandler))
                    {
                        client.BaseAddress = new Uri(credenciales.url);
                        client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                        client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                        client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                        client.DefaultRequestHeaders.Add("Odata.maxpagesize", "10");
                        client.DefaultRequestHeaders.Add("Odata.nextLink", "true");
                        client.DefaultRequestHeaders.Add("charset", "utf-8");
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Get, $"api/data/v9.0/{consulta}");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        var response = client.SendAsync(createRequest).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            respuesta = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                            var valor = respuesta["value"];
                            string resultado = JsonConvert.SerializeObject(valor);
                            body = JsonConvert.DeserializeObject<dynamic>(resultado);
                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = response.Content.ReadAsStringAsync().Result;
                            excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                            if (excepciones != null)
                                error = excepciones.error.message;
                            else
                                error = "error en Retrieve";

                            throw new Exception(error);
                        }
                    }
                }

                return body;
            }
            catch (Exception ex)
            {
                new Excepciones(credenciales.cliente, "Error ante el retrieve de la entidad: " + EntityName + "| Descripción: " + ex.Message);
                throw;
            }
        }
    }
}
