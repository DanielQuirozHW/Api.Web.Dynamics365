using RestSharp;

namespace Api.Web.Dynamics365.Servicios
{
    public class Nosis
    {
        public string _usuario { get; set; }
        public string _token { get; set; }
        public string _grupoVariables { get; set; }
        public string _sexo { get; set; }
        public string apiKey { get; set; }

        public string ConsultarPorCUIT(string documento)
        {
            try
            {
                string respuesta = string.Empty;

                RestClient client = new RestClient("https://ws01.nosis.com/rest");
                RestRequest request = new RestRequest("variables", Method.Get);
                request.RequestFormat = DataFormat.Json;
                request.AddParameter("usuario", _usuario);
                request.AddParameter("token", _token);
                request.AddParameter("documento", documento);
                request.AddParameter("VR", _grupoVariables);
                request.AddParameter("Format", "JSON");

                RestResponse response = client.Execute(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }
                else if(response.ResponseStatus.ToString() == "Error")
                { 
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string OnboardingINIT(string documento, string contactid, string urlCallback)
        {
            try
            {
                string respuesta = string.Empty;

                RestClient client = new RestClient("https://onbweb.nosis.com/api/init");
                RestRequest request = new RestRequest("Init", Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("X-API-KEY", apiKey);
                request.AddParameter("documento", documento);
                request.AddParameter("Sexo", _sexo);
                request.AddParameter("GrupoOnb", 1);
                request.AddParameter("UrlCallback", urlCallback + "/validaridentidad?dni=" + documento.Trim() + "&contactid=" + contactid + "&estado=Finalizado");
                request.AddParameter("Format", "JSON");

                RestResponse response = client.Execute(request);

                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string OnboardingINF(string ticket)
        {
            try
            {
                string respuesta = string.Empty;

                RestClient client = new RestClient("https://onbweb.nosis.com/api/inf");
                RestRequest request = new RestRequest("Init", Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("X-API-KEY", apiKey);
                request.AddParameter("Ticket", ticket);
                request.AddParameter("Format", "JSON");

                RestResponse response = client.Execute(request);


                if (response.ResponseStatus.ToString() == "Completed")
                {
                    respuesta = response.Content;
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
