using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Api.Web.Dynamics365.Models.HRF_Pampabi_PortalCandidato;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class HRF_Pampabi_PortalCandidatoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        public static ApiDynamicsV2 api = new();  
        public HRF_Pampabi_PortalCandidatoController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        [Route("api/portalcandidato/candidato")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> CrearCandidato([FromBody] Candidato candidato)
        {
            try
            {
                #region Credenciales
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    throw new Exception("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    throw new Exception("No existen credenciales para ese cliente.");
                }
                #endregion
                #region Validaciones
                if (!ModelState.IsValid)
                {
                    List<string> errors = new List<string>();

                    if (ModelState.Count > 0)
                    {
                        foreach (var item in ModelState)
                        {
                            errors.Add(item.Value.Errors.FirstOrDefault().ErrorMessage);
                        }
                    }

                    string errorJSON = JsonConvert.SerializeObject(errors);
                    throw new Exception(errorJSON);
                }
                #endregion 

                JObject Candidato = new()
                {
                    { "new_nombredepila", candidato.nombre },
                    { "new_apellidos", candidato.apellido },
                    { "new_correoelectronico", candidato.correo }
                };

                if (candidato.telefono != null)
                    Candidato.Add("new_telefonocelular", candidato.telefono);

                if (candidato.linkedin != null)
                    Candidato.Add("new_linkedinn", candidato.linkedin);

                ResponseAPI responseAPI = await api.CreateRecord("new_candidatos", Candidato, credenciales);

                if (!responseAPI.ok)
                {
                    return BadRequest($"Error ante la generación del candidato - {responseAPI.descripcion}");
                }

                return Ok(responseAPI.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/portalcandidato/actualizarcandidato")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> ActualizarCandidato([FromBody] CandidatoActualizacion candidato)
        {
            try
            {
                #region Credenciales
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    throw new Exception("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    throw new Exception("No existen credenciales para ese cliente.");
                }
                #endregion
                #region Validaciones
                if (!ModelState.IsValid)
                {
                    List<string> errors = new List<string>();

                    if (ModelState.Count > 0)
                    {
                        foreach (var item in ModelState)
                        {
                            errors.Add(item.Value.Errors.FirstOrDefault().ErrorMessage);
                        }
                    }

                    string errorJSON = JsonConvert.SerializeObject(errors);
                    throw new Exception(errorJSON);
                }
                #endregion 

                JObject Candidato = new JObject();
                if (candidato.nombre != null)
                    Candidato.Add("new_nombredepila", candidato.nombre);
                if (candidato.apellido != null)
                    Candidato.Add("new_apellidos", candidato.apellido);
                if (candidato.telefono != null)
                    Candidato.Add("new_telefonocelular", candidato.telefono);
                if (candidato.linkedin != null)
                    Candidato.Add("new_linkedinn", candidato.linkedin);
                if (candidato.tipoDocumento != null)
                    Candidato.Add("new_tipodocumento", Convert.ToInt32(candidato.tipoDocumento));
                if (candidato.documento != null)
                    Candidato.Add("new_nrodocumento", candidato.documento);
                if (candidato.fechaNacimiento != null)
                    Candidato.Add("new_fechanacimiento", candidato.fechaNacimiento);
                if (candidato.pais != null)
                    Candidato.Add("new_pais@odata.bind", "/new_paises(" + candidato.pais + ")");

                ResponseAPI responseAPI = await api.UpdateRecord("new_candidatos", candidato.id, Candidato, credenciales);

                if (!responseAPI.ok)
                {
                    return BadRequest($"Error ante la actualización del candidato - {responseAPI.descripcion}");
                }

                return Ok(responseAPI.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/portalcandidato/postulacion")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> PostularCandidato([FromBody] Postulacion postulacion)
        {
            try
            {
                #region Credenciales
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    throw new Exception("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    throw new Exception("No existen credenciales para ese cliente.");
                }
                #endregion
                #region Validaciones
                if (!ModelState.IsValid)
                {
                    List<string> errors = new List<string>();

                    if (ModelState.Count > 0)
                    {
                        foreach (var item in ModelState)
                        {
                            errors.Add(item.Value.Errors.FirstOrDefault().ErrorMessage);
                        }
                    }

                    string errorJSON = JsonConvert.SerializeObject(errors);
                    throw new Exception(errorJSON);
                }
                #endregion

                JObject Postulacion = new()
                {
                    { "new_candidato@odata.bind", "/new_candidatos(" + postulacion.candidato + ")" },
                    { "new_busquedadepersonal@odata.bind", "/new_busquedadepersonals(" + postulacion.busqueda + ")" }
                };

                ResponseAPI responseAPI = await api.CreateRecord("new_candidatoporbusquedas", Postulacion, credenciales);

                if (!responseAPI.ok)
                {
                    return BadRequest($"Error ante la postulación del candidato - {responseAPI.descripcion}");
                }

                return Ok(responseAPI.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/portalcandidato/adjuntar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> AdjuntarArchivo(string candidatoid)
        {
            try
            {
                #region Credenciales
                var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
                if (clienteClaim == null)
                {
                    throw new Exception("El usuario no contiene un cliente asociado para operar.");
                }
                var cliente_db = clienteClaim.Value;
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
                if (credenciales == null)
                {
                    throw new Exception("No existen credenciales para ese cliente.");
                }
                #endregion

                var archivos = HttpContext.Request.Form.Files;
                string nota_id = string.Empty;

                if (archivos.Count > 0)
                {
                    foreach (var file in archivos)
                    {
                        byte[] fileInBytes = new byte[file.Length];
                        using (BinaryReader theReader = new BinaryReader(file.OpenReadStream()))
                        {
                            fileInBytes = theReader.ReadBytes(Convert.ToInt32(file.Length));
                        }

                        string fileAsString = Convert.ToBase64String(fileInBytes);

                        JObject annotation = new()
                        {
                            { "subject", file.FileName },
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName }
                        };

                        if (candidatoid != string.Empty)
                            annotation.Add("objectid_new_candidato@odata.bind", "/new_candidatos(" + candidatoid + ")");

                        ResponseAPI responseAPI = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!responseAPI.ok)
                        {
                            return BadRequest($"Error en la creación del archivo - {responseAPI.descripcion}");
                        }

                        nota_id = responseAPI.descripcion;
                    }
                }

                return Ok(nota_id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
