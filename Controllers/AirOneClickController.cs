using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using static Api.Web.Dynamics365.Models.AirOneClick;
using static Api.Web.Dynamics365.Models.HRFactors;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class AirOneClickController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private string cliente;
        public AirOneClickController(ApplicationDbContext context)
        {
            this.context = context;
        }

        #region DeclaracionDeVentas
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/aironeclick/declaraciondeventas")]
        public async Task<IActionResult> CrearDeclaracionDeVentas([FromBody] DeclaracionDeVentas declaracionDeVentas)
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
                JObject declaracion = new()
                {
                    { "new_Cliente@odata.bind", "/accounts(" + declaracionDeVentas.new_cliente + ")" },
                };

                ////Datos Generales
                if (declaracionDeVentas.new_fecha != null && declaracionDeVentas.new_fecha != string.Empty)
                    declaracion.Add("new_fecha", declaracionDeVentas.new_fecha);
                if (declaracionDeVentas.new_aeropuerto != null && declaracionDeVentas.new_aeropuerto != string.Empty)
                    declaracion.Add("new_Aeropuerto@odata.bind", "/new_aeropuertos(" + declaracionDeVentas.new_aeropuerto + ")");
                if (declaracionDeVentas.new_facturacion > 0)
                    declaracion.Add("new_facturacion", declaracionDeVentas.new_facturacion);
                if (declaracionDeVentas.transactioncurrencyid != null && declaracionDeVentas.transactioncurrencyid != string.Empty)
                    declaracion.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + declaracionDeVentas.transactioncurrencyid + ")");
                if (declaracionDeVentas.new_liquidar > 0)
                    declaracion.Add("new_liquidar", declaracionDeVentas.new_liquidar);
                if (declaracionDeVentas.statuscode > 0)
                    declaracion.Add("statuscode", declaracionDeVentas.statuscode);

                ResponseAPI respuesta = await api.CreateRecord("new_declaraciondeventases", declaracion, credenciales);

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
        [Route("api/aironeclick/declaraciondeventas")]
        public async Task<IActionResult> ActualizarDeclaracionDeVentas([FromBody] DeclaracionDeVentas declaracionDeVentas)
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
                JObject declaracion = new()
                {
                    { "new_Cliente@odata.bind", "/accounts(" + declaracionDeVentas.new_cliente + ")" },
                };

                ////Datos Generales
                if (declaracionDeVentas.new_fecha != null && declaracionDeVentas.new_fecha != string.Empty)
                    declaracion.Add("new_fecha", declaracionDeVentas.new_fecha);
                if (declaracionDeVentas.new_aeropuerto != null && declaracionDeVentas.new_aeropuerto != string.Empty)
                    declaracion.Add("new_Aeropuerto@odata.bind", "/new_aeropuertos(" + declaracionDeVentas.new_aeropuerto + ")");
                if (declaracionDeVentas.new_facturacion > 0)
                    declaracion.Add("new_facturacion", declaracionDeVentas.new_facturacion);
                if (declaracionDeVentas.transactioncurrencyid != null && declaracionDeVentas.transactioncurrencyid != string.Empty)
                    declaracion.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + declaracionDeVentas.transactioncurrencyid + ")");
                if (declaracionDeVentas.new_liquidar > 0)
                    declaracion.Add("new_liquidar", declaracionDeVentas.new_liquidar);
                if (declaracionDeVentas.statuscode > 0)
                    declaracion.Add("statuscode", declaracionDeVentas.statuscode);

                ResponseAPI respuesta = await api.UpdateRecord("new_declaraciondeventases", declaracionDeVentas.new_declaraciondeventasid, declaracion, credenciales);

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
        [Route("api/aironeclick/declaraciondeventas")]
        public async Task<IActionResult> InactivarDeclaracionDeVentas(string declaracionid)
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

                if (declaracionid == null || declaracionid == string.Empty)
                    return BadRequest("El id del dato bancario esta vacio");

                JObject declaracion = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("new_declaraciondeventases", declaracionid, declaracion, credenciales);

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
        [Route("api/aironeclick/documentaciondeclaraciones")]
        public async Task<IActionResult> DocumentacionPorCuentaYestado(string declaraciondeventa_id)
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
                if (declaraciondeventa_id == null || declaraciondeventa_id == string.Empty)
                    return BadRequest("El id de la declaracion esta vacio");

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

                        JObject annotation = new JObject
                        {
                            { "subject", file.FileName },
                            { "isdocument", true },
                            { "mimetype", file.ContentType },
                            { "documentbody", fileAsString },
                            { "filename", file.FileName }
                        };

                        if (declaraciondeventa_id != string.Empty)
                            annotation.Add("objectid_new_declaraciondeventas@odata.bind", "/new_declaraciondeventases(" + declaraciondeventa_id + ")");

                        ResponseAPI resultado = await api.CreateRecord("annotations", annotation, credenciales);
                        
                        if (!resultado.ok) //OK
                        {
                            throw new Exception(resultado.descripcion);
                        }
                    }
                }

                return Ok("Documento subido con exito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
        #region Facturas
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/aironeclick/facturas")]
        public async Task<IActionResult> CrearFacturas([FromBody] Facturas facturas)
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
                JObject factura = new()
                {
                    { "customerid_account@odata.bind", "/accounts(" + facturas.customerid + ")" },
                    { "name", facturas.name },
                };

                ////Datos Generales
                if (facturas.totalamount > 0)
                    factura.Add("totalamount", facturas.totalamount);
                if (facturas.discountpercentage > 0)
                    factura.Add("new_liquidar", facturas.discountpercentage);
                if (facturas.transactioncurrencyid != null && facturas.transactioncurrencyid != string.Empty)
                    factura.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + facturas.transactioncurrencyid + ")");
                if (facturas.statuscode > 0)
                    factura.Add("statuscode", facturas.statuscode);

                ResponseAPI respuesta = await api.CreateRecord("invoices", factura, credenciales);

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

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/aironeclick/facturas")]
        public async Task<IActionResult> ActualizarFacturas([FromBody] Facturas facturas)
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
                JObject factura = new()
                {
                    { "Customerid_account@odata.bind", "/accounts(" + facturas.customerid + ")" },
                    { "name", facturas.name },
                };

                ////Datos Generales
                if (facturas.totalamount > 0)
                    factura.Add("totalamount", facturas.totalamount);
                if (facturas.discountpercentage > 0)
                    factura.Add("new_liquidar", facturas.discountpercentage);
                if (facturas.transactioncurrencyid != null && facturas.transactioncurrencyid != string.Empty)
                    factura.Add("transactioncurrencyid@odata.bind", "/transactioncurrencies(" + facturas.transactioncurrencyid + ")");
                if (facturas.statuscode > 0)
                    factura.Add("statuscode", facturas.statuscode);

                ResponseAPI respuesta = await api.UpdateRecord("invoices", facturas.invoiceid ,factura, credenciales);

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
        [Route("api/aironeclick/facturas")]
        public async Task<IActionResult> InactivarFacturas(string facturaid)
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

                if (facturaid == null || facturaid == string.Empty)
                    return BadRequest("El id del dato bancario esta vacio");

                JObject factura = new()
                {
                    { "statecode", 1 },
                };

                ResponseAPI resultado = await api.UpdateRecord("invoices", facturaid, factura, credenciales);

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
