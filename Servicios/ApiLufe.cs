using Api.Web.Dynamics365.Clases;
using RestSharp;
using RestSharp.Authenticators;

namespace Api.Web.Dynamics365.Servicios
{
    public class ApiLufe
    {
        public string apiKey { get; set; }
        public async Task<string> GetEntidad(string cliente, long cuit, string apikey)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new("https://legajounicoapi.produccion.gob.ar/lufe/");
                RestRequest request = new($"entidades/{cuit}", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("apikey", apikey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }
                else
                {
                    throw new Exception(response.Content);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar documentos de signatura | Descripción: " + ex.Message);
                throw;
            }
        }
        public async Task<string> GetAutoridades(string cliente, long cuit, string apikey)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new("https://legajounicoapi.produccion.gob.ar/lufe/");
                RestRequest request = new($"entidades/{cuit}/autoridades", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("apikey", apikey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }
                else
                {
                    throw new Exception(response.Content);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar documentos de signatura | Descripción: " + ex.Message);
                throw;
            }
        }
        public async Task<string> GetDocumentos(string cliente, long cuit, string apikey)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new("https://legajounicoapi.produccion.gob.ar/lufe/");
                RestRequest request = new($"entidades/{cuit}/documentos", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("apikey", apikey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar documentos de signatura | Descripción: " + ex.Message);
                throw;
            }
        }
        public async Task<string> GetDocumentosPorPeriodo(string cliente, long cuit, string periodo, string apikey)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new("https://legajounicoapi.produccion.gob.ar/lufe/");
                RestRequest request = new($"entidades/{cuit}/periodos/{periodo}/documentos", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("apikey", apikey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar documentos de signatura | Descripción: " + ex.Message);
                throw;
            }
        }
        public async Task<string> GetIndicadores(string cliente, long cuit, string apikey)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new("https://legajounicoapi.produccion.gob.ar/lufe/");
                RestRequest request = new($"entidades/{cuit}/indicadores", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("apikey", apikey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar documentos de signatura | Descripción: " + ex.Message);
                throw;
            }
        }
        public async Task<string> GetIndicadoresPostBalance(string cliente, long cuit, string apikey)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new("https://legajounicoapi.produccion.gob.ar/lufe/");
                RestRequest request = new($"entidades/{cuit}/indicadorespostbalance", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("apikey", apikey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar documentos de signatura | Descripción: " + ex.Message);
                throw;
            }
        }
        public async Task<byte[]> GetBase64Document(string cliente, string url)
        {
            try
            {
                string respuesta = string.Empty;
                HttpClient myClien = new();
                myClien.DefaultRequestHeaders.Add("Accept", "application/json");
                myClien.DefaultRequestHeaders.Add("apikey", "yHp2A7m7GVgDK4R6Szv9hS9MXxo0dGPxgA6PdwmR");
                var prueba = myClien.GetByteArrayAsync(url);
                return prueba.Result;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar documentos de signatura | Descripción: " + ex.Message);
                throw;
            }
        }
    }
}
