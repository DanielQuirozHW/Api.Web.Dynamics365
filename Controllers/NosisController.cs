using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Api.Web.Dynamics365.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;
using static Api.Web.Dynamics365.Models.Nosis_api;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class NosisController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IErrorLogService errorLogService;

        public NosisController(ApplicationDbContext context, IErrorLogService errorLogService)
        {
            this.context = context;
            this.errorLogService = errorLogService;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/nosis/consultarpordocumento")]
        public async Task<IActionResult> ConsultarDocumento([FromBody] consultaDocumento consultaDoc)
        {
            try
            {
                #region Validaciones
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    return BadRequest("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    return BadRequest("No existen credenciales para ese cliente.");
                }
                #endregion

                respuestaDocumento respuesta = new respuestaDocumento();
                Nosis_api nosis_api = new();
                Nosis nosis = new();
                nosis._usuario = consultaDoc.usuario;
                nosis._token = consultaDoc.token;
                nosis._grupoVariables = consultaDoc.grupo;

                string resultado = nosis.ConsultarPorCUIT(consultaDoc.documento);

                JObject resp = JsonConvert.DeserializeObject<JObject>(resultado);

                if (resp != null)
                {
                    var consulta = resp["Contenido"];
                    nosis_api = JsonConvert.DeserializeObject<Nosis_api>(consulta.ToString());

                    if (nosis_api.Resultado.Estado == "200")
                    {
                       respuesta.CI_Vig_Detalle_PorEntidad = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Nombre == "CI_Vig_Detalle_PorEntidad");
                       respuesta.CI_Vig_PeorSit = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Nombre == "CI_Vig_PeorSit");
                       respuesta.CI_Vig_Total_Monto = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Nombre == "CI_Vig_Total_Monto");
                    }
                }

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/nosis/consultardatosnosis")]
        public async Task<IActionResult> ConsultarDatosNosis([FromBody] consultaDatosNosis consultaDatos)
        {
            try
            {
                #region Validaciones
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    return BadRequest("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    return BadRequest("No existen credenciales para ese cliente.");
                }
                #endregion
                // Obtén la solicitud HTTP actual
                ApiDynamicsV2 api = new(errorLogService);
                Nosis nosis = new();
                Nosis_api nosis_api = new();
                nosis._usuario = consultaDatos.usuario;
                nosis._token = consultaDatos.token;
                nosis._grupoVariables = consultaDatos.grupo;

                string resultado = nosis.ConsultarPorCUIT(consultaDatos.documento);

                if (!string.IsNullOrEmpty(resultado))
                {
                    JObject resp = JsonConvert.DeserializeObject<JObject>(resultado);
                    if (resp != null)
                    {
                        List<Nosis_api.variables> listaVariables = new();
                        var consulta = resp["Contenido"];
                        nosis_api = JsonConvert.DeserializeObject<Nosis_api>(consulta.ToString());
                        if (nosis_api.Resultado.Estado == "200")
                        {
                            variables variableScore = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Descripcion == "Score");
                            variables variableScoreTendencia = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Nombre == "SCO_6m_Tendencia");
                            variables variableCantSinFondoNoPagado = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Nombre == "HC_12m_SF_NoPag_Cant");
                            variables variableMontoSinFondoNoPagado = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Nombre == "HC_12m_SF_NoPag_Monto");
                            variables variableCantSinFondoPagado = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Nombre == "HC_12m_SF_Pag_Cant");
                            variables variableMontoSinFondoPagado = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Nombre == "HC_12m_SF_Pag_Monto");
                            variables variableCompromisoMensual = nosis_api.Datos.Variables.ToList<Nosis_api.variables>().Find(x => x.Nombre == "CDA_COMPMENSUALES");

                            if (consultaDatos.cuenta_id != null)
                            {
                                string mensaje = "Consulta Exitosa.";
                                JObject cuenta = new();
                                if (variableScore != null && variableScore.Valor != "") cuenta.Add("new_score", Convert.ToInt32(variableScore.Valor));
                                if (variableScoreTendencia != null && variableScoreTendencia.Valor != "") cuenta.Add("new_scoretendenciatexto", ObtenerScoreTendencia(variableScoreTendencia.Valor));
                                if (variableCantSinFondoNoPagado != null && variableCantSinFondoNoPagado.Valor != "") cuenta.Add("new_cantidadchequessinfondosnopagados12meses", Convert.ToInt32(variableCantSinFondoNoPagado.Valor));
                                if (variableMontoSinFondoNoPagado != null && variableMontoSinFondoNoPagado.Valor != "") cuenta.Add("new_montosinfondonopagados12meses", Convert.ToDecimal(variableMontoSinFondoNoPagado.Valor));
                                if (variableCantSinFondoPagado != null && variableCantSinFondoPagado.Valor != "") cuenta.Add("new_cantidadchequessinfondospagados12meses", Convert.ToInt32(variableCantSinFondoPagado.Valor));
                                if (variableMontoSinFondoPagado != null && variableMontoSinFondoPagado.Valor != "") cuenta.Add("new_montosinfondospagados12meses", Convert.ToDecimal(variableMontoSinFondoPagado.Valor));
                                if (variableCompromisoMensual != null && variableCompromisoMensual.Valor != "") cuenta.Add("new_compromisomensual", Convert.ToDecimal(variableCompromisoMensual.Valor));
                                if (mensaje != "") cuenta.Add("new_respuestanosis", mensaje);

                                ResponseAPI responseAPI = await api.UpdateRecord("accounts", consultaDatos.cuenta_id, cuenta, credenciales);
                                if (!responseAPI.ok)
                                {
                                    return BadRequest(responseAPI.descripcion);
                                }

                                return Ok(responseAPI.descripcion);
                            }
                            else
                            {
                                JObject cuenta = new()
                                {
                                    { "new_respuestanosis", "El id de la cuenta esta vacio." }
                                };

                                await api.UpdateRecord("accounts", consultaDatos.cuenta_id, cuenta, credenciales);
                                return BadRequest("El id de la cuenta esta vacio.");
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(nosis_api.Resultado.Novedad))
                            {
                                JObject cuenta = new()
                                {
                                    { "new_respuestanosis", nosis_api.Resultado.Novedad }
                                };

                                await api.UpdateRecord("accounts", consultaDatos.cuenta_id, cuenta, credenciales);
                                return BadRequest(nosis_api.Resultado.Novedad);
                            }
                            else
                            {
                                return BadRequest("Error al consultar NOSIS.");
                            }
                        }
                    }
                }

                return BadRequest("Respuesta Nosis vacia.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/nosis/consultarvariablesnosis")]
        public async Task<IActionResult> ConsultarVariablesNosis([FromBody] consultaDatosNosis consultaDatos)
        {
            try
            {
                #region Validaciones
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    return BadRequest("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    return BadRequest("No existen credenciales para ese cliente.");
                }
                #endregion
                // Obtén la solicitud HTTP actual
                ApiDynamicsV2 api = new(errorLogService);
                Nosis nosis = new();
                Nosis_api nosis_api = new();
                nosis._usuario = consultaDatos.usuario;
                nosis._token = consultaDatos.token;
                nosis._grupoVariables = consultaDatos.grupo;

                string resultado = nosis.ConsultarPorCUIT(consultaDatos.documento);

                if (!string.IsNullOrEmpty(resultado))
                {
                    JObject resp = JsonConvert.DeserializeObject<JObject>(resultado);
                    if (resp != null)
                    {
                        List<variables> listaVariables = new();
                        var consulta = resp["Contenido"];
                        nosis_api = JsonConvert.DeserializeObject<Nosis_api>(consulta.ToString());
                        if (nosis_api.Resultado.Estado == "200")
                        {
                            List<variables> variables = nosis_api.Datos.Variables.ToList<variables>();
                            return Ok(variables);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(nosis_api.Resultado.Novedad))
                            {
                                JObject cuenta = new()
                                {
                                    { "new_respuestanosis", nosis_api.Resultado.Novedad }
                                };

                                await api.UpdateRecord("accounts", consultaDatos.cuenta_id, cuenta, credenciales);
                                return BadRequest(nosis_api.Resultado.Novedad);
                            }
                            else
                            {
                                return BadRequest("Error al consultar NOSIS.");
                            }
                        }
                    }
                }

                return BadRequest("Respuesta Nosis vacia.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public static string ObtenerScoreTendencia(string valor)
        {
            switch (valor)
            {
                case "0":
                    return "Estable";
                case "1":
                    return "Creciente ";
                case "-1":
                    return "Decreciente";
                default:
                    return "-";
            }
        }
    }
}
