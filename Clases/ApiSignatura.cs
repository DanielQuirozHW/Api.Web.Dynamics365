using Api.Web.Dynamics365.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using static Api.Web.Dynamics365.Models.Signatura;

namespace Api.Web.Dynamics365.Clases
{
    public class ApiSignatura
    {
        public string apiKey { get; set; }
     
        public async Task<string> getDocumentos(string cliente)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new RestClient("https://connect.signatura.co/api/v2");
                RestRequest request = new RestRequest("documents", Method.Get);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + apiKey);

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

        public async Task<string> GetDocumentDetail(string id, string cliente)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new("https://connect.signatura.co/api/v2");
                RestRequest request = new($"documents/{id}", Method.Get)
                {
                    RequestFormat = RestSharp.DataFormat.Json
                };
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + apiKey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar detalle de documento de signatura | Descripción: " + ex.Message);
                throw;
            }
        }

        public async Task<string> GetSignatureDetail(string id, Credenciales cliente)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new ("https://connect.signatura.co/api/v2");
                RestRequest request = new($"signatures/{id}", Method.Get)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + apiKey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente.cliente, "Error al recuperar detalle de firma de signatura | Descripción: " + ex.Message);
                throw;
            }
        }

        public async Task<string> createDocument(Documents.file documento, Credenciales credenciales, string documentacionporcuenta_id = null, string documentacionporoperacion_id = null)
        {
            try
            {
                ApiDynamics api = new ApiDynamics();
                string mensajeError = string.Empty;
                string respuesta = string.Empty;
                RestClient client = new("https://connect.signatura.co/api/v2");
                RestRequest request = new("documents/create", Method.Post);
                //request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + apiKey);
                request.AddBody(documento);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }
                else if (response.ResponseStatus.ToString() == "Error")
                {
                    Error elemento = JsonConvert.DeserializeObject<Error>(response.Content);
                    string errores = string.Empty;
                    JObject error = new JObject();

                    errores = obtenerDescripcionError(elemento);

                    if (elemento.file_content != null)
                    {
                        mensajeError = "El tipo de archivo es incorrecto. Por favor, corrobore que el documento sea de tipo PDF.";
                    }

                    if (elemento.selected_emails != null)
                    {
                        mensajeError = "El correo electrónico es inválido.";
                    }

                    if (mensajeError == String.Empty)
                    {
                        mensajeError = "El documento no se pudo enviar a signatura. Por favor, pongase en contacto con la mesa de ayuda.";
                    }

                    if(documentacionporcuenta_id != null)
                    {
                        error.Add("new_firmaelectronica", mensajeError);
                        api.UpdateRecord("new_documentacionporcuentas", documentacionporcuenta_id, error, credenciales);
                    }
                    else if(documentacionporoperacion_id != null)
                    {
                        error.Add("new_firmaelectronica", mensajeError);
                        api.UpdateRecord("new_documentacionporoperacions", documentacionporoperacion_id, error, credenciales);
                    }

                    respuesta = "Error";

                    new Excepciones(credenciales.cliente, "Error al crear documento en signatura | Descripción: " + errores);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(credenciales.cliente, "Error al crear documento de signatura | Descripción: " + ex.Message);
                throw;
            }
        }

        public async Task<string> CancelDocument(string id, string cliente)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new ("https://connect.signatura.co/api/v2");
                RestRequest request = new($"/documents/{id}/cancel", Method.Patch)
                {
                    RequestFormat = RestSharp.DataFormat.Json
                };
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + apiKey);
                CancelDocument cancelacionD = new()
                {
                    cancel_reason = "Cancelacion a peticion",
                };
                string canlJson = JsonConvert.SerializeObject(cancelacionD);
                request.AddBody(canlJson);
                //if (!string.IsNullOrEmpty(cancelacion))
                //{
                //    CancelDocument cancelacionD = new()
                //    {
                //        cancel_reason = cancelacion,
                //    };
                //    string canlJson = JsonConvert.SerializeObject(cancelacionD);
                //    request.AddBody(canlJson);
                //}


                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al cancelar documento de signatura | Descripción: " + ex.Message);
                throw;
            }
        }

        public async Task<string> invalidateSignature(Invalidar firma, string cliente)
        {
            try
            {
                InvalidarFirma invalidarFirma = new InvalidarFirma();
                string respuesta = string.Empty;
                RestClient client = new RestClient("https://connect.signatura.co/api/v2");
                RestRequest request = new RestRequest($"signatures/{firma.id}/invalidate", Method.Patch);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + apiKey);

                invalidarFirma.invalidation_reason = firma.invalidation_reason;
                invalidarFirma.notify = firma.notify;
                if (firma.new_signer != null)
                    invalidarFirma.new_signer = firma.new_signer;
                else
                    invalidarFirma.new_signer = String.Empty;
                
                request.AddBody(invalidarFirma);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al invalidar firma de signatura | Descripción: " + ex.Message);
                throw;
            }
        }

        public async Task<string> resendSignature(Reenviar firma, string cliente)
        {
            try
            {
                InvalidarFirma invalidarFirma = new InvalidarFirma();
                string respuesta = string.Empty;
                RestClient client = new RestClient("https://connect.signatura.co/api/v2");
                RestRequest request = new RestRequest($"signatures/{firma.id}/resend-invitation", Method.Post);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + apiKey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al reenviar invitación de signatura | Descripción: " + ex.Message);
                throw;
            }
        }

        public async Task<string> completeDocument(string id, string cliente)
        {
            try
            {
                string respuesta = string.Empty;
                RestClient client = new RestClient("https://connect.signatura.co/api/v2");
                RestRequest request = new RestRequest($"{id}/complete", Method.Patch);
                request.RequestFormat = RestSharp.DataFormat.Json;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + apiKey);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al completar documento de signatura | Descripción: " + ex.Message);
                throw;
            }
        }

        public async Task<string> GetCertificate(string id, string cliente)
        {
            try
            {
                string respuesta = string.Empty;
                var cli = new HttpClient();
                cli.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);
                var response = await cli.GetAsync($"https://connect.signatura.co/api/v2/documents/{id}/download/pdf-certificate");

                if (response.ReasonPhrase != "Not Found")
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    byte[] bytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        bytes = memoryStream.ToArray();
                    }
                    string base64 = Convert.ToBase64String(bytes);
                    respuesta = base64;
                }
                else
                {
                    respuesta = response.ReasonPhrase;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar certificado de signatura | Descripción: " + ex.Message);
                throw;
            }
        }

        public async Task<string> DownloadDocument(string id, string cliente)
        {
            try
            {
                string respuesta = string.Empty;
                var cli = new HttpClient();
                cli.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", apiKey);
                var response = await cli.GetAsync($"https://connect.signatura.co/api/v2/documents/{id}/download/document");

                if (response.ReasonPhrase != "Not Found")
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    byte[] bytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        bytes = memoryStream.ToArray();
                    }
                    string base64 = Convert.ToBase64String(bytes);
                    respuesta = base64;
                }
                else
                {
                    respuesta = response.ReasonPhrase;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                new Excepciones(cliente, "Error al recuperar documento de signatura | Descripción: " + ex.Message);
                throw;
            }
        }

        public static string obtenerDescripcionError(Error error)
        {
            string resultado = string.Empty;

            if (error.file_content != null)
            {
                foreach (var item in error.file_content)
                {
                    resultado += item;
                }
            }

            if (error.validations != null)
            {
                foreach (var item in error.validations)
                {
                    resultado += item;
                }
            }

            if (error.selected_emails != null)
            {
                foreach (var item in error.selected_emails)
                {
                    if(item.Value.Length > 0)
                    {
                        foreach(var email in item.Value)
                        {
                            resultado += email;
                        }
                    }
                    else
                    {
                        resultado += item;
                    }
                }
            }

            if (error.fashion != null)
            {
                foreach (var item in error.fashion)
                {
                    resultado += item;
                }
            }

            return resultado;
        }
    }
}
