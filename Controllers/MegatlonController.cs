using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using DocumentFormat.OpenXml.Bibliography;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System.Net;
using System.Net.Mail;
using static Api.Web.Dynamics365.Models.AirOneClick;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;
using static Api.Web.Dynamics365.Models.HRFactors;
using static Api.Web.Dynamics365.Models.Lufe;
using static Api.Web.Dynamics365.Models.Megatlon;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class MegatlonController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;
        private readonly FirestoreDb _firestore;
        public MegatlonController(IConfiguration _configuration, 
            UserManager<ApplicationUser> userManager, ApplicationDbContext context, FirestoreDb firestore)
        {
            configuration = _configuration;
            this.userManager = userManager;
            this.context = context;
            _firestore = firestore;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/megatlon/importadorusuarios")]
        public async Task<IActionResult> ImportadorUsuarios()
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
                ApiDynamicsV2 apiDynamics = new();


                //int deleted = 0;
                //var pagedEnumerable = FirebaseAuth.DefaultInstance.ListUsersAsync(null);

                //// Recorremos todos los usuarios
                //await foreach (ExportedUserRecord user in pagedEnumerable)
                //{
                //    await FirebaseAuth.DefaultInstance.DeleteUserAsync(user.Uid);
                //    deleted++;
                //}

                //return Ok(new { message = $"Usuarios eliminados: {deleted}" });


                JArray resultadoUsuarios = await BuscarUsuarios(apiDynamics, credenciales);
                if (resultadoUsuarios.Count <= 0)
                {
                    return BadRequest("No se encontro usuarios para registrar");
                }

                List<UsuarioMegatlon> listaUsuarios = ArmarUsuarios(resultadoUsuarios);

                if (listaUsuarios.Count <= 0)
                {
                    return BadRequest("No se pudo armar la lista de usuarios para registrar");
                }

                foreach (var usuario in listaUsuarios)
                {
                    try
                    {
                        UserRecordArgs arg = new()
                        {
                            Email = usuario.emailaddress1,
                            Password = "Mega1234"
                        };

                        var user = await FirebaseAuth.DefaultInstance.CreateUserAsync(arg);

                        if (user == null)
                        {
                            return BadRequest("No se pudo crear el usuario en firebase");
                        }

                        CollectionReference usersRef = _firestore.Collection("usuarios");
                        object data = new
                        {
                            uid = user.Uid,
                            email = user.Email,
                            contactid = usuario.contactid,
                            IDUsuarioIntranet = usuario.new_idusuariointranet,
                        };

                        DocumentReference docRef = await usersRef.AddAsync(data);
                    }
                    catch (Exception)
                    {

                    }
                }

                return Ok("resultado");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region PORTAL BACK
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/megatlon/mantenimientopreventivo")]
        public async Task<IActionResult> MantenimientoPreventivo([FromBody] MantenimientoPreventivo mantenimientoPreventivo)
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
                ApiDynamics apiDynamics = new ApiDynamics();

                JObject mantenimiento = new JObject
                {
                    { "new_medicionmantenimiento", mantenimientoPreventivo.new_medicionmantenimiento }
                };
                if (mantenimientoPreventivo.new_observaciones != null) mantenimiento.Add("new_observaciones", mantenimientoPreventivo.new_observaciones);

                string resultado = apiDynamics.UpdateRecord("new_elementoses", mantenimientoPreventivo.new_elementosid, mantenimiento, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/megatlon/mantenimientopreventivoyadjunto")]
        public async Task<IActionResult> MantenimientoPreventivoYAdjunto([FromBody] MantenimientoPreventivo mantenimientoPreventivo)
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
                ApiDynamics apiDynamics = new ApiDynamics();
                var archivos = HttpContext.Request.Form.Files;
                string nota_id = string.Empty;
                string resultadoElemento = string.Empty;

                if (mantenimientoPreventivo.new_elementosid != string.Empty)
                {
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

                            JObject annotation = new JObject();
                            annotation.Add("subject", file.FileName);
                            annotation.Add("isdocument", true);
                            annotation.Add("mimetype", file.ContentType);
                            annotation.Add("documentbody", fileAsString);
                            annotation.Add("filename", file.FileName);
                            annotation.Add("objectid_new_elementos@odata.bind", "/new_elementoses(" + mantenimientoPreventivo.new_elementosid + ")");

                            nota_id = apiDynamics.CreateRecord("annotations", annotation, credenciales);
                        }
                    }
                }

                JObject mantenimiento = new JObject
                {
                    { "new_medicionmantenimiento", mantenimientoPreventivo.new_medicionmantenimiento }
                };
                if (mantenimientoPreventivo.new_observaciones != null) mantenimiento.Add("new_observaciones", mantenimientoPreventivo.new_observaciones);

                resultadoElemento = apiDynamics.UpdateRecord("new_elementoses", mantenimientoPreventivo.new_elementosid, mantenimiento, credenciales);

                return Ok(resultadoElemento);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/megatlon/mantenimientopreventivo/estado")]
        public async Task<IActionResult> EstadoMantenimientoPreventivo([FromBody] EstadoMantenimientoPreventivo mantenimientoPreventivo)
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
                ApiDynamics apiDynamics = new ApiDynamics();

                JObject mantenimiento = new JObject
                {
                    { "statuscode", mantenimientoPreventivo.statuscode }
                };

                string resultado = apiDynamics.UpdateRecord("new_mantenimientopreventivodiarios", mantenimientoPreventivo.new_mantenimientopreventivodiarioid, mantenimiento, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/megatlon/mantenimientopreventivo/adjuntar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> AdjuntarArchivo(string elementoid)
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
                ApiDynamics apiDynamics = new ApiDynamics();
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

                        JObject annotation = new JObject();
                        annotation.Add("subject", file.FileName);
                        annotation.Add("isdocument", true);
                        annotation.Add("mimetype", file.ContentType);
                        annotation.Add("documentbody", fileAsString);
                        annotation.Add("filename", file.FileName);

                        if (elementoid != string.Empty)
                            annotation.Add("objectid_new_elementos@odata.bind", "/new_elementoses(" + elementoid + ")");

                        nota_id = apiDynamics.CreateRecord("annotations", annotation, credenciales);
                    }
                }

                return Ok(nota_id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/megatlon/tarea")]
        public async Task<IActionResult> Tarea([FromBody] Tarea tarea)
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
                ApiDynamics apiDynamics = new ApiDynamics();

                JObject Tarea = new JObject();

                if (tarea.subject != null)
                    Tarea.Add("subject", tarea.subject);
                if (tarea.new_accion != null)
                    Tarea.Add("new_Accion@odata.bind", "/new_accions(" + tarea.new_accion + ")");
                if (tarea.new_instalacion != null)
                    Tarea.Add("new_Instalacion@odata.bind", "/new_instalacionesporsedes(" + tarea.new_instalacion + ")");
                if (tarea.new_sede != null)
                    Tarea.Add("new_Sede@odata.bind", "/accounts(" + tarea.new_sede + ")");
                if (tarea.new_subinstalacion != null)
                    Tarea.Add("new_Subinstalacion@odata.bind", "/new_subinstalacions(" + tarea.new_subinstalacion + ")");
                if (tarea.new_origen != 0)
                    Tarea.Add("new_origen", tarea.new_origen);
                if (tarea.new_prioridad != 0)
                    Tarea.Add("new_prioridad", tarea.new_prioridad);
                if (tarea.description != null)
                    Tarea.Add("description", tarea.description);
                if (tarea.new_resolucion != null)
                    Tarea.Add("new_resolucion", tarea.new_resolucion);
                if (tarea.scheduledstart != null)
                    Tarea.Add("scheduledstart", tarea.scheduledstart);
                if (tarea.new_porcentajerealizado != 0)
                    Tarea.Add("new_porcentajerealizado", tarea.new_porcentajerealizado);
                if (tarea.scheduledend != null)
                    Tarea.Add("scheduledend", tarea.scheduledend);

                string resultado = apiDynamics.UpdateRecord("tasks", tarea.activityid, Tarea, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/megatlon/cerrartarea")]
        public async Task<IActionResult> CerrarTarea([FromBody] CerrarTarea tarea)
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
                ApiDynamics apiDynamics = new ApiDynamics();

                JObject Tarea = new JObject();
                Tarea.Add("statecode", 1);

                if (tarea.new_resolucion != null)
                    Tarea.Add("new_resolucion", tarea.new_resolucion);

                string resultado = apiDynamics.UpdateRecord("tasks", tarea.activityid, Tarea, credenciales);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region PORTAL FRONT
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/megatlon/caso")]
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
                ApiDynamics apiDynamics = new();
                ApiDynamicsV2 apiDynamicsV2 = new();
                string resultadoCaso = string.Empty;

                JObject Caso = new()
                {
                    { "new_cliente@odata.bind", $"/contacts({caso.contactid})"},
                    { "new_fechaalta", DateTime.Now }
                };
               
                if(caso?.asuntoPrimario?.Length > 0)
                    Caso.Add("new_asuntoprimario", Convert.ToInt32(caso.asuntoPrimario));
                if (caso?.asunto?.Length > 0)
                    Caso.Add("subjectid@odata.bind", $"/subjects({caso.asunto})");
                if (caso?.solicitante?.Length > 0)
                    Caso.Add("new_Solicitante@odata.bind", $"/contacts({caso.solicitante})");
                if (caso?.puestoSolicitante?.Length > 0)
                    Caso.Add("new_puestodelsolicitante", caso.puestoSolicitante);
                if (caso?.tipoCaso?.Length > 0)
                    Caso.Add("casetypecode", Convert.ToInt32(caso.tipoCaso));
                if (caso?.comentarios?.Length > 0)
                    Caso.Add("new_comentarios", caso.comentarios);
                if (caso?.descripcion?.Length > 0)
                    Caso.Add("description", caso.descripcion);
                if (caso?.sucursal?.Length > 0)
                    Caso.Add("customerid_account@odata.bind", $"/accounts({caso.sucursal})");
                if (caso?.instalacionPorSede?.Length > 0)
                    Caso.Add("new_InstalacionporSede@odata.bind", $"/new_instalacionesporsedes({caso.instalacionPorSede})");
                if (caso?.equipoDetenido?.Length > 0)
                    Caso.Add("new_equipodetenido", caso.equipoDetenido);
                if (caso?.prioridad?.Length > 0)
                    Caso.Add("prioritycode", Convert.ToInt32(caso.prioridad));
                if (caso?.derivar?.Length > 0)
                    Caso.Add("new_derivacion", Convert.ToInt32(caso.derivar));
                //if (caso?.areaEscalar?.Length > 0)
                //    Caso.Add("new_areaaescalar@odata.bind", $"/new_areas({caso.areaEscalar})");
                if (caso?.equipoRelacionado?.Length > 0)
                    Caso.Add("new_equiporelacionado", caso.equipoRelacionado);
                if (caso?.origenCaso?.Length > 0)
                    Caso.Add("caseorigincode", Convert.ToInt32(caso.origenCaso));
                if (caso?.accountid?.Length > 0)
                    Caso.Add("customerid_account@odata.bind", $"/accounts({caso.accountid})");
                if (caso?.rubro?.Length > 0)
                    Caso.Add("new_rubro", Convert.ToInt32(caso.rubro));
                if (caso?.subRubro?.Length > 0)
                    Caso.Add("new_subrubro", Convert.ToInt32(caso.subRubro));
                //DISENO
                if (caso?.tipoCorporeo?.Length > 0)
                    Caso.Add("new_tipodecorporeo", Convert.ToInt32(caso.tipoCorporeo));
                if (caso?.tipoLockers?.Length > 0)
                    Caso.Add("new_tipodelockers", Convert.ToInt32(caso.tipoLockers));
                if (caso?.tipoReglamento?.Length > 0)
                    Caso.Add("new_tipodereglamento", Convert.ToInt32(caso.tipoReglamento));
                if (caso?.tipoImagen?.Length > 0)
                    Caso.Add("new_tipodeimagen", Convert.ToInt32(caso.tipoImagen));
                if (caso?.tipoFolleteria?.Length > 0)
                    Caso.Add("new_tipodefolletera", Convert.ToInt32(caso.tipoFolleteria));
                if (caso?.tipoCarteles?.Length > 0)
                    Caso.Add("new_tipodecarteles", Convert.ToInt32(caso.tipoCarteles));
                if (caso?.nivelServicio?.Length > 0)
                    Caso.Add("contractservicelevelcode", Convert.ToInt32(caso.nivelServicio));
                if (caso?.medidasSalon1?.Length > 0)
                    Caso.Add("new_medidassaln1", caso.medidasSalon1);
                if (caso?.medidasSalon2?.Length > 0)
                    Caso.Add("new_medidassaln2", caso.medidasSalon2);
                if (caso?.medidaSpinning?.Length > 0)
                    Caso.Add("new_medidassalnspinning", caso.medidaSpinning);
                if (caso?.medidaSuperficie?.Length > 0)
                    Caso.Add("new_medidasdelasuperficie", caso.medidaSuperficie);
                if (caso?.cantidad?.Length > 0)
                    Caso.Add("new_cantidad", caso.cantidad);
                if (caso?.soporteMantenimiento?.Length > 0)
                    Caso.Add("new_soporterequieremantenimiento", caso.soporteMantenimiento);

                if (caso?.asuntoPrimario == "4")
                {
                    JArray AsuntosPorArea = BuscarAsuntoPorArea(caso.asunto, apiDynamics, credenciales);
                    if (AsuntosPorArea.Count > 0)
                    {
                        AsuntoPorArea asuntoXarea =  JsonConvert.DeserializeObject<AsuntoPorArea>(AsuntosPorArea[0].ToString());

                        if (asuntoXarea?.new_administrativo?.Length > 0)
                            Caso.Add("new_administrativo", asuntoXarea.new_administrativo);
                        if (asuntoXarea?.new_area?.Length > 0)
                            Caso.Add("new_area@odata.bind", $"/new_areas({asuntoXarea.new_area})");
                        if (asuntoXarea?.new_sla?.Length > 0)
                            Caso.Add("new_sla", Convert.ToInt32(asuntoXarea.new_sla));
                        if (asuntoXarea?.new_derivacion?.Length > 0)
                            Caso.Add("new_derivacion", Convert.ToInt32(asuntoXarea.new_derivacion));
                        if (asuntoXarea?.new_requiereautorizacion?.Length > 0)
                            Caso.Add("new_requiereautorizacion", asuntoXarea.new_requiereautorizacion);
                        if (asuntoXarea?.new_gerentecomercial?.Length > 0)
                            Caso.Add("new_gerentecomercial", asuntoXarea.new_gerentecomercial);
                        if (asuntoXarea?.new_gerentedeservicios?.Length > 0)
                            Caso.Add("new_gerentedeservicios", asuntoXarea.new_gerentedeservicios);
                        //if (asuntoXarea?.new_coordinadordeventas?.Length > 0)
                        //    Caso.Add("new_coordinadordeventas", asuntoXarea.new_coordinadordeventas);
                        if (asuntoXarea?.new_coordinadordeservicios?.Length > 0)
                            Caso.Add("new_coordinadordeservicios", asuntoXarea.new_coordinadordeservicios);
                        if (asuntoXarea?.new_coordinadordepileta?.Length > 0)
                            Caso.Add("new_coordinadordepileta", asuntoXarea.new_coordinadordepileta);
                        if (asuntoXarea?.new_gerenteregional?.Length > 0)
                            Caso.Add("new_gerenteregional", asuntoXarea.new_gerenteregional);
                        if (asuntoXarea?.new_director?.Length > 0)
                            Caso.Add("new_director", asuntoXarea.new_director);
                        if (asuntoXarea?.new_responsablededarrespuesta?.Length > 0)
                            Caso.Add("new_responsablededarrespuesta", Convert.ToInt32(asuntoXarea.new_responsablededarrespuesta));
                        //if (asuntoXarea?.new_tipodepropietarioareaaescalar?.Length > 0)
                        //    Caso.Add("new_tipodepropietarioareaaescalar", Convert.ToInt32(asuntoXarea.new_tipodepropietarioareaaescalar));
                        //if (asuntoXarea?.new_usuariofijoderivableareaaescalar?.Length > 0)
                        //    Caso.Add("new_usuariofijoderivableareaaescalar@odata.bind", $"/systemusers({asuntoXarea.new_usuariofijoderivableareaaescalar})");
                        if (asuntoXarea?.new_areaaescalar?.Length > 0)
                            Caso.Add("new_areaaescalar@odata.bind", $"/new_areas({asuntoXarea.new_areaaescalar})");
                        if (asuntoXarea?.new_asuntosagrupacin?.Length > 0)
                            Caso.Add("new_AsuntosAgrupacin@odata.bind", $"/new_agrupacinasuntoses({asuntoXarea.new_asuntosagrupacin})");
                    }
                }

                ResponseAPI resultado = await apiDynamicsV2.CreateRecord("incidents", Caso, credenciales);

                if (!resultado.ok)
                {
                    return BadRequest(resultado.descripcion);
                }
                else
                {
                    JArray casoCreado = BuscarTicketCaso(resultado.descripcion, apiDynamics, credenciales);
                    if (casoCreado?.Count > 0)
                    {
                        CasoTkt ticket = JsonConvert.DeserializeObject<CasoTkt>(casoCreado[0].ToString());
                        if (ticket?.ticketnumber?.Length > 0)
                        {
                            resultadoCaso = ticket.ticketnumber + ';' + resultado.descripcion;
                        }
                        else
                        {
                            resultadoCaso = resultado.descripcion;
                        }
                    }
                    else
                    {
                        resultadoCaso = resultado.descripcion;
                    }
                }

                return Ok(resultadoCaso);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/megatlon/adjuntos")]
        public async Task<IActionResult> Adjuntos(string caso_id = null, string legales_id = null, string busqueda_id = null, string candidatoPorBusqueda_id = null)
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

                        if (legales_id?.Length > 0)
                            annotation.Add("objectid_new_documentoslegales@odata.bind", "/new_documentoslegaleses(" + legales_id + ")");

                        if (busqueda_id?.Length > 0)
                            annotation.Add("objectid_new_busquedadepersonal@odata.bind", "/new_busquedadepersonals(" + busqueda_id + ")");

                        if (candidatoPorBusqueda_id?.Length > 0)
                            annotation.Add("objectid_new_candidatoporbusqueda@odata.bind", "/new_candidatoporbusquedas(" + candidatoPorBusqueda_id + ")");

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

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/megatlon/documentoslegales")]
        public async Task<IActionResult> DocumentoLegal([FromBody] DocumentoLegal documentoLegal)
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
                JObject DocumentoLegal = new()
                {
                    {"new_Cliente@odata.bind", $"/contacts({documentoLegal.new_cliente})" },
                    {"new_fechaderecepcin", documentoLegal.new_fechaderecepcin },
                    {"new_descripcindeldocumento", documentoLegal.new_descripcindeldocumento },
                    {"new_Sede@odata.bind", $"/accounts({documentoLegal.new_sede})" },
                    {"new_PersonaqueRecepcion@odata.bind", $"/contacts({documentoLegal.new_personaquerecepcion})" },
                };

                if (documentoLegal?.new_observaciones?.Length > 0)
                    DocumentoLegal.Add("new_observaciones", documentoLegal.new_observaciones);

                
                ResponseAPI resultado = await api.CreateRecord("new_documentoslegaleses", DocumentoLegal, credenciales);

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
        [Route("api/megatlon/evaluacionperiodoprueba")]
        public async Task<IActionResult> Evaluacion([FromBody] EvaluacionPeriodoPrueba evaluacionPeriodoPrueba)
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
                List<ItemEvaluacionPeriodoPrueba> listaEvaluaciones = new ();

                if (evaluacionPeriodoPrueba?.evaluaciones?.Length > 0)
                {
                    JArray Items = JArray.Parse(evaluacionPeriodoPrueba.evaluaciones);
                    listaEvaluaciones = JsonConvert.DeserializeObject<List<ItemEvaluacionPeriodoPrueba>>(Items.ToString());

                    if (!listaEvaluaciones.Equals(0))
                    {
                        foreach (var item in listaEvaluaciones)
                        {
                            JObject itemEvaluacion = new(); //new_itemevaluaciondeperiododepruebas

                            if (item.valor != string.Empty) itemEvaluacion.Add("new_resultado1ermes", Convert.ToInt32(item.valor));
                            if (item.valor2 != string.Empty) itemEvaluacion.Add("new_resultado2domes", Convert.ToInt32(item.valor2));
                            if (item.valor3 != string.Empty) itemEvaluacion.Add("new_resultado3ermes", Convert.ToInt32(item.valor3));

                            await api.UpdateRecord("new_itemevaluaciondeperiododepruebas", item.id, itemEvaluacion, credenciales);
                        }
                    }

                    JObject eva = new();

                    if (evaluacionPeriodoPrueba?.comentarios30?.Length > 0)
                        eva.Add("new_30dias", evaluacionPeriodoPrueba.comentarios30);
                    if (evaluacionPeriodoPrueba?.comentarios60?.Length > 0)
                        eva.Add("new_60dias", evaluacionPeriodoPrueba.comentarios60);
                    if (evaluacionPeriodoPrueba?.comentarios80?.Length > 0)
                        eva.Add("new_80dias", evaluacionPeriodoPrueba.comentarios80);
                    if (evaluacionPeriodoPrueba?.fechaIngreso?.Length > 0)
                        eva.Add("new_fechadeingreso", evaluacionPeriodoPrueba.fechaIngreso);
                    if (evaluacionPeriodoPrueba?.induccion?.Length > 0)
                        eva.Add("new_elempleadoparticipodelcursodeinduccion", evaluacionPeriodoPrueba.induccion);
                    if (evaluacionPeriodoPrueba?.esReferido?.Length > 0)
                        eva.Add("new_esreferido", evaluacionPeriodoPrueba.esReferido);
                    if (evaluacionPeriodoPrueba?.periodoPrueba?.Length > 0)
                        eva.Add("new_pasaperiododeprueba", evaluacionPeriodoPrueba.periodoPrueba);

                    if (eva.Count > 0)
                    {
                        await api.UpdateRecord("new_evaluaciondeperiododepruebas", evaluacionPeriodoPrueba.evaluacionid, eva, credenciales);
                    }

                    return Ok("EXITO");
                }
                else if (evaluacionPeriodoPrueba?.evaluacionid?.Length > 0)
                {
                    JObject eva = new();

                    if (evaluacionPeriodoPrueba?.comentarios30?.Length > 0)
                        eva.Add("new_30dias", evaluacionPeriodoPrueba.comentarios30);
                    if (evaluacionPeriodoPrueba?.comentarios60?.Length > 0)
                        eva.Add("new_60dias", evaluacionPeriodoPrueba.comentarios60);
                    if (evaluacionPeriodoPrueba?.comentarios80?.Length > 0)
                        eva.Add("new_80dias", evaluacionPeriodoPrueba.comentarios80);
                    if (evaluacionPeriodoPrueba?.fechaIngreso?.Length > 0)
                        eva.Add("new_fechadeingreso", evaluacionPeriodoPrueba.fechaIngreso);
                    if (evaluacionPeriodoPrueba?.induccion?.Length > 0)
                        eva.Add("new_elempleadoparticipodelcursodeinduccion", evaluacionPeriodoPrueba.induccion);
                    if (evaluacionPeriodoPrueba?.esReferido?.Length > 0)
                        eva.Add("new_esreferido", evaluacionPeriodoPrueba.esReferido);
                    if (evaluacionPeriodoPrueba?.periodoPrueba?.Length > 0)
                        eva.Add("new_pasaperiododeprueba", evaluacionPeriodoPrueba.periodoPrueba);

                    if (eva.Count > 0)
                    {
                        await api.UpdateRecord("new_evaluaciondeperiododepruebas", evaluacionPeriodoPrueba.evaluacionid, eva, credenciales);
                    }
                }

                return Ok("OK");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/megatlon/busquedapersonal")]
        public async Task<IActionResult> BusquedaPersonal([FromBody] BusquedaPersonal busquedaPersonal)
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
                JObject busqueda = new()
                {
                    { "new_puesto@odata.bind", "/new_cargos(" + busquedaPersonal.new_puesto + ")" },
                    { "new_tipodepuesto", busquedaPersonal.new_tipodepuesto },
                    //{ "new_reportaraa@odata.bind", "/contacts(" + busquedaPersonal.new_reportaraa + ")" },
                    { "new_jornadalaboral", busquedaPersonal.new_jornadalaboral },
                    { "new_descripciongeneraldelpuesto", busquedaPersonal.new_descripciongeneraldelpuesto }
                };


                if (!string.IsNullOrEmpty(busquedaPersonal.new_reportaraa))
                    busqueda.Add("new_reportaraa@odata.bind", "/contacts(" + busquedaPersonal.new_reportaraa + ")");
                if (!string.IsNullOrEmpty(busquedaPersonal.new_area))
                    busqueda.Add("new_Area@odata.bind", "/new_areas(" + busquedaPersonal.new_area + ")");
                if (!string.IsNullOrEmpty(busquedaPersonal.new_sucursal))
                    busqueda.Add("new_sucursal@odata.bind", "/accounts(" + busquedaPersonal.new_sucursal + ")");
                if (!string.IsNullOrEmpty(busquedaPersonal.new_reemplazaraa))
                    busqueda.Add("new_reemplazaraa@odata.bind", "/contacts(" + busquedaPersonal.new_reemplazaraa + ")");
                if (busquedaPersonal.new_tipodebusqueda > 0)
                    busqueda.Add("new_tipodebusqueda", busquedaPersonal.new_tipodebusqueda);
                if (!string.IsNullOrEmpty(busquedaPersonal.new_autorizadopor))
                    busqueda.Add("new_Autorizadopor@odata.bind", "/contacts(" + busquedaPersonal.new_autorizadopor + ")");
                if (!string.IsNullOrEmpty(busquedaPersonal.new_personasacargosino))
                    busqueda.Add("new_personasacargosino", busquedaPersonal.new_personasacargosino);
                if (!string.IsNullOrEmpty(busquedaPersonal.new_contactocreador))
                    busqueda.Add("new_contactocreador@odata.bind", "/contacts(" + busquedaPersonal.new_contactocreador + ")");
                if (!string.IsNullOrEmpty(busquedaPersonal.new_justificacindelabsqueda))
                    busqueda.Add("new_justificacindelabsqueda", busquedaPersonal.new_justificacindelabsqueda);

                if (busquedaPersonal.new_expertise > 0)
                    busqueda.Add("new_expertise", busquedaPersonal.new_expertise);
                if (busquedaPersonal.new_preferenciadegenero > 0)
                    busqueda.Add("new_preferenciadegenero", busquedaPersonal.new_preferenciadegenero);
                if (busquedaPersonal.new_nivelmnimodeeducacin > 0)
                    busqueda.Add("new_nivelmnimodeeducacin", busquedaPersonal.new_nivelmnimodeeducacin);
                if (busquedaPersonal.new_estadodeniveldeeducacin > 0)
                    busqueda.Add("new_estadodeniveldeeducacin", busquedaPersonal.new_estadodeniveldeeducacin);

                if (!string.IsNullOrEmpty(busquedaPersonal.new_edadestimada))
                    busqueda.Add("new_edadestimada", busquedaPersonal.new_edadestimada);
                if (!string.IsNullOrEmpty(busquedaPersonal.new_motivodelgenero))
                    busqueda.Add("new_motivodelgenero", busquedaPersonal.new_motivodelgenero);
                if (!string.IsNullOrEmpty(busquedaPersonal.new_experiencia))
                    busqueda.Add("new_experiencia", busquedaPersonal.new_experiencia);
                if (!string.IsNullOrEmpty(busquedaPersonal.new_competenciascaractersticasdepersonalidad))
                    busqueda.Add("new_competenciascaractersticasdepersonalidad", busquedaPersonal.new_competenciascaractersticasdepersonalidad);
                if (!string.IsNullOrEmpty(busquedaPersonal.new_fechadeingreso))
                    busqueda.Add("new_fechadeingreso", busquedaPersonal.new_fechadeingreso);
                if (!string.IsNullOrEmpty(busquedaPersonal.new_reemplazode))
                    busqueda.Add("new_reemplazode", busquedaPersonal.new_reemplazode);
                if (!string.IsNullOrEmpty(busquedaPersonal.new_reportaa))
                    busqueda.Add("new_reportaa", busquedaPersonal.new_reportaa);

                ResponseAPI respuesta = await api.CreateRecord("new_busquedadepersonals", busqueda, credenciales);

                if (!respuesta.ok)
                {
                    return BadRequest(respuesta.descripcion);
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
        [Route("api/megatlon/aprobarbusquedapersonal")]
        public async Task<IActionResult> AprobarBusquedaPersonal([FromBody] AprobarBusquedaPersonal busquedaPersonal)
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
                JObject busqueda = new()
                {
                    { "new_ContactoAprobador@odata.bind", "/contacts(" + busquedaPersonal.new_contactoaprobador + ")" },
                    { "new_aprobacion", busquedaPersonal.new_aprobacion },
                    { "new_observacionesaprobador", busquedaPersonal.new_observacionesaprobador }
                };

                ResponseAPI respuesta = await api.UpdateRecord("new_busquedadepersonals", busquedaPersonal.new_busquedadepersonalid, busqueda, credenciales);

                if (!respuesta.ok)
                {
                    return BadRequest(respuesta.descripcion);
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
        [Route("api/megatlon/postulacioncandidato")]
        public async Task<IActionResult> CandidatoBusqueda([FromBody] PostulacionCandidato postulacion)
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

                JObject _postulacion = new()
                {
                    { "new_observacionessedesareas", postulacion.new_observacionessedesareas },
                    { "statuscode", postulacion.statuscode}
                };

                ResponseAPI respuesta = await api.UpdateRecord("new_candidatoporbusquedas", postulacion.new_candidatoporbusquedaid, _postulacion, credenciales);

                if (!respuesta.ok)
                {
                    return BadRequest(respuesta.descripcion);
                }

                return Ok("respuesta.descripcion");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public static JArray BuscarAsuntoPorArea(string asunto, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "new_asuntoxareas";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='new_asuntoxarea'>" +
                                                           "<attribute name='new_administrativo'/> " +
                                                           "<attribute name='new_area'/> " +
                                                           "<attribute name='new_sla'/> " +
                                                           "<attribute name='new_derivacion'/> " +
                                                           "<attribute name='new_requiereautorizacion'/> " +
                                                           "<attribute name='new_gerentecomercial'/> " +
                                                           "<attribute name='new_gerentedeservicios'/> " +
                                                           //"<attribute name='new_coordinadordeventas'/> " +
                                                           "<attribute name='new_coordinadordeservicios'/> " +
                                                           "<attribute name='new_coordinadordepileta'/> " +
                                                           "<attribute name='new_gerenteregional'/> " +
                                                           "<attribute name='new_director'/> " +
                                                           "<attribute name='new_responsablededarrespuesta'/> " +
                                                           //"<attribute name='new_tipodepropietarioareaaescalar'/> " +
                                                           //"<attribute name='new_usuariofijoderivableareaaescalar'/> " +
                                                           "<attribute name='new_areaaescalar'/> " +
                                                           "<attribute name='new_asuntosagrupacin'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='new_asunto' operator='eq' value='{asunto}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static JArray BuscarTicketCaso(string caso_id, ApiDynamics api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;

                api.EntityName = "incidents";

                fetchXML = "<fetch mapping='logical'>" +
                                                "<entity name='incident'>" +
                                                           "<attribute name='ticketnumber'/> " +
                                                           "<filter type='and'>" +
                                                                $"<condition attribute='incidentid' operator='eq' value='{caso_id}' />" +
                                                           "</filter>" +
                                                "</entity>" +
                                            "</fetch>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = WebUtility.UrlEncode(fetchXML);
                    }

                    respuesta = api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<JArray> BuscarUsuarios(ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = new();
                string fetchXML = string.Empty;

                api.EntityName = "contacts";

                fetchXML = "<entity name='contact'>" +
                                "<attribute name='new_idusuariointranet'/> " +
                                "<attribute name='contactid'/>" +
                                "<attribute name='emailaddress1'/>" +
                                "<filter type='and'>" +
                                "<condition attribute='createdon' operator='on-or-after' value='2025-08-25' />" +
                                "<condition attribute='new_idusuariointranet' operator='not-null' />" +
                                "<condition attribute='new_idusuariointranet' operator='not-like' value='2%' />" +
                                "<condition attribute='statecode' operator='eq' value='0' />" +
                                "<condition attribute='new_tipodecliente' operator='eq' value='100000000' />" +
                                "</filter>" +
                        "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    ResponseAPI responseAPI = await api.RetrieveMultipleWithFetchV2(api, credenciales);
                    if (responseAPI.ok)
                        respuesta = responseAPI.coleccion;
                    else
                        throw new Exception(responseAPI.descripcion);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                //logger.LogInformation($"Error retrieve fetch en entidad {api.EntityName} - {ex.Message}");
                throw;
            }
        }

        public static List<UsuarioMegatlon> ArmarUsuarios(JToken usuariosJT)
        {
            return JsonConvert.DeserializeObject<List<UsuarioMegatlon>>(usuariosJT.ToString());
        }
        #endregion
    }
}