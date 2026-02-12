using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ConvertApiDotNet;
using static Api.Web.Dynamics365.Models.ConvertirDocumento;
using Newtonsoft.Json.Linq;
using Api.Web.Dynamics365.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Api.Web.Dynamics365.Models;
using Microsoft.EntityFrameworkCore;
using static Api.Web.Dynamics365.Models.Converter;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class ConvertController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ConvertController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/convertirdocumento")]
        public async Task<IActionResult> Convert([FromBody] Converter converter)
        {
            try
            {
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

                string b64 = string.Empty;
                ApiDynamics api = new ApiDynamics();
                string nombre = string.Empty;

                if (converter.nombreArchivo.Contains('.'))
                {
                    nombre = converter.nombreArchivo.Split('.')[0];   
                }

                var convertApi = new ConvertApi("jlwmaFOehpOT5X6B");
                var docBytes = System.Convert.FromBase64String(converter.documentoB64);

                var stream = new MemoryStream(docBytes);

                var convertToPdf = await convertApi.ConvertAsync(converter.tipoDeArchivo, converter.tipoDeArchivoAConvertir,
                    new ConvertApiFileParam(stream, converter.nombreArchivo));

                var outputStream = await convertToPdf.Files[0].FileStreamAsync();

                using (MemoryStream ms = new MemoryStream())
                {
                    outputStream.CopyTo(ms);
                    byte[] memoryStreamArray = ms.ToArray();
                    b64 = System.Convert.ToBase64String(memoryStreamArray);
                }
                    
                JObject annotation = new JObject();
                annotation.Add("subject", nombre);
                annotation.Add("isdocument", true);
                annotation.Add("mimetype", "application/pdf");
                annotation.Add("documentbody", b64);
                annotation.Add("filename", $"{nombre}.pdf");

                if (converter.entidadAsociadaID != string.Empty)
                    annotation.Add($"objectid_{converter.campoEntidadAsociada}@odata.bind", $"/{converter.entidadAsociada}(" + converter.entidadAsociadaID + ")");

                string nota_id = api.CreateRecord("annotations", annotation, credenciales);


                return Ok(nota_id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/convertirdocumento/stream")]
        public async Task<IActionResult> ConverterStream([FromBody] ConverterStream converter)
        {
            try
            {
                ResponseStream response = new ResponseStream();
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

                string b64 = string.Empty;
                
                var convertApi = new ConvertApi("jlwmaFOehpOT5X6B");
                var docBytes = System.Convert.FromBase64String(converter.documentoB64);

                var stream = new MemoryStream(docBytes);

                var convertToPdf = await convertApi.ConvertAsync(converter.tipoDeArchivo, converter.tipoDeArchivoAConvertir,
                    new ConvertApiFileParam(stream, converter.nombreArchivo));

                var outputStream = await convertToPdf.Files[0].FileStreamAsync();

                using (MemoryStream ms = new MemoryStream())
                {
                    outputStream.CopyTo(ms);
                    byte[] memoryStreamArray = ms.ToArray();
                    b64 = System.Convert.ToBase64String(memoryStreamArray);
                    response.stream = b64;
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
