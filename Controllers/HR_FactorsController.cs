using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;
using static Api.Web.Dynamics365.Models.HRFactors;
using static Api.Web.Dynamics365.Models.Lufe;
using static Api.Web.Dynamics365.Models.Megatlon;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class HR_FactorsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private string cliente;
        public HR_FactorsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        //HWAPPLICATIONS
        #region Empleado
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/empleado")]
        public async Task<IActionResult> ActualizarEmpleado([FromBody] Empleado empleado)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject empleado_hrf = new()
                {
                    //Educacion
                    { "new_primariocompleto", empleado.new_primariocompleto },
                    { "new_secundariocompleto", empleado.new_secundariocompleto },
                    { "new_secundarioincompleto", empleado.new_secundarioincompleto },
                    { "new_bachiller", empleado.new_bachiller },
                    { "new_tecnico", empleado.new_tecnico },
                    { "new_peritomercantil", empleado.new_peritomercantil },
                    { "new_sexo", empleado.new_sexo }
                };
                //Datos Generales
                if (empleado.new_nombredepila != null && empleado.new_nombredepila != string.Empty)
                    empleado_hrf.Add("new_nombredepila", empleado.new_nombredepila);
                if (empleado.new_apellidos != null && empleado.new_apellidos != string.Empty)
                    empleado_hrf.Add("new_apellidos", empleado.new_apellidos);
                if (empleado.new_numerolegajo > 0)
                    empleado_hrf.Add("new_numerolegajo", empleado.new_numerolegajo);
                if (empleado.new_tipodocumento > 0)
                    empleado_hrf.Add("new_tipodocumento", empleado.new_tipodocumento);
                if (empleado.new_nrodocumento != null && empleado.new_nrodocumento != string.Empty)
                    empleado_hrf.Add("new_nrodocumento", empleado.new_nrodocumento);
                if (empleado.new_cuitcuil != null && empleado.new_cuitcuil != string.Empty)
                    empleado_hrf.Add("new_cuitcuil", empleado.new_cuitcuil);
                if (empleado.new_correoelectronico != null && empleado.new_correoelectronico != string.Empty)
                    empleado_hrf.Add("new_correoelectronico", empleado.new_correoelectronico);
                if (empleado.new_estadocivil > 0)
                    empleado_hrf.Add("new_estadocivil", empleado.new_estadocivil);
                if (empleado.new_telefonomovil != null && empleado.new_telefonomovil != string.Empty)
                    empleado_hrf.Add("new_telefonomovil", empleado.new_telefonomovil);
                if (empleado.new_telefonoparticular != null && empleado.new_telefonoparticular != string.Empty)
                    empleado_hrf.Add("new_telefonoparticular", empleado.new_telefonoparticular);
                if (empleado.new_extenciontelefonica != null && empleado.new_extenciontelefonica != string.Empty)
                    empleado_hrf.Add("new_extenciontelefonica", empleado.new_extenciontelefonica);
                if (empleado.new_tipodeincorporacion > 0)
                    empleado_hrf.Add("new_tipodeincorporacion", empleado.new_tipodeincorporacion);
                //Datos de Nacimiento
                if (empleado.new_fechanacimiento != null && empleado.new_fechanacimiento != string.Empty)
                    empleado_hrf.Add("new_fechanacimiento", empleado.new_fechanacimiento);
                if (empleado.new_paisnacimiento != null && empleado.new_paisnacimiento != string.Empty)
                    empleado_hrf.Add("new_paisnacimiento@odata.bind", "/new_paises(" + empleado.new_paisnacimiento + ")");
                if (empleado.new_edad > 0)
                    empleado_hrf.Add("new_edad", empleado.new_edad);
                if (empleado.new_provincianacimiento != null && empleado.new_provincianacimiento != string.Empty)
                    empleado_hrf.Add("new_provincianacimiento@odata.bind", "/new_provincias(" + empleado.new_provincianacimiento + ")");
                //Ultimo Domicilio
                if (empleado.new_calle != null && empleado.new_calle != string.Empty)
                    empleado_hrf.Add("new_calle", empleado.new_calle);
                if (empleado.new_nro != null && empleado.new_nro != string.Empty)
                    empleado_hrf.Add("new_nro", empleado.new_nro);
                if (empleado.new_piso != null && empleado.new_piso != string.Empty)
                    empleado_hrf.Add("new_piso", empleado.new_piso);
                if (empleado.new_depto != null && empleado.new_depto != string.Empty)
                    empleado_hrf.Add("new_depto", empleado.new_depto);
                if (empleado.new_localidad != null && empleado.new_localidad != string.Empty)
                    empleado_hrf.Add("new_localidad@odata.bind", "/new_localidads(" + empleado.new_localidad + ")");
                if (empleado.new_codigopostal != null && empleado.new_codigopostal != string.Empty)
                    empleado_hrf.Add("new_codigopostal", empleado.new_codigopostal);
                if (empleado.new_provincia != null && empleado.new_provincia != string.Empty)
                    empleado_hrf.Add("new_provincia@odata.bind", "/new_provincias(" + empleado.new_provincia + ")");
                if (empleado.new_pais != null && empleado.new_pais != string.Empty)
                    empleado_hrf.Add("new_pais@odata.bind", "/new_paises(" + empleado.new_pais + ")");

                ResponseAPI respuesta = await api.UpdateRecord("new_empleados", empleado.new_empleadoid, empleado_hrf, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok("Empleado actualizado");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region UniversidadPorEmpleado
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/universidadporempleado")]
        public async Task<IActionResult> CrearUniversidadPorEmpleado([FromBody] UniversidadPorEmpleado universidad)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject universidad_hrf = new();

                if (universidad.new_empleado != null && universidad.new_empleado != string.Empty)
                    universidad_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + universidad.new_empleado + ")");
                if (universidad.new_universidad != null && universidad.new_universidad != string.Empty)
                    universidad_hrf.Add("new_universidad@odata.bind", "/new_universidads(" + universidad.new_universidad + ")");
                if (universidad.new_carrera != null && universidad.new_carrera != string.Empty)
                    universidad_hrf.Add("new_carrera@odata.bind", "/new_carreras(" + universidad.new_carrera + ")");
                if (universidad.new_fechadeingreso != null && universidad.new_fechadeingreso != string.Empty)
                    universidad_hrf.Add("new_fechadeingreso", universidad.new_fechadeingreso);
                if (universidad.new_fechaegreso != null && universidad.new_fechaegreso != string.Empty)
                    universidad_hrf.Add("new_fechaegreso", universidad.new_fechaegreso);
                if (universidad.new_tipodecarrera > 0)
                    universidad_hrf.Add("new_tipodecarrera", universidad.new_tipodecarrera);

                ResponseAPI resultado = await api.CreateRecord("new_universidadporcontactos", universidad_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/universidadporempleado")]
        public async Task<IActionResult> ActualizarUniversidadPorEmpleado([FromBody] UniversidadPorEmpleado universidad)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject universidad_hrf = new();

                if (universidad.new_empleado != null && universidad.new_empleado != string.Empty)
                    universidad_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + universidad.new_empleado + ")");
                if (universidad.new_universidad != null && universidad.new_universidad != string.Empty)
                    universidad_hrf.Add("new_universidad@odata.bind", "/new_universidads(" + universidad.new_universidad + ")");
                if (universidad.new_carrera != null && universidad.new_carrera != string.Empty)
                    universidad_hrf.Add("new_carrera@odata.bind", "/new_carreras(" + universidad.new_carrera + ")");
                if (universidad.new_fechadeingreso != null && universidad.new_fechadeingreso != string.Empty)
                    universidad_hrf.Add("new_fechadeingreso", universidad.new_fechadeingreso);
                if (universidad.new_fechaegreso != null && universidad.new_fechaegreso != string.Empty)
                    universidad_hrf.Add("new_fechaegreso", universidad.new_fechaegreso);
                if (universidad.new_tipodecarrera > 0)
                    universidad_hrf.Add("new_tipodecarrera", universidad.new_tipodecarrera);

                ResponseAPI resultado = await api.UpdateRecord("new_universidadporcontactos", universidad.new_universidadporcontactoId, universidad_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/universidadporempleado")]
        public async Task<IActionResult> InactivarUniversidadPorEmpleado([FromBody] UniversidadPorEmpleado universidad)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (universidad.new_universidadporcontactoId == null || universidad.new_universidadporcontactoId == string.Empty)
                    return BadRequest("El id de la universidad por empleado esta vacio");

                JObject universidad_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_universidadporcontactos", universidad.new_universidadporcontactoId, universidad_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region IdiomaPorEmpleado
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/idiomaporempleado")]
        public async Task<IActionResult> CrearIdiomaPorEmpleado([FromBody] IdiomaPorEmpleado idioma)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject idioma_hrf = new()
                {
                    { "new_habla", idioma.new_habla },
                    { "new_lee", idioma.new_lee },
                    { "new_escribe", idioma.new_escribe }
                };

                if (idioma.new_empleado != null && idioma.new_empleado != string.Empty)
                    idioma_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + idioma.new_empleado + ")");
                if (idioma.new_idioma != null && idioma.new_idioma != string.Empty)
                    idioma_hrf.Add("new_idioma@odata.bind", "/new_idiomas(" + idioma.new_idioma + ")");
                if (idioma.new_nivel > 0)
                    idioma_hrf.Add("new_nivel", idioma.new_nivel);

                ResponseAPI resultado = await api.CreateRecord("new_idiomaporcontactos", idioma_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/idiomaporempleado")]
        public async Task<IActionResult> ActualizarIdiomaPorEmpleado([FromBody] IdiomaPorEmpleado idioma)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject idioma_hrf = new()
                {
                    { "new_habla", idioma.new_habla },
                    { "new_lee", idioma.new_lee },
                    { "new_escribe", idioma.new_escribe }
                };

                if (idioma.new_empleado != null && idioma.new_empleado != string.Empty)
                    idioma_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + idioma.new_empleado + ")");
                if (idioma.new_idioma != null && idioma.new_idioma != string.Empty)
                    idioma_hrf.Add("new_idioma@odata.bind", "/new_idiomas(" + idioma.new_idioma + ")");
                if (idioma.new_nivel > 0)
                    idioma_hrf.Add("new_nivel", idioma.new_nivel);

                ResponseAPI resultado = await api.UpdateRecord("new_idiomaporcontactos", idioma.new_idiomaporcontactoid, idioma_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/idiomaporempleado")]
        public async Task<IActionResult> InactivarIdiomaPorEmpleado([FromBody] IdiomaPorEmpleado idioma)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (idioma.new_idiomaporcontactoid == null || idioma.new_idiomaporcontactoid == string.Empty)
                    return BadRequest("El id de la universidad por empleado esta vacio");

                JObject universidad_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_idiomaporcontactos", idioma.new_idiomaporcontactoid, universidad_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Trayectoria
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/trayectoria")]
        public async Task<IActionResult> CrearIdiomaTrayectoria([FromBody] Trayectoria trayectoria)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject trayectoria_hrf = new()
                {
                    { "new_trayectoriaenlacompania", trayectoria.new_trayectoriaenlacompania }
                };

                if (trayectoria.new_empleado != null && trayectoria.new_empleado != string.Empty)
                    trayectoria_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + trayectoria.new_empleado + ")");
                if (trayectoria.new_empresa != null && trayectoria.new_empresa != string.Empty)
                    trayectoria_hrf.Add("new_empresa@odata.bind", "/new_empresas(" + trayectoria.new_empresa + ")");
                if (trayectoria.new_puesto != null && trayectoria.new_puesto != string.Empty)
                    trayectoria_hrf.Add("new_Puesto@odata.bind", "/new_cargos(" + trayectoria.new_puesto + ")");
                if (trayectoria.new_fechadesde != null && trayectoria.new_fechadesde != string.Empty)
                    trayectoria_hrf.Add("new_fechadesde", trayectoria.new_fechadesde);
                if (trayectoria.new_fechahasta != null && trayectoria.new_fechahasta != string.Empty)
                    trayectoria_hrf.Add("new_fechahasta", trayectoria.new_fechahasta);

                ResponseAPI resultado = await api.CreateRecord("new_trayectorias", trayectoria_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/trayectoria")]
        public async Task<IActionResult> ActualizarTrayectoria([FromBody] Trayectoria trayectoria)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject trayectoria_hrf = new()
                {
                    { "new_trayectoriaenlacompania", trayectoria.new_trayectoriaenlacompania }
                };

                if (trayectoria.new_empleado != null && trayectoria.new_empleado != string.Empty)
                    trayectoria_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + trayectoria.new_empleado + ")");
                if (trayectoria.new_empresa != null && trayectoria.new_empresa != string.Empty)
                    trayectoria_hrf.Add("new_empresa@odata.bind", "/new_empresas(" + trayectoria.new_empresa + ")");
                if (trayectoria.new_puesto != null && trayectoria.new_puesto != string.Empty)
                    trayectoria_hrf.Add("new_Puesto@odata.bind", "/new_cargos(" + trayectoria.new_puesto + ")");
                if (trayectoria.new_fechadesde != null && trayectoria.new_fechadesde != string.Empty)
                    trayectoria_hrf.Add("new_fechadesde", trayectoria.new_fechadesde);
                if (trayectoria.new_fechahasta != null && trayectoria.new_fechahasta != string.Empty)
                    trayectoria_hrf.Add("new_fechahasta", trayectoria.new_fechahasta);

                ResponseAPI resultado = await api.UpdateRecord("new_trayectorias", trayectoria.new_trayectoriaid, trayectoria_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/trayectoria")]
        public async Task<IActionResult> InactivarTrayectoria([FromBody] Trayectoria trayectoria)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (trayectoria.new_trayectoriaid == null || trayectoria.new_trayectoriaid == string.Empty)
                    return BadRequest("El id de la trayectoria por empleado esta vacio");

                JObject trayectoria_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_trayectorias", trayectoria.new_trayectoriaid, trayectoria_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region FamiliarDelEmpleado 
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/familiardelempleado")]
        public async Task<IActionResult> CrearFamiliarPorEmpleado([FromBody] FamiliarDelEmpleado familiar)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject familiar_hrf = new();

                if (familiar.new_empleado != null && familiar.new_empleado != string.Empty)
                    familiar_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + familiar.new_empleado + ")");
                if (familiar.new_tipodocumento > 0)
                    familiar_hrf.Add("new_tipodocumento", familiar.new_tipodocumento);
                if (familiar.new_nrodocumento != null && familiar.new_nrodocumento != string.Empty)
                    familiar_hrf.Add("new_nrodocumento", familiar.new_nrodocumento);
                if (familiar.new_nombredepila != null && familiar.new_nombredepila != string.Empty)
                    familiar_hrf.Add("new_nombredepila", familiar.new_nombredepila);
                if (familiar.new_apellidos != null && familiar.new_apellidos != string.Empty)
                    familiar_hrf.Add("new_apellidos", familiar.new_apellidos);
                if (familiar.new_fechanacimiento != null && familiar.new_fechanacimiento != string.Empty)
                    familiar_hrf.Add("new_fechanacimiento", familiar.new_fechanacimiento);
                if (familiar.new_ocupacion > 0)
                    familiar_hrf.Add("new_ocupacion", familiar.new_ocupacion);
                if (familiar.new_sexo > 0)
                    familiar_hrf.Add("new_sexo", familiar.new_sexo);
                if (familiar.new_parentesco != null && familiar.new_parentesco != string.Empty)
                    familiar_hrf.Add("new_Parentesco@odata.bind", "/new_parentescos(" + familiar.new_parentesco + ")");

                ResponseAPI resultado = await api.CreateRecord("new_familiardeempleados", familiar_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/familiardelempleado")]
        public async Task<IActionResult> ActualizarFamiliarPorEmpleado([FromBody] FamiliarDelEmpleado familiar)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject familiar_hrf = new();

                if (familiar.new_empleado != null && familiar.new_empleado != string.Empty)
                    familiar_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + familiar.new_empleado + ")");
                if (familiar.new_tipodocumento > 0)
                    familiar_hrf.Add("new_tipodocumento", familiar.new_tipodocumento);
                if (familiar.new_nrodocumento != null && familiar.new_nrodocumento != string.Empty)
                    familiar_hrf.Add("new_nrodocumento", familiar.new_nrodocumento);
                if (familiar.new_nombredepila != null && familiar.new_nombredepila != string.Empty)
                    familiar_hrf.Add("new_nombredepila", familiar.new_nombredepila);
                if (familiar.new_apellidos != null && familiar.new_apellidos != string.Empty)
                    familiar_hrf.Add("new_apellidos", familiar.new_apellidos);
                if (familiar.new_fechanacimiento != null && familiar.new_fechanacimiento != string.Empty)
                    familiar_hrf.Add("new_fechanacimiento", familiar.new_fechanacimiento);
                if (familiar.new_ocupacion > 0)
                    familiar_hrf.Add("new_ocupacion", familiar.new_ocupacion);
                if (familiar.new_sexo > 0)
                    familiar_hrf.Add("new_sexo", familiar.new_sexo);
                if (familiar.new_parentesco != null && familiar.new_parentesco != string.Empty)
                    familiar_hrf.Add("new_Parentesco@odata.bind", "/new_parentescos(" + familiar.new_parentesco + ")");

                ResponseAPI resultado = await api.UpdateRecord("new_familiardeempleados", familiar.new_familiardeempleadoid, familiar_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/familiardelempleado")]
        public async Task<IActionResult> InactivarFamiliarPorEmpleado([FromBody] FamiliarDelEmpleado familiar)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (familiar.new_familiardeempleadoid == null || familiar.new_familiardeempleadoid == string.Empty)
                    return BadRequest("El id del familiar por empleado esta vacio");

                JObject familiar_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_familiardeempleados", familiar.new_familiardeempleadoid, familiar_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Insumo
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/insumo")]
        public async Task<IActionResult> CrearInsumo([FromBody] Insumo insumo)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject insumo_hrf = new();

                if (insumo.new_empleado != null && insumo.new_empleado != string.Empty)
                    insumo_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + insumo.new_empleado + ")");
                if (insumo.new_modelo > 0)
                    insumo_hrf.Add("new_modelo", insumo.new_modelo);
                if (insumo.new_tipodeinsumo > 0)
                    insumo_hrf.Add("new_tipodeinsumo", insumo.new_tipodeinsumo);
                if (insumo.new_marca > 0)
                    insumo_hrf.Add("new_marca", insumo.new_marca);
                if (insumo.statuscode > 0)
                    insumo_hrf.Add("statuscode", insumo.statuscode);
                if (insumo.new_observaciones != null && insumo.new_observaciones != string.Empty)
                    insumo_hrf.Add("new_observaciones", insumo.new_observaciones);

                ResponseAPI resultado = await api.CreateRecord("new_insumoparapersonals", insumo_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/insumo")]
        public async Task<IActionResult> ActualizarInsumo([FromBody] Insumo insumo)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject insumo_hrf = new();

                if (insumo.new_empleado != null && insumo.new_empleado != string.Empty)
                    insumo_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + insumo.new_empleado + ")");
                if (insumo.new_modelo > 0)
                    insumo_hrf.Add("new_modelo", insumo.new_modelo);
                if (insumo.new_tipodeinsumo > 0)
                    insumo_hrf.Add("new_tipodeinsumo", insumo.new_tipodeinsumo);
                if (insumo.new_marca > 0)
                    insumo_hrf.Add("new_marca", insumo.new_marca);
                if (insumo.statuscode > 0)
                    insumo_hrf.Add("statuscode", insumo.statuscode);
                if (insumo.new_observaciones != null && insumo.new_observaciones != string.Empty)
                    insumo_hrf.Add("new_observaciones", insumo.new_observaciones);

                ResponseAPI resultado = await api.UpdateRecord("new_insumoparapersonals", insumo.new_insumoparapersonalid, insumo_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/insumo")]
        public async Task<IActionResult> InactivarInsumo([FromBody] Insumo insumo)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (insumo.new_insumoparapersonalid == null || insumo.new_insumoparapersonalid == string.Empty)
                    return BadRequest("El id del insumo esta vacio");

                JObject insumo_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_insumoparapersonals", insumo.new_insumoparapersonalid, insumo_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region DatosBancario
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/datosbancario")]
        public async Task<IActionResult> CrearDatoBancario([FromBody] DatosBancarios datoBancario)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject datoBancario_hrf = new();

                if (datoBancario.new_empleado != null && datoBancario.new_empleado != string.Empty)
                    datoBancario_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + datoBancario.new_empleado + ")");
                if (datoBancario.new_banco != null && datoBancario.new_banco != string.Empty)
                    datoBancario_hrf.Add("new_banco@odata.bind", "/new_bancos(" + datoBancario.new_banco + ")");
                if (datoBancario.new_tipodecuenta > 0)
                    datoBancario_hrf.Add("new_tipodecuenta", datoBancario.new_tipodecuenta);
                if (datoBancario.new_numerocuenta != null && datoBancario.new_numerocuenta != string.Empty)
                    datoBancario_hrf.Add("new_numerocuenta", datoBancario.new_numerocuenta);
                if (datoBancario.new_cbu != null && datoBancario.new_cbu != string.Empty)
                    datoBancario_hrf.Add("new_cbu", datoBancario.new_cbu);
                if (datoBancario.transactioncurrencyid != null && datoBancario.transactioncurrencyid != string.Empty)
                    datoBancario_hrf.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + datoBancario.transactioncurrencyid + ")");

                ResponseAPI resultado = await api.CreateRecord("new_cuentabancarias", datoBancario_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/datosbancario")]
        public async Task<IActionResult> ActualizarDatoBancario([FromBody] DatosBancarios datoBancario)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject datoBancario_hrf = new();

                if (datoBancario.new_empleado != null && datoBancario.new_empleado != string.Empty)
                    datoBancario_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + datoBancario.new_empleado + ")");
                if (datoBancario.new_banco != null && datoBancario.new_banco != string.Empty)
                    datoBancario_hrf.Add("new_banco@odata.bind", "/new_bancos(" + datoBancario.new_banco + ")");
                if (datoBancario.new_tipodecuenta > 0)
                    datoBancario_hrf.Add("new_tipodecuenta", datoBancario.new_tipodecuenta);
                if (datoBancario.new_numerocuenta != null && datoBancario.new_numerocuenta != string.Empty)
                    datoBancario_hrf.Add("new_numerocuenta", datoBancario.new_numerocuenta);
                if (datoBancario.new_cbu != null && datoBancario.new_cbu != string.Empty)
                    datoBancario_hrf.Add("new_cbu", datoBancario.new_cbu);
                if (datoBancario.transactioncurrencyid != null && datoBancario.transactioncurrencyid != string.Empty)
                    datoBancario_hrf.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + datoBancario.transactioncurrencyid + ")");

                ResponseAPI resultado = await api.UpdateRecord("new_cuentabancarias", datoBancario.new_cuentabancariaid, datoBancario_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/datosbancario")]
        public async Task<IActionResult> InactivarDatoBancario([FromBody] DatosBancarios datoBancario)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (datoBancario.new_cuentabancariaid == null || datoBancario.new_cuentabancariaid == string.Empty)
                    return BadRequest("El id del dato bancario esta vacio");

                JObject datoBancario_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_cuentabancarias", datoBancario.new_cuentabancariaid, datoBancario_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region CargaHoraria
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/cargahoraria")]
        public async Task<IActionResult> CrearCargaHoraria([FromBody] CargaHoraria cargaHoraria)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject cargaHoraria_hrf = new()
                {
                    { "new_facturable", cargaHoraria.new_facturable }
                };

                if (cargaHoraria.new_empleado != null && cargaHoraria.new_empleado != string.Empty)
                    cargaHoraria_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + cargaHoraria.new_empleado + ")");
                if (cargaHoraria.new_proyecto != null && cargaHoraria.new_proyecto != string.Empty)
                    cargaHoraria_hrf.Add("new_Proyecto@odata.bind", "/new_proyectos(" + cargaHoraria.new_proyecto + ")");
                if (cargaHoraria.new_asignacion != null && cargaHoraria.new_asignacion != string.Empty)
                    cargaHoraria_hrf.Add("new_Asignacion@odata.bind", "/new_asignacions(" + cargaHoraria.new_asignacion + ")");
                if (cargaHoraria.new_cliente != null && cargaHoraria.new_cliente != string.Empty)
                    cargaHoraria_hrf.Add("new_Cliente@odata.bind", "/accounts(" + cargaHoraria.new_cliente + ")");
                if (cargaHoraria.new_fechadecarga != null && cargaHoraria.new_fechadecarga != string.Empty)
                    cargaHoraria_hrf.Add("new_fechadecarga", cargaHoraria.new_fechadecarga);
                if (cargaHoraria.new_horas > 0)
                    cargaHoraria_hrf.Add("new_horas", cargaHoraria.new_horas);
                if (cargaHoraria.new_descripcion != null && cargaHoraria.new_descripcion != string.Empty)
                    cargaHoraria_hrf.Add("new_descripcion", cargaHoraria.new_descripcion);
                if (cargaHoraria.new_devengadoen > 0)
                    cargaHoraria_hrf.Add("new_devengadoen", cargaHoraria.new_devengadoen);
                if (cargaHoraria.new_caso != null && cargaHoraria.new_caso != string.Empty)
                    cargaHoraria_hrf.Add("new_caso@odata.bind", "/incidents(" + cargaHoraria.new_caso + ")");

                ResponseAPI resultado = await api.CreateRecord("new_cargahorarias", cargaHoraria_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/cargahoraria")]
        public async Task<IActionResult> ActualizarCargaHoraria([FromBody] CargaHoraria cargaHoraria)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject cargaHoraria_hrf = new()
                {
                    { "new_facturable", cargaHoraria.new_facturable }
                };

                if (cargaHoraria.new_empleado != null && cargaHoraria.new_empleado != string.Empty)
                    cargaHoraria_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + cargaHoraria.new_empleado + ")");
                if (cargaHoraria.new_proyecto != null && cargaHoraria.new_proyecto != string.Empty)
                    cargaHoraria_hrf.Add("new_Proyecto@odata.bind", "/new_proyectos(" + cargaHoraria.new_proyecto + ")");
                if (cargaHoraria.new_asignacion != null && cargaHoraria.new_asignacion != string.Empty)
                    cargaHoraria_hrf.Add("new_Asignacion@odata.bind", "/new_asignacions(" + cargaHoraria.new_asignacion + ")");
                if (cargaHoraria.new_cliente != null && cargaHoraria.new_cliente != string.Empty)
                    cargaHoraria_hrf.Add("new_Cliente@odata.bind", "/accounts(" + cargaHoraria.new_cliente + ")");
                if (cargaHoraria.new_fechadecarga != null && cargaHoraria.new_fechadecarga != string.Empty)
                    cargaHoraria_hrf.Add("new_fechadecarga", cargaHoraria.new_fechadecarga);
                if (cargaHoraria.new_horas > 0)
                    cargaHoraria_hrf.Add("new_horas", cargaHoraria.new_horas);
                if (cargaHoraria.new_descripcion != null && cargaHoraria.new_descripcion != string.Empty)
                    cargaHoraria_hrf.Add("new_descripcion", cargaHoraria.new_descripcion);
                if (cargaHoraria.new_devengadoen > 0)
                    cargaHoraria_hrf.Add("new_devengadoen", cargaHoraria.new_devengadoen);
                if (cargaHoraria.new_caso != null && cargaHoraria.new_caso != string.Empty)
                    cargaHoraria_hrf.Add("new_caso@odata.bind", "/incidents(" + cargaHoraria.new_caso + ")");

                ResponseAPI resultado = await api.UpdateRecord("new_cargahorarias", cargaHoraria.new_cargahorariaid, cargaHoraria_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/cargahoraria")]
        public async Task<IActionResult> InactivarCargaHoraria([FromBody] CargaHoraria cargaHoraria)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (cargaHoraria.new_cargahorariaid == null || cargaHoraria.new_cargahorariaid == string.Empty)
                    return BadRequest("El id de la carga horaria esta vacio");

                JObject cargaHoraria_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_cargahorarias", cargaHoraria.new_cargahorariaid, cargaHoraria_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Asignacion
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/asignacion")]
        public async Task<IActionResult> CrearAsignacion([FromBody] Asignacion asignacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject asignacion_hrf = new();

                if (asignacion.new_empleado != null && asignacion.new_empleado != string.Empty)
                    asignacion_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + asignacion.new_empleado + ")");
                if (asignacion.new_name != null && asignacion.new_name != string.Empty)
                    asignacion_hrf.Add("new_name", asignacion.new_name);
                if (asignacion.new_solucindelacualpartir != null && asignacion.new_solucindelacualpartir != string.Empty)
                    asignacion_hrf.Add("new_solucindelacualpartir", asignacion.new_solucindelacualpartir);
                if (asignacion.new_proyecto != null && asignacion.new_proyecto != string.Empty)
                    asignacion_hrf.Add("new_Proyecto@odata.bind", "/new_proyectos(" + asignacion.new_proyecto + ")");
                if (asignacion.new_rolenelproyecto != null && asignacion.new_rolenelproyecto != string.Empty)
                    asignacion_hrf.Add("new_RolenelProyecto@odata.bind", "/new_rolenelproyectos(" + asignacion.new_rolenelproyecto + ")");
                if (asignacion.new_periodo != null && asignacion.new_periodo != string.Empty)
                    asignacion_hrf.Add("new_Periodo@odata.bind", "/new_periodos(" + asignacion.new_periodo + ")");
                if (asignacion.new_tarifa > 0)
                    asignacion_hrf.Add("new_tarifa", asignacion.new_tarifa);
                if (asignacion.new_cantidadhoras > 0)
                    asignacion_hrf.Add("new_cantidadhoras", asignacion.new_cantidadhoras);
                if (asignacion.statuscode > 0)
                    asignacion_hrf.Add("statuscode", asignacion.statuscode);
                if (asignacion.new_tipodeasignacion > 0)
                    asignacion_hrf.Add("new_tipodeasignacion", asignacion.new_tipodeasignacion);
                if (!string.IsNullOrEmpty(asignacion.new_modificadoporempleado))
                    asignacion_hrf.Add("new_modificadoporempleado@odata.bind", "/new_empleados(" + asignacion.new_modificadoporempleado + ")");
                if (asignacion.new_niveldecriticidad > 0)
                    asignacion_hrf.Add("new_niveldecriticidad", asignacion.new_niveldecriticidad);
                if (!string.IsNullOrEmpty(asignacion.new_fechaestimadainicio))
                    asignacion_hrf.Add("new_fechaestimadainicio", asignacion.new_fechaestimadainicio);
                if (!string.IsNullOrEmpty(asignacion.new_fechaestimadafin))
                    asignacion_hrf.Add("new_fechaestimadafin", asignacion.new_fechaestimadafin);
                if (asignacion.new_tiemporealporestado > 0)
                    asignacion_hrf.Add("new_tiemporealporestado", asignacion.new_tiemporealporestado);
                if (asignacion.new_naturalezadelaasignacion > 0)
                    asignacion_hrf.Add("new_naturalezadelaasignacion", asignacion.new_naturalezadelaasignacion);

                ResponseAPI resultado = await api.CreateRecord("new_asignacions", asignacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/asignacion")]
        public async Task<IActionResult> ActualizarAsignacion([FromBody] Asignacion asignacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject asignacion_hrf = new();

                if (asignacion.new_empleado != null && asignacion.new_empleado != string.Empty)
                    asignacion_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + asignacion.new_empleado + ")");
                if (asignacion.new_name != null && asignacion.new_name != string.Empty)
                    asignacion_hrf.Add("new_name", asignacion.new_name);
                if (asignacion.new_solucindelacualpartir != null && asignacion.new_solucindelacualpartir != string.Empty)
                    asignacion_hrf.Add("new_solucindelacualpartir", asignacion.new_solucindelacualpartir);
                if (asignacion.new_proyecto != null && asignacion.new_proyecto != string.Empty)
                    asignacion_hrf.Add("new_Proyecto@odata.bind", "/new_proyectos(" + asignacion.new_proyecto + ")");
                if (asignacion.new_rolenelproyecto != null && asignacion.new_rolenelproyecto != string.Empty)
                    asignacion_hrf.Add("new_RolenelProyecto@odata.bind", "/new_rolenelproyectos(" + asignacion.new_rolenelproyecto + ")");
                if (asignacion.new_periodo != null && asignacion.new_periodo != string.Empty)
                    asignacion_hrf.Add("new_Periodo@odata.bind", "/new_periodos(" + asignacion.new_periodo + ")");
                if (asignacion.new_tarifa > 0)
                    asignacion_hrf.Add("new_tarifa", asignacion.new_tarifa);
                if (asignacion.new_cantidadhoras > 0)
                    asignacion_hrf.Add("new_cantidadhoras", asignacion.new_cantidadhoras);
                if (asignacion.statuscode > 0)
                    asignacion_hrf.Add("statuscode", asignacion.statuscode);
                if (asignacion.new_tipodeasignacion > 0)
                    asignacion_hrf.Add("new_tipodeasignacion", asignacion.new_tipodeasignacion);
                if (!string.IsNullOrEmpty(asignacion.new_modificadoporempleado))
                    asignacion_hrf.Add("new_modificadoporempleado@odata.bind", "/new_empleados(" + asignacion.new_modificadoporempleado + ")");
                if (asignacion.new_tiemporealporestado > 0)
                    asignacion_hrf.Add("new_tiemporealporestado", asignacion.new_tiemporealporestado);
                if (asignacion.new_naturalezadelaasignacion > 0)
                    asignacion_hrf.Add("new_naturalezadelaasignacion", asignacion.new_naturalezadelaasignacion);

                ResponseAPI resultado = await api.UpdateRecord("new_asignacions", asignacion.new_asignacionid, asignacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/asignacion")]
        public async Task<IActionResult> InactivarAsignacion([FromBody] Asignacion asignacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (asignacion.new_asignacionid == null || asignacion.new_asignacionid == string.Empty)
                    return BadRequest("El id de la asignacion esta vacio");

                JObject asignacion_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_asignacions", asignacion.new_asignacionid, asignacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/asignarasignacion")]
        public async Task<IActionResult> AsignarAsignacion([FromBody] Asignacion asignacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject asignacion_hrf = new();

                if (!string.IsNullOrEmpty(asignacion.user))
                    asignacion_hrf.Add("ownerid@odata.bind", "/systemusers(" + asignacion.user + ")");
                if (!string.IsNullOrEmpty(asignacion.new_modificadoporempleado))
                    asignacion_hrf.Add("new_modificadoporempleado@odata.bind", "/new_empleados(" + asignacion.new_modificadoporempleado + ")");

                ResponseAPI resultado = await api.UpdateRecord("new_asignacions", asignacion.new_asignacionid, asignacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/asignacioneintegrantes")]
        public async Task<IActionResult> CrearAsignacionIntegrantes([FromBody] AsignacionIntegrantes asignacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject asignacion_hrf = new();

                if (asignacion.new_empleado != null && asignacion.new_empleado != string.Empty)
                    asignacion_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + asignacion.new_empleado + ")");
                if (asignacion.new_name != null && asignacion.new_name != string.Empty)
                    asignacion_hrf.Add("new_name", asignacion.new_name);
                if (asignacion.new_solucindelacualpartir != null && asignacion.new_solucindelacualpartir != string.Empty)
                    asignacion_hrf.Add("new_solucindelacualpartir", asignacion.new_solucindelacualpartir);
                if (asignacion.new_proyecto != null && asignacion.new_proyecto != string.Empty)
                    asignacion_hrf.Add("new_Proyecto@odata.bind", "/new_proyectos(" + asignacion.new_proyecto + ")");
                if (asignacion.new_rolenelproyecto != null && asignacion.new_rolenelproyecto != string.Empty)
                    asignacion_hrf.Add("new_RolenelProyecto@odata.bind", "/new_rolenelproyectos(" + asignacion.new_rolenelproyecto + ")");
                if (asignacion.new_periodo != null && asignacion.new_periodo != string.Empty)
                    asignacion_hrf.Add("new_Periodo@odata.bind", "/new_periodos(" + asignacion.new_periodo + ")");
                if (asignacion.new_tarifa > 0)
                    asignacion_hrf.Add("new_tarifa", asignacion.new_tarifa);
                if (asignacion.new_cantidadhoras > 0)
                    asignacion_hrf.Add("new_cantidadhoras", asignacion.new_cantidadhoras);
                if (asignacion.statuscode > 0)
                    asignacion_hrf.Add("statuscode", asignacion.statuscode);
                if (asignacion.new_tipodeasignacion > 0)
                    asignacion_hrf.Add("new_tipodeasignacion", asignacion.new_tipodeasignacion);
                if (!string.IsNullOrEmpty(asignacion.new_modificadoporempleado))
                    asignacion_hrf.Add("new_modificadoporempleado@odata.bind", "/new_empleados(" + asignacion.new_modificadoporempleado + ")");
                if (asignacion.new_niveldecriticidad > 0)
                    asignacion_hrf.Add("new_niveldecriticidad", asignacion.new_niveldecriticidad);
                if (!string.IsNullOrEmpty(asignacion.new_fechaestimadainicio))
                    asignacion_hrf.Add("new_fechaestimadainicio", asignacion.new_fechaestimadainicio);
                if (!string.IsNullOrEmpty(asignacion.new_fechaestimadafin))
                    asignacion_hrf.Add("new_fechaestimadafin", asignacion.new_fechaestimadafin);
                if (asignacion.new_tiemporealporestado > 0)
                    asignacion_hrf.Add("new_tiemporealporestado", asignacion.new_tiemporealporestado);
                if (asignacion.new_naturalezadelaasignacion > 0)
                    asignacion_hrf.Add("new_naturalezadelaasignacion", asignacion.new_naturalezadelaasignacion);

                ResponseAPI resultado = await api.CreateRecord("new_asignacions", asignacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                if (asignacion.integrantes != null && asignacion.integrantes.Length > 0)
                {
                    foreach (var integrante in asignacion.integrantes)
                    {
                        JObject integrante_hrf = new();

                        if (!string.IsNullOrEmpty(resultado.descripcion))
                            integrante_hrf.Add("new_Asignacion@odata.bind", "/new_asignacions(" + resultado.descripcion + ")");
                        if (!string.IsNullOrEmpty(integrante.new_integrante))
                            integrante_hrf.Add("new_Integrante@odata.bind", "/systemusers(" + integrante.new_integrante + ")");
                        if (!string.IsNullOrEmpty(integrante.new_rolenasignacion))
                            integrante_hrf.Add("new_rolenasignacion", integrante.new_rolenasignacion);
                        if (!string.IsNullOrEmpty(integrante.new_modificadoporempleado))
                            integrante_hrf.Add("new_modificadoporempleado@odata.bind", "/new_empleados(" + integrante.new_modificadoporempleado + ")");

                        ResponseAPI resultadoIntegrnate = await api.CreateRecord("new_integranteporasignacions", integrante_hrf, credenciales);

                        if (!resultadoIntegrnate.ok) //OK
                        {
                            throw new Exception(resultado.descripcion);
                        }
                    }
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/integrante")]
        public async Task<IActionResult> CrearIntegrante([FromBody] Integrantes integrante)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject integrante_hrf = new();

                if (!string.IsNullOrEmpty(integrante.new_asignacion))
                    integrante_hrf.Add("new_Asignacion@odata.bind", "/new_asignacions(" + integrante.new_asignacion + ")");
                if (!string.IsNullOrEmpty(integrante.new_integrante))
                    integrante_hrf.Add("new_Integrante@odata.bind", "/systemusers(" + integrante.new_integrante + ")");
                if (!string.IsNullOrEmpty(integrante.new_rolenasignacion))
                    integrante_hrf.Add("new_rolenasignacion", integrante.new_rolenasignacion);
                if (!string.IsNullOrEmpty(integrante.new_modificadoporempleado))
                    integrante_hrf.Add("new_modificadoporempleado@odata.bind", "/new_empleados(" + integrante.new_modificadoporempleado + ")");

                
                ResponseAPI resultado = await api.CreateRecord("new_integranteporasignacions", integrante_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/integrante")]
        public async Task<IActionResult> ActualizarIntegrante([FromBody] Integrantes integrante)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject integrante_hrf = new();

                if (!string.IsNullOrEmpty(integrante.new_asignacion))
                    integrante_hrf.Add("new_Asignacion@odata.bind", "/new_asignacions(" + integrante.new_asignacion + ")");
                if (!string.IsNullOrEmpty(integrante.new_integrante))
                    integrante_hrf.Add("new_Integrante@odata.bind", "/systemusers(" + integrante.new_integrante + ")");
                if (!string.IsNullOrEmpty(integrante.new_rolenasignacion))
                    integrante_hrf.Add("new_rolenasignacion", integrante.new_rolenasignacion);
                if (!string.IsNullOrEmpty(integrante.new_modificadoporempleado))
                    integrante_hrf.Add("new_modificadoporempleado@odata.bind", "/new_empleados(" + integrante.new_modificadoporempleado + ")");

                ResponseAPI resultado = await api.UpdateRecord("new_integranteporasignacions", integrante.new_integranteporasignacionid, integrante_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/integrante")]
        public async Task<IActionResult> InactivarIntegrante([FromBody] Integrantes integrante)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (integrante.new_integranteporasignacionid == null || integrante.new_integranteporasignacionid == string.Empty)
                    return BadRequest("El id del integrante esta vacio");

                JObject integrante_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_integranteporasignacions", integrante.new_integranteporasignacionid, integrante_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/hrfactors/adjuntosasignacion")]
        public async Task<IActionResult> AdjuntosAsignacion(string asignacion_id, string titulo = null)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                var archivos = HttpContext.Request.Form.Files;

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
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName },
                            { "objectid_new_asignacion@odata.bind", "/new_asignacions(" + asignacion_id + ")"}
                        };

                        if (titulo != null)
                            annotation.Add("subject", titulo);
                        else
                            annotation.Add("subject", file.FileName);

                        ResponseAPI resultado = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!resultado.ok) //OK
                        {
                            throw new Exception(resultado.descripcion);
                        }
                    }
                }

                return Ok("Adjunto cargado con exito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/hrfactors/actividad")]
        public async Task<IActionResult> ActividadNota([FromBody] ActividadNota actividad)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject annotation = new()
                {
                    { "subject", actividad.titulo },
                    { "notetext", actividad.descripcion },
                };

                if (!string.IsNullOrEmpty(actividad.asignacion_id))
                    annotation.Add("objectid_new_asignacion@odata.bind", "/new_asignacions(" + actividad.asignacion_id + ")");

                ResponseAPI resultado = await api.CreateRecord("annotations", annotation, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/hrfactors/adjuntoactividad")]
        public async Task<IActionResult> ActividadAdjunto(string actividad_id)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                var archivos = HttpContext.Request.Form.Files;

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
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName }
                        };

                        ResponseAPI resultado = await api.UpdateRecord("annotations", actividad_id, annotation, credenciales);

                        if (!resultado.ok) //OK
                        {
                            throw new Exception(resultado.descripcion);
                        }

                        return Ok(resultado.descripcion);
                    }
                }

                return Ok("No se encontraron archivos adjuntos.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/comentario")]
        public async Task<IActionResult> CrearComentario([FromBody] Comentarios comentario)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject _comentario = new()
                {
                    { "source", 1 }
                };

                if (!string.IsNullOrEmpty(comentario.title))
                    _comentario.Add("title", comentario.title);

                if (!string.IsNullOrEmpty(comentario.regardingobjectid))
                    _comentario.Add("regardingobjectid_new_asignacion@odata.bind", "/new_asignacions(" + comentario.regardingobjectid + ")");

                if (!string.IsNullOrEmpty(comentario.comments))
                    _comentario.Add("comments", comentario.comments);

                if (!string.IsNullOrEmpty(comentario.new_empleado))
                    _comentario.Add("new_Empleado@odata.bind", "/new_empleados(" + comentario.new_empleado + ")");

                ResponseAPI resultado = await api.CreateRecord("feedback", _comentario, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/comentario")]
        public async Task<IActionResult> ActualizarComentario([FromBody] Comentarios comentario)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject _comentario = new()
                {
                    { "source", 1 }
                };

                if (!string.IsNullOrEmpty(comentario.title))
                    _comentario.Add("title", comentario.title);

                if (!string.IsNullOrEmpty(comentario.regardingobjectid))
                    _comentario.Add("regardingobjectid_new_asignacion@odata.bind", "/new_asignacions(" + comentario.regardingobjectid + ")");

                if (!string.IsNullOrEmpty(comentario.comments))
                    _comentario.Add("comments", comentario.comments);

                if (!string.IsNullOrEmpty(comentario.new_empleado))
                    _comentario.Add("new_Empleado@odata.bind", "/new_empleados(" + comentario.new_empleado + ")");

                ResponseAPI resultado = await api.UpdateRecord("feedback", comentario.feedbackid, _comentario, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/comentario")]
        public async Task<IActionResult> InactivarComentario([FromBody] Comentarios comentario)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (comentario.feedbackid == null || comentario.feedbackid == string.Empty)
                    return BadRequest("El id del comentario esta vacio");

                JObject asignacion_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("feedback", comentario.feedbackid, asignacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Licencias
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/licencia")]
        public async Task<IActionResult> CrearLicencia([FromBody] Licencias licencia)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject licencia_hrf = new();

                if (licencia.new_empleado != null && licencia.new_empleado != string.Empty)
                    licencia_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + licencia.new_empleado + ")");
                if (licencia.new_tipodelicencia != null && licencia.new_tipodelicencia != string.Empty)
                    licencia_hrf.Add("new_tipodelicencia@odata.bind", "/new_tipodelicencias(" + licencia.new_tipodelicencia + ")");
                if (licencia.new_cantidadhoraslicencia > 0)
                    licencia_hrf.Add("new_cantidadhoraslicencia", licencia.new_cantidadhoraslicencia);
                if (licencia.new_fechadesde != null && licencia.new_fechadesde != string.Empty)
                    licencia_hrf.Add("new_fechadesde", licencia.new_fechadesde);
                if (licencia.new_fechahasta != null && licencia.new_fechahasta != string.Empty)
                    licencia_hrf.Add("new_fechahasta", licencia.new_fechahasta);
                if (licencia.new_horadesde > 0)
                    licencia_hrf.Add("new_horadesde", licencia.new_horadesde);
                if (licencia.new_horahasta > 0)
                    licencia_hrf.Add("new_horahasta", licencia.new_horahasta);
                if (licencia.new_comentarios != null && licencia.new_comentarios != string.Empty)
                    licencia_hrf.Add("new_comentarios",  licencia.new_comentarios);
                if (licencia.new_vacaciones != null && licencia.new_vacaciones != string.Empty)
                    licencia_hrf.Add("new_Vacaciones@odata.bind", "/new_vacacioneses(" + licencia.new_vacaciones + ")");
                if (licencia.new_fechadesolicitud != null && licencia.new_fechadesolicitud != string.Empty)
                    licencia_hrf.Add("new_fechadesolicitud", licencia.new_fechadesolicitud); 

                ResponseAPI resultado = await api.CreateRecord("new_licencias", licencia_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/licencia")]
        public async Task<IActionResult> ActualizarLicencia([FromBody] Licencias licencia)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject licencia_hrf = new();

                if (licencia.new_empleado != null && licencia.new_empleado != string.Empty)
                    licencia_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + licencia.new_empleado + ")");
                if (licencia.new_tipodelicencia != null && licencia.new_tipodelicencia != string.Empty)
                    licencia_hrf.Add("new_tipodelicencia@odata.bind", "/new_tipodelicencias(" + licencia.new_tipodelicencia + ")");
                if (licencia.new_cantidadhoraslicencia > 0)
                    licencia_hrf.Add("new_cantidadhoraslicencia", licencia.new_cantidadhoraslicencia);
                if (licencia.new_fechadesde != null && licencia.new_fechadesde != string.Empty)
                    licencia_hrf.Add("new_fechadesde", licencia.new_fechadesde);
                if (licencia.new_fechahasta != null && licencia.new_fechahasta != string.Empty)
                    licencia_hrf.Add("new_fechahasta", licencia.new_fechahasta);
                if (licencia.new_horadesde > 0)
                    licencia_hrf.Add("new_horadesde", licencia.new_horadesde);
                if (licencia.new_horahasta > 0)
                    licencia_hrf.Add("new_horahasta", licencia.new_horahasta);
                if (licencia.new_comentarios != null && licencia.new_comentarios != string.Empty)
                    licencia_hrf.Add("new_comentarios", licencia.new_comentarios);
                if (licencia.new_vacaciones != null && licencia.new_vacaciones != string.Empty)
                    licencia_hrf.Add("new_Vacaciones@odata.bind", "/new_vacacioneses(" + licencia.new_vacaciones + ")");
                if (licencia.new_fechadesolicitud != null && licencia.new_fechadesolicitud != string.Empty)
                    licencia_hrf.Add("new_fechadesolicitud", licencia.new_fechadesolicitud);

                ResponseAPI resultado = await api.UpdateRecord("new_licencias", licencia.new_licenciaid, licencia_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/licencia")]
        public async Task<IActionResult> InactivarLicencia([FromBody] Licencias licencia)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (licencia.new_licenciaid == null || licencia.new_licenciaid == string.Empty)
                    return BadRequest("El id de la licencia esta vacio");

                JObject licencia_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_licencias", licencia.new_licenciaid, licencia_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Mejoras
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/propuestasymejoras")]
        public async Task<IActionResult> Mejoras([FromBody] PropuestaYmejora propuestaYmejora)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject lipropuestaYmejora_hrf = new();

                if (propuestaYmejora.new_empleado != null && propuestaYmejora.new_empleado != string.Empty)
                    lipropuestaYmejora_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + propuestaYmejora.new_empleado + ")");
                if (propuestaYmejora.new_name?.Length > 0)
                    lipropuestaYmejora_hrf.Add("new_name", propuestaYmejora.new_name);
                if (propuestaYmejora.new_propuesta?.Length > 0)
                    lipropuestaYmejora_hrf.Add("new_propuesta", propuestaYmejora.new_propuesta);

                ResponseAPI resultado = await api.CreateRecord("new_propuestaymejorases", lipropuestaYmejora_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/propuestasymejoras")]
        public async Task<IActionResult> ActualizarMejoras([FromBody] PropuestaYmejora propuestaYmejora)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject lipropuestaYmejora_hrf = new();

                if (propuestaYmejora.new_empleado != null && propuestaYmejora.new_empleado != string.Empty)
                    lipropuestaYmejora_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + propuestaYmejora.new_empleado + ")");
                if (propuestaYmejora.new_name?.Length > 0)
                    lipropuestaYmejora_hrf.Add("new_name", propuestaYmejora.new_name);
                if (propuestaYmejora.new_propuesta?.Length > 0)
                    lipropuestaYmejora_hrf.Add("new_propuesta", propuestaYmejora.new_propuesta);

                ResponseAPI resultado = await api.UpdateRecord("new_propuestaymejorases", propuestaYmejora.new_propuestaymejorasid, lipropuestaYmejora_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/propuestasymejoras")]
        public async Task<IActionResult> InactivarMejoras([FromBody] PropuestaYmejora propuestaYmejora)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (propuestaYmejora.new_propuestaymejorasid == null || propuestaYmejora.new_propuestaymejorasid == string.Empty)
                    return BadRequest("El id de la propuesta esta vacio");

                JObject universidad_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_propuestaymejorases", propuestaYmejora.new_propuestaymejorasid, universidad_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Pagina WEB
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/contacto")]
        public async Task<IActionResult> CrearContacto([FromBody] ContactoHWA contacto)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                string description = "Empresa: {0} - Producto De Interes: {1} - Contacto Desde: {2} - Descripcion: {3}";
                string empresa = string.Empty;
                string productoDeInteres = string.Empty;
                string contactoDesde = string.Empty;

                JObject contacto_hwa = new()
                {
                    { "firstname", contacto.firstname},
                    { "lastname", contacto.lastname},
                    { "emailaddress1", contacto.emailaddress1}
                };
                
                if (!string.IsNullOrEmpty(contacto.jobtitle))
                    contacto_hwa.Add("jobtitle", contacto.jobtitle);

                if (!string.IsNullOrEmpty(contacto.telephone1))
                    contacto_hwa.Add("telephone1", contacto.telephone1);

                string descripcionF = string.Format(description, contacto?.empresa, contacto?.productoDeInteres, contacto?.contactoDesde, contacto?.description);

                if (descripcionF?.Length <= 2000)
                    contacto_hwa.Add("description", descripcionF);
                else
                    contacto_hwa.Add("description", descripcionF[..1999]);

                ResponseAPI resultado = await api.CreateRecord("contacts", contacto_hwa, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/clientePotencial")]
        public async Task<IActionResult> CrearClientePotencial([FromBody] ClientePotencialHWA cliente)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                string description = "Producto De Interes: {0} - Contacto Desde: {1} - Descripcion: {2}";
                string empresa = string.Empty;
                string productoDeInteres = string.Empty;
                string contactoDesde = string.Empty;

                JObject _cliente = new()
                {
                    { "firstname", cliente.firstname},
                    { "lastname", cliente.lastname},
                    { "emailaddress1", cliente.emailaddress1}
                };

                if (!string.IsNullOrEmpty(cliente.jobtitle))
                    _cliente.Add("jobtitle", cliente.jobtitle);

                if (!string.IsNullOrEmpty(cliente.mobilephone))
                    _cliente.Add("mobilephone", cliente.mobilephone);

                if (!string.IsNullOrEmpty(cliente.companyname))
                    _cliente.Add("companyname", cliente.companyname);

                string descripcionF = string.Format(description, cliente?.productoDeInteres, cliente?.contactoDesde, cliente?.description);

                if (descripcionF?.Length <= 2000)
                    _cliente.Add("description", descripcionF);
                else
                    _cliente.Add("description", descripcionF[..1999]);

                if (!string.IsNullOrEmpty(cliente.new_referidodesde))
                    _cliente.Add("new_referidodesde", cliente.new_referidodesde);

                ResponseAPI resultado = await api.CreateRecord("leads", _cliente, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Casos
        //CASOS
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/contactohwa")]
        public async Task<IActionResult> CrearContactoHW([FromBody] ContactoHRCasos contacto)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject _contacto = new();

                if (!string.IsNullOrEmpty(contacto.firstname))
                    _contacto.Add("firstname", contacto.firstname);
                if (!string.IsNullOrEmpty(contacto.lastname))
                    _contacto.Add("lastname", contacto.lastname);
                if (!string.IsNullOrEmpty(contacto.new_cuitcuil))
                    _contacto.Add("new_cuitcuil", contacto.new_cuitcuil);
                if (!string.IsNullOrEmpty(contacto.mobilephone))
                    _contacto.Add("mobilephone", contacto.mobilephone);
                if (!string.IsNullOrEmpty(contacto.new_fechaultimavalidacionidentidadrenaper))
                    _contacto.Add("new_fechaultimavalidacionidentidadrenaper", contacto.new_fechaultimavalidacionidentidadrenaper);
                if (!string.IsNullOrEmpty(contacto.new_resultadoultimavalidacionidentidadrenaper))
                    _contacto.Add("new_resultadoultimavalidacionidentidadrenaper", contacto.new_resultadoultimavalidacionidentidadrenaper);
                if (!string.IsNullOrEmpty(contacto.new_nombrepersonavalidada))
                    _contacto.Add("new_nombrepersonavalidada", contacto.new_nombrepersonavalidada);
                if (!string.IsNullOrEmpty(contacto.new_dnipersonavalidada))
                    _contacto.Add("new_dnipersonavalidada", contacto.new_dnipersonavalidada);

                ResponseAPI resultado = await api.CreateRecord("contacts", _contacto, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/contactohwa")]
        public async Task<IActionResult> ActualizarContactoHW([FromBody] ContactoHRCasos contacto)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject _contacto = new();

                if (!string.IsNullOrEmpty(contacto.firstname))
                    _contacto.Add("firstname", contacto.firstname);
                if (!string.IsNullOrEmpty(contacto.lastname))
                    _contacto.Add("lastname", contacto.lastname);
                if (!string.IsNullOrEmpty(contacto.new_cuitcuil))
                    _contacto.Add("new_cuitcuil", contacto.new_cuitcuil);
                if (!string.IsNullOrEmpty(contacto.mobilephone))
                    _contacto.Add("mobilephone", contacto.mobilephone);
                if (!string.IsNullOrEmpty(contacto.new_fechaultimavalidacionidentidadrenaper))
                    _contacto.Add("new_fechaultimavalidacionidentidadrenaper", contacto.new_fechaultimavalidacionidentidadrenaper);
                if (!string.IsNullOrEmpty(contacto.new_resultadoultimavalidacionidentidadrenaper))
                    _contacto.Add("new_resultadoultimavalidacionidentidadrenaper", contacto.new_resultadoultimavalidacionidentidadrenaper);
                if (!string.IsNullOrEmpty(contacto.new_nombrepersonavalidada))
                    _contacto.Add("new_nombrepersonavalidada", contacto.new_nombrepersonavalidada);
                if (!string.IsNullOrEmpty(contacto.new_dnipersonavalidada))
                    _contacto.Add("new_dnipersonavalidada", contacto.new_dnipersonavalidada);
                if (!string.IsNullOrEmpty(contacto.jobtitle))
                    _contacto.Add("jobtitle", contacto.jobtitle);
                if (!string.IsNullOrEmpty(contacto.emailaddress2))
                    _contacto.Add("emailaddress2", contacto.emailaddress2);

                ResponseAPI resultado = await api.UpdateRecord("contacts", contacto.contactid, _contacto, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/hrfactors/casohwa")]
        public async Task<IActionResult> CrearCaso([FromBody] Caso caso)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 apiDynamicsV2 = new();

                JObject Caso = new()
                {
                    { "primarycontactid@odata.bind", $"/contacts({caso.contactid})"},
                    { "customerid_account@odata.bind", $"/accounts({caso.accountid})"}
                };

                if (caso?.titulo?.Length > 0)
                    Caso.Add("title", caso.titulo);
                if (caso?.asunto?.Length > 0)
                    Caso.Add("subjectid@odata.bind", $"/subjects({caso.asunto})");
                if (caso?.descripcion?.Length > 0)
                    Caso.Add("description", caso.descripcion);
                if (caso?.tipoDeSolicitud?.Length > 0)
                    Caso.Add("new_tipodesolicitud", Convert.ToInt32(caso.tipoDeSolicitud));
                if (caso?.tipoCaso?.Length > 0)
                    Caso.Add("casetypecode", Convert.ToInt32(caso.tipoCaso));

                ResponseAPI resultado = await apiDynamicsV2.CreateRecord("incidents", Caso, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/hrfactors/adjuntoshwa")]
        public async Task<IActionResult> Adjuntos(string caso_id = null)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                var archivos = HttpContext.Request.Form.Files;

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

                        if (caso_id?.Length > 0)
                            annotation.Add("objectid_incident@odata.bind", "/incidents(" + caso_id + ")");

                        ResponseAPI resultado = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!resultado.ok) //OK
                        {
                            throw new Exception(resultado.descripcion);
                        }

                        return Ok(resultado.descripcion);
                    }
                }

                return Ok("No hay adjuntos cargados.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Comentarios
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/comentariosporasignacion")]
        public async Task<IActionResult> CrearComentarioporAsignacion([FromBody] Comentario comentario)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject _comentario = new();

                if (comentario.new_tipo > 0)
                    _comentario.Add("new_tipo", comentario.new_tipo);
                if (!string.IsNullOrEmpty(comentario.new_detalle))
                    _comentario.Add("new_detalle", comentario.new_detalle);
                if (!string.IsNullOrEmpty(comentario.new_asignacion))
                    _comentario.Add("new_Asignacion@odata.bind", "/new_asignacions(" + comentario.new_asignacion + ")");
                if (!string.IsNullOrEmpty(comentario.new_empleadopara))
                    _comentario.Add("new_Empleadopara@odata.bind", "/new_empleados(" + comentario.new_empleadopara + ")");
                if (!string.IsNullOrEmpty(comentario.new_empleadode))
                    _comentario.Add("new_Empleadode@odata.bind", "/new_empleados(" + comentario.new_empleadode + ")"); 
                if (!string.IsNullOrEmpty(comentario.new_para))
                    _comentario.Add("new_Para@odata.bind", "/systemusers(" + comentario.new_para + ")");

                ResponseAPI resultado = await api.CreateRecord("new_comentarioporasignacions", _comentario, credenciales);

                if (!resultado.ok) //ok
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/comentariosporasignacion")]
        public async Task<IActionResult> ActualizarComentarioporAsignacion([FromBody] Comentario comentario)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject _comentario = new();

                if (comentario.new_tipo > 0)
                    _comentario.Add("new_tipo", comentario.new_tipo);
                if (!string.IsNullOrEmpty(comentario.new_detalle))
                    _comentario.Add("new_detalle", comentario.new_detalle);
                if (!string.IsNullOrEmpty(comentario.new_asignacion))
                    _comentario.Add("new_Asignacion@odata.bind", "/new_asignacions(" + comentario.new_asignacion + ")");
                if (!string.IsNullOrEmpty(comentario.new_empleadopara))
                    _comentario.Add("new_Empleadopara@odata.bind", "/new_empleados(" + comentario.new_empleadopara + ")");
                if (!string.IsNullOrEmpty(comentario.new_empleadode))
                    _comentario.Add("new_Empleadode@odata.bind", "/new_empleados(" + comentario.new_empleadode + ")");
                if (!string.IsNullOrEmpty(comentario.new_para))
                    _comentario.Add("new_Para@odata.bind", "/systemusers(" + comentario.new_para + ")");

                ResponseAPI resultado = await api.UpdateRecord("new_comentarioporasignacions", comentario.new_comentarioporasignacionid, _comentario, credenciales);

                if (!resultado.ok) //ok
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/comentariosporasignacion")]
        public async Task<IActionResult> InactivarComentarioporAsignacionn([FromBody] Comentario comentario)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (string.IsNullOrEmpty(comentario.new_comentarioporasignacionid))
                    return BadRequest("el id del comentario esta vacio");

                JObject trayectoria_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_comentarioporasignacions", comentario.new_comentarioporasignacionid,
                    trayectoria_hrf, credenciales);

                if (!resultado.ok) //ok
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/hrfactors/adjuntoscomentariosporasignacion")]
        public async Task<IActionResult> AdjuntosComentariosPorAsignacion(string comentario_id, string titulo = null)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                var archivos = HttpContext.Request.Form.Files;

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
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName },
                            { "objectid_new_comentarioporasignacion@odata.bind", "/new_comentarioporasignacions(" + comentario_id + ")"}
                        };

                        if (titulo != null)
                            annotation.Add("subject", titulo);
                        else
                            annotation.Add("subject", file.FileName);

                        ResponseAPI resultado = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!resultado.ok) //OK
                        {
                            throw new Exception(resultado.descripcion);
                        }
                    }
                }

                return Ok("Adjunto cargado con exito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        //HR ONE CLICK
        #region  Evaluacion Docente
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/evaluaciondocente")]
        public async Task<IActionResult> EvaluacionDocente([FromBody] EvaluacionDocente evaluacionDocente)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();

                if(evaluacionDocente.itemsDeLaEvalucion.Length > 0)
                {
                    foreach (var itemEvaluacion in evaluacionDocente.itemsDeLaEvalucion)
                    {
                        JObject item = new();
                        if(itemEvaluacion.new_respuesta > 0)
                            item.Add("new_respuesta", itemEvaluacion.new_respuesta);
                        if (itemEvaluacion.new_opcionpolivalencia > 0)
                            item.Add("new_opcionpolivalencia", itemEvaluacion.new_opcionpolivalencia);

                        await api.UpdateRecord("new_preguntadelaevaluacions", itemEvaluacion.new_preguntadelaevaluacionid, item, credenciales);
                    }
                } 

                JObject evaluacionDocente_hrf = new();

                if (evaluacionDocente.new_valoracionfinal > 0)
                    evaluacionDocente_hrf.Add("new_valoracionfinal", evaluacionDocente.new_valoracionfinal);
                if(evaluacionDocente.new_cualitativo != null && evaluacionDocente.new_cualitativo != string.Empty)
                    evaluacionDocente_hrf.Add("new_cualitativo", evaluacionDocente.new_cualitativo);

                ResponseAPI resultado = await api.UpdateRecord("new_evaluacions", evaluacionDocente.new_evaluacionid, evaluacionDocente_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/itemevaluaciondocente")]
        public async Task<IActionResult> ItemEvaluacionDocente([FromBody] ItemDeLaEvaluacion itemEvaluacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();

                JObject item = new();
                if (itemEvaluacion.new_respuesta > 0)
                    item.Add("new_respuesta", itemEvaluacion.new_respuesta);
                if (itemEvaluacion.new_opcionpolivalencia > 0)
                    item.Add("new_opcionpolivalencia", itemEvaluacion.new_opcionpolivalencia);

                ResponseAPI resultado = await api.UpdateRecord("new_preguntadelaevaluacions", itemEvaluacion.new_preguntadelaevaluacionid, item, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Licencias Docente
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/asignaciondocente")]
        public async Task<IActionResult> AsignacionDocente([FromBody] AsignacionDocente asignacionDocente)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();

                JObject asignacionDocente_hrf = new();

                if (asignacionDocente.new_acepta > 0)
                    asignacionDocente_hrf.Add("new_acepta", asignacionDocente.new_acepta);

                ResponseAPI resultado = await api.UpdateRecord("new_aceptaciondedivisions", asignacionDocente.new_aceptaciondedivisionid, asignacionDocente_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Docente
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/gestion")]
        public async Task<IActionResult> CrearGestionDocente([FromBody] Gestion gestion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject gestion_hrf = new();

                if (gestion.new_empleado?.Length > 0)
                    gestion_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + gestion.new_empleado + ")");
                if (gestion.new_name?.Length > 0)
                    gestion_hrf.Add("new_name", gestion.new_name);
                if (gestion.new_tipodeincidencia > 0)
                    gestion_hrf.Add("new_tipodeincidencia", gestion.new_tipodeincidencia);
                if (gestion.new_tema?.Length > 0)
                    gestion_hrf.Add("new_Tema@odata.bind", "/new_temas(" + gestion.new_tema + ")");
                if (gestion.new_descripcion?.Length > 0)
                    gestion_hrf.Add("new_descripcion", gestion.new_descripcion);

                ResponseAPI resultado = await api.CreateRecord("new_incidencias", gestion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/planificacionunificada")]
        public async Task<IActionResult> CrearPlanificacionUnificada([FromBody] PlanificacionUnificada planificacionUnificada)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();

                JObject planificacionUnificada_hrf = new()
                {
                    {"new_transferenciadeinvestigacion", planificacionUnificada.new_transferenciadeinvestigacion },
                    {"new_desarrollodeinvestigacion", planificacionUnificada.new_desarrollodeinvestigacion },
                    {"new_extensionpatpic", planificacionUnificada.new_extensionpatpic },
                    {"new_didacticas1ernivel", planificacionUnificada.new_didacticas1ernivel },
                    {"new_didcticas2donivel", planificacionUnificada.new_didcticas2donivel }
                };

                if (planificacionUnificada.new_fechadevencimiento?.Length > 0)
                    planificacionUnificada_hrf.Add("new_fechadevencimiento", planificacionUnificada.new_fechadevencimiento);
                if (planificacionUnificada.new_tipodeextension > 0)
                    planificacionUnificada_hrf.Add("new_tipodeextension", planificacionUnificada.new_tipodeextension);
                if (planificacionUnificada.new_teamteachinginvitado?.Length > 0)
                    planificacionUnificada_hrf.Add("new_TeamTeachingInvitado@odata.bind", "/new_empleados(" + planificacionUnificada.new_teamteachinginvitado + ")");
                if (planificacionUnificada.new_experiencia > 0)
                    planificacionUnificada_hrf.Add("new_experiencia", planificacionUnificada.new_experiencia);
                if (planificacionUnificada.new_tipodidacticas1ernivel?.Length > 0)
                    planificacionUnificada_hrf.Add("new_tipodidacticas1ernivel", planificacionUnificada.new_tipodidacticas1ernivel); //Segundo nivel
                if (planificacionUnificada.new_institutos?.Length > 0)
                    planificacionUnificada_hrf.Add("new_institutos", planificacionUnificada.new_institutos);
                if (planificacionUnificada.new_diferenciales > 0)
                    planificacionUnificada_hrf.Add("new_diferenciales", planificacionUnificada.new_diferenciales);
                if (planificacionUnificada.new_categoriateorica?.Length > 0)
                    planificacionUnificada_hrf.Add("new_categoriateorica", planificacionUnificada.new_categoriateorica);
                if (planificacionUnificada.new_innovaciontecnologica?.Length > 0)
                    planificacionUnificada_hrf.Add("new_innovaciontecnologica", planificacionUnificada.new_innovaciontecnologica);
                if (planificacionUnificada.new_dimension?.Length > 0)
                    planificacionUnificada_hrf.Add("new_dimension", planificacionUnificada.new_dimension);
                if (planificacionUnificada.new_bibliografabsica?.Length > 0)
                    planificacionUnificada_hrf.Add("new_bibliografabsica", planificacionUnificada.new_bibliografabsica);
                if (planificacionUnificada.new_bibliografaampliatoria?.Length > 0)
                    planificacionUnificada_hrf.Add("new_bibliografaampliatoria", planificacionUnificada.new_bibliografaampliatoria);
                if (planificacionUnificada.new_tipodedidcticas1ernivel > 0)
                    planificacionUnificada_hrf.Add("new_tipodedidcticas1ernivel", planificacionUnificada.new_tipodedidcticas1ernivel); //Segundo nivel

                ResponseAPI resultado = await api.UpdateRecord("new_planificacinunificadas", planificacionUnificada.new_planificacinunificadaid, planificacionUnificada_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/incidenciadocente")]
        public async Task<IActionResult> IncidenciasDocentes([FromBody] IncidenciasDocentes incidenciaDocente)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject incidenciaDocente_hrf = new();

                if (incidenciaDocente.new_empleado?.Length > 0)
                    incidenciaDocente_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + incidenciaDocente.new_empleado + ")");
                if (incidenciaDocente.new_name?.Length > 0)
                    incidenciaDocente_hrf.Add("new_name", incidenciaDocente.new_name);
                if (incidenciaDocente.new_tipodeincidencia > 0)
                    incidenciaDocente_hrf.Add("new_tipodeincidencia", incidenciaDocente.new_tipodeincidencia);
                if (incidenciaDocente.new_tema?.Length > 0)
                    incidenciaDocente_hrf.Add("new_Tema@odata.bind", "/new_temas(" + incidenciaDocente.new_tema + ")");

                ResponseAPI resultado = await api.CreateRecord("new_incidenciasdocenteses", incidenciaDocente_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //AreaAcademica
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/areaacademica")]
        public async Task<IActionResult> AreaAcademica([FromBody] AreaAcademica areaAcademica)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject areaAcademica_hrf = new()
                {
                    {"new_conicet",  areaAcademica.new_conicet},
                    {"new_programade",  areaAcademica.new_programade}
                };

                if (areaAcademica.new_informacionadicional?.Length > 0)
                    areaAcademica_hrf.Add("new_informacionadicional", areaAcademica.new_informacionadicional);
                if (areaAcademica.new_subdisciplina?.Length > 0)
                    areaAcademica_hrf.Add("new_Subdisciplina@odata.bind", "/new_subdisciplinas(" + areaAcademica.new_subdisciplina + ")");
                if (areaAcademica.new_cantidadtotaldetesis > 0)
                    areaAcademica_hrf.Add("new_cantidadtotaldetesis", areaAcademica.new_cantidadtotaldetesis);
                if (areaAcademica.new_canttesisdoctoralesdirigactualmente > 0)
                    areaAcademica_hrf.Add("new_canttesisdoctoralesdirigactualmente", areaAcademica.new_canttesisdoctoralesdirigactualmente);
                if (areaAcademica.new_canttesismaestriasdirigyconcult > 0)
                    areaAcademica_hrf.Add("new_canttesismaestriasdirigyconcult", areaAcademica.new_canttesismaestriasdirigyconcult);
                if (areaAcademica.new_cantmaestriasquedirigeactualmente > 0)
                    areaAcademica_hrf.Add("new_cantmaestriasquedirigeactualmente", areaAcademica.new_cantmaestriasquedirigeactualmente);
                if (areaAcademica.new_canttesinasytrabfinales > 0)
                    areaAcademica_hrf.Add("new_canttesinasytrabfinales", areaAcademica.new_canttesinasytrabfinales);
                if (areaAcademica.new_canttesinasytrabajosquedirigeactualmente > 0)
                    areaAcademica_hrf.Add("new_canttesinasytrabajosquedirigeactualmente", areaAcademica.new_canttesinasytrabajosquedirigeactualmente);
                if (areaAcademica.new_experienciaeneducacinadistancia?.Length > 0)
                    areaAcademica_hrf.Add("new_experienciaeneducacinadistancia", areaAcademica.new_experienciaeneducacinadistancia);
                if (areaAcademica.new_nivel > 0)
                    areaAcademica_hrf.Add("new_nivel", areaAcademica.new_nivel);

                ResponseAPI resultado = await api.UpdateRecord("new_empleados",  areaAcademica.new_empleadoid , areaAcademica_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/trayectoriadocente")]
        public async Task<IActionResult> TrayectoriaDocente([FromBody] TrayectoriaDocente trayectoriaDocente)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject trayectoriaDocente_hrf = new();

                if (!string.IsNullOrEmpty(trayectoriaDocente.new_plandetrabajodocente))
                    trayectoriaDocente_hrf.Add("new_PlandeTrabajoDocente@odata.bind", "/new_plandetrabajodocentes(" + trayectoriaDocente.new_plandetrabajodocente + ")");
                if (trayectoriaDocente.new_empleado?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + trayectoriaDocente.new_empleado + ")");
                if (trayectoriaDocente.new_tipodetrayectoria > 0)
                    trayectoriaDocente_hrf.Add("new_tipodetrayectoria", trayectoriaDocente.new_tipodetrayectoria);
                if (trayectoriaDocente.new_institucinacadmica?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_InstitucinAcadmica@odata.bind", "/new_universidads(" + trayectoriaDocente.new_institucinacadmica + ")");
                if (trayectoriaDocente.new_funcion > 0)
                    trayectoriaDocente_hrf.Add("new_funcion", trayectoriaDocente.new_funcion);
                if (trayectoriaDocente.new_designacin > 0)
                    trayectoriaDocente_hrf.Add("new_designacin", trayectoriaDocente.new_designacin);
                if (trayectoriaDocente.new_fechadeinicio?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_fechadeinicio", trayectoriaDocente.new_fechadeinicio);
                if (trayectoriaDocente.new_duracindelcursado > 0)
                    trayectoriaDocente_hrf.Add("new_duracindelcursado", trayectoriaDocente.new_duracindelcursado);
                if (trayectoriaDocente.new_disciplina?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_Disciplina@odata.bind", "/new_disciplinas(" + trayectoriaDocente.new_disciplina + ")");
                if (trayectoriaDocente.new_subdisciplina?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_Subdisciplina@odata.bind", "/new_subdisciplinas(" + trayectoriaDocente.new_subdisciplina + ")");
                if (trayectoriaDocente.new_cargo > 0)
                    trayectoriaDocente_hrf.Add("new_cargo", trayectoriaDocente.new_cargo);
                if (trayectoriaDocente.new_unidadacadmica > 0)
                    trayectoriaDocente_hrf.Add("new_unidadacadmica", trayectoriaDocente.new_unidadacadmica);
                if (trayectoriaDocente.new_otrocargofuncin?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_otrocargofuncin", trayectoriaDocente.new_otrocargofuncin);
                if (trayectoriaDocente.new_fechadefinalizacin?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_fechadefinalizacin", trayectoriaDocente.new_fechadefinalizacin);
                if (trayectoriaDocente.new_dedicacinsemanal > 0)
                    trayectoriaDocente_hrf.Add("new_dedicacinsemanal", trayectoriaDocente.new_dedicacinsemanal);
                if (trayectoriaDocente.statuscode > 0)
                    trayectoriaDocente_hrf.Add("statuscode", trayectoriaDocente.statuscode);
                if (trayectoriaDocente.new_canthoras > 0)
                    trayectoriaDocente_hrf.Add("new_canthoras", trayectoriaDocente.new_canthoras);

                ResponseAPI resultado = await api.CreateRecord("new_trayectoriadocentes", trayectoriaDocente_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/trayectoriadocente")]
        public async Task<IActionResult> ActualizarTrayectoriaDocente([FromBody] TrayectoriaDocente trayectoriaDocente)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject trayectoriaDocente_hrf = new();

                if (trayectoriaDocente.new_empleado?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + trayectoriaDocente.new_empleado + ")");
                if (trayectoriaDocente.new_tipodetrayectoria > 0)
                    trayectoriaDocente_hrf.Add("new_tipodetrayectoria", trayectoriaDocente.new_tipodetrayectoria);
                if (trayectoriaDocente.new_institucinacadmica?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_InstitucinAcadmica@odata.bind", "/new_universidads(" + trayectoriaDocente.new_institucinacadmica + ")");
                if (trayectoriaDocente.new_funcion > 0)
                    trayectoriaDocente_hrf.Add("new_funcion", trayectoriaDocente.new_funcion);
                if (trayectoriaDocente.new_designacin > 0)
                    trayectoriaDocente_hrf.Add("new_designacin", trayectoriaDocente.new_designacin);
                if (trayectoriaDocente.new_fechadeinicio?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_fechadeinicio", trayectoriaDocente.new_fechadeinicio);
                if (trayectoriaDocente.new_duracindelcursado > 0)
                    trayectoriaDocente_hrf.Add("new_duracindelcursado", trayectoriaDocente.new_duracindelcursado);
                if (trayectoriaDocente.new_disciplina?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_Disciplina@odata.bind", "/new_disciplinas(" + trayectoriaDocente.new_disciplina + ")");
                if (trayectoriaDocente.new_subdisciplina?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_Subdisciplina@odata.bind", "/new_subdisciplinas(" + trayectoriaDocente.new_subdisciplina + ")");
                if (trayectoriaDocente.new_cargo > 0)
                    trayectoriaDocente_hrf.Add("new_cargo", trayectoriaDocente.new_cargo);
                if (trayectoriaDocente.new_unidadacadmica > 0)
                    trayectoriaDocente_hrf.Add("new_unidadacadmica", trayectoriaDocente.new_unidadacadmica);
                if (trayectoriaDocente.new_otrocargofuncin?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_otrocargofuncin", trayectoriaDocente.new_otrocargofuncin);
                if (trayectoriaDocente.new_fechadefinalizacin?.Length > 0)
                    trayectoriaDocente_hrf.Add("new_fechadefinalizacin", trayectoriaDocente.new_fechadefinalizacin);
                if (trayectoriaDocente.new_dedicacinsemanal > 0)
                    trayectoriaDocente_hrf.Add("new_dedicacinsemanal", trayectoriaDocente.new_dedicacinsemanal);
                if (trayectoriaDocente.statuscode > 0)
                    trayectoriaDocente_hrf.Add("statuscode", trayectoriaDocente.statuscode);
                if (trayectoriaDocente.new_canthoras > 0)
                    trayectoriaDocente_hrf.Add("new_canthoras", trayectoriaDocente.new_canthoras);

                ResponseAPI resultado = await api.UpdateRecord("new_trayectoriadocentes", trayectoriaDocente.new_trayectoriadocenteid, trayectoriaDocente_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/trayectoriadocente")]
        public async Task<IActionResult> InactivarTrayectoriaDocente([FromBody] TrayectoriaDocente trayectoriaDocente)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (string.IsNullOrEmpty(trayectoriaDocente.new_trayectoriadocenteid))
                    return BadRequest("El id de la trayectoria esta vacio");

                JObject trayectoria_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_trayectoriadocentes", trayectoriaDocente.new_trayectoriadocenteid,
                    trayectoria_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/proyectodeinvestigacion")]
        public async Task<IActionResult> ProyectoDeInvestigacion([FromBody] ProyectoDeInvestigacion proyectoDeInvestigacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject proyectoDeInvestigacion_hrf = new()
                {
                      {"new_experienciaenpi",  proyectoDeInvestigacion.new_experienciaenpi}
                };

                if (!string.IsNullOrEmpty(proyectoDeInvestigacion.new_plandetrabajo))
                    proyectoDeInvestigacion_hrf.Add("new_PlandeTrabajo@odata.bind", "/new_plandetrabajodocentes(" + proyectoDeInvestigacion.new_plandetrabajo + ")");
                if (proyectoDeInvestigacion.new_empleado?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + proyectoDeInvestigacion.new_empleado + ")");
                if (proyectoDeInvestigacion.new_name?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_name", proyectoDeInvestigacion.new_name);
                if (proyectoDeInvestigacion.new_fechadeinicio?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_fechadeinicio", proyectoDeInvestigacion.new_fechadeinicio);
                if (proyectoDeInvestigacion.new_fechadefinalizacin?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_fechadefinalizacin", proyectoDeInvestigacion.new_fechadefinalizacin);
                if (proyectoDeInvestigacion.new_institucion?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Institucion@odata.bind", "/new_universidads(" + proyectoDeInvestigacion.new_institucion + ")");
                if (proyectoDeInvestigacion.new_institucionevaluadora?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_InstitucionEvaluadora@odata.bind", "/new_universidads(" + proyectoDeInvestigacion.new_institucionevaluadora + ")");
                if (proyectoDeInvestigacion.new_institucionfinanciadora?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_InstitucionFinanciadora@odata.bind", "/new_universidads(" + proyectoDeInvestigacion.new_institucionfinanciadora + ")");
                if (proyectoDeInvestigacion.new_carcterdelaparticipcin > 0)
                    proyectoDeInvestigacion_hrf.Add("new_carcterdelaparticipcin", proyectoDeInvestigacion.new_carcterdelaparticipcin);
                if (proyectoDeInvestigacion.new_division?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Division@odata.bind", "/new_divisions(" + proyectoDeInvestigacion.new_division + ")");
                if (proyectoDeInvestigacion.new_metaria?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Metaria@odata.bind", "/new_materias(" + proyectoDeInvestigacion.new_metaria + ")");
                if (proyectoDeInvestigacion.new_matriz?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Matriz@odata.bind", "/new_matrizintegradadeasignaturases(" + proyectoDeInvestigacion.new_matriz + ")");
                if (proyectoDeInvestigacion.new_principalesresultados?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_principalesresultados", proyectoDeInvestigacion.new_principalesresultados);

                ResponseAPI resultado = await api.CreateRecord("new_proyectosdeinvestigacins", proyectoDeInvestigacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/proyectodeinvestigacion")]
        public async Task<IActionResult> ActualizarProyectoDeInvestigacion([FromBody] ProyectoDeInvestigacion proyectoDeInvestigacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject proyectoDeInvestigacion_hrf = new()
                {
                      {"new_experienciaenpi",  proyectoDeInvestigacion.new_experienciaenpi}
                };

                if (!string.IsNullOrEmpty(proyectoDeInvestigacion.new_plandetrabajo))
                    proyectoDeInvestigacion_hrf.Add("new_PlandeTrabajo@odata.bind", "/new_plandetrabajodocentes(" + proyectoDeInvestigacion.new_plandetrabajo + ")");
                if (proyectoDeInvestigacion.new_empleado?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + proyectoDeInvestigacion.new_empleado + ")");
                if (proyectoDeInvestigacion.new_name?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_name", proyectoDeInvestigacion.new_name);
                if (proyectoDeInvestigacion.new_fechadeinicio?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_fechadeinicio", proyectoDeInvestigacion.new_fechadeinicio);
                if (proyectoDeInvestigacion.new_fechadefinalizacin?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_fechadefinalizacin", proyectoDeInvestigacion.new_fechadefinalizacin);
                if (proyectoDeInvestigacion.new_institucion?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Institucion@odata.bind", "/new_universidads(" + proyectoDeInvestigacion.new_institucion + ")");
                if (proyectoDeInvestigacion.new_institucionevaluadora?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_InstitucionEvaluadora@odata.bind", "/new_universidads(" + proyectoDeInvestigacion.new_institucionevaluadora + ")");
                if (proyectoDeInvestigacion.new_institucionfinanciadora?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_InstitucionFinanciadora@odata.bind", "/new_universidads(" + proyectoDeInvestigacion.new_institucionfinanciadora + ")");
                if (proyectoDeInvestigacion.new_carcterdelaparticipcin > 0)
                    proyectoDeInvestigacion_hrf.Add("new_carcterdelaparticipcin", proyectoDeInvestigacion.new_carcterdelaparticipcin);
                if (proyectoDeInvestigacion.new_division?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Division@odata.bind", "/new_divisions(" + proyectoDeInvestigacion.new_division + ")");
                if (proyectoDeInvestigacion.new_metaria?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Metaria@odata.bind", "/new_materias(" + proyectoDeInvestigacion.new_metaria + ")");
                if (proyectoDeInvestigacion.new_matriz?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_Matriz@odata.bind", "/new_matrizintegradadeasignaturases(" + proyectoDeInvestigacion.new_matriz + ")");
                if (proyectoDeInvestigacion.new_principalesresultados?.Length > 0)
                    proyectoDeInvestigacion_hrf.Add("new_principalesresultados", proyectoDeInvestigacion.new_principalesresultados);

                ResponseAPI resultado = await api.UpdateRecord("new_proyectosdeinvestigacins", proyectoDeInvestigacion.new_proyectosdeinvestigacinid, proyectoDeInvestigacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/proyectodeinvestigacion")]
        public async Task<IActionResult> InactivarProyectoDeInvestigacion([FromBody] ProyectoDeInvestigacion proyectoDeInvestigacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (proyectoDeInvestigacion.new_proyectosdeinvestigacinid == null || proyectoDeInvestigacion.new_proyectosdeinvestigacinid == string.Empty)
                    return BadRequest("El id de del proyecto esta vacio");

                JObject proyecto_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_proyectosdeinvestigacins", proyectoDeInvestigacion.new_proyectosdeinvestigacinid, proyecto_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/publicacionenrevista")]
        public async Task<IActionResult> PublicacionEnRevista([FromBody] PublicacionEnRevista publicacionEnRevista)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject publicacionEnRevista_hrf = new()
                {
                      {"new_conarbitraje",  publicacionEnRevista.new_conarbitraje}
                };

                if (!string.IsNullOrEmpty(publicacionEnRevista.new_plandetrabajodocente))
                    publicacionEnRevista_hrf.Add("new_PlandeTrabajoDocente@odata.bind", "/new_plandetrabajodocentes(" + publicacionEnRevista.new_plandetrabajodocente + ")");
                if (publicacionEnRevista.new_empleado?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + publicacionEnRevista.new_empleado + ")");
                if (publicacionEnRevista.new_name?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_name", publicacionEnRevista.new_name);
                if (publicacionEnRevista.new_autores?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_autores", publicacionEnRevista.new_autores); 
                if (publicacionEnRevista.new_revista?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_revista", publicacionEnRevista.new_revista); 
                if (publicacionEnRevista.new_formacindocenteasociadasicorresponde?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_FormacinDocenteAsociadasicorresponde@odata.bind", "/new_formacindocentes(" + publicacionEnRevista.new_formacindocenteasociadasicorresponde + ")");
                if (publicacionEnRevista.new_fechadepublicacin?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_fechadepublicacin", publicacionEnRevista.new_fechadepublicacin);
                if (publicacionEnRevista.new_volumen?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_volumen", publicacionEnRevista.new_volumen); 
                if (publicacionEnRevista.new_paginas > 0)
                    publicacionEnRevista_hrf.Add("new_paginas", publicacionEnRevista.new_paginas); 
                if (publicacionEnRevista.new_sitiowebconinformacin?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_sitiowebconinformacin", publicacionEnRevista.new_sitiowebconinformacin);
                if (publicacionEnRevista.new_palabrasclave?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_palabrasclave", publicacionEnRevista.new_palabrasclave);
                if (publicacionEnRevista.statuscode > 0)
                    publicacionEnRevista_hrf.Add("statuscode", publicacionEnRevista.statuscode);

                ResponseAPI resultado = await api.CreateRecord("new_publicacionenrevistas", publicacionEnRevista_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/publicacionenrevista")]
        public async Task<IActionResult> ActualizarPublicacionEnRevista([FromBody] PublicacionEnRevista publicacionEnRevista)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject publicacionEnRevista_hrf = new()
                {
                      {"new_conarbitraje",  publicacionEnRevista.new_conarbitraje}
                };

                if (!string.IsNullOrEmpty(publicacionEnRevista.new_plandetrabajodocente))
                    publicacionEnRevista_hrf.Add("new_PlandeTrabajoDocente@odata.bind", "/new_plandetrabajodocentes(" + publicacionEnRevista.new_plandetrabajodocente + ")");
                if (publicacionEnRevista.new_empleado?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + publicacionEnRevista.new_empleado + ")");
                if (publicacionEnRevista.new_name?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_name", publicacionEnRevista.new_name);
                if (publicacionEnRevista.new_autores?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_autores", publicacionEnRevista.new_autores);
                if (publicacionEnRevista.new_revista?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_revista", publicacionEnRevista.new_revista);
                if (publicacionEnRevista.new_formacindocenteasociadasicorresponde?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_FormacinDocenteAsociadasicorresponde@odata.bind", "/new_formacindocentes(" + publicacionEnRevista.new_formacindocenteasociadasicorresponde + ")");
                if (publicacionEnRevista.new_fechadepublicacin?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_fechadepublicacin", publicacionEnRevista.new_fechadepublicacin);
                if (publicacionEnRevista.new_volumen?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_volumen", publicacionEnRevista.new_volumen);
                if (publicacionEnRevista.new_paginas > 0)
                    publicacionEnRevista_hrf.Add("new_paginas", publicacionEnRevista.new_paginas);
                if (publicacionEnRevista.new_sitiowebconinformacin?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_sitiowebconinformacin", publicacionEnRevista.new_sitiowebconinformacin);
                if (publicacionEnRevista.new_palabrasclave?.Length > 0)
                    publicacionEnRevista_hrf.Add("new_palabrasclave", publicacionEnRevista.new_palabrasclave);
                if (publicacionEnRevista.statuscode > 0)
                    publicacionEnRevista_hrf.Add("statuscode", publicacionEnRevista.statuscode);

                ResponseAPI resultado = await api.UpdateRecord("new_publicacionenrevistas", publicacionEnRevista.new_publicacionenrevistaid,
                    publicacionEnRevista_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/publicacionenrevista")]
        public async Task<IActionResult> InactivarPublicacionEnRevista([FromBody] PublicacionEnRevista publicacionEnRevista)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (string.IsNullOrEmpty(publicacionEnRevista.new_publicacionenrevistaid))
                    return BadRequest("El id de la publicacion esta vacio");

                JObject publicacion_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_publicacionenrevistas", publicacionEnRevista.new_publicacionenrevistaid,
                    publicacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/presentacionacongresos")]
        public async Task<IActionResult> PresentacionACongresos([FromBody] PresentacionACongresos presentacionACongresos)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject presentacionACongresos_hrf = new();

                if (presentacionACongresos.new_name?.Length > 0)
                    presentacionACongresos_hrf.Add("new_name", presentacionACongresos.new_name);
                if (presentacionACongresos.new_autores?.Length > 0)
                    presentacionACongresos_hrf.Add("new_autores", presentacionACongresos.new_autores);
                if (presentacionACongresos.new_evento?.Length > 0)
                    presentacionACongresos_hrf.Add("new_evento", presentacionACongresos.new_evento);
                if (presentacionACongresos.new_lugarderealizacin?.Length > 0)
                    presentacionACongresos_hrf.Add("new_lugarderealizacin", presentacionACongresos.new_lugarderealizacin);
                if (presentacionACongresos.new_fechaderealizacin?.Length > 0)
                    presentacionACongresos_hrf.Add("new_fechaderealizacin", presentacionACongresos.new_fechaderealizacin);
                if (presentacionACongresos.new_sitiowebconinformacin?.Length > 0)
                    presentacionACongresos_hrf.Add("new_sitiowebconinformacin", presentacionACongresos.new_sitiowebconinformacin);
                if (presentacionACongresos.new_palabrasclave?.Length > 0)
                    presentacionACongresos_hrf.Add("new_palabrasclave", presentacionACongresos.new_palabrasclave);
                if (presentacionACongresos.new_empleado?.Length > 0)
                    presentacionACongresos_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + presentacionACongresos.new_empleado + ")");

                ResponseAPI resultado = await api.CreateRecord("new_presentacinacongresosseminarioses", presentacionACongresos_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/titulodepropiedad")]
        public async Task<IActionResult> TituloDePropiedad([FromBody] TituloDePropiedad tituloDePropiedad)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject tituloDePropiedad_hrf = new();

                if (tituloDePropiedad.new_empleado?.Length > 0)
                    tituloDePropiedad_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + tituloDePropiedad.new_empleado + ")");
                if (tituloDePropiedad.new_name?.Length > 0)
                    tituloDePropiedad_hrf.Add("new_name", tituloDePropiedad.new_name);
                if (tituloDePropiedad.new_titular?.Length > 0)
                    tituloDePropiedad_hrf.Add("new_titular", tituloDePropiedad.new_titular);
                if (tituloDePropiedad.new_fechasolicitud?.Length > 0)
                    tituloDePropiedad_hrf.Add("new_fechasolicitud", tituloDePropiedad.new_fechasolicitud);
                if (tituloDePropiedad.new_fechaotorgamiento?.Length > 0)
                    tituloDePropiedad_hrf.Add("new_fechaotorgamiento", tituloDePropiedad.new_fechaotorgamiento);

                ResponseAPI resultado = await api.CreateRecord("new_ttulodepropiedadintelectuals", tituloDePropiedad_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/desarrollonopasible")]
        public async Task<IActionResult> DesarrolloNoPasible([FromBody] DesarrolloNoPasible desarrolloNoPasible)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject tituloDePropiedad_hrf = new();

                if (desarrolloNoPasible.new_name?.Length > 0)
                    tituloDePropiedad_hrf.Add("new_name", desarrolloNoPasible.new_name);
                if (desarrolloNoPasible.new_descripcion?.Length > 0)
                    tituloDePropiedad_hrf.Add("new_descripcion", desarrolloNoPasible.new_descripcion);
                if (desarrolloNoPasible.new_empleado?.Length > 0)
                    tituloDePropiedad_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + desarrolloNoPasible.new_empleado + ")");

                ResponseAPI resultado = await api.CreateRecord("new_desarrollonopasiblepropiedadintelectuals", tituloDePropiedad_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/reunioncientifico")]
        public async Task<IActionResult> ReunionCientifico([FromBody] ReunionCientifico reunionCientifico)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject reunionCientifico_hrf = new();

                if (reunionCientifico.new_name?.Length > 0)
                    reunionCientifico_hrf.Add("new_name", reunionCientifico.new_name);
                if (reunionCientifico.new_evento?.Length > 0)
                    reunionCientifico_hrf.Add("new_evento", reunionCientifico.new_evento); 
                if (reunionCientifico.new_lugar?.Length > 0)
                    reunionCientifico_hrf.Add("new_lugar", reunionCientifico.new_lugar);
                if (reunionCientifico.new_formadeparticipacin > 0)
                    reunionCientifico_hrf.Add("new_formadeparticipacin", reunionCientifico.new_formadeparticipacin);
                if (reunionCientifico.new_fecha?.Length > 0)
                    reunionCientifico_hrf.Add("new_fecha", reunionCientifico.new_fecha);
                if (reunionCientifico.new_empleado?.Length > 0)
                    reunionCientifico_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + reunionCientifico.new_empleado + ")");

                ResponseAPI resultado = await api.CreateRecord("new_reunioncientficas", reunionCientifico_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/participacionencomites")]
        public async Task<IActionResult> ParticipacionEnComites([FromBody] ParticipacionEnComites participacionEnComites)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject participacionEnComites_hrf = new();

                if (participacionEnComites.new_name?.Length > 0)
                    participacionEnComites_hrf.Add("new_name", participacionEnComites.new_name);
                if (participacionEnComites.new_institucinconvocante?.Length > 0)
                    participacionEnComites_hrf.Add("new_InstitucinConvocante@odata.bind", "/new_universidads(" + participacionEnComites.new_institucinconvocante + ")");
                if (participacionEnComites.new_tipodeevaluacin > 0)
                    participacionEnComites_hrf.Add("new_tipodeevaluacin", participacionEnComites.new_tipodeevaluacin);
                if (participacionEnComites.new_lugar?.Length > 0)
                    participacionEnComites_hrf.Add("new_lugar", participacionEnComites.new_lugar);
                if (participacionEnComites.new_fecha?.Length > 0)
                    participacionEnComites_hrf.Add("new_fecha", participacionEnComites.new_fecha);
                if (participacionEnComites.new_empleado?.Length > 0)
                    participacionEnComites_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + participacionEnComites.new_empleado + ")");

                ResponseAPI resultado = await api.CreateRecord("new_participacionencomitesyjuradoses", participacionEnComites_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Codocente
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/codocente")]
        public async Task<IActionResult> ActualizarCodocente([FromBody] Empleado empleado)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject empleado_hrf = new();
                //Datos Generales
                if (empleado.new_nombredepila != null && empleado.new_nombredepila != string.Empty)
                    empleado_hrf.Add("new_nombredepila", empleado.new_nombredepila);
                if (empleado.new_apellidos != null && empleado.new_apellidos != string.Empty)
                    empleado_hrf.Add("new_apellidos", empleado.new_apellidos);
                if (empleado.new_numerolegajo > 0)
                    empleado_hrf.Add("new_numerolegajo", empleado.new_numerolegajo);
                if (empleado.new_tipodocumento > 0)
                    empleado_hrf.Add("new_tipodocumento", empleado.new_tipodocumento);
                if (empleado.new_nrodocumento != null && empleado.new_nrodocumento != string.Empty)
                    empleado_hrf.Add("new_nrodocumento", empleado.new_nrodocumento);
                if (empleado.new_cuitcuil != null && empleado.new_cuitcuil != string.Empty)
                    empleado_hrf.Add("new_cuitcuil", empleado.new_cuitcuil);
                if (empleado.new_correoelectronico != null && empleado.new_correoelectronico != string.Empty)
                    empleado_hrf.Add("new_correoelectronico", empleado.new_correoelectronico);
                if (empleado.new_estadocivil > 0)
                    empleado_hrf.Add("new_estadocivil", empleado.new_estadocivil);
                if (empleado.new_telefonomovil != null && empleado.new_telefonomovil != string.Empty)
                    empleado_hrf.Add("new_telefonomovil", empleado.new_telefonomovil);
                if (empleado.new_telefonoparticular != null && empleado.new_telefonoparticular != string.Empty)
                    empleado_hrf.Add("new_telefonoparticular", empleado.new_telefonoparticular);
                if (empleado.new_extenciontelefonica != null && empleado.new_extenciontelefonica != string.Empty)
                    empleado_hrf.Add("new_extenciontelefonica", empleado.new_extenciontelefonica);
                if (empleado.new_tipodeincorporacion > 0)
                    empleado_hrf.Add("new_tipodeincorporacion", empleado.new_tipodeincorporacion);
                //Datos de Nacimiento
                if (empleado.new_fechanacimiento != null && empleado.new_fechanacimiento != string.Empty)
                    empleado_hrf.Add("new_fechanacimiento", empleado.new_fechanacimiento);
                if (empleado.new_paisnacimiento != null && empleado.new_paisnacimiento != string.Empty)
                    empleado_hrf.Add("new_paisnacimiento@odata.bind", "/new_paises(" + empleado.new_paisnacimiento + ")");
                if (empleado.new_edad > 0)
                    empleado_hrf.Add("new_edad", empleado.new_edad);
                if (empleado.new_provincianacimiento != null && empleado.new_provincianacimiento != string.Empty)
                    empleado_hrf.Add("new_provincianacimiento@odata.bind", "/new_provincias(" + empleado.new_provincianacimiento + ")");
                //Ultimo Domicilio
                if (empleado.new_calle != null && empleado.new_calle != string.Empty)
                    empleado_hrf.Add("new_calle", empleado.new_calle);
                if (empleado.new_nro != null && empleado.new_nro != string.Empty)
                    empleado_hrf.Add("new_nro", empleado.new_nro);
                if (empleado.new_piso != null && empleado.new_piso != string.Empty)
                    empleado_hrf.Add("new_piso", empleado.new_piso);
                if (empleado.new_depto != null && empleado.new_depto != string.Empty)
                    empleado_hrf.Add("new_depto", empleado.new_depto);
                if (empleado.new_localidad != null && empleado.new_localidad != string.Empty)
                    empleado_hrf.Add("new_localidad@odata.bind", "/new_localidads(" + empleado.new_localidad + ")");
                if (empleado.new_codigopostal != null && empleado.new_codigopostal != string.Empty)
                    empleado_hrf.Add("new_codigopostal", empleado.new_codigopostal);
                if (empleado.new_provincia != null && empleado.new_provincia != string.Empty)
                    empleado_hrf.Add("new_provincia@odata.bind", "/new_provincias(" + empleado.new_provincianacimiento + ")");
                if (empleado.new_pais != null && empleado.new_pais != string.Empty)
                    empleado_hrf.Add("new_pais@odata.bind", "/new_paises(" + empleado.new_pais + ")");


                ResponseAPI respuesta = await api.UpdateRecord("new_empleados", empleado.new_empleadoid, empleado_hrf, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok("Empleado actualizado");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/solicituddomicilio")]
        public async Task<IActionResult> SolicitudActualizaciónDomicilio([FromBody] CambioDomicilio cambioDomicilio)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject cambiodomicilio_hrf = new();

                if (cambioDomicilio.new_name?.Length > 0)
                    cambiodomicilio_hrf.Add("new_name", cambioDomicilio.new_name);
                if (cambioDomicilio.new_empleado?.Length > 0)
                    cambiodomicilio_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + cambioDomicilio.new_empleado + ")");
                if (cambioDomicilio.new_fechasolicitud?.Length > 0)
                    cambiodomicilio_hrf.Add("new_fechasolicitud", cambioDomicilio.new_fechasolicitud);
                if (cambioDomicilio.new_fechavigencia?.Length > 0)
                    cambiodomicilio_hrf.Add("new_fechavigencia", cambioDomicilio.new_fechavigencia);
                if (cambioDomicilio.new_calle?.Length > 0)
                    cambiodomicilio_hrf.Add("new_calle", cambioDomicilio.new_calle);
                if (cambioDomicilio.new_nro?.Length > 0)
                    cambiodomicilio_hrf.Add("new_nro", cambioDomicilio.new_nro);
                if (cambioDomicilio.new_depto?.Length > 0)
                    cambiodomicilio_hrf.Add("new_depto", cambioDomicilio.new_depto);
                if (cambioDomicilio.new_piso?.Length > 0)
                    cambiodomicilio_hrf.Add("new_piso", cambioDomicilio.new_piso);
                if (cambioDomicilio.new_barrio?.Length > 0)
                    cambiodomicilio_hrf.Add("new_barrio", cambioDomicilio.new_barrio);
                if (cambioDomicilio.new_localidad?.Length > 0)
                    cambiodomicilio_hrf.Add("new_Localidad@odata.bind", "/new_localidads(" + cambioDomicilio.new_localidad + ")");
                if (cambioDomicilio.new_provincia?.Length > 0)
                    cambiodomicilio_hrf.Add("new_Provincia@odata.bind", "/new_provincias(" + cambioDomicilio.new_provincia + ")");
                if (cambioDomicilio.new_pais?.Length > 0)
                    cambiodomicilio_hrf.Add("new_Pais@odata.bind", "/new_paises(" + cambioDomicilio.new_pais + ")");
                if (cambioDomicilio.new_estadosolicitud > 0)
                    cambiodomicilio_hrf.Add("new_estadosolicitud", cambioDomicilio.new_estadosolicitud);

                ResponseAPI resultado = await api.CreateRecord("new_solicitudcambiodedomicilios", cambiodomicilio_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/estadocivil")]
        public async Task<IActionResult> EstadoCivil([FromBody] EstadoCivil estadoCivil)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject estadoCivil_hrf = new();

                if (estadoCivil.new_name?.Length > 0)
                    estadoCivil_hrf.Add("new_name", estadoCivil.new_name);
                if (estadoCivil.new_empleado?.Length > 0)
                    estadoCivil_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + estadoCivil.new_empleado + ")");
                if (estadoCivil.new_fechasolicitud?.Length > 0)
                    estadoCivil_hrf.Add("new_fechasolicitud", estadoCivil.new_fechasolicitud);
                if (estadoCivil.new_estadocivil > 0)
                    estadoCivil_hrf.Add("new_estadocivil", estadoCivil.new_estadocivil);
                if (estadoCivil.new_estadosolicitud > 0)
                    estadoCivil_hrf.Add("new_estadosolicitud", estadoCivil.new_estadosolicitud);
                if (estadoCivil.new_fechadevencimiento?.Length > 0)
                    estadoCivil_hrf.Add("new_fechadevencimiento", estadoCivil.new_fechadevencimiento);

                ResponseAPI resultado = await api.CreateRecord("new_solicitudcambioestadocivils", estadoCivil_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/familiar")]
        public async Task<IActionResult> AltaFamiliar([FromBody] Familiar familiar)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject familiar_hrf = new();

                if (familiar.new_name?.Length > 0)
                    familiar_hrf.Add("new_name", familiar.new_name);
                if (familiar.new_empleado?.Length > 0)
                    familiar_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + familiar.new_empleado + ")");
                if (familiar.new_fechasolicitud?.Length > 0)
                    familiar_hrf.Add("new_fechasolicitud", familiar.new_fechasolicitud);
                if (familiar.new_tipodocumento > 0)
                    familiar_hrf.Add("new_tipodocumento", familiar.new_tipodocumento);
                if (familiar.new_nrodocumento?.Length > 0)
                    familiar_hrf.Add("new_nrodocumento", familiar.new_nrodocumento);
                if (familiar.new_apellidos?.Length > 0)
                    familiar_hrf.Add("new_apellidos", familiar.new_apellidos);
                if (familiar.new_nombredepila?.Length > 0)
                    familiar_hrf.Add("new_nombredepila", familiar.new_nombredepila);
                if (familiar.new_fechanacimiento?.Length > 0)
                    familiar_hrf.Add("new_fechanacimiento", familiar.new_fechanacimiento);
                if (familiar.new_genero > 0)
                    familiar_hrf.Add("new_genero", familiar.new_genero);
                if (familiar.new_generoautopercibido > 0)
                    familiar_hrf.Add("new_generoautopercibido", familiar.new_generoautopercibido);
                if (familiar.new_edad > 0)
                    familiar_hrf.Add("new_edad", familiar.new_edad);
                if (familiar.new_cargadefamilia > 0)
                    familiar_hrf.Add("new_cargadefamilia", familiar.new_cargadefamilia);
                if (familiar.new_parentesco?.Length > 0)
                    familiar_hrf.Add("new_Parentesco@odata.bind", "/new_parentescos(" + familiar.new_parentesco + ")");

                ResponseAPI resultado = await api.CreateRecord("new_solicitudaltafamiliars", familiar_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/educacionformal")]
        public async Task<IActionResult> EducacionFormal([FromBody] EducacionFormal educacionFormal)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject educacionFormal_hrf = new();

                if (educacionFormal.new_name?.Length > 0)
                    educacionFormal_hrf.Add("new_name", educacionFormal.new_name);
                if (educacionFormal.new_empleado?.Length > 0)
                    educacionFormal_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + educacionFormal.new_empleado + ")");
                if (educacionFormal.new_titulo?.Length > 0)
                    educacionFormal_hrf.Add("new_titulo", educacionFormal.new_titulo);
                if (educacionFormal.new_universidad?.Length > 0)
                    educacionFormal_hrf.Add("new_Universidad@odata.bind", "/new_universidads(" + educacionFormal.new_universidad + ")");
                if (educacionFormal.new_tipodecarrera > 0)
                    educacionFormal_hrf.Add("new_tipodecarrera", educacionFormal.new_tipodecarrera);
                if (educacionFormal.new_carrerasubdisciplinasconeau?.Length > 0)
                    educacionFormal_hrf.Add("new_CarreraSubdisciplinasConeau@odata.bind", "/new_subdisciplinas(" + educacionFormal.new_carrerasubdisciplinasconeau + ")");
                if (educacionFormal.new_fechaingreso?.Length > 0)
                    educacionFormal_hrf.Add("new_fechaingreso", educacionFormal.new_fechaingreso);
                if (educacionFormal.new_fechaegreso?.Length > 0)
                    educacionFormal_hrf.Add("new_fechaegreso", educacionFormal.new_fechaegreso);

                ResponseAPI resultado = await api.CreateRecord("new_solicitudaltadeeducacionformals", educacionFormal_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/cuentabancaria")]
        public async Task<IActionResult> CuentaBancarial([FromBody] CuentaBancaria cuentaBancaria)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject cuentaBancaria_hrf = new();

                if (cuentaBancaria.new_name?.Length > 0)
                    cuentaBancaria_hrf.Add("new_name", cuentaBancaria.new_name);
                if (cuentaBancaria.new_empleado?.Length > 0)
                    cuentaBancaria_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + cuentaBancaria.new_empleado + ")");
                if (cuentaBancaria.new_banco?.Length > 0)
                    cuentaBancaria_hrf.Add("new_Banco@odata.bind", "/new_bancos(" + cuentaBancaria.new_banco + ")");
                if (cuentaBancaria.new_tipodecuenta > 0)
                    cuentaBancaria_hrf.Add("new_tipodecuenta", cuentaBancaria.new_tipodecuenta);
                if (cuentaBancaria.new_numerocuenta?.Length > 0)
                    cuentaBancaria_hrf.Add("new_numerocuenta", cuentaBancaria.new_numerocuenta);
                if (cuentaBancaria.new_cbu?.Length > 0)
                    cuentaBancaria_hrf.Add("new_cbu", cuentaBancaria.new_cbu);
                if (cuentaBancaria.new_formadepago > 0)
                    cuentaBancaria_hrf.Add("new_formadepago", cuentaBancaria.new_formadepago);

                ResponseAPI resultado = await api.CreateRecord("new_solicitudaltacuentabancarias", cuentaBancaria_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/solicitarliquidacion")]
        public async Task<IActionResult> SolicitarLiquidacion([FromBody] Novedad novedad)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject novedad_hrf = new();

                if (novedad.new_empleado?.Length > 0)
                    novedad_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + novedad.new_empleado + ")");
                if (novedad.new_tipodenovedaddepago.Length > 0)
                    novedad_hrf.Add("new_TipodeNovedaddepago@odata.bind", "/new_tipodenovedaddepagos(" + novedad.new_tipodenovedaddepago + ")");
                if (novedad.new_tipodenovedad > 0)
                    novedad_hrf.Add("new_tipodenovedad", novedad.new_tipodenovedad);
                if (novedad.new_cantidadhoras > 0)
                    novedad_hrf.Add("new_cantidadhoras", novedad.new_cantidadhoras);
                if (novedad.new_cantidad > 0)
                    novedad_hrf.Add("new_cantidad", novedad.new_cantidad);
                if (novedad.new_cantidadfinal > 0)
                    novedad_hrf.Add("new_cantidadfinal", novedad.new_cantidadfinal);
                if (novedad.new_porcentaje > 0)
                    novedad_hrf.Add("new_porcentaje", novedad.new_porcentaje);
                if (novedad.transactioncurrencyid?.Length > 0)
                    novedad_hrf.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + novedad.transactioncurrencyid + ")");
                if (novedad.new_importe > 0)
                    novedad_hrf.Add("new_importe", novedad.new_importe);

                ResponseAPI resultado = await api.CreateRecord("new_novedads", novedad_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/aprobarrechazarnovedad")]
        public async Task<IActionResult> AprobarRechazarNovedad([FromBody] AceptarRechazarNovedad novedad)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject novedad_hrf = new();

                if (novedad.new_tipodenovedaddepago.Length > 0)
                    novedad_hrf.Add("new_TipodeNovedaddepago@odata.bind", "/new_tipodenovedaddepagos(" + novedad.new_tipodenovedaddepago + ")");
                if (novedad.new_fechadenovedad?.Length > 0)
                    novedad_hrf.Add("new_fechadenovedad", novedad.new_fechadenovedad);
                if (novedad.transactioncurrencyid?.Length > 0)
                    novedad_hrf.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + novedad.transactioncurrencyid + ")");
                if (novedad.new_aprobar > 0)
                    novedad_hrf.Add("new_aprobar", novedad.new_aprobar);
                if (novedad.new_fechadeaprobacin?.Length > 0)
                    novedad_hrf.Add("new_fechadeaprobacin", novedad.new_fechadeaprobacin);
                if (novedad.new_motivoderechazo?.Length > 0)
                    novedad_hrf.Add("new_motivoderechazo", novedad.new_motivoderechazo);

                ResponseAPI resultado = await api.UpdateRecord("new_novedads", novedad.new_novedadId, novedad_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/solicitudlicencia")]
        public async Task<IActionResult> SolicitudLicencia([FromBody] Licencia licencia)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject licencia_hrf = new();

                if (licencia.new_empleado?.Length > 0)
                    licencia_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + licencia.new_empleado + ")");
                if (licencia.new_tipodelicencia?.Length > 0)
                    licencia_hrf.Add("new_tipodelicencia@odata.bind", "/new_tipodelicencias(" + licencia.new_tipodelicencia + ")");
                if (licencia.new_fechadesde?.Length > 0)
                    licencia_hrf.Add("new_fechadesde", licencia.new_fechadesde);
                if (licencia.new_diassolicitados > 0)
                    licencia_hrf.Add("new_diassolicitados", licencia.new_diassolicitados);
                if (licencia.new_comentarios?.Length > 0)
                    licencia_hrf.Add("new_comentarios", licencia.new_comentarios);

                ResponseAPI resultado = await api.CreateRecord("new_licencias", licencia_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/aprobarrechazarlicencia")]
        public async Task<IActionResult> AceptarRechazarLicencia([FromBody] AceptarRechazarLicencia licencia)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject licencia_hrf = new()
                {
                    {"new_licenciaprolongada", licencia.new_licenciaprolongada }
                };

                if (licencia.new_tipodelicencia?.Length > 0)
                    licencia_hrf.Add("new_tipodelicencia@odata.bind", "/new_tipodelicencias(" + licencia.new_tipodelicencia + ")");
                if (licencia.new_diassolicitados > 0)
                    licencia_hrf.Add("new_diassolicitados", licencia.new_diassolicitados);
                if (licencia.new_fechadesde?.Length > 0)
                    licencia_hrf.Add("new_fechadesde", licencia.new_fechadesde);
                if (licencia.new_fechahasta?.Length > 0)
                    licencia_hrf.Add("new_fechahasta", licencia.new_fechahasta);
                if (licencia.new_comentarios?.Length > 0)
                    licencia_hrf.Add("new_comentarios", licencia.new_comentarios);
                if (licencia.new_aprobador1?.Length > 0)
                    licencia_hrf.Add("new_Aprobador1@odata.bind", "/new_empleados(" + licencia.new_aprobador1 + ")");
                if (licencia.new_aprobacionsupervisor > 0)
                    licencia_hrf.Add("new_aprobacionsupervisor", licencia.new_aprobacionsupervisor);
                if (licencia.new_validador1?.Length > 0)
                    licencia_hrf.Add("new_Validador1@odata.bind", "/new_empleados(" + licencia.new_validador1 + ")");
                if (licencia.new_validacion1 > 0)
                    licencia_hrf.Add("new_validacion1", licencia.new_validacion1);
                if (licencia.new_validadorrecursoshumanos?.Length > 0)
                    licencia_hrf.Add("new_ValidadorRecursosHumanos@odata.bind", "/new_empleados(" + licencia.new_validadorrecursoshumanos + ")");
                if (licencia.new_aprobacion3 > 0)
                    licencia_hrf.Add("new_aprobacion3", licencia.new_aprobacion3);

                ResponseAPI resultado = await api.UpdateRecord("new_licencias", licencia.new_licenciaId, licencia_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/vacaciones")]
        public async Task<IActionResult> Vacaciones([FromBody] Vacaciones vacaciones)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject vacaciones_hrf = new();

                if (vacaciones.new_empleado?.Length > 0)
                    vacaciones_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + vacaciones.new_empleado + ")");
                if (vacaciones.new_tipodelicencia?.Length > 0)
                    vacaciones_hrf.Add("new_tipodelicencia@odata.bind", "/new_tipodelicencias(" + vacaciones.new_tipodelicencia + ")");
                if (vacaciones.new_subperiodovacacionaldefinido?.Length > 0)
                    vacaciones_hrf.Add("new_SubperiodoVacacionalDefinido@odata.bind", "/new_subperiodovacacionaldefinidos(" + vacaciones.new_subperiodovacacionaldefinido + ")");
                if (vacaciones.new_fechadesolicitud?.Length > 0)
                    vacaciones_hrf.Add("new_fechadesolicitud", vacaciones.new_fechadesolicitud);
                if (vacaciones.new_diassolicitados > 0)
                    vacaciones_hrf.Add("new_diassolicitados", vacaciones.new_diassolicitados);
                if (vacaciones.new_fechadesde?.Length > 0)
                    vacaciones_hrf.Add("new_fechadesde", vacaciones.new_fechadesde);

                ResponseAPI resultado = await api.CreateRecord("new_licencias", vacaciones_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/aprobarrechazarvacaciones")]
        public async Task<IActionResult> AprobarRechazarVacaciones([FromBody] AceptarRechazarVacaciones vacaciones)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject vacaciones_hrf = new();

                if (vacaciones.new_empleado?.Length > 0)
                    vacaciones_hrf.Add("new_empleado@odata.bind", "/new_empleados(" + vacaciones.new_empleado + ")");
                if (vacaciones.new_tipodelicencia?.Length > 0)
                    vacaciones_hrf.Add("new_tipodelicencia@odata.bind", "/new_tipodelicencias(" + vacaciones.new_tipodelicencia + ")");
                if (vacaciones.new_fechadesolicitud?.Length > 0)
                    vacaciones_hrf.Add("new_fechadesolicitud", vacaciones.new_fechadesolicitud);
                if (vacaciones.new_subperiodovacacionaldefinido?.Length > 0)
                    vacaciones_hrf.Add("new_SubperiodoVacacionalDefinido@odata.bind", "/new_subperiodovacacionaldefinidos(" + vacaciones.new_subperiodovacacionaldefinido + ")");
                if (vacaciones.new_diassolicitados > 0)
                    vacaciones_hrf.Add("new_diassolicitados", vacaciones.new_diassolicitados);
                if (vacaciones.new_fechahasta?.Length > 0)
                    vacaciones_hrf.Add("new_fechahasta", vacaciones.new_fechahasta);
                if (vacaciones.new_aprobador1?.Length > 0)
                    vacaciones_hrf.Add("new_Aprobador1@odata.bind", "/new_empleados(" + vacaciones.new_aprobador1 + ")");
                if (vacaciones.new_aprobacionsupervisor > 0)
                    vacaciones_hrf.Add("new_aprobacionsupervisor", vacaciones.new_aprobacionsupervisor);
                if (vacaciones.new_aprobador2?.Length > 0)
                    vacaciones_hrf.Add("new_Aprobador2@odata.bind", "/new_empleados(" + vacaciones.new_aprobador2 + ")");
                if (vacaciones.new_aprobacion2 > 0)
                    vacaciones_hrf.Add("new_aprobacion2", vacaciones.new_aprobacion2);
                if (vacaciones.new_aprobador3?.Length > 0)
                    vacaciones_hrf.Add("new_Aprobador3@odata.bind", "/new_empleados(" + vacaciones.new_aprobador3 + ")");
                if (vacaciones.new_aprobacion3 > 0)
                    vacaciones_hrf.Add("new_aprobacion3", vacaciones.new_aprobacion3);

                ResponseAPI resultado = await api.UpdateRecord("new_licencias", vacaciones.new_licenciaId, vacaciones_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/cancelarvacaciones")]
        public async Task<IActionResult> CancelarVacaciones([FromBody] CancelarVacaciones vacaciones)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject vacaciones_hrf = new();

                if (vacaciones.new_name?.Length > 0)
                    vacaciones_hrf.Add("new_name", vacaciones.new_name);
                if (vacaciones.new_diassolicitados > 0)
                    vacaciones_hrf.Add("new_diassolicitados", vacaciones.new_diassolicitados);

                ResponseAPI resultado = await api.UpdateRecord("new_licencias", vacaciones.new_licenciaId, vacaciones_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/objetivo")]
        public async Task<IActionResult> Objetivo([FromBody] Objetivo objetivo)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject objetivo_hrf = new();

                if (objetivo.new_name?.Length > 0)
                    objetivo_hrf.Add("new_name", objetivo.new_name);
                if (objetivo.new_empleado?.Length > 0)
                    objetivo_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + objetivo.new_empleado + ")");
                if (objetivo.new_evaluaciondepgd.Length > 0)
                    objetivo_hrf.Add("new_EvaluacionDePGD@odata.bind", "/new_evaluaciondedesempenios(" + objetivo.new_evaluaciondepgd + ")");
                if (objetivo.new_objetivo.Length > 0)
                    objetivo_hrf.Add("new_Objetivo@odata.bind", "/new_objetivoses(" + objetivo.new_objetivo + ")");
                if (objetivo.new_resultadoclave?.Length > 0)
                    objetivo_hrf.Add("new_resultadoclave", objetivo.new_resultadoclave);
                if (objetivo.cr20a_deavance > 0)
                    objetivo_hrf.Add("cr20a_deavance", objetivo.cr20a_deavance);
                if (objetivo.new_ponderacionlider > 0)
                    objetivo_hrf.Add("new_ponderacionlider", objetivo.new_ponderacionlider);
                if (objetivo.cr20a_status > 0)
                    objetivo_hrf.Add("cr20a_status", objetivo.cr20a_status);
                //if (objetivo.statuscode > 0)
                //    objetivo_hrf.Add("statuscode", objetivo.statuscode);

                ResponseAPI resultado = await api.CreateRecord("new_objetivodeevaluacions", objetivo_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/objetivo")]
        public async Task<IActionResult> ActualizarObjetivo([FromBody] Objetivo objetivo)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject objetivo_hrf = new();

                if (objetivo.new_name?.Length > 0)
                    objetivo_hrf.Add("new_name", objetivo.new_name);
                if (objetivo.new_empleado?.Length > 0)
                    objetivo_hrf.Add("new_Empleado@odata.bind", "/new_empleados(" + objetivo.new_empleado + ")");
                if (objetivo.new_evaluaciondepgd.Length > 0)
                    objetivo_hrf.Add("new_EvaluacionDePGD@odata.bind", "/new_evaluaciondedesempenios(" + objetivo.new_evaluaciondepgd + ")");
                if (objetivo.new_objetivo.Length > 0)
                    objetivo_hrf.Add("new_Objetivo@odata.bind", "/new_objetivoses(" + objetivo.new_objetivo + ")");
                if (objetivo.new_resultadoclave?.Length > 0)
                    objetivo_hrf.Add("new_resultadoclave", objetivo.new_resultadoclave);
                if (objetivo.cr20a_deavance > 0)
                    objetivo_hrf.Add("cr20a_deavance", objetivo.cr20a_deavance);
                if (objetivo.new_ponderacionlider > 0)
                    objetivo_hrf.Add("new_ponderacionlider", objetivo.new_ponderacionlider);
                if (objetivo.cr20a_status > 0)
                    objetivo_hrf.Add("cr20a_status", objetivo.cr20a_status);
                //if (objetivo.statuscode > 0)
                //    objetivo_hrf.Add("statuscode", objetivo.statuscode);

                ResponseAPI resultado = await api.UpdateRecord("new_objetivodeevaluacions", objetivo.new_objetivodeevaluacionid, objetivo_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/subperiodovacacional")]
        public async Task<IActionResult> SubperiodoVacacional([FromBody] SubperiodoVacacional subVacacional)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject subVacacional_hrf = new();

                if (subVacacional.new_unidadorganizativa?.Length > 0)
                    subVacacional_hrf.Add("new_UnidadOrganizativa@odata.bind", $"/new_empleados({subVacacional.new_unidadorganizativa})");
                if (subVacacional.new_subperiodovacacionalasociado?.Length > 0)
                    subVacacional_hrf.Add("new_SubperiodoVacacionalAsociado@odata.bind", $"/new_empleados({subVacacional.new_subperiodovacacionalasociado})");
                if (subVacacional.new_tamaonomina > 0)
                    subVacacional_hrf.Add("new_tamaonomina", subVacacional.new_tamaonomina);
                if (subVacacional.new_porcentajeacumplirobjetivo > 0)
                    subVacacional_hrf.Add("new_porcentajeacumplirobjetivo", subVacacional.new_porcentajeacumplirobjetivo);
                if (subVacacional.new_porcentajelicenciasenelsubperiodo > 0)
                    subVacacional_hrf.Add("new_porcentajelicenciasenelsubperiodo", subVacacional.new_porcentajelicenciasenelsubperiodo);
                if (subVacacional.new_cantidadlicencias > 0)
                    subVacacional_hrf.Add("new_cantidadlicencias", subVacacional.new_cantidadlicencias);
                if (subVacacional.new_aprobador?.Length > 0)
                    subVacacional_hrf.Add("new_Aprobador@odata.bind", $"/new_empleados({subVacacional.new_aprobador})");
                if (subVacacional.new_aprobacion > 0)
                    subVacacional_hrf.Add("new_aprobacion", subVacacional.new_aprobacion);

                ResponseAPI resultado = await api.UpdateRecord("new_subperiodovacacionalporas", subVacacional.new_subperiodovacacionalporaid, subVacacional_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/evaluacionpgd")]
        public async Task<IActionResult> EvaluacionPGD([FromBody] EvaluacionPGD evaluacionPGD)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject evaluacionPGD_hrf = new();

                if (evaluacionPGD.new_estadodelaautoevaluacin > 0)
                    evaluacionPGD_hrf.Add("new_estadodelaautoevaluacin", evaluacionPGD.new_estadodelaautoevaluacin);
                if (evaluacionPGD.new_estadodelaevaluacindellder > 0)
                    evaluacionPGD_hrf.Add("new_estadodelaevaluacindellder", evaluacionPGD.new_estadodelaevaluacindellder);


                ResponseAPI resultado = await api.UpdateRecord("new_evaluaciondepgds", evaluacionPGD.new_evaluaciondepgdid, evaluacionPGD_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/aironeclick/adjuntosolicitud")]
        public async Task<IActionResult> Adjuntos(string cambioDomicilio_id = null, string estadoCivil_id = null, string familiar_id = null, 
                string educacionFormal_id = null, string cuentaBancaria_id = null, string licencia_id = null, string incidenciaDocente = null, string gestion = null)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                var archivos = HttpContext.Request.Form.Files;

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

                        if (cambioDomicilio_id?.Length > 0)
                            annotation.Add("objectid_new_solicitudcambiodedomicilio@odata.bind", "/new_solicitudcambiodedomicilios(" + cambioDomicilio_id + ")");

                        if (estadoCivil_id?.Length > 0)
                            annotation.Add("objectid_new_solicitudcambioestadocivil@odata.bind", "/new_solicitudcambioestadocivils(" + estadoCivil_id + ")");

                        if (familiar_id?.Length > 0)
                            annotation.Add("objectid_new_solicitudaltafamiliar@odata.bind", "/new_solicitudaltafamiliars(" + familiar_id + ")");

                        if (educacionFormal_id?.Length > 0)
                            annotation.Add("objectid_new_universidadporcontacto@odata.bind", "/new_universidadporcontactos(" + educacionFormal_id + ")");

                        if (cuentaBancaria_id?.Length > 0)
                            annotation.Add("objectid_new_solicitudaltacuentabancaria@odata.bind", "/new_solicitudaltacuentabancarias(" + cuentaBancaria_id + ")");

                        if (licencia_id?.Length > 0)
                            annotation.Add("objectid_new_licencia@odata.bind", "/new_licencias(" + licencia_id + ")");

                        if (incidenciaDocente?.Length > 0)
                            annotation.Add("objectid_new_incidenciasdocentes@odata.bind", "/new_incidenciasdocenteses(" + incidenciaDocente + ")");

                        if (gestion?.Length > 0)
                            annotation.Add("objectid_new_incidencia@odata.bind", "/new_incidencias(" + gestion + ")");


                        ResponseAPI resultado = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!resultado.ok) //OK
                        {
                            throw new Exception(resultado.descripcion);
                        }
                    }
                }

                return Ok("Adjunto cargado con exito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/participanteporevento")]
        public async Task<IActionResult> ActualizarParticipantePorEvento([FromBody] ParticipantePorEventoHRF participante)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                JObject _participante = new();

                if (participante.statuscode > 0)
                    _participante.Add("statuscode", participante.statuscode);
                if (participante.new_aplica > 0)
                    _participante.Add("new_aplica", participante.new_aplica);

                ResponseAPI resultado = await api.UpdateRecord("new_participanteporeventodecapacitacions", participante.new_participanteporeventodecapacitacionid, _participante, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        #endregion
        #region Austral
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/hrfactors/planificacionactividades")]
        public async Task<IActionResult> CrearPlanificacionActividades([FromBody] PlanificacionActivadadesDocente planificacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 apiDynamicsV2 = new();

                JObject Planificacion = new()
                {
                    { "new_PlandeTrabajoDocente@odata.bind", $"/new_plandetrabajodocentes({planificacion.new_plandetrabajodocente})"}
                };

                if (!string.IsNullOrEmpty(planificacion.new_name))
                    Planificacion.Add("new_name", planificacion.new_name);
                if (!string.IsNullOrEmpty(planificacion.new_actividadesdocentesplanificada))
                    Planificacion.Add("new_ActividadesDocentesPlanificada@odata.bind", $"/new_tipodeactividads({planificacion.new_actividadesdocentesplanificada})");
                if (!string.IsNullOrEmpty(planificacion.new_carrera))
                    Planificacion.Add("new_Carrera@odata.bind", $"/new_carrerases({planificacion.new_carrera})");
                if (!string.IsNullOrEmpty(planificacion.new_materia))
                    Planificacion.Add("new_Materia@odata.bind", $"/new_materias({planificacion.new_materia})");

                if (planificacion.new_planificacindepreparacionhorasreloj > 0)
                    Planificacion.Add("new_planificacindepreparacionhorasreloj", planificacion.new_planificacindepreparacionhorasreloj);
                if (planificacion.new_planificacindedictadohorasreloj > 0)
                    Planificacion.Add("new_planificacindedictadohorasreloj", planificacion.new_planificacindedictadohorasreloj);
                if (planificacion.new_planificacindeevaluacinhorasreloj > 0)
                    Planificacion.Add("new_planificacindeevaluacinhorasreloj", planificacion.new_planificacindeevaluacinhorasreloj);
                if (planificacion.new_tipo > 0)
                    Planificacion.Add("new_tipo", planificacion.new_tipo);

                ResponseAPI resultado = await apiDynamicsV2.CreateRecord("new_planificacindeactividadesdeldocentes", Planificacion, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/hrfactors/planificacionactividades")]
        public async Task<IActionResult> ActualizarPlanificacionActividades([FromBody] PlanificacionActivadadesDocente planificacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 apiDynamicsV2 = new();

                JObject Planificacion = new()
                {
                    { "new_PlandeTrabajoDocente@odata.bind", $"/new_plandetrabajodocentes({planificacion.new_plandetrabajodocente})"}
                };

                if (!string.IsNullOrEmpty(planificacion.new_name))
                    Planificacion.Add("new_name", planificacion.new_name);
                if (!string.IsNullOrEmpty(planificacion.new_actividadesdocentesplanificada))
                    Planificacion.Add("new_ActividadesDocentesPlanificada@odata.bind", $"/new_tipodeactividads({planificacion.new_actividadesdocentesplanificada})");
                if (!string.IsNullOrEmpty(planificacion.new_carrera))
                    Planificacion.Add("new_Carrera@odata.bind", $"/new_carrerases({planificacion.new_carrera})");
                if (!string.IsNullOrEmpty(planificacion.new_materia))
                    Planificacion.Add("new_Materia@odata.bind", $"/new_materias({planificacion.new_materia})");
                if (planificacion.new_planificacindepreparacionhorasreloj > 0)
                    Planificacion.Add("new_planificacindepreparacionhorasreloj", planificacion.new_planificacindepreparacionhorasreloj);
                if (planificacion.new_planificacindedictadohorasreloj > 0)
                    Planificacion.Add("new_planificacindedictadohorasreloj", planificacion.new_planificacindedictadohorasreloj);
                if (planificacion.new_planificacindeevaluacinhorasreloj > 0)
                    Planificacion.Add("new_planificacindeevaluacinhorasreloj", planificacion.new_planificacindeevaluacinhorasreloj);
                if (planificacion.new_tipo > 0)
                    Planificacion.Add("new_tipo", planificacion.new_tipo);

                ResponseAPI resultado = await apiDynamicsV2.UpdateRecord("new_planificacindeactividadesdeldocentes", planificacion.new_planificacindeactividadesdeldocenteid, Planificacion, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/planificacionactividades")]
        public async Task<IActionResult> InactivarPlanificacionActividades([FromBody] PlanificacionActivadadesDocente planificacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (planificacion.new_planificacindeactividadesdeldocenteid == null || planificacion.new_planificacindeactividadesdeldocenteid == string.Empty)
                    return BadRequest("El id de la planificacion esta vacio");

                JObject planificacion_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_planificacindeactividadesdeldocentes", planificacion.new_planificacindeactividadesdeldocenteid, planificacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/hrfactors/postulacionnombramientodocente")]
        public async Task<IActionResult> CrearPostulacionNombramiento([FromBody] PostulacionNombramientoDocente postulacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 apiDynamicsV2 = new();

                JObject Postulacion = new()
                {
                    { "new_ConcursoDocente@odata.bind", $"/new_concursodocentes({postulacion.new_concursodocente})"}
                };

                if (!string.IsNullOrEmpty(postulacion.new_docente))
                    Postulacion.Add("new_Docente@odata.bind", $"/new_empleados({postulacion.new_docente})");

                ResponseAPI resultado = await apiDynamicsV2.CreateRecord("new_postulacinaconcursodocentes", Postulacion, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/postulacionnombramientodocente")]
        public async Task<IActionResult> InactivarPostulacionNombramiento([FromBody] PostulacionNombramientoDocente postulacion)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (string.IsNullOrEmpty(postulacion.new_postulacinaconcursodocenteid))
                    return BadRequest("El id de la postulacion esta vacio");

                JObject postulacion_hrf = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_postulacinaconcursodocentes", postulacion.new_postulacinaconcursodocenteid,
                    postulacion_hrf, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
        //PECAM
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/hrfactors/horastrabajadas")]
        public async Task<IActionResult> CrearHorasTrabajadas([FromBody] HorasTrabajadas horasTrabajadas)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 apiDynamicsV2 = new();

                JObject _horasTrabajadas = new()
                {
                    { "new_Empleado@odata.bind", $"/new_empleados({horasTrabajadas.new_empleado})"}
                };

                if (!string.IsNullOrEmpty(horasTrabajadas.new_tipodehoras))
                    _horasTrabajadas.Add("new_Tipodehoras@odata.bind", $"/new_tipodehoras({horasTrabajadas.new_tipodehoras})");
                if (!string.IsNullOrEmpty(horasTrabajadas.new_periododeliquidacion))
                    _horasTrabajadas.Add("new_PeriododeLiquidacion@odata.bind", $"/new_periodos({horasTrabajadas.new_periododeliquidacion})");
                if (!string.IsNullOrEmpty(horasTrabajadas.new_obra))
                    _horasTrabajadas.Add("new_Obra@odata.bind", $"/new_obras({horasTrabajadas.new_obra})");
                if (horasTrabajadas.new_cantidaddehoras > 0)
                    _horasTrabajadas.Add("new_cantidaddehoras", horasTrabajadas.new_cantidaddehoras);
                if (!string.IsNullOrEmpty(horasTrabajadas.new_fechadecarga))
                    _horasTrabajadas.Add("new_fechadecarga", horasTrabajadas.new_fechadecarga);

                ResponseAPI resultado = await apiDynamicsV2.CreateRecord("new_horastrabajadases", _horasTrabajadas, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //PECAM
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut]
        [Route("api/hrfactors/horastrabajadas")]
        public async Task<IActionResult> ActualizarHorasTrabajadas([FromBody] HorasTrabajadas horasTrabajadas)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 apiDynamicsV2 = new();

                JObject _horasTrabajadas = new()
                {
                    { "new_Empleado@odata.bind", $"/new_empleados({horasTrabajadas.new_empleado})"}
                };

                if (!string.IsNullOrEmpty(horasTrabajadas.new_tipodehoras))
                    _horasTrabajadas.Add("new_Tipodehoras@odata.bind", $"/new_tipodehoras({horasTrabajadas.new_tipodehoras})");
                if (!string.IsNullOrEmpty(horasTrabajadas.new_periododeliquidacion))
                    _horasTrabajadas.Add("new_PeriododeLiquidacion@odata.bind", $"/new_periodos({horasTrabajadas.new_periododeliquidacion})");
                if (!string.IsNullOrEmpty(horasTrabajadas.new_obra))
                    _horasTrabajadas.Add("new_Obra@odata.bind", $"/new_obras({horasTrabajadas.new_obra})");
                if (horasTrabajadas.new_cantidaddehoras > 0)
                    _horasTrabajadas.Add("new_cantidaddehoras", horasTrabajadas.new_cantidaddehoras);
                if (!string.IsNullOrEmpty(horasTrabajadas.new_fechadecarga))
                    _horasTrabajadas.Add("new_fechadecarga", horasTrabajadas.new_fechadecarga);

                ResponseAPI resultado = await apiDynamicsV2.UpdateRecord("new_horastrabajadases", horasTrabajadas.new_horastrabajadasid, _horasTrabajadas, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hrfactors/horastrabajadas")]
        public async Task<IActionResult> InactivarHorasTrabajadas([FromBody] HorasTrabajadas horasTrabajadas)
        {
            try
            {
                #region Credenciales
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
                ApiDynamicsV2 api = new();
                if (string.IsNullOrEmpty(horasTrabajadas.new_horastrabajadasid))
                    return BadRequest("El id de la hora trabajada esta vacia");

                JObject _horasTrabajadas = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_horastrabajadases", horasTrabajadas.new_horastrabajadasid,
                    _horasTrabajadas, credenciales);

                if (!resultado.ok) //OK
                {
                    throw new Exception(resultado.descripcion);
                }

                return Ok(resultado.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
