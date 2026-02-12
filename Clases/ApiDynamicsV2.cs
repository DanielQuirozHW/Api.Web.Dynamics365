using Api.Web.Dynamics365.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using static Api.Web.Dynamics365.Clases.Errores;
using System;
using System.Net;
using System.Web;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Wordprocessing;
using PuppeteerSharp;
using static Api.Web.Dynamics365.Controllers.SgrOneClickController;
using Azure;
using Microsoft.IdentityModel.Tokens;
using Api.Web.Dynamics365.Servicios;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using static Google.Apis.Requests.BatchRequest;
using System.Text.Json;

namespace Api.Web.Dynamics365.Clases
{
    public class ApiDynamicsV2
    {
        private readonly IErrorLogService _errorLogService;

        private readonly string _url;

        private readonly string _jsonBody;
        public string EntityName { get; set; }
        public string Attributes { get; set; }
        public string Filter { get; set; }
        public string FetchXML { get; set; }

        private static readonly string FetchTemplate = "<fetch page='{0}' paging-cookie=''  mapping='logical' output-format='xml-platform' version='1.0' distinct='false'>'{1}'</fetch>";

        private static readonly string FetchTemplatePaginado = "<fetch page='{0}' mapping='logical' output-format='xml-platform' version='1.0' distinct='false' paging-cookie='{1}'>'{2}'</fetch>";
        public ApiDynamicsV2()
        {

        }
        public ApiDynamicsV2(IErrorLogService errorLogService, string url = null, string jsonBody = null)
        {
            _errorLogService = errorLogService;
            _url = url;
            _jsonBody = jsonBody;
        }
        public async Task<ResponseAPI> CreateRecord(string entityName, JObject entity, Credenciales credenciales)
        {
            ResponseAPI response = new();
            HttpMessageHandler messageHandler;
            HttpResponseMessage mesaage;
            Errores excepciones;
            Excepciones excepcion = new();

            try
            {
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());

                    using HttpClient client = new(messageHandler);
                    client.BaseAddress = new Uri(credenciales.url);
                    client.Timeout = new TimeSpan(0, 10, 0);  //10 minutes
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpRequestMessage createRequest = new(HttpMethod.Post, $"api/data/v9.0/{entityName}");
                    createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                    createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                    createRequest.Content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json");
                    mesaage = client.SendAsync(createRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                    //mesaage = client.SendAsync(createRequest).Result;
                    response.codigo = (int)mesaage.StatusCode;
                    response.ok = mesaage.IsSuccessStatusCode;

                    if (mesaage.IsSuccessStatusCode)
                    {
                        string uri = mesaage.Headers.Location.AbsoluteUri;
                        string[] uriSplit = uri.Split('(');
                        string id = uriSplit[1].Replace(')', ' ').Trim();
                        response.codigo = (int)mesaage.StatusCode;
                        response.descripcion = id;
                    }
                    else
                    {
                        string error = string.Empty;
                        string resultado = mesaage.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                        if (excepciones != null)
                            error = excepciones.error.message;
                        else
                            error = "Error en create";

                        throw new Exception(error);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_errorLogService != null)
                {
                    await _errorLogService.CreateErrorLogAsync(new ErrorLog
                    {
                        Level = "Error",
                        Message = $"Exepción al crear en la entidad {entityName}",
                        ExceptionDetails = _jsonBody,
                        Url = _url,
                        Source = credenciales.cliente,
                        StackTrace = ex.Message,
                    });
                }
                else
                {
                    await excepcion.CrearExcepcion("Error en metodo CreateRecord", credenciales.cliente, "Exepción al crear en la entidad " + entityName + "  : " + ex.Message);
                }
                response.descripcion = ex.Message;
                return response;
            }

            return response;
        }
        public async Task<ResponseAPI> UpdateRecord(string entityName, string entityId, JObject entity, Credenciales credenciales)
        {
            ResponseAPI response = new();
            HttpMessageHandler messageHandler;
            HttpResponseMessage mesaage;
            Errores excepciones;
            Excepciones excepcion = new();

            try
            {
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());

                    using HttpClient client = new HttpClient(messageHandler);
                    client.BaseAddress = new Uri(credenciales.url);
                    client.Timeout = new TimeSpan(0, 10, 0);  //10 minutes
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpRequestMessage createRequest = new HttpRequestMessage(new HttpMethod("PATCH"), $"api/data/v9.0/{entityName}({entityId})");
                    createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                    createRequest.Content = new StringContent(entity.ToString(), Encoding.UTF8, "application/json");
                    mesaage = client.SendAsync(createRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                    response.codigo = (int)mesaage.StatusCode;
                    response.ok = mesaage.IsSuccessStatusCode;

                    if (mesaage.IsSuccessStatusCode)
                    {
                        string uri = mesaage.Headers.Location.AbsoluteUri;
                        string[] uriSplit = uri.Split('(');
                        string id = uriSplit[1].Replace(')', ' ').Trim();
                        response.codigo = (int)mesaage.StatusCode;
                        response.descripcion = id;
                    }
                    else
                    {
                        string error = string.Empty;
                        string resultado = mesaage.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                        if (excepciones != null)
                            error = excepciones.error.message;
                        else
                            error = "Error en update";

                        throw new Exception(error);
                    }
                }
            }
            catch (Exception ex)
            {
                if(_errorLogService != null)
                {
                    await _errorLogService.CreateErrorLogAsync(new ErrorLog
                    {
                        Level = "Error",
                        Message = $"Exepción al actualizar en la entidad {entityName} - id: {entityId}",
                        ExceptionDetails = _jsonBody,
                        Url = _url,
                        Source = credenciales.cliente,
                        StackTrace = ex.Message,
                    });
                }
                else
                {
                    await excepcion.CrearExcepcion("Error en metodo Update Record", credenciales.cliente, "Exepción al actualizar en la entidad " + entityName + "  : " + ex.Message);
                }
                
                response.descripcion = ex.Message;
                return response;
            }

            return response;
        }
        public async Task<JArray> RetrieveMultipleWithFetch(ApiDynamicsV2 api, Credenciales credenciales)
        {
            string consulta = string.Empty;
            int page = 1;
            JArray body = null;
            HttpMessageHandler messageHandler;
            Errores excepciones;
            Excepciones excepcion = new();
            FetchXML fetchXml = new();

            try
            {
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());

                    using HttpClient client = new(messageHandler);

                    client.BaseAddress = new Uri(credenciales.url);
                    client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    client.DefaultRequestHeaders.Add("Odata.maxpagesize", "10");
                    client.DefaultRequestHeaders.Add("Odata.nextLink", "true");
                    client.DefaultRequestHeaders.Add("charset", "utf-8");
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    string fetch = string.Format(FetchTemplate, page, api.FetchXML);
                    if (fetch.Contains("documentbody"))
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(fetch);

                        var node = xmlDoc.SelectSingleNode("//condition[@attribute='annotationid']");
                        string annotationId = node?.Attributes["value"]?.Value;

                        if (fetch != null)
                        {
                            if (!string.IsNullOrEmpty(annotationId))
                            {
                                consulta += $"/{api.EntityName}({annotationId})";
                            }
                            else
                            {
                                consulta += $"/{api.EntityName}";
                            }
                        }

                        var attributeNodes = xmlDoc.SelectNodes("//entity[@name='annotation']/attribute");

                        List<string> atributos = new();

                        foreach (XmlNode attr in attributeNodes)
                        {
                            string attrName = attr.Attributes["name"].Value;
                            atributos.Add(attrName);
                        }

                        if(atributos.Count > 0)
                        {
                            consulta += "?$select=" + string.Join(",", atributos);
                        }   

                        HttpRequestMessage createRequest2 = new(HttpMethod.Get, $"api/data/v9.2/{consulta}");
                        createRequest2.Headers.Add("Prefer", "odata.include-annotations=*");
                        var response2 = client.SendAsync(createRequest2).ConfigureAwait(false).GetAwaiter().GetResult();

                        if (response2.IsSuccessStatusCode)
                        {
                            var json = await response2.Content.ReadAsStringAsync();
                            using var doc = JsonDocument.Parse(json);
                            var base64 = doc.RootElement.GetProperty("documentbody").GetString();
                           
                            fetch = WebUtility.UrlEncode(fetch);
                            consulta = string.Empty;
                            consulta += api.EntityName;
                            if (fetch != null) consulta += $"?fetchXml={fetch}";

                            HttpRequestMessage createRequest = new(HttpMethod.Get, $"api/data/v9.2/{consulta}");
                            createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                            var response = client.SendAsync(createRequest).ConfigureAwait(false).GetAwaiter().GetResult();

                            if (response.IsSuccessStatusCode)
                            {
                                fetchXml = JsonConvert.DeserializeObject<FetchXML>(response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
                                body = fetchXml.Value;
                                var item = body.FirstOrDefault();
                                if (item != null)
                                {
                                    item["documentbody"] = base64;
                                }
                                return body;
                            }
                            else
                            {
                                string error = string.Empty;
                                string resultado = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                                excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                                if (excepciones != null)
                                    error = excepciones.error.message;
                                else
                                    error = "error en Retrieve";

                                throw new Exception(error);
                            }
                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = response2.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                            excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                            if (excepciones != null)
                                error = excepciones.error.message;
                            else
                                error = "error en Retrieve";

                            throw new Exception(error);
                        }
                    }
                    else
                    {
                        fetch = WebUtility.UrlEncode(fetch);
                        consulta += api.EntityName;
                        if (fetch != null) consulta += $"?fetchXml={fetch}";

                        HttpRequestMessage createRequest = new(HttpMethod.Get, $"api/data/v9.2/{consulta}");
                        createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                        var response = client.SendAsync(createRequest).ConfigureAwait(false).GetAwaiter().GetResult();

                        if (response.IsSuccessStatusCode)
                        {
                            var pagina = false;
                            fetchXml = JsonConvert.DeserializeObject<FetchXML>(response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
                            body = fetchXml.Value;
                            pagina = fetchXml.Morerecords;
                            //while (!string.IsNullOrEmpty(fetchXml.Fetchxmlpagingcookie))
                            while (pagina)
                            {
                                page++;
                                FetchXML fetchXml2 = await PagignCookie(api.FetchXML, api.EntityName, page, fetchXml.Fetchxmlpagingcookie, credenciales);
                                body.Merge(fetchXml2.Value);
                                pagina = fetchXml2.Morerecords;
                            }
                        }
                        else
                        {
                            string error = string.Empty;
                            string resultado = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
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
                if (_errorLogService != null)
                {
                    await _errorLogService.CreateErrorLogAsync(new ErrorLog
                    {
                        Level = "Error",
                        Message = $"Error en metodo RetrieveMultiple",
                        ExceptionDetails = consulta,
                        Url = _url,
                        Source = credenciales.cliente,
                        StackTrace = ex.Message,
                    });
                }
                else
                {
                    await excepcion.CrearExcepcion("Error en metodo RetrieveMultiple", credenciales.cliente, "Entidad " + api.EntityName + "  : " + ex.Message);
                }
            }

            return body;
        }
        public async Task<FetchXML> PagignCookie(string fetchXML, string entityName, int page, string paging, Credenciales credenciales)
        {
            FetchXML fetchXml = new();
            HttpMessageHandler messageHandler;
            Errores excepciones;
            Excepciones excepcion = new();

            try
            {
                var Fetchxmlpagingcookie = HttpUtility.HtmlDecode(paging);
                XDocument xdoc = XDocument.Parse(Fetchxmlpagingcookie);
                string pagingCookie = xdoc.Root?.Attribute("pagingcookie")?.Value;
                pagingCookie = HttpUtility.UrlDecode(pagingCookie);
                pagingCookie = HttpUtility.UrlDecode(pagingCookie);
                pagingCookie = HttpUtility.HtmlEncode(pagingCookie);
                string fetchPagin = string.Format(FetchTemplatePaginado, page, pagingCookie, fetchXML);
                string fetch = WebUtility.UrlEncode(fetchPagin);
                string consulta = entityName;
                if (fetch != null) consulta += $"?fetchXml={fetch}";

                messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());

                using HttpClient client = new(messageHandler);

                client.BaseAddress = new Uri(credenciales.url);
                client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                client.DefaultRequestHeaders.Add("Odata.maxpagesize", "10");
                client.DefaultRequestHeaders.Add("Odata.nextLink", "true");
                client.DefaultRequestHeaders.Add("charset", "utf-8");
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpRequestMessage createRequest = new(HttpMethod.Get, $"api/data/v9.2/{consulta}");
                createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                var responseNext = client.SendAsync(createRequest).ConfigureAwait(false).GetAwaiter().GetResult();

                if (responseNext.IsSuccessStatusCode)
                {
                    fetchXml = JsonConvert.DeserializeObject<FetchXML>(responseNext.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
                }
                else 
                {
                    string error = string.Empty;
                    string resultado = responseNext.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

                    if (excepciones != null)
                        error = excepciones.error.message;
                    else
                        error = "error en Retrieve";

                    throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                if (_errorLogService != null)
                {
                    await _errorLogService.CreateErrorLogAsync(new ErrorLog
                    {
                        Level = "Error",
                        Message = $"Error en metodo RetrieveMultiple PagignCookie",
                        ExceptionDetails = paging,
                        Url = _url,
                        Source = credenciales.cliente,
                        StackTrace = ex.Message,
                    });
                }
                else
                {
                    await excepcion.CrearExcepcion("Error en metodo RetrieveMultiple PagignCookie", credenciales.cliente, "Entidad " + entityName + "  : " + ex.Message);
                }   
            }

            return fetchXml;
        }
        public async Task<ResponseAPI> RetrieveMultipleWithFetchV2(ApiDynamicsV2 api, Credenciales credenciales)
        {
            ResponseAPI responseAPI = new();
            string consulta = string.Empty;
            int page = 1;
            HttpMessageHandler messageHandler;
            Errores? excepciones;
            Excepciones excepcion = new();
            FetchXML fetchXml = new();

            try
            {
                if (credenciales != null)
                {
                    messageHandler = new ApiToken(credenciales.clientid, credenciales.clientsecret, credenciales.tenantid, credenciales.url,
                                    new HttpClientHandler());

                    using HttpClient client = new(messageHandler);

                    client.BaseAddress = new Uri(credenciales.url);
                    client.Timeout = new TimeSpan(0, 2, 0);  //2 minutes
                    client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                    client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                    client.DefaultRequestHeaders.Add("Odata.maxpagesize", "10");
                    client.DefaultRequestHeaders.Add("Odata.nextLink", "true");
                    client.DefaultRequestHeaders.Add("charset", "utf-8");
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    string fetch = string.Format(FetchTemplate, page, api.FetchXML);
                    fetch = WebUtility.UrlEncode(fetch);
                    consulta += api.EntityName;
                    if (fetch != null) consulta += $"?fetchXml={fetch}";

                    HttpRequestMessage createRequest = new(HttpMethod.Get, $"api/data/v9.0/{consulta}");
                    createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
                    var response = client.SendAsync(createRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                    responseAPI.codigo = (int)response.StatusCode;
                    responseAPI.ok = response.IsSuccessStatusCode;

                    if (response.IsSuccessStatusCode)
                    {
                        var pagina = false;
                        fetchXml = JsonConvert.DeserializeObject<FetchXML>(response?.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
                        responseAPI.coleccion = fetchXml.Value;
                        pagina = fetchXml.Morerecords;
                        while (pagina)
                        {
                            page++;
                            FetchXML fetchXml2 = await PagignCookie(api.FetchXML, api.EntityName, page, fetchXml.Fetchxmlpagingcookie, credenciales);
                            responseAPI.coleccion.Merge(fetchXml2.Value);
                            pagina = fetchXml2.Morerecords;
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
            catch (Exception ex)
            {
                if (_errorLogService != null)
                {
                    await _errorLogService.CreateErrorLogAsync(new ErrorLog
                    {
                        Level = "Error",
                        Message = $"Error en metodo RetrieveMultiple",
                        ExceptionDetails = consulta,
                        Url = _url,
                        Source = credenciales.cliente,
                        StackTrace = ex.Message,
                    });
                }
                else
                {
                    await excepcion.CrearExcepcion("Error en metodo RetrieveMultiple", credenciales.cliente, "Entidad " + api.EntityName + "  : " + ex.Message);
                }
            }

            return responseAPI;
        }
    }

    //string fetch = String.Format(FetchTemplate, 1, ' ', api.FetchXML);
    //if(api.FetchXML != null && api.FetchXML.Contains("documentbody"))
    //{
    //    string fetch = api.FetchXML;
    //JArray array = await GetDocumentoBodyAsync(api, credenciales, client);
    //return array;

    //private static async Task<JArray> GetDocumentoBodyAsync(ApiDynamics api, Credenciales credenciales, HttpClient client)
    //{
    //    try
    //    {
    //        JArray body = null;
    //        string consulta = string.Empty;
    //        string fetch = api.FetchXML;
    //        var xmlDoc = new XmlDocument();
    //        xmlDoc.LoadXml(fetch);

    //        var node = xmlDoc.SelectSingleNode("//condition[@attribute='annotationid']");
    //        string annotationId = node?.Attributes["value"]?.Value;

    //        if (fetch != null)
    //        {
    //            if (!string.IsNullOrEmpty(annotationId))
    //            {
    //                consulta += $"/{api.EntityName}({annotationId})";
    //            }
    //            else
    //            {
    //                consulta += $"/{api.EntityName}";
    //            }
    //        }

    //        var attributeNodes = xmlDoc.SelectNodes("//entity[@name='annotation']/attribute");

    //        List<string> atributos = new();

    //        foreach (XmlNode attr in attributeNodes)
    //        {
    //            string attrName = attr.Attributes["name"].Value;
    //            atributos.Add(attrName);
    //        }

    //        if (atributos.Count > 0)
    //        {
    //            consulta += "?$select=" + string.Join(",", atributos);
    //        }

    //        HttpRequestMessage createRequest2 = new(HttpMethod.Get, $"api/data/v9.2/{consulta}");
    //        createRequest2.Headers.Add("Prefer", "odata.include-annotations=*");
    //        var response2 = client.SendAsync(createRequest2).Result;

    //        if (response2.IsSuccessStatusCode)
    //        {
    //            var json = await response2.Content.ReadAsStringAsync();
    //            using var doc = JsonDocument.Parse(json);
    //            var base64 = doc.RootElement.GetProperty("documentbody").GetString();

    //            fetch = WebUtility.UrlEncode(fetch);
    //            consulta = string.Empty;
    //            consulta += api.EntityName;
    //            if (fetch != null) consulta += $"?fetchXml={fetch}";

    //            HttpRequestMessage createRequest = new(HttpMethod.Get, $"api/data/v9.2/{consulta}");
    //            createRequest.Headers.Add("Prefer", "odata.include-annotations=*");
    //            var response = client.SendAsync(createRequest).Result;

    //            if (response.IsSuccessStatusCode)
    //            {
    //                FetchXML fetchXml = new();
    //                fetchXml = JsonConvert.DeserializeObject<FetchXML>(response.Content.ReadAsStringAsync().Result);
    //                body = fetchXml.Value;
    //                var item = body.FirstOrDefault();
    //                if (item != null)
    //                {
    //                    item["documentbody"] = base64;
    //                }
    //                return body;
    //            }
    //            else
    //            {
    //                string error = string.Empty;
    //                string resultado = response.Content.ReadAsStringAsync().Result;
    //                //excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

    //                //if (excepciones != null)
    //                //    error = excepciones.error.message;
    //                //else
    //                error = "error en Retrieve";

    //                throw new Exception(error);
    //            }
    //        }
    //        else
    //        {
    //            string error = string.Empty;
    //            string resultado = response2.Content.ReadAsStringAsync().Result;
    //            //excepciones = JsonConvert.DeserializeObject<Errores>(resultado);

    //            //if (excepciones != null)
    //            //    error = excepciones.error.message;
    //            //else
    //            //    error = "error en Retrieve";

    //            throw new Exception(error);
    //        }

    //        return body;
    //    }
    //    catch (Exception)
    //    {

    //        throw;
    //    }
    //}
//}
}
