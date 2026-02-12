using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using static Api.Web.Dynamics365.Models.Afip;
using static Api.Web.Dynamics365.Models.HRFactors;
using static Api.Web.Dynamics365.Models.HROneClick;
using static Api.Web.Dynamics365.Models.Lufe;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class HROneClickController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public HROneClickController(ApplicationDbContext context)
        {
            this.context = context;
        }

        #region Empleado
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/empleado")]
        public async Task<IActionResult> CrearEmpleado([FromBody] EmpleadoHR empleado)
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
                    //Educacion.
                    { "new_primariocompleto", empleado.new_primariocompleto },
                    { "new_secundariocompleto", empleado.new_secundariocompleto },
                    { "new_secundarioincompleto", empleado.new_secundarioincompleto },
                    { "new_bachiller", empleado.new_bachiller },
                    { "new_tecnico", empleado.new_tecnico },
                    { "new_peritomercantil", empleado.new_peritomercantil },
                    { "new_sexo", empleado.new_sexo },
                    { "new_turnorotativo", empleado.new_turnorotativo }
                };  
                //Datos Generales
                if (!string.IsNullOrEmpty(empleado.new_empresa))
                    empleado_hrf.Add("new_empresa@odata.bind", $"/new_empresas({empleado.new_empresa})");
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
                //Cargo Actual
                if (!string.IsNullOrEmpty(empleado.new_puesto))
                    empleado_hrf.Add("new_puesto@odata.bind", $"/new_cargos({empleado.new_puesto})");
                //if (!string.IsNullOrEmpty(empleado.new_reportaaid))
                //    empleado_hrf.Add("new_reportaaid@odata.bind", $"/new_empleados({empleado.new_reportaaid})");
                if (!string.IsNullOrEmpty(empleado.new_unidadfuncional))
                    empleado_hrf.Add("new_UnidadFuncional@odata.bind", $"/new_unidadesfuncionaleses({empleado.new_unidadfuncional})");
                if (!string.IsNullOrEmpty(empleado.new_fechainiciocargo))
                    empleado_hrf.Add("new_fechainiciocargo", empleado.new_fechainiciocargo);
                if (!string.IsNullOrEmpty(empleado.new_categoria))
                    empleado_hrf.Add("new_categoria@odata.bind", $"/new_categoriasalarials({empleado.new_categoria})");
                //Antiguedad
                if (!string.IsNullOrEmpty(empleado.new_fechaingreso))
                    empleado_hrf.Add("new_fechaingreso", empleado.new_fechaingreso);
                if (!string.IsNullOrEmpty(empleado.new_fechavacaciones))
                    empleado_hrf.Add("new_fechavacaciones", empleado.new_fechavacaciones);
                if (!string.IsNullOrEmpty(empleado.new_fechadejubilacion))
                    empleado_hrf.Add("new_fechadejubilacion", empleado.new_fechadejubilacion);
                if (!string.IsNullOrEmpty(empleado.new_fechadebaja))
                    empleado_hrf.Add("new_fechadebaja", empleado.new_fechadebaja);
                if (empleado.new_motivodebaja > 0)
                    empleado_hrf.Add("new_motivodebaja", empleado.new_motivodebaja);
                if (empleado.new_salariobrutovigente > 0)
                    empleado_hrf.Add("new_salariobrutovigente", empleado.new_salariobrutovigente);
                if (!string.IsNullOrEmpty(empleado.new_convenio))
                    empleado_hrf.Add("new_convenio@odata.bind", $"/new_convenios({empleado.new_convenio})");
                //Horario Laboral
                if (empleado.new_horadesde > 0)
                    empleado_hrf.Add("new_horadesde", empleado.new_horadesde);
                if (empleado.new_horahasta > 0)
                    empleado_hrf.Add("new_horahasta", empleado.new_horahasta);
                if (!string.IsNullOrEmpty(empleado.new_fechavigenciadesde))
                    empleado_hrf.Add("new_fechavigenciadesde", empleado.new_fechavigenciadesde);
                if (!string.IsNullOrEmpty(empleado.new_fechavigenciahasta))
                    empleado_hrf.Add("new_fechavigenciahasta", empleado.new_fechavigenciahasta);
                //Datos de Nacimiento
                if (empleado.new_fechanacimiento != null && empleado.new_fechanacimiento != string.Empty)
                    empleado_hrf.Add("new_fechanacimiento", empleado.new_fechanacimiento);
                if (empleado.new_paisnacimiento != null && empleado.new_paisnacimiento != string.Empty)
                    empleado_hrf.Add("new_paisnacimiento@odata.bind", "/new_paises(" + empleado.new_paisnacimiento + ")");
                if (empleado.new_edad > 0)
                    empleado_hrf.Add("new_edad", empleado.new_edad);
                if (empleado.new_provincianacimiento != null && empleado.new_provincianacimiento != string.Empty)
                    empleado_hrf.Add("new_provincianacimiento@odata.bind", "/new_provincias(" + empleado.new_provincianacimiento + ")");
                if (!string.IsNullOrEmpty(empleado.new_localidadnacimiento))
                    empleado_hrf.Add("new_localidadnacimiento@odata.bind", $"/new_localidads({empleado.new_localidadnacimiento})");
                //Ultimo Domicilio
                if (empleado.new_calle != null && empleado.new_calle != string.Empty)
                    empleado_hrf.Add("new_calle", empleado.new_calle);
                if (empleado.new_nro != null && empleado.new_nro != string.Empty)
                    empleado_hrf.Add("new_nro", empleado.new_nro);
                if (empleado.new_piso != null && empleado.new_piso != string.Empty)
                    empleado_hrf.Add("new_piso", empleado.new_piso);
                if (empleado.new_depto != null && empleado.new_depto != string.Empty)
                    empleado_hrf.Add("new_depto", empleado.new_depto);
                if (!string.IsNullOrEmpty(empleado.new_sufijo))
                    empleado_hrf.Add("new_sufijo", empleado.new_sufijo);
                if (empleado.new_localidad != null && empleado.new_localidad != string.Empty)
                    empleado_hrf.Add("new_localidad@odata.bind", "/new_localidads(" + empleado.new_localidad + ")");
                if (empleado.new_codigopostal != null && empleado.new_codigopostal != string.Empty)
                    empleado_hrf.Add("new_codigopostal", empleado.new_codigopostal);
                if (empleado.new_provincia != null && empleado.new_provincia != string.Empty)
                    empleado_hrf.Add("new_provincia@odata.bind", "/new_provincias(" + empleado.new_provincianacimiento + ")");
                if (empleado.new_pais != null && empleado.new_pais != string.Empty)
                    empleado_hrf.Add("new_pais@odata.bind", "/new_paises(" + empleado.new_pais + ")");

                ResponseAPI respuesta = await api.CreateRecord("new_empleados", empleado_hrf, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/empleado")]
        public async Task<IActionResult> ActualizarEmpleado([FromBody] EmpleadoHR empleado)
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
                    { "new_sexo", empleado.new_sexo },
                    { "new_turnorotativo", empleado.new_turnorotativo }
                };
                //Datos Generales
                if (!string.IsNullOrEmpty(empleado.new_empresa))
                    empleado_hrf.Add("new_empresa@odata.bind", $"/new_empresas({empleado.new_empresa})");
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
                //Cargo Actual
                if (!string.IsNullOrEmpty(empleado.new_puesto))
                    empleado_hrf.Add("new_puesto@odata.bind", $"/new_cargos({empleado.new_puesto})");
                //if (!string.IsNullOrEmpty(empleado.new_reportaaid))
                //    empleado_hrf.Add("new_ReportaaId@odata.bind", $"/new_empleados({empleado.new_reportaaid})");
                if (!string.IsNullOrEmpty(empleado.new_unidadfuncional))
                    empleado_hrf.Add("new_UnidadFuncional@odata.bind", $"/new_unidadesfuncionaleses({empleado.new_unidadfuncional})");
                if (!string.IsNullOrEmpty(empleado.new_fechainiciocargo))
                    empleado_hrf.Add("new_fechainiciocargo", empleado.new_fechainiciocargo);
                if (!string.IsNullOrEmpty(empleado.new_categoria))
                    empleado_hrf.Add("new_categoria@odata.bind", $"/new_categoriasalarials({empleado.new_categoria})");
                //Antiguedad
                if (!string.IsNullOrEmpty(empleado.new_fechaingreso))
                    empleado_hrf.Add("new_fechaingreso", empleado.new_fechaingreso);
                if (!string.IsNullOrEmpty(empleado.new_fechavacaciones))
                    empleado_hrf.Add("new_fechavacaciones", empleado.new_fechavacaciones);
                if (!string.IsNullOrEmpty(empleado.new_fechadejubilacion))
                    empleado_hrf.Add("new_fechadejubilacion", empleado.new_fechadejubilacion);
                if (!string.IsNullOrEmpty(empleado.new_fechadebaja))
                    empleado_hrf.Add("new_fechadebaja", empleado.new_fechadebaja);
                if (empleado.new_motivodebaja > 0)
                    empleado_hrf.Add("new_motivodebaja", empleado.new_motivodebaja);
                if (empleado.new_salariobrutovigente > 0)
                    empleado_hrf.Add("new_salariobrutovigente", empleado.new_salariobrutovigente);
                if (!string.IsNullOrEmpty(empleado.new_convenio))
                    empleado_hrf.Add("new_convenio@odata.bind", $"/new_convenios({empleado.new_convenio})");
                //Horario Laboral
                if (empleado.new_horadesde > 0)
                    empleado_hrf.Add("new_horadesde", empleado.new_horadesde);
                if (empleado.new_horahasta > 0)
                    empleado_hrf.Add("new_horahasta", empleado.new_horahasta);
                if (!string.IsNullOrEmpty(empleado.new_fechavigenciadesde))
                    empleado_hrf.Add("new_fechavigenciadesde", empleado.new_fechavigenciadesde);
                if (!string.IsNullOrEmpty(empleado.new_fechavigenciahasta))
                    empleado_hrf.Add("new_fechavigenciahasta", empleado.new_fechavigenciahasta);
                //Datos de Nacimiento
                if (empleado.new_fechanacimiento != null && empleado.new_fechanacimiento != string.Empty)
                    empleado_hrf.Add("new_fechanacimiento", empleado.new_fechanacimiento);
                if (empleado.new_paisnacimiento != null && empleado.new_paisnacimiento != string.Empty)
                    empleado_hrf.Add("new_paisnacimiento@odata.bind", "/new_paises(" + empleado.new_paisnacimiento + ")");
                if (empleado.new_edad > 0)
                    empleado_hrf.Add("new_edad", empleado.new_edad);
                if (empleado.new_provincianacimiento != null && empleado.new_provincianacimiento != string.Empty)
                    empleado_hrf.Add("new_provincianacimiento@odata.bind", "/new_provincias(" + empleado.new_provincianacimiento + ")");
                if (!string.IsNullOrEmpty(empleado.new_localidadnacimiento))
                    empleado_hrf.Add("new_localidadnacimiento@odata.bind", $"/new_localidads({empleado.new_localidadnacimiento})");
                //Ultimo Domicilio
                if (empleado.new_calle != null && empleado.new_calle != string.Empty)
                    empleado_hrf.Add("new_calle", empleado.new_calle);
                if (empleado.new_nro != null && empleado.new_nro != string.Empty)
                    empleado_hrf.Add("new_nro", empleado.new_nro);
                if (empleado.new_piso != null && empleado.new_piso != string.Empty)
                    empleado_hrf.Add("new_piso", empleado.new_piso);
                if (empleado.new_depto != null && empleado.new_depto != string.Empty)
                    empleado_hrf.Add("new_depto", empleado.new_depto);
                if (!string.IsNullOrEmpty(empleado.new_sufijo))
                    empleado_hrf.Add("new_sufijo", empleado.new_sufijo);
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

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/empleado")]
        public async Task<IActionResult> InactivarEmpleado([FromBody] EmpleadoHR empleado)
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
                if (empleado.new_empleadoid == null || empleado.new_empleadoid == string.Empty)
                    return BadRequest("El id del empleado esta vacio");

                JObject empleado_hroc = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_empleados", empleado.new_empleadoid, empleado_hroc, credenciales);

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
        #region Evaluacion
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/evaluacion")]
        public async Task<IActionResult> CrearEvaluacion([FromBody] EvaluacionHR evaluacion)
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
                JObject evaluacion_hrf = new();

                if (!string.IsNullOrEmpty(evaluacion.new_fechadevencimiento))
                    evaluacion_hrf.Add("new_fechadevencimiento", evaluacion.new_fechadevencimiento);
                if (evaluacion.new_tipodeevaluacion > 0)
                    evaluacion_hrf.Add("new_tipodeevaluacion", evaluacion.new_tipodeevaluacion);
                if (!string.IsNullOrEmpty(evaluacion.new_evaluador))
                    evaluacion_hrf.Add("new_evaluado@odata.bind", $"/new_empleados({evaluacion.new_evaluado})");
                if (!string.IsNullOrEmpty(evaluacion.new_evaluador))
                    evaluacion_hrf.Add("new_evaluador@odata.bind", $"/new_empleados({evaluacion.new_evaluador})");
                if (evaluacion.statuscode > 0)
                    evaluacion_hrf.Add("statuscode", evaluacion.statuscode);

                ResponseAPI respuesta = await api.CreateRecord("new_evaluacions", evaluacion_hrf, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/evaluacion")]
        public async Task<IActionResult> ActualizarEvaluacion([FromBody] EvaluacionHR evaluacion)
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
                JObject evaluacion_hrf = new();

                if (!string.IsNullOrEmpty(evaluacion.new_fechadevencimiento))
                    evaluacion_hrf.Add("new_fechadevencimiento", evaluacion.new_fechadevencimiento);
                if (evaluacion.new_tipodeevaluacion > 0)
                    evaluacion_hrf.Add("new_tipodeevaluacion", evaluacion.new_tipodeevaluacion);
                if (!string.IsNullOrEmpty(evaluacion.new_evaluador))
                    evaluacion_hrf.Add("new_evaluador@odata.bind", $"/new_empleados({evaluacion.new_evaluador})");
                if (!string.IsNullOrEmpty(evaluacion.new_evaluado))
                    evaluacion_hrf.Add("new_evaluado@odata.bind", $"/new_empleados({evaluacion.new_evaluado})");
                if (evaluacion.statuscode > 0)
                    evaluacion_hrf.Add("statuscode", evaluacion.statuscode);

                ResponseAPI respuesta = await api.UpdateRecord("new_evaluacions", evaluacion.new_evaluacionid, evaluacion_hrf, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok("Evaluacion actualizada");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/evaluacion")]
        public async Task<IActionResult> InactivarEvaluacion([FromBody] EvaluacionHR evaluacion)
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
                if (evaluacion.new_evaluacionid == null || evaluacion.new_evaluacionid == string.Empty)
                    return BadRequest("El id de la evaluacion esta vacio");

                JObject evaluacion_hroc = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_evaluacions", evaluacion.new_evaluacionid, evaluacion_hroc, credenciales);

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
        #region RequerimientoDeCapacitacion
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/requerimientodecapacitacion")]
        public async Task<IActionResult> CrearRequerimiento([FromBody] RequerimientoDeCapacitacionHR requerimiento)
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
                JObject requerimiento_hr = new();

                if (!string.IsNullOrEmpty(requerimiento.new_name))
                    requerimiento_hr.Add("new_name", requerimiento.new_name);
                if (!string.IsNullOrEmpty(requerimiento.new_curso))
                    requerimiento_hr.Add("new_curso@odata.bind", $"/new_cursos({requerimiento.new_curso})");
                if (!string.IsNullOrEmpty(requerimiento.new_fecharequerimiento))
                    requerimiento_hr.Add("new_fecharequerimiento", requerimiento.new_fecharequerimiento);
                if (!string.IsNullOrEmpty(requerimiento.new_fechapretendida))
                    requerimiento_hr.Add("new_fechapretendida", requerimiento.new_fechapretendida);
                if (!string.IsNullOrEmpty(requerimiento.new_solicitadopor))
                    requerimiento_hr.Add("new_solicitadopor@odata.bind", $"/new_empleados({requerimiento.new_solicitadopor})");
                //if (!string.IsNullOrEmpty(requerimiento.ownerid))
                //    requerimiento_hr.Add("ownerid@odata.bind", $"/new_empleados({requerimiento.ownerid})");
                if (requerimiento.new_horasenclase > 0)
                    requerimiento_hr.Add("new_horasenclase", requerimiento.new_horasenclase);
                if (requerimiento.new_duracionendias > 0)
                    requerimiento_hr.Add("new_duracionendias", requerimiento.new_duracionendias);
                if (requerimiento.statuscode > 0)
                    requerimiento_hr.Add("statuscode", requerimiento.statuscode);
                if (!string.IsNullOrEmpty(requerimiento.new_motivodelaprioridad))
                    requerimiento_hr.Add("new_motivodelaprioridad", requerimiento.new_motivodelaprioridad);
                if (!string.IsNullOrEmpty(requerimiento.new_observaciones))
                    requerimiento_hr.Add("new_observaciones", requerimiento.new_observaciones); 
                if (requerimiento.new_prioridad > 0)
                    requerimiento_hr.Add("new_prioridad", requerimiento.new_prioridad);
                //if (!string.IsNullOrEmpty(requerimiento.new_eventoid))
                //    requerimiento_hr.Add("new_eventoid@odata.bind", $"/new_eventodecapacitacions({requerimiento.new_eventoid})"); 
                if (!string.IsNullOrEmpty(requerimiento.new_aprobador1))
                    requerimiento_hr.Add("new_Aprobador1@odata.bind", $"/new_empleados({requerimiento.new_aprobador1})");
                if (!string.IsNullOrEmpty(requerimiento.new_aprobador2))
                    requerimiento_hr.Add("new_Aprobador2@odata.bind", $"/new_empleados({requerimiento.new_aprobador2})");
                if (requerimiento.new_aprueba1 > 0)
                    requerimiento_hr.Add("new_aprueba1", requerimiento.new_aprueba1);
                if (requerimiento.new_aprueba2 > 0)
                    requerimiento_hr.Add("new_aprueba2", requerimiento.new_aprueba2);

                ResponseAPI respuesta = await api.CreateRecord("new_requerimientodecapacitacions", requerimiento_hr, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/requerimientodecapacitacion")]
        public async Task<IActionResult> ActualizarRequerimiento([FromBody] RequerimientoDeCapacitacionHR requerimiento)
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
                JObject requerimiento_hr = new();

                if (!string.IsNullOrEmpty(requerimiento.new_name))
                    requerimiento_hr.Add("new_name", requerimiento.new_name);
                if (!string.IsNullOrEmpty(requerimiento.new_curso))
                    requerimiento_hr.Add("new_curso@odata.bind", $"/new_cursos({requerimiento.new_curso})");
                if (!string.IsNullOrEmpty(requerimiento.new_fecharequerimiento))
                    requerimiento_hr.Add("new_fecharequerimiento", requerimiento.new_fecharequerimiento);
                if (!string.IsNullOrEmpty(requerimiento.new_fechapretendida))
                    requerimiento_hr.Add("new_fechapretendida", requerimiento.new_fechapretendida);
                if (!string.IsNullOrEmpty(requerimiento.new_solicitadopor))
                    requerimiento_hr.Add("new_solicitadopor@odata.bind", $"/new_empleados({requerimiento.new_solicitadopor})");
                //if (!string.IsNullOrEmpty(requerimiento.ownerid))
                //    requerimiento_hr.Add("ownerid@odata.bind", $"/new_empleados({requerimiento.ownerid})");
                if (requerimiento.new_horasenclase > 0)
                    requerimiento_hr.Add("new_horasenclase", requerimiento.new_horasenclase);
                if (requerimiento.new_duracionendias > 0)
                    requerimiento_hr.Add("new_duracionendias", requerimiento.new_duracionendias);
                if (requerimiento.statuscode > 0)
                    requerimiento_hr.Add("statuscode", requerimiento.statuscode);
                if (!string.IsNullOrEmpty(requerimiento.new_motivodelaprioridad))
                    requerimiento_hr.Add("new_motivodelaprioridad", requerimiento.new_motivodelaprioridad);
                if (!string.IsNullOrEmpty(requerimiento.new_observaciones))
                    requerimiento_hr.Add("new_observaciones", requerimiento.new_observaciones);
                if (requerimiento.new_prioridad > 0)
                    requerimiento_hr.Add("new_prioridad", requerimiento.new_prioridad);
                //if (!string.IsNullOrEmpty(requerimiento.new_eventoid))
                //    requerimiento_hr.Add("new_eventoid@odata.bind", $"/new_eventodecapacitacions({requerimiento.new_eventoid})"); 
                if (!string.IsNullOrEmpty(requerimiento.new_aprobador1))
                    requerimiento_hr.Add("new_Aprobador1@odata.bind", $"/new_empleados({requerimiento.new_aprobador1})");
                if (!string.IsNullOrEmpty(requerimiento.new_aprobador2))
                    requerimiento_hr.Add("new_Aprobador2@odata.bind", $"/new_empleados({requerimiento.new_aprobador2})");
                if (requerimiento.new_aprueba1 > 0)
                    requerimiento_hr.Add("new_aprueba1", requerimiento.new_aprueba1);
                if (requerimiento.new_aprueba2 > 0)
                    requerimiento_hr.Add("new_aprueba2", requerimiento.new_aprueba2);

                ResponseAPI respuesta = await api.UpdateRecord("new_requerimientodecapacitacions", requerimiento.new_requerimientodecapacitacionid, requerimiento_hr, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/requerimientodecapacitacion")]
        public async Task<IActionResult> InactivarRequerimiento([FromBody] RequerimientoDeCapacitacionHR requerimiento)
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
                if (requerimiento.new_requerimientodecapacitacionid == null || requerimiento.new_requerimientodecapacitacionid == string.Empty)
                    return BadRequest("El id del requerimiento esta vacio");

                JObject requerimiento_hr = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_requerimientodecapacitacions", requerimiento.new_requerimientodecapacitacionid, requerimiento_hr, credenciales);

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
        #region PlanDeCapacitacion
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/plandecapacitacion")]
        public async Task<IActionResult> CrearPlanDeCapacitacion([FromBody] PlanDeCapacitacionHR planDeCapacitacion)
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
                JObject planDeCapacitacion_hr = new();

                if (!string.IsNullOrEmpty(planDeCapacitacion.new_name))
                    planDeCapacitacion_hr.Add("new_name", planDeCapacitacion.new_name);
                if (!string.IsNullOrEmpty(planDeCapacitacion.new_periodo))
                    planDeCapacitacion_hr.Add("new_Periodo@odata.bind", $"/new_periodos({planDeCapacitacion.new_periodo})");
                if (planDeCapacitacion.new_presupuestoasignado > 0)
                    planDeCapacitacion_hr.Add("new_presupuestoasignado", planDeCapacitacion.new_presupuestoasignado);
          
                ResponseAPI respuesta = await api.CreateRecord("new_plandecapacitacions", planDeCapacitacion_hr, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/plandecapacitacion")]
        public async Task<IActionResult> ActualizarPlanDeCapacitacion([FromBody] PlanDeCapacitacionHR planDeCapacitacion)
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
                JObject planDeCapacitacion_hr = new();

                if (!string.IsNullOrEmpty(planDeCapacitacion.new_name))
                    planDeCapacitacion_hr.Add("new_name", planDeCapacitacion.new_name);
                if (!string.IsNullOrEmpty(planDeCapacitacion.new_periodo))
                    planDeCapacitacion_hr.Add("new_Periodo@odata.bind", $"/new_periodos({planDeCapacitacion.new_periodo})");
                if (planDeCapacitacion.new_presupuestoasignado > 0)
                    planDeCapacitacion_hr.Add("new_presupuestoasignado", planDeCapacitacion.new_presupuestoasignado);

                ResponseAPI respuesta = await api.UpdateRecord("new_plandecapacitacions", planDeCapacitacion.new_plandecapacitacionid, planDeCapacitacion_hr, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/plandecapacitacion")]
        public async Task<IActionResult> InactivarPlan([FromBody] PlanDeCapacitacionHR planDeCapacitacion)
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
                if (planDeCapacitacion.new_plandecapacitacionid == null || planDeCapacitacion.new_plandecapacitacionid == string.Empty)
                    return BadRequest("El id del plan de capacitacion esta vacio");

                JObject planDeCapacitacion_hr = new()
                {
                    { "statecode", 1 }
                };

                ResponseAPI resultado = await api.UpdateRecord("new_plandecapacitacions", planDeCapacitacion.new_plandecapacitacionid, planDeCapacitacion_hr, credenciales);

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
        #region Curso
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/curso")]
        public async Task<IActionResult> CrearCurso([FromBody] Curso curso)
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
                JObject curso_hr = new()
                {
                    { "new_requiereeficacia", curso.new_requiereeficacia },
                    { "new_elearning", curso.new_elearning },
                    { "new_interna", curso.new_interna },
                    { "new_incompany", curso.new_incompany },
                    { "new_externa", curso.new_externa },
                };

                if (!string.IsNullOrEmpty(curso.new_name))
                    curso_hr.Add("new_name", curso.new_name);
                if (curso.new_accion > 0)
                    curso_hr.Add("new_accion", curso.new_accion);
                if (curso.statuscode > 0)
                    curso_hr.Add("statuscode", curso.statuscode);
                if (curso.new_duracion > 0)
                    curso_hr.Add("new_duracion", curso.new_duracion);
                if (!string.IsNullOrEmpty(curso.new_objetivo))
                    curso_hr.Add("new_objetivo", curso.new_objetivo);
                if (!string.IsNullOrEmpty(curso.new_url))
                    curso_hr.Add("new_url", curso.new_url);
                if (!string.IsNullOrEmpty(curso.new_contenido))
                    curso_hr.Add("new_contenido", curso.new_contenido);

                 ResponseAPI respuesta = await api.CreateRecord("new_cursos", curso_hr, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/curso")]
        public async Task<IActionResult> ActualizarCurso([FromBody] Curso curso)
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
                JObject curso_hr = new()
                {
                    { "new_requiereeficacia", curso.new_requiereeficacia },
                    { "new_elearning", curso.new_elearning },
                    { "new_interna", curso.new_interna },
                    { "new_incompany", curso.new_incompany },
                    { "new_externa", curso.new_externa },
                };

                if (!string.IsNullOrEmpty(curso.new_name))
                    curso_hr.Add("new_name", curso.new_name);
                if (curso.new_accion > 0)
                    curso_hr.Add("new_accion", curso.new_accion);
                if (curso.statuscode > 0)
                    curso_hr.Add("statuscode", curso.statuscode);
                if (curso.new_duracion > 0)
                    curso_hr.Add("new_duracion", curso.new_duracion);
                if (!string.IsNullOrEmpty(curso.new_objetivo))
                    curso_hr.Add("new_objetivo", curso.new_objetivo);
                if (!string.IsNullOrEmpty(curso.new_url))
                    curso_hr.Add("new_url", curso.new_url);
                if (!string.IsNullOrEmpty(curso.new_contenido))
                    curso_hr.Add("new_contenido", curso.new_contenido);

                ResponseAPI respuesta = await api.UpdateRecord("new_cursos", curso.new_cursoid, curso_hr, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/curso")]
        public async Task<IActionResult> InactivarCurso([FromBody] Curso curso)
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
                if (curso.new_cursoid == null || curso.new_cursoid == string.Empty)
                    return BadRequest("El id del curso esta vacio");

                JObject curso_hr = new()
                {
                    { "statecode", 1 }
                };

                ResponseAPI resultado = await api.UpdateRecord("new_cursos", curso.new_cursoid, curso_hr, credenciales);

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
        #region Encuesta
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/encuesta")]
        public async Task<IActionResult> CrearEncuesta([FromBody] Encuesta encuesta)
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
                JObject encuesta_hr = new();

                if (!string.IsNullOrEmpty(encuesta.new_fecha))
                    encuesta_hr.Add("new_fecha", encuesta.new_fecha);
                if (!string.IsNullOrEmpty(encuesta.new_fechavencimiento))
                    encuesta_hr.Add("new_fechavencimiento", encuesta.new_fechavencimiento);
                if (!string.IsNullOrEmpty(encuesta.new_encuestado))
                    encuesta_hr.Add("new_Encuestado@odata.bind", $"/new_empleados({encuesta.new_encuestado})");
                if (encuesta.statuscode > 0)
                    encuesta_hr.Add("statuscode", encuesta.statuscode);
                if (!string.IsNullOrEmpty(encuesta.new_name))
                    encuesta_hr.Add("new_name", encuesta.new_name);
                if (!string.IsNullOrEmpty(encuesta.new_template))
                    encuesta_hr.Add("new_Template@odata.bind", $"/new_templatedeencuestas({encuesta.new_template})");
                if (!string.IsNullOrEmpty(encuesta.new_introduccion))
                    encuesta_hr.Add("new_introduccion", encuesta.new_introduccion);
                if (!string.IsNullOrEmpty(encuesta.new_comentarios))
                    encuesta_hr.Add("new_comentarios", encuesta.new_comentarios);
                //if (encuesta.new_puntajeidealencuesta > 0)
                //    encuesta_hr.Add("new_puntajeidealencuesta", encuesta.new_puntajeidealencuesta);
                //if (encuesta.new_satisfacciondelaencuesta > 0)
                //    encuesta_hr.Add("new_satisfacciondelaencuesta", encuesta.new_satisfacciondelaencuesta);

                ResponseAPI respuesta = await api.CreateRecord("new_encuestas", encuesta_hr, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/encuesta")]
        public async Task<IActionResult> ActualizarEncuesta([FromBody] Encuesta encuesta)
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
                JObject encuesta_hr = new();

                if (!string.IsNullOrEmpty(encuesta.new_fecha))
                    encuesta_hr.Add("new_fecha", encuesta.new_fecha);
                if (!string.IsNullOrEmpty(encuesta.new_fechavencimiento))
                    encuesta_hr.Add("new_fechavencimiento", encuesta.new_fechavencimiento);
                if (!string.IsNullOrEmpty(encuesta.new_encuestado))
                    encuesta_hr.Add("new_Encuestado@odata.bind", $"/new_empleados({encuesta.new_encuestado})");
                if (encuesta.statuscode > 0)
                    encuesta_hr.Add("statuscode", encuesta.statuscode);
                if (!string.IsNullOrEmpty(encuesta.new_name))
                    encuesta_hr.Add("new_name", encuesta.new_name);
                if (!string.IsNullOrEmpty(encuesta.new_template))
                    encuesta_hr.Add("new_Template@odata.bind", $"/new_templatedeencuestas({encuesta.new_template})");
                if (!string.IsNullOrEmpty(encuesta.new_introduccion))
                    encuesta_hr.Add("new_introduccion", encuesta.new_introduccion);
                if (!string.IsNullOrEmpty(encuesta.new_comentarios))
                    encuesta_hr.Add("new_comentarios", encuesta.new_comentarios);
                //if (encuesta.new_puntajeidealencuesta > 0)
                //    encuesta_hr.Add("new_puntajeidealencuesta", encuesta.new_puntajeidealencuesta);
                //if (encuesta.new_satisfacciondelaencuesta > 0)
                //    encuesta_hr.Add("new_satisfacciondelaencuesta", encuesta.new_satisfacciondelaencuesta);

                ResponseAPI respuesta = await api.UpdateRecord("new_encuestas", encuesta.new_encuestaid, encuesta_hr, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/encuesta")]
        public async Task<IActionResult> InactivarEncuesta([FromBody] Encuesta encuesta)
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
                if (encuesta.new_encuestaid == null || encuesta.new_encuestaid == string.Empty)
                    return BadRequest("El id de la encuesta esta vacio");

                JObject encuesta_hr = new()
                {
                    { "statecode", 1 }
                };

                ResponseAPI resultado = await api.UpdateRecord("new_encuestas", encuesta.new_encuestaid, encuesta_hr, credenciales);

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
        #region Alumno
        //
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/evaluaciondocente")]
        public async Task<IActionResult> EvaluacionDocente([FromBody] EvaluacionDocenteHR evaluacionDocente)
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
                JObject _evaluacionDocente = new();

                if (!string.IsNullOrEmpty(evaluacionDocente.new_cualitativo))
                    _evaluacionDocente.Add("new_cualitativo", evaluacionDocente.new_cualitativo);
                if (evaluacionDocente.new_valoracionfinal > 0)
                    _evaluacionDocente.Add("new_valoracionfinal", evaluacionDocente.new_valoracionfinal);

                ResponseAPI respuesta = await api.UpdateRecord("new_evaluacions", evaluacionDocente.new_evaluacionid, _evaluacionDocente, credenciales);

                if (!respuesta.ok) //OK
                {
                    throw new Exception(respuesta.descripcion);
                }

                return Ok(respuesta.descripcion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
        #region Evaluacion
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/evaluacionpgd")]
        public async Task<IActionResult> EvaluacionPGD([FromBody] EvaluacionPGDHROC evaluacionPGD)
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
                JObject evaluacionPGD_hrf = new()
                {
                    { "new_autoevaluacion", evaluacionPGD.new_autoevaluacion }, 
                    { "new_elcolaboradorhacambiadosupropsito", evaluacionPGD.new_elcolaboradorhacambiadosupropsito }
                };

                if (evaluacionPGD.new_estadodelaautoevaluacin > 0)
                    evaluacionPGD_hrf.Add("new_estadodelaautoevaluacin", evaluacionPGD.new_estadodelaautoevaluacin);
                if (evaluacionPGD.new_estadodelaevaluacindellder > 0)
                    evaluacionPGD_hrf.Add("new_estadodelaevaluacindellder", evaluacionPGD.new_estadodelaevaluacindellder);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_ciclo))
                    evaluacionPGD_hrf.Add("new_Ciclo@odata.bind", $"/new_ciclodepgds({evaluacionPGD.new_ciclo})"); 
                if (!string.IsNullOrEmpty(evaluacionPGD.new_lder))
                    evaluacionPGD_hrf.Add("new_Lder@odata.bind", $"/new_empleados({evaluacionPGD.new_lder})");
                if (!string.IsNullOrEmpty(evaluacionPGD.new_evaluado))
                    evaluacionPGD_hrf.Add("new_Evaluado@odata.bind", $"/new_empleados({evaluacionPGD.new_evaluado})");
                if (evaluacionPGD.statuscode > 0)
                    evaluacionPGD_hrf.Add("statuscode", evaluacionPGD.statuscode);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_fechainicioautoevaluacion))
                    evaluacionPGD_hrf.Add("new_fechainicioautoevaluacion", evaluacionPGD.new_fechainicioautoevaluacion);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_fechavencimientoautoevaluacin))
                    evaluacionPGD_hrf.Add("new_fechavencimientoautoevaluacin", evaluacionPGD.new_fechavencimientoautoevaluacin);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_comentariosyobservaciones))
                    evaluacionPGD_hrf.Add("new_comentariosyobservaciones", evaluacionPGD.new_comentariosyobservaciones);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_comentariosyobservacionesdeautoevaluacion))
                    evaluacionPGD_hrf.Add("new_comentariosyobservacionesdeautoevaluacion", evaluacionPGD.new_comentariosyobservacionesdeautoevaluacion);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_miproposito))
                    evaluacionPGD_hrf.Add("new_miproposito", evaluacionPGD.new_miproposito);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_nuevoproposito))
                    evaluacionPGD_hrf.Add("new_nuevoproposito", evaluacionPGD.new_nuevoproposito);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_comentariosyobervacionesdesuproposito))
                    evaluacionPGD_hrf.Add("new_comentariosyobervacionesdesuproposito", evaluacionPGD.new_comentariosyobervacionesdesuproposito);
                if (evaluacionPGD.new_interesendesarrolloprox6meses > 0)
                    evaluacionPGD_hrf.Add("new_interesendesarrolloprox6meses", evaluacionPGD.new_interesendesarrolloprox6meses);
                if (evaluacionPGD.new_interesendesarrolloprximos12meses > 0)
                    evaluacionPGD_hrf.Add("new_interesendesarrolloprximos12meses", evaluacionPGD.new_interesendesarrolloprximos12meses);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_puestoaspiracionalprximos6meses))
                    evaluacionPGD_hrf.Add("new_PuestoAspiracionalprximos6meses@odata.bind", $"/new_cargos({evaluacionPGD.new_puestoaspiracionalprximos6meses})");
                if (!string.IsNullOrEmpty(evaluacionPGD.new_posicinaspiracionalprximos6meses))
                    evaluacionPGD_hrf.Add("new_PosicinAspiracionalprximos6meses@odata.bind", $"/new_posicions({evaluacionPGD.new_posicinaspiracionalprximos6meses})");
                if (!string.IsNullOrEmpty(evaluacionPGD.new_unidadorganizativaaspiracionalprximos6mes))
                    evaluacionPGD_hrf.Add("new_UnidadOrganizativaAspiracionalprximos6mes@odata.bind", $"/new_unidadorganigramas({evaluacionPGD.new_unidadorganizativaaspiracionalprximos6mes})");
                if (!string.IsNullOrEmpty(evaluacionPGD.new_puestoaspiracionalprximos12meses))
                    evaluacionPGD_hrf.Add("new_PuestoAspiracionalprximos12meses@odata.bind", $"/new_cargos({evaluacionPGD.new_puestoaspiracionalprximos12meses})");
                if (!string.IsNullOrEmpty(evaluacionPGD.new_posicinaspiracionalprximos12meses))
                    evaluacionPGD_hrf.Add("new_PosicinAspiracionalprximos12meses@odata.bind", $"/new_posicions({evaluacionPGD.new_posicinaspiracionalprximos12meses})");
                if (!string.IsNullOrEmpty(evaluacionPGD.new_unidadorganizativaaspiracionalprximos12me))
                    evaluacionPGD_hrf.Add("new_UnidadOrganizativaAspiracionalprximos12me@odata.bind", $"/new_unidadorganigramas({evaluacionPGD.new_unidadorganizativaaspiracionalprximos12me})");
                if (!string.IsNullOrEmpty(evaluacionPGD.new_comentariosyobservacionesdemiproposito))
                    evaluacionPGD_hrf.Add("new_comentariosyobservacionesdemiproposito", evaluacionPGD.new_comentariosyobservacionesdemiproposito);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_fechainicioevaluacindellider))
                    evaluacionPGD_hrf.Add("new_fechainicioevaluacindellider", evaluacionPGD.new_fechainicioevaluacindellider);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_fechavencimientoevaluacindellider))
                    evaluacionPGD_hrf.Add("new_fechavencimientoevaluacindellider", evaluacionPGD.new_fechavencimientoevaluacindellider);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_comentariosyobservacionesdelaevaluacion))
                    evaluacionPGD_hrf.Add("new_comentariosyobservacionesdelaevaluacion", evaluacionPGD.new_comentariosyobservacionesdelaevaluacion);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_comentariosyobservacionesaspeval))
                    evaluacionPGD_hrf.Add("new_comentariosyobservacionesaspeval", evaluacionPGD.new_comentariosyobservacionesaspeval);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_fechainiciofeedback))
                    evaluacionPGD_hrf.Add("new_fechainiciofeedback", evaluacionPGD.new_fechainiciofeedback);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_fechavencimientofeedback))
                    evaluacionPGD_hrf.Add("new_fechavencimientofeedback", evaluacionPGD.new_fechavencimientofeedback);
                if(!string.IsNullOrEmpty(evaluacionPGD.new_fechayhoradelencuentrodefeedback))
                    evaluacionPGD_hrf.Add("new_fechayhoradelencuentrodefeedback", evaluacionPGD.new_fechayhoradelencuentrodefeedback);
                if (evaluacionPGD.new_estadodelencuentrodefeedback > 0)
                    evaluacionPGD_hrf.Add("new_estadodelencuentrodefeedback", evaluacionPGD.new_estadodelencuentrodefeedback);
                if (evaluacionPGD.new_scoreglobal > 0)
                    evaluacionPGD_hrf.Add("new_scoreglobal", evaluacionPGD.new_scoreglobal);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_comentariosyobservacionesdelfeedback))
                    evaluacionPGD_hrf.Add("new_comentariosyobservacionesdelfeedback", evaluacionPGD.new_comentariosyobservacionesdelfeedback);
                if (!string.IsNullOrEmpty(evaluacionPGD.new_comentariosyobservacionesdelfeedbacklider))
                    evaluacionPGD_hrf.Add("new_comentariosyobservacionesdelfeedbacklider", evaluacionPGD.new_comentariosyobservacionesdelfeedbacklider);

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

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/objetivo")]
        public async Task<IActionResult> CrearObjetivo([FromBody] GestiónDeObjetivosHROC objetivo)
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

                if (!string.IsNullOrEmpty(objetivo.new_evaluaciondepgd))
                    objetivo_hrf.Add("new_EvaluacionDePGD@odata.bind", "/new_evaluaciondedesempenios(" + objetivo.new_evaluaciondepgd + ")");
                if (!string.IsNullOrEmpty(objetivo.new_objetivo))
                    objetivo_hrf.Add("new_Objetivo@odata.bind", "/new_objetivoses(" + objetivo.new_objetivo + ")");
                if (objetivo.new_tipodeobjetivo > 0)
                    objetivo_hrf.Add("new_tipodeobjetivo", objetivo.new_tipodeobjetivo);
                if (!string.IsNullOrEmpty(objetivo.new_perspectivadenegocio))
                    objetivo_hrf.Add("new_PerspectivadeNegocio@odata.bind", "/new_perspectivadenegocios(" + objetivo.new_perspectivadenegocio + ")");
                if (!string.IsNullOrEmpty(objetivo.new_plazo))
                    objetivo_hrf.Add("new_plazo", objetivo.new_plazo);
                if (objetivo.new_ponderacionlider > 0)
                    objetivo_hrf.Add("new_ponderacionlider", objetivo.new_ponderacionlider);
                if (!string.IsNullOrEmpty(objetivo.new_fuentedemedicion))
                    objetivo_hrf.Add("new_fuentedemedicion", objetivo.new_fuentedemedicion);
                if (!string.IsNullOrEmpty(objetivo.new_piso))
                    objetivo_hrf.Add("new_piso", objetivo.new_piso);
                if (!string.IsNullOrEmpty(objetivo.new_target))
                    objetivo_hrf.Add("new_target", objetivo.new_target);
                if (!string.IsNullOrEmpty(objetivo.new_techo))
                    objetivo_hrf.Add("new_techo", objetivo.new_techo);
                if (!string.IsNullOrEmpty(objetivo.new_name))
                    objetivo_hrf.Add("new_name", objetivo.new_name);

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
        [Route("api/hroneclick/objetivo")]
        public async Task<IActionResult> ActualizarObjetivo([FromBody] GestiónDeObjetivosHROC objetivo)
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

                if (!string.IsNullOrEmpty(objetivo.new_evaluaciondepgd))
                    objetivo_hrf.Add("new_EvaluacionDePGD@odata.bind", "/new_evaluaciondedesempenios(" + objetivo.new_evaluaciondepgd + ")");
                if (!string.IsNullOrEmpty(objetivo.new_objetivo))
                    objetivo_hrf.Add("new_Objetivo@odata.bind", "/new_objetivoses(" + objetivo.new_objetivo + ")");
                if (objetivo.new_tipodeobjetivo > 0)
                    objetivo_hrf.Add("new_tipodeobjetivo", objetivo.new_tipodeobjetivo);
                if (!string.IsNullOrEmpty(objetivo.new_perspectivadenegocio))
                    objetivo_hrf.Add("new_PerspectivadeNegocio@odata.bind", "/new_perspectivadenegocios(" + objetivo.new_perspectivadenegocio + ")");
                if (!string.IsNullOrEmpty(objetivo.new_plazo))
                    objetivo_hrf.Add("new_plazo", objetivo.new_plazo);
                if (objetivo.new_ponderacionlider > 0)
                    objetivo_hrf.Add("new_ponderacionlider", objetivo.new_ponderacionlider);
                if (!string.IsNullOrEmpty(objetivo.new_fuentedemedicion))
                    objetivo_hrf.Add("new_fuentedemedicion", objetivo.new_fuentedemedicion);
                if (!string.IsNullOrEmpty(objetivo.new_piso))
                    objetivo_hrf.Add("new_piso", objetivo.new_piso);
                if (!string.IsNullOrEmpty(objetivo.new_target))
                    objetivo_hrf.Add("new_target", objetivo.new_target);
                if (!string.IsNullOrEmpty(objetivo.new_techo))
                    objetivo_hrf.Add("new_techo", objetivo.new_techo);
                if (!string.IsNullOrEmpty(objetivo.new_name))
                    objetivo_hrf.Add("new_name", objetivo.new_name);

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

        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/objetivo")]
        public async Task<IActionResult> InactivarObjetivo([FromBody] GestiónDeObjetivosHROC objetivo)
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
                if (objetivo.new_objetivodeevaluacionid == null || objetivo.new_objetivodeevaluacionid == string.Empty)
                    return BadRequest("El id del objetivo esta vacio");

                JObject _objetivo = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_objetivodeevaluacions", objetivo.new_objetivodeevaluacionid, _objetivo, credenciales);

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
        [Route("api/hroneclick/itempgd")]
        public async Task<IActionResult> CrearItemPGD([FromBody] ItemDePGDHROC item)
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
                JObject _itemPGD = new();

                if (!string.IsNullOrEmpty(item.new_evaluaciondepgd))
                    _itemPGD.Add("new_EvaluaciondePGD@odata.bind", "/new_evaluaciondepgds(" + item.new_evaluaciondepgd + ")");
                if (item.new_tipodeitemdeevaluacion > 0)
                    _itemPGD.Add("new_tipodeitemdeevaluacion", item.new_tipodeitemdeevaluacion);
                if (!string.IsNullOrEmpty(item.new_competencia))
                    _itemPGD.Add("new_competencia@odata.bind", "/new_competencias(" + item.new_competencia + ")");
                if (item.new_valoracin > 0)
                    _itemPGD.Add("new_valoracin", item.new_valoracin);
                if (item.new_valoraciondellider > 0)
                    _itemPGD.Add("new_valoraciondellider", item.new_valoraciondellider);
                if (item.new_tipodeinstancia > 0)
                    _itemPGD.Add("new_tipodeinstancia", item.new_tipodeinstancia);
                if (!string.IsNullOrEmpty(item.new_plandesucesin))
                    _itemPGD.Add("new_PlandeSucesin@odata.bind", "/new_plandesucesions(" + item.new_plandesucesin + ")");

                ResponseAPI resultado = await api.CreateRecord("new_itemdeevaluaciondedesempeos", _itemPGD, credenciales);

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
        [Route("api/hroneclick/itempgd")]
        public async Task<IActionResult> ActualizarItemPGD([FromBody] ItemDePGDHROC item)
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
                JObject _itemPGD = new();

                if (!string.IsNullOrEmpty(item.new_evaluaciondepgd))
                    _itemPGD.Add("new_EvaluaciondePGD@odata.bind", "/new_evaluaciondepgds(" + item.new_evaluaciondepgd + ")");
                if (item.new_tipodeitemdeevaluacion > 0)
                    _itemPGD.Add("new_tipodeitemdeevaluacion", item.new_tipodeitemdeevaluacion);
                if (!string.IsNullOrEmpty(item.new_competencia))
                    _itemPGD.Add("new_competencia@odata.bind", "/new_competencias(" + item.new_competencia + ")");
                if (item.new_valoracin > 0)
                    _itemPGD.Add("new_valoracin", item.new_valoracin);
                if (item.new_valoraciondellider > 0)
                    _itemPGD.Add("new_valoraciondellider", item.new_valoraciondellider);
                if (item.new_tipodeinstancia > 0)
                    _itemPGD.Add("new_tipodeinstancia", item.new_tipodeinstancia);
                if (!string.IsNullOrEmpty(item.new_plandesucesin))
                    _itemPGD.Add("new_PlandeSucesin@odata.bind", "/new_plandesucesions(" + item.new_plandesucesin + ")");

                ResponseAPI resultado = await api.UpdateRecord("new_itemdeevaluaciondedesempeos", item.new_itemdeevaluaciondedesempeoid, _itemPGD, credenciales);

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
        [Route("api/hroneclick/itempgd")]
        public async Task<IActionResult> InactivarItemPGD([FromBody] ItemDePGDHROC item)
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
                if (item.new_itemdeevaluaciondedesempeoid == null || item.new_itemdeevaluaciondedesempeoid == string.Empty)
                    return BadRequest("El id del item esta vacio");

                JObject _item = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_itemdeevaluaciondedesempeos", item.new_itemdeevaluaciondedesempeoid, _item, credenciales);

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
        [Route("api/hroneclick/metaprioritaria")]
        public async Task<IActionResult> CrearMetaPrioritaria([FromBody] MetaPrioritariaHROC meta)
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
                JObject _meta = new();

                if (!string.IsNullOrEmpty(meta.new_evaluacionpgd))
                    _meta.Add("new_EvaluacionPGD@odata.bind", "/new_evaluaciondedesempenios(" + meta.new_evaluacionpgd + ")");
                if (!string.IsNullOrEmpty(meta.new_name))
                    _meta.Add("new_name", meta.new_name);
                if (!string.IsNullOrEmpty(meta.new_accion))
                    _meta.Add("new_accion", meta.new_accion);
                if (!string.IsNullOrEmpty(meta.new_evidencia))
                    _meta.Add("new_evidencia", meta.new_evidencia);
                if (!string.IsNullOrEmpty(meta.new_fechadesde))
                    _meta.Add("new_fechadesde", meta.new_fechadesde);
                if (!string.IsNullOrEmpty(meta.new_fechahasta))
                    _meta.Add("new_fechahasta", meta.new_fechahasta);

                ResponseAPI resultado = await api.CreateRecord("new_metaprioritarias", _meta, credenciales);

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
        [Route("api/hroneclick/metaprioritaria")]
        public async Task<IActionResult> ActualizarMetaPrioritaria([FromBody] MetaPrioritariaHROC meta)
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
                JObject _meta = new();

                if (!string.IsNullOrEmpty(meta.new_evaluacionpgd))
                    _meta.Add("new_EvaluacionPGD@odata.bind", "/new_evaluaciondedesempenios(" + meta.new_evaluacionpgd + ")");
                if (!string.IsNullOrEmpty(meta.new_name))
                    _meta.Add("new_name", meta.new_name);
                if (!string.IsNullOrEmpty(meta.new_accion))
                    _meta.Add("new_accion", meta.new_accion);
                if (!string.IsNullOrEmpty(meta.new_evidencia))
                    _meta.Add("new_evidencia", meta.new_evidencia);
                if (!string.IsNullOrEmpty(meta.new_fechadesde))
                    _meta.Add("new_fechadesde", meta.new_fechadesde);
                if (!string.IsNullOrEmpty(meta.new_fechahasta))
                    _meta.Add("new_fechahasta", meta.new_fechahasta);

                ResponseAPI resultado = await api.UpdateRecord("new_metaprioritarias", meta.new_metaprioritariaid, _meta, credenciales);

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
        [Route("api/hroneclick/metaprioritaria")]
        public async Task<IActionResult> InactivarMetaPrioritaria([FromBody] MetaPrioritariaHROC meta)
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
                if (meta.new_metaprioritariaid == null || meta.new_metaprioritariaid == string.Empty)
                    return BadRequest("El id de la meta esta vacio");

                JObject _meta = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_metaprioritarias", meta.new_metaprioritariaid, _meta, credenciales);

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
        [Route("api/hroneclick/participanteporevento")]
        public async Task<IActionResult> ActualizarParticipantePorEvento([FromBody] ParticipantePorEventoHROC participante)
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

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/hroneclick/requerimientodepersonal")]
        public async Task<IActionResult> CrearRequerimientoDePersonal([FromBody] RequerimientoDePersonalHROC requerimiento)
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
                JObject _requerimiento = new();

                if (!string.IsNullOrEmpty(requerimiento.new_empleadosolicitante))
                    _requerimiento.Add("new_EmpleadoSolicitante@odata.bind", "/new_empleados(" + requerimiento.new_empleadosolicitante + ")");
                if (!string.IsNullOrEmpty(requerimiento.new_cliente))
                    _requerimiento.Add("new_Cliente@odata.bind", "/new_clientes(" + requerimiento.new_cliente + ")");
                if (requerimiento.new_prioridad > 0)
                    _requerimiento.Add("new_prioridad", requerimiento.new_prioridad);
                if (!string.IsNullOrEmpty(requerimiento.new_puesto))
                    _requerimiento.Add("new_Puesto@odata.bind", "/new_cargos(" + requerimiento.new_puesto + ")");
                if (requerimiento.statuscode > 0) 
                    _requerimiento.Add("statuscode", requerimiento.statuscode); 
                if(!string.IsNullOrEmpty(requerimiento.new_perfil))
                    _requerimiento.Add("new_perfil", requerimiento.new_perfil); 
                if (!string.IsNullOrEmpty(requerimiento.new_proyectos))
                    _requerimiento.Add("new_proyectos@odata.bind", "/new_proyectos(" + requerimiento.new_proyectos + ")");
                if (requerimiento.new_cantidaddehorasmensuales > 0)
                    _requerimiento.Add("new_cantidaddehorasmensuales", requerimiento.new_cantidaddehorasmensuales);
                if (requerimiento.new_vacante > 0)
                    _requerimiento.Add("new_vacante", requerimiento.new_vacante);
                if (requerimiento.new_jornadadetrabajo > 0)
                    _requerimiento.Add("new_jornadadetrabajo", requerimiento.new_jornadadetrabajo);
                if (requerimiento.new_modalidaddecontratacin > 0)
                    _requerimiento.Add("new_modalidaddecontratacin", requerimiento.new_modalidaddecontratacin);
                if (requerimiento.new_duracindelacontratacin > 0)
                    _requerimiento.Add("new_duracindelacontratacin", requerimiento.new_duracindelacontratacin);
                if (!string.IsNullOrEmpty(requerimiento.new_fechaidealdeinicio))
                    _requerimiento.Add("new_fechaidealdeinicio", requerimiento.new_fechaidealdeinicio);
                if (!string.IsNullOrEmpty(requerimiento.new_descripcionproyecto))
                    _requerimiento.Add("new_descripcionproyecto", requerimiento.new_descripcionproyecto);
                if (!string.IsNullOrEmpty(requerimiento.new_requerimientodelperfilacontratar))
                    _requerimiento.Add("new_requerimientodelperfilacontratar", requerimiento.new_requerimientodelperfilacontratar);
                if (!string.IsNullOrEmpty(requerimiento.new_condicinespecialesdesegurodeaccidente))
                    _requerimiento.Add("new_condicinespecialesdesegurodeaccidente", requerimiento.new_condicinespecialesdesegurodeaccidente);
                if (!string.IsNullOrEmpty(requerimiento.new_beneficioadicional))
                    _requerimiento.Add("new_beneficioadicional", requerimiento.new_beneficioadicional);
                if (!string.IsNullOrEmpty(requerimiento.new_comentariosgenerales))
                    _requerimiento.Add("new_comentariosgenerales", requerimiento.new_comentariosgenerales);
                if (!string.IsNullOrEmpty(requerimiento.new_solicituddepuestonuevo))
                    _requerimiento.Add("new_solicituddepuestonuevo@odata.bind", "/new_solicituddepuestonuevos(" + requerimiento.new_solicituddepuestonuevo + ")");
                if (!string.IsNullOrEmpty(requerimiento.new_empleadoaprobador1))
                    _requerimiento.Add("new_EmpleadoAprobador1@odata.bind", "/new_empleados(" + requerimiento.new_empleadoaprobador1 + ")");
                if (requerimiento.new_aprobador1 > 0)
                    _requerimiento.Add("new_aprobador1", requerimiento.new_aprobador1);

                ResponseAPI resultado = await api.CreateRecord("new_solicituddecandidatos", _requerimiento, credenciales);

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
        [Route("api/hroneclick/requerimientodepersonal")]
        public async Task<IActionResult> ActualizarRequerimientoDePersonal([FromBody] RequerimientoDePersonalHROC requerimiento)
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
                JObject _requerimiento = new();

                if (!string.IsNullOrEmpty(requerimiento.new_empleadosolicitante))
                    _requerimiento.Add("new_EmpleadoSolicitante@odata.bind", "/new_empleados(" + requerimiento.new_empleadosolicitante + ")");
                if (!string.IsNullOrEmpty(requerimiento.new_cliente))
                    _requerimiento.Add("new_Cliente@odata.bind", "/new_clientes(" + requerimiento.new_cliente + ")");
                if (requerimiento.new_prioridad > 0)
                    _requerimiento.Add("new_prioridad", requerimiento.new_prioridad);
                if (!string.IsNullOrEmpty(requerimiento.new_puesto))
                    _requerimiento.Add("new_Puesto@odata.bind", "/new_cargos(" + requerimiento.new_puesto + ")");
                if (requerimiento.statuscode > 0)
                    _requerimiento.Add("statuscode", requerimiento.statuscode);
                if (!string.IsNullOrEmpty(requerimiento.new_perfil))
                    _requerimiento.Add("new_perfil", requerimiento.new_perfil);
                if (!string.IsNullOrEmpty(requerimiento.new_proyectos))
                    _requerimiento.Add("new_proyectos@odata.bind", "/new_proyectos(" + requerimiento.new_proyectos + ")");
                if (requerimiento.new_cantidaddehorasmensuales > 0)
                    _requerimiento.Add("new_cantidaddehorasmensuales", requerimiento.new_cantidaddehorasmensuales);
                if (requerimiento.new_vacante > 0)
                    _requerimiento.Add("new_vacante", requerimiento.new_vacante);
                if (requerimiento.new_jornadadetrabajo > 0)
                    _requerimiento.Add("new_jornadadetrabajo", requerimiento.new_jornadadetrabajo);
                if (requerimiento.new_modalidaddecontratacin > 0)
                    _requerimiento.Add("new_modalidaddecontratacin", requerimiento.new_modalidaddecontratacin);
                if (requerimiento.new_duracindelacontratacin > 0)
                    _requerimiento.Add("new_duracindelacontratacin", requerimiento.new_duracindelacontratacin);
                if (!string.IsNullOrEmpty(requerimiento.new_fechaidealdeinicio))
                    _requerimiento.Add("new_fechaidealdeinicio", requerimiento.new_fechaidealdeinicio);
                if (!string.IsNullOrEmpty(requerimiento.new_descripcionproyecto))
                    _requerimiento.Add("new_descripcionproyecto", requerimiento.new_descripcionproyecto);
                if (!string.IsNullOrEmpty(requerimiento.new_requerimientodelperfilacontratar))
                    _requerimiento.Add("new_requerimientodelperfilacontratar", requerimiento.new_requerimientodelperfilacontratar);
                if (!string.IsNullOrEmpty(requerimiento.new_condicinespecialesdesegurodeaccidente))
                    _requerimiento.Add("new_condicinespecialesdesegurodeaccidente", requerimiento.new_condicinespecialesdesegurodeaccidente);
                if (!string.IsNullOrEmpty(requerimiento.new_beneficioadicional))
                    _requerimiento.Add("new_beneficioadicional", requerimiento.new_beneficioadicional);
                if (!string.IsNullOrEmpty(requerimiento.new_comentariosgenerales))
                    _requerimiento.Add("new_comentariosgenerales", requerimiento.new_comentariosgenerales);
                if (!string.IsNullOrEmpty(requerimiento.new_solicituddepuestonuevo))
                    _requerimiento.Add("new_solicituddepuestonuevo@odata.bind", "/new_solicituddepuestonuevos(" + requerimiento.new_solicituddepuestonuevo + ")");
                if (!string.IsNullOrEmpty(requerimiento.new_empleadoaprobador1))
                    _requerimiento.Add("new_EmpleadoAprobador1@odata.bind", "/new_empleados(" + requerimiento.new_empleadoaprobador1 + ")");
                if (requerimiento.new_aprobador1 > 0)
                    _requerimiento.Add("new_aprobador1", requerimiento.new_aprobador1);

                ResponseAPI resultado = await api.UpdateRecord("new_solicituddecandidatos", requerimiento.new_solicituddecandidatoid, _requerimiento, credenciales);

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
        [Route("api/hroneclick/requerimientodepersonal")]
        public async Task<IActionResult> InactivarRequerimientoDePersonal([FromBody] RequerimientoDePersonalHROC requerimiento)
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
                if (requerimiento.new_solicituddecandidatoid == null || requerimiento.new_solicituddecandidatoid == string.Empty)
                    return BadRequest("El id del requerimiento esta vacio");

                JObject _requerimiento = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_solicituddecandidatos", requerimiento.new_solicituddecandidatoid, _requerimiento, credenciales);

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
    }
}
