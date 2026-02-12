using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using ConvertApiDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.ServiceModel.Channels;
using static Api.Web.Dynamics365.Models.Casfog_Sindicadas;
using static Api.Web.Dynamics365.Models.Documents;
using static Api.Web.Dynamics365.Models.Signatura;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class SignaturaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        public SignaturaController(ApplicationDbContext context)
        {
            this.context = context;
        }

        #region Documentos
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/documentos")]
        [HttpGet]
        public async Task<IEnumerable<Documents>> ObtenerDocumentos([FromBody] Documentos documentos)
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
                Documents documents = new();
                List<Documents> listaDocumentos = new();
                ApiSignatura apiSignatura = new()
                {
                    apiKey = documentos.apiKey
                };

                string respuesta = await apiSignatura.getDocumentos(documentos.usuarioApi);
                JArray resp = JArray.Parse(respuesta);

                if (resp != null)
                {
                    foreach (var item in resp.Children())
                    {
                        documents = JsonConvert.DeserializeObject<Documents>(item.ToString());
                        listaDocumentos.Add(documents);
                    }
                }

                return listaDocumentos;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/documentos/detalle")]
        [HttpPost]
        public async Task<ActionResult<DocumentDetail>> DetalleDocumentos([FromBody] Detalle detalle)
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
                DocumentDetail documents = new();
                List<DocumentDetail> listaDocumentos = new();
                ApiDynamicsV2 api = new();
                ApiSignatura apiSignatura = new()
                {
                    apiKey = detalle.apiKey
                };
                string respuesta = await apiSignatura.GetDocumentDetail(detalle.id, detalle.usuarioApi);
                if (!string.IsNullOrEmpty(respuesta))
                {
                    documents = JsonConvert.DeserializeObject<DocumentDetail>(respuesta);
                }

                string estadoDocumento = string.Empty;
                estadoDocumento = ObtenerEstadoDocumento(documents.status);

                JObject Documento = new()
                {
                    { "new_archivado", documents.archived }
                };

                if (!string.IsNullOrEmpty(estadoDocumento))
                    Documento.Add("statuscode", Convert.ToInt32(estadoDocumento));

                ResponseAPI documento_id = await api.UpdateRecord("new_documentosconfirmaelectronicas", detalle.documentoid, Documento, credenciales);

                return Ok(documents);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/documentos")]
        [HttpPost]
        public async Task<ActionResult> CrearDocumento([FromBody] CrearDocumento document)
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
            ApiDynamics apiV = new();
            ApiSignatura apiSignatura = new()
            {
                apiKey = document.ApiKey
            };
            string resultado = string.Empty;
            String file64 = string.Empty;
            List<Firmante> firmantes = new();
            
            try
            {
                string baseq = string.Empty;
                string titulo = string.Empty;
                string tipo = string.Empty;
                string socio = string.Empty;
                string nombreDocumento = string.Empty;
                string tipoFirma = string.Empty;
                string annotation_id = string.Empty;
                bool requiereFirmanteUN = false;
                JArray notas = null;

                if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                {
                    notas = await ObtenerDocumentoPorDocumentoID(document.DocumentacionPorCuenta_id, api, credenciales);
                }
                else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                {
                    notas = await ObtenerDocumentoOPPorDocumentoID(document.DocumentacionPorOperacion_id, api, credenciales);

                    if (!string.IsNullOrEmpty(document.Socio_id))
                    {
                        JArray socios = await ObtenerSocio(document.Socio_id, api, credenciales);
                        if (socios?.Count > 0)
                        {
                            socio = ObtenerSocioName(socios);
                        }
                    }
                }

                if (notas != null && notas.Count > 0)
                {
                    file documento = new();
                    responseSignature signature = new();
                    Signatures sig = new ();
                    Signatures[] firmas = new Signatures[1];
                    List<string> listaValidaciones = new();
                    string[] validacionesCrm;

                    foreach (var item in notas.Children())
                    {
                        if (item["annotationid"] != null)
                            annotation_id = item["annotationid"].ToString();

                        JArray NotaDocumentBody = await BuscarDocumentBody(annotation_id, api, credenciales);
                        if (NotaDocumentBody.Count == 0)
                            return BadRequest("No se encontro el body de la nota.");

                        DocumentBodyTemplate documentBodyTemplate = ArmarDocumentBodyTemplate(NotaDocumentBody);
                        if (!string.IsNullOrEmpty(documentBodyTemplate.documentbody))
                        {
                            baseq = documentBodyTemplate.documentbody;
                        }
                        else
                        {
                            return BadRequest("No se encontro el body de la nota.");
                        }

                        //if (item["documentbody"] != null)
                        //baseq = item["documentbody"].ToString();

                        if (item["filename"] != null)
                            titulo = item["filename"].ToString();

                        if (item["mimetype"] != null)   
                            tipo = item["mimetype"].ToString();

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            if (item["documento.new_cuentaid"] != null)
                                socio = item["documento.new_cuentaid@OData.Community.Display.V1.FormattedValue"].ToString();

                            if (item["documento.new_documentoid"] != null)
                                nombreDocumento = item["documento.new_documentoid@OData.Community.Display.V1.FormattedValue"].ToString();

                            if (item["documento.new_requierefirmanteunidaddenegocio"] != null && (bool)item["documento.new_requierefirmanteunidaddenegocio"] == true)
                            {
                                requiereFirmanteUN = true;
                            }
                        }
                        else
                        {
                            if (item["documento.new_documento"] != null)
                                nombreDocumento = item["documento.new_documento@OData.Community.Display.V1.FormattedValue"].ToString();

                            if (item["documento.new_requierefirmanteunidaddenegocio"] != null && (bool)item["documento.new_requierefirmanteunidaddenegocio"] == true)
                            {
                                requiereFirmanteUN = true;
                            }
                        }

                        if (item["documento.new_tipodevalidacion"] != null)
                        {
                            string tiposValidaciones = item["documento.new_tipodevalidacion@OData.Community.Display.V1.FormattedValue"].ToString();
                            if (tiposValidaciones.Contains(";"))
                            {
                                validacionesCrm = tiposValidaciones.Split(';');
                                foreach (var validacion in validacionesCrm)
                                {
                                    listaValidaciones.Add(validacion);
                                }
                            }
                            else
                            {
                                listaValidaciones.Add(tiposValidaciones);
                            }
                        }

                        if (item["documento.new_tipodefirma"] != null)
                        {
                            tipoFirma = ObtenerTipoFirma(item["documento.new_tipodefirma@OData.Community.Display.V1.FormattedValue"].ToString());
                        }
                        else
                        {
                            tipoFirma = ObtenerTipoFirma("");
                        }
                    }

                    int cantidadValidaciones = int.MinValue;

                    if (listaValidaciones.Count > 0)
                        cantidadValidaciones = listaValidaciones.Count;
                    else
                        cantidadValidaciones = 1;

                    string[] validaciones = new string[cantidadValidaciones];
                    if (listaValidaciones.Count > 0)
                    {
                        for (int i = 0; i < listaValidaciones.Count; i++)
                        {
                            string validacion = obtenerValidacion(listaValidaciones[i].Trim());
                            validaciones[i] = validacion;
                        }
                    }
                    else
                    {
                        validaciones[0] = "EM";
                    }

                    FirmantesYFirmas firmantesYFirmas = null;

                    if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                    {
                        firmantesYFirmas = await BuscarYArmarFirmantes(document.Documentacion_id, document.Socio_id, api, credenciales, document.DocumentacionPorCuenta_id);
                    }
                    else
                    {
                        firmantesYFirmas = await BuscarYArmarFirmantes(document.Documentacion_id, document.Socio_id, api, credenciales, null, document.DocumentacionPorOperacion_id);
                    }

                    if (firmantesYFirmas.firmantes == null || firmantesYFirmas.firmantes.Count == 0)
                    {
                        JObject error = new()
                        {
                            { "new_firmaelectronica", "No se encontraron firmantes." }
                        };

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                        }

                        return BadRequest("No se encontraron firmantes.");
                    }

                    //BUSCAR FIRMANTE EN UN
                    List<string> correos = new();
                    if (requiereFirmanteUN)
                    {
                        JArray FirmanteUN = await BuscarFirmanteUnidadDeNegocio(document.UnidadDeNegocio, api, credenciales);
                        if (FirmanteUN.Count > 0)
                        {
                            FirmanteSignatura firmanteUN = JsonConvert.DeserializeObject<FirmanteSignatura>(FirmanteUN.First().ToString());
                            if (string.IsNullOrEmpty(firmanteUN.emailaddress1))
                            {
                                JObject error = new()
                                {
                                    { "new_firmaelectronica", "El contacto firmante de la unidad de negocio debe contener correo electrónico" }
                                };

                                if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                                {
                                    await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                                }
                                else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                                {
                                    await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                                }

                                return BadRequest("El contacto firmante de la unidad de negocio debe contener correo electrónico.");
                            }

                            firmantesYFirmas.firmantes.Add(firmanteUN);
                        }
                    }
                    
                    foreach (var item in firmantesYFirmas.firmantes)
                    {
                        if (item.emailaddress1 != null)
                        {
                            //Arrojar excepcion de correo electronico
                            firmantes.Add(new Firmante(item?.contactid, item?.emailaddress1, item?.firstname));
                            correos.Add(item.emailaddress1);
                        }
                    }

                    if (tipo == "docx" || tipo == "application/vnd.openxmlformats-officedocument.wordprocessingml.document") 
                    {
                        JObject error = new()
                        {
                            { "new_firmaelectronica", "El archivo debe ser PDF." }
                        };

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                        }
                        
                        return BadRequest("El archivo debe ser PDF.");
                    }
                    else
                    {
                        documento.title = titulo;
                        documento.file_content = baseq;
                    }

                    documento.fashion = tipoFirma;
                    documento.validations = validaciones;
                    documento.selected_emails = correos.ToArray();
                    documento.required_signatures = firmantesYFirmas.firmasRequeridas;
                    string response = string.Empty;

                    if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                    {
                        response = await apiSignatura.createDocument(documento, credenciales, document.DocumentacionPorCuenta_id);
                    }
                    else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                    {
                        response = await apiSignatura.createDocument(documento, credenciales, null, document.DocumentacionPorOperacion_id);
                    }

                    if (response != null && response != "" && response != "Error")
                    {
                        string estadoDocumento = string.Empty;
                        signature = JsonConvert.DeserializeObject<responseSignature>(response);
                        estadoDocumento = ObtenerEstadoDocumento(signature.status);

                        if (titulo.Contains("."))
                        {
                            string[] tituloCompleto = titulo.Split('.');
                            nombreDocumento = tituloCompleto[0];
                        }

                        JObject Documento = new()
                        {
                            { "new_id", signature.id },
                            { "new_name", socio + " - " + nombreDocumento },
                            { "new_titulo", signature.title },
                            { "new_firmasrequeridas", signature.required_signatures },
                            { "new_configuracion", signature.file_content },
                            { "new_urlcompleta", signature.complete_url },
                            { "new_urlfirma", signature.fixed_sign_url },
                            { "new_UnidaddeNegocio@odata.bind", "/businessunits(" + document.UnidadDeNegocio + ")" }
                        };

                        {  }

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            Documento.Add("new_DocumentacionporCuenta@odata.bind", "/new_documentacionporcuentas(" + document.DocumentacionPorCuenta_id + ")");
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            Documento.Add("new_documentacionporoperacion@odata.bind", "/new_documentacionporoperacions(" + document.DocumentacionPorOperacion_id + ")");
                        }

                        if (!string.IsNullOrEmpty(estadoDocumento)) 
                            Documento.Add("statuscode", Convert.ToInt32(estadoDocumento));

                        ResponseAPI respuestaDocumentoConFirma = await api.CreateRecord("new_documentosconfirmaelectronicas", Documento, credenciales);

                        if (!respuestaDocumentoConFirma.ok)
                        {
                            JObject error = new()
                            {
                                { "new_firmaelectronica", "Error al crear documento con firma electronica." }
                            };

                            if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                            {
                                await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                            }
                            else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                            {
                                await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                            }

                            return BadRequest("Error al crear documento con firma electronica.");    
                        }

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            await ActualizarDescripcionEstadoFirmaElectronica(api,
                                "El documento se genero con éxito.", document.DocumentacionPorCuenta_id, credenciales);
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            JObject Docuxcuenta = new()
                            {
                                { "new_firmaelectronica", "El documento se genero con éxito." }
                            };

                            await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, Docuxcuenta, credenciales);
                        }

                        JObject annotation = new()
                        {
                            { "subject", documento.title },
                            { "isdocument", true },
                            { "mimetype", "application/pdf" },
                            { "documentbody", documento.file_content },
                            { "filename", documento.title }
                        };

                        if (respuestaDocumentoConFirma.ok)
                            annotation.Add("objectid_new_documentosconfirmaelectronica@odata.bind", "/new_documentosconfirmaelectronicas(" + respuestaDocumentoConFirma.descripcion + ")");

                        ResponseAPI respuestaCreacionNota = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!respuestaCreacionNota.ok)
                        {
                            JObject error = new()
                            {
                                { "new_firmaelectronica", "Error al crear nota." }
                            };

                            if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                            {
                                await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                            }
                            else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                            {
                                await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                            }

                            return BadRequest("Error al crear nota.");
                        }

                        //Eliminar nota docu x cuenta
                        string borrar_nota = apiV.DeleteRecord("annotations", annotation_id, credenciales);

                        if (signature.signatures != null && signature.signatures.Length > 0)
                        {
                            foreach (var firma in signature.signatures)
                            {
                                string estadoFirma = string.Empty;
                                string correoFirma = string.Empty;
                                string contact_id = string.Empty;
                                string nombreContacto = string.Empty;

                                estadoFirma = ObtenerEstadoFirmaDocumento(firma.status);

                                string detalleFirma = await apiSignatura.GetSignatureDetail(firma.id, credenciales);

                                if (detalleFirma != string.Empty && detalleFirma != null)
                                {
                                    Signatura detalle = JsonConvert.DeserializeObject<Signatura>(detalleFirma);

                                    if (detalle.validations.EM != null) //Extraemos el correo de la validacion
                                    {
                                        correoFirma = detalle.validations.EM.value;
                                    }

                                    if (correoFirma != string.Empty) //Buscamos el id del contacto por el correo
                                    {
                                        contact_id = firmantes.FirstOrDefault(x => x.correo.Equals(correoFirma)).contactid;

                                        nombreContacto = firmantes.FirstOrDefault(x => x.correo.Equals(correoFirma)).nombre;
                                    }
                                }

                                JObject Firma = new()
                                {
                                    { "new_id", firma.id },
                                    { "new_name", nombreContacto + " - " + socio + " - " + nombreDocumento },
                                    { "new_url", firma.url },
                                    { "new_DocumentoconFirmaElectronica@odata.bind", "/new_documentosconfirmaelectronicas(" + respuestaDocumentoConFirma.descripcion + ")" },
                                    { "new_Firmante@odata.bind", "/contacts(" + contact_id + ")" },
                                    { "new_UnidaddeNegocio@odata.bind", "/businessunits(" + document.UnidadDeNegocio + ")" }
                                };
                                if (!string.IsNullOrEmpty(estadoFirma)) 
                                    Firma.Add("statuscode", Convert.ToInt32(estadoFirma));
                                if (firma.validations != null)
                                    Firma.Add("new_validaciones", GenerarDescripcionValidacion(firma.validations));

                                ResponseAPI respuestaFirma = await api.CreateRecord("new_firmantesdedocumentoses", Firma, credenciales);

                                if (!respuestaFirma.ok)
                                {
                                    JObject error = new()
                                    {
                                        { "new_firmaelectronica", "Error al crear firmante de documento." }
                                    };

                                    if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                                    {
                                        await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                                    }
                                    else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                                    {
                                        await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                                    }

                                    return BadRequest("Error al crear firmante de documento.");
                                }
                            }
                        }
                    }
                    else
                    {
                        JObject error = new()
                        {
                            { "new_firmaelectronica", "Hubo un error al enviar el documento a signatura." }
                        };

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                        }
                    }

                }
                else
                {
                    JObject error = new()
                    {
                        { "new_firmaelectronica", "No se encontro el adjunto en la documentacion." }
                    };

                    if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                    {
                        await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                    }
                    else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                    {
                        await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                    }
                    return BadRequest("No se encontro el adjunto en la documentacion.");
                }

                return Ok("Proceso de signatura finalizado");
            }
            catch (Exception ex)
            {
                new Excepciones(credenciales.cliente, "Exepción en controlador Documentos signatura" + ex.Message);
                JObject error = new()
                {
                    { "new_firmaelectronica", "Error al generar firma." }
                };
                await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);

                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/generardocumento")]
        [HttpPost]
        public async Task<ActionResult> CrearDocumentoSignatura([FromBody] CrearDocumento document)
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
            ApiDynamics apiV = new();
            ApiSignatura apiSignatura = new()
            {
                apiKey = document.ApiKey
            };
            string resultado = string.Empty;
            String file64 = string.Empty;
            List<Firmante> firmantes = new();

            try
            {
                string baseq = string.Empty;
                string titulo = string.Empty;
                string tipo = string.Empty;
                string socio = string.Empty;
                string nombreDocumento = string.Empty;
                string tipoFirma = string.Empty;
                string annotation_id = string.Empty;
                bool requiereFirmanteUN = false;
                int cantidadFirmasRequeridas = 0;
                JArray notas = null;

                if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                {
                    notas = await ObtenerDocumentoPorDocumentoIDSignatura(document.DocumentacionPorCuenta_id, api, credenciales);
                }
                else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                {
                    notas = await ObtenerDocumentoOPPorDocumentoIDSignatura(document.DocumentacionPorOperacion_id, api, credenciales);

                    if (!string.IsNullOrEmpty(document.Socio_id))
                    {
                        JArray socios = await ObtenerSocio(document.Socio_id, api, credenciales);
                        if (socios?.Count > 0)
                        {
                            socio = ObtenerSocioName(socios);
                        }
                    }
                }

                if (notas != null && notas.Count > 0)
                {
                    file documento = new();
                    responseSignature signature = new();
                    Signatures sig = new();
                    Signatures[] firmas = new Signatures[1]; 
                    List<string> listaValidaciones = new();
                    string[] validacionesCrm;

                    foreach (var item in notas.Children())
                    {
                        if (item["annotationid"] != null)
                            annotation_id = item["annotationid"].ToString();

                        JArray NotaDocumentBody = await BuscarDocumentBody(annotation_id, api, credenciales);
                        if (NotaDocumentBody.Count == 0)
                            return BadRequest("No se encontro el body de la nota.");

                        DocumentBodyTemplate documentBodyTemplate = ArmarDocumentBodyTemplate(NotaDocumentBody);
                        if (!string.IsNullOrEmpty(documentBodyTemplate.documentbody))
                        {
                            baseq = documentBodyTemplate.documentbody;
                        }
                        else
                        {
                            return BadRequest("No se encontro el body de la nota.");
                        }

                        //if (item["documentbody"] != null)
                        //    baseq = item["documentbody"].ToString();

                        if (item["filename"] != null)
                            titulo = item["filename"].ToString();

                        if (item["mimetype"] != null)
                            tipo = item["mimetype"].ToString();

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            if (item["documento.new_cuentaid"] != null)
                                socio = item["documento.new_cuentaid@OData.Community.Display.V1.FormattedValue"].ToString();

                            if (item["documento.new_documentoid"] != null)
                                nombreDocumento = item["documento.new_documentoid@OData.Community.Display.V1.FormattedValue"].ToString();

                            if (item["documento.new_requierefirmanteunidaddenegocio"] != null && (bool)item["documento.new_requierefirmanteunidaddenegocio"] == true)
                            {
                                requiereFirmanteUN = true;
                            }
                        }
                        else
                        {
                            if (item["documento.new_documento"] != null)
                                nombreDocumento = item["documento.new_documento@OData.Community.Display.V1.FormattedValue"].ToString();

                            if (item["documento.new_requierefirmanteunidaddenegocio"] != null && (bool)item["documento.new_requierefirmanteunidaddenegocio"] == true)
                            {
                                requiereFirmanteUN = true;
                            }
                        }

                        if (item["documento.new_tipodevalidacion"] != null)
                        {
                            string tiposValidaciones = item["documento.new_tipodevalidacion@OData.Community.Display.V1.FormattedValue"].ToString();
                            if (tiposValidaciones.Contains(";"))
                            {
                                validacionesCrm = tiposValidaciones.Split(';');
                                foreach (var validacion in validacionesCrm)
                                {
                                    listaValidaciones.Add(validacion);
                                }
                            }
                            else
                            {
                                listaValidaciones.Add(tiposValidaciones);
                            }
                        }

                        if (item["documento.new_tipodefirma"] != null)
                        {
                            tipoFirma = ObtenerTipoFirma(item["documento.new_tipodefirma@OData.Community.Display.V1.FormattedValue"].ToString());
                        }
                        else
                        {
                            tipoFirma = ObtenerTipoFirma("");
                        }

                        if (item["documento.new_cantidadfirmasrequeridas"] != null)
                        {
                            cantidadFirmasRequeridas = (int)item["documento.new_cantidadfirmasrequeridas"];
                        }
                    }

                    int cantidadValidaciones = int.MinValue;

                    if (listaValidaciones.Count > 0)
                        cantidadValidaciones = listaValidaciones.Count;
                    else
                        cantidadValidaciones = 1;

                    string[] validaciones = new string[cantidadValidaciones];
                    if (listaValidaciones.Count > 0)
                    {
                        for (int i = 0; i < listaValidaciones.Count; i++)
                        {
                            string validacion = obtenerValidacion(listaValidaciones[i].Trim());
                            validaciones[i] = validacion;
                        }
                    }
                    else
                    {
                        validaciones[0] = "EM";
                    }

                    FirmantesYFirmas firmantesYFirmas = null;

                    if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                    {
                        firmantesYFirmas = await BuscarYArmarFirmantes(document.Documentacion_id, document.Socio_id, api, credenciales, document.DocumentacionPorCuenta_id);
                    }
                    else
                    {
                        firmantesYFirmas = await BuscarYArmarFirmantes(document.Documentacion_id, document.Socio_id, api, credenciales, null, document.DocumentacionPorOperacion_id);
                    }

                    if (firmantesYFirmas.firmantes == null || firmantesYFirmas.firmantes.Count == 0)
                    {
                        JObject error = new()
                        {
                            { "new_firmaelectronica", "No se encontraron firmantes." }
                        };

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                        }

                        return BadRequest("No se encontraron firmantes.");
                    }

                    //BUSCAR FIRMANTE EN UN
                    List<string> correos = new();
                    if (requiereFirmanteUN)
                    {
                        JArray FirmanteUN = await BuscarFirmanteUnidadDeNegocio(document.UnidadDeNegocio, api, credenciales);
                        if (FirmanteUN.Count > 0)
                        {
                            FirmanteSignatura firmanteUN = JsonConvert.DeserializeObject<FirmanteSignatura>(FirmanteUN.First().ToString());
                            if (string.IsNullOrEmpty(firmanteUN.emailaddress1))
                            {
                                JObject error = new()
                                {
                                    { "new_firmaelectronica", "El contacto firmante de la unidad de negocio debe contener correo electrónico" }
                                };

                                if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                                {
                                    await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                                }
                                else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                                {
                                    await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                                }

                                return BadRequest("El contacto firmante de la unidad de negocio debe contener correo electrónico.");
                            }

                            firmantesYFirmas.firmantes.Add(firmanteUN);
                        }
                    }

                    foreach (var item in firmantesYFirmas.firmantes)
                    {
                        if (item.emailaddress1 != null)
                        {
                            firmantes.Add(new Firmante(item?.contactid, item?.emailaddress1, item?.firstname));
                            correos.Add(item.emailaddress1);
                        }
                        else
                        {
                            JObject error = new()
                            {
                                { "new_firmaelectronica", $"El contacto {item?.firstname} {item?.lastname} no posee correo electrónico" }
                            };
                            await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);

                            return BadRequest($"El contacto {item?.firstname} {item?.lastname} no posee correo electrónico");
                        }
                    }

                    if (tipo == "docx" || tipo == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                    {
                        JObject error = new()
                        {
                            { "new_firmaelectronica", "El archivo debe ser PDF." }
                        };

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                        }

                        return BadRequest("El archivo debe ser PDF.");
                    }
                    else
                    {
                        documento.title = titulo;
                        documento.file_content = baseq;
                    }

                    documento.fashion = tipoFirma;
                    documento.validations = validaciones;
                    documento.selected_emails = correos.ToArray();
                    if (cantidadFirmasRequeridas > 0)
                        documento.required_signatures = cantidadFirmasRequeridas;
                    else
                        documento.required_signatures = firmantesYFirmas.firmasRequeridas;

                    if (cantidadFirmasRequeridas != documento.selected_emails.Length)
                    {
                        JObject error = new()
                        {
                            { "new_firmaelectronica", "Las firmas requeridas no deben ser superiores al número de firmantes." }
                        };

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                        }

                        return BadRequest("Las firmas requeridas no deben ser superiores al número de firmantes.");
                    }

                    string response = string.Empty;

                    if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                    {
                        response = await apiSignatura.createDocument(documento, credenciales, document.DocumentacionPorCuenta_id);
                    }
                    else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                    {
                        response = await apiSignatura.createDocument(documento, credenciales, null, document.DocumentacionPorOperacion_id);
                    }

                    if (response != null && response != "" && response != "Error")
                    {
                        string estadoDocumento = string.Empty;
                        signature = JsonConvert.DeserializeObject<responseSignature>(response);
                        estadoDocumento = ObtenerEstadoDocumento(signature.status);

                        if (titulo.Contains("."))
                        {
                            string[] tituloCompleto = titulo.Split('.');
                            nombreDocumento = tituloCompleto[0];
                        }

                        JObject Documento = new()
                        {
                            { "new_id", signature.id },
                            { "new_name", socio + " - " + nombreDocumento },
                            { "new_titulo", signature.title },
                            { "new_firmasrequeridas", signature.required_signatures },
                            { "new_configuracion", signature.file_content },
                            { "new_urlcompleta", signature.complete_url },
                            { "new_urlfirma", signature.fixed_sign_url },
                            { "new_UnidaddeNegocio@odata.bind", "/businessunits(" + document.UnidadDeNegocio + ")" }
                        };

                        { }

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            Documento.Add("new_DocumentacionporCuenta@odata.bind", "/new_documentacionporcuentas(" + document.DocumentacionPorCuenta_id + ")");
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            Documento.Add("new_documentacionporoperacion@odata.bind", "/new_documentacionporoperacions(" + document.DocumentacionPorOperacion_id + ")");
                        }

                        if (!string.IsNullOrEmpty(estadoDocumento))
                            Documento.Add("statuscode", Convert.ToInt32(estadoDocumento));

                        ResponseAPI respuestaDocumentoConFirma = await api.CreateRecord("new_documentosconfirmaelectronicas", Documento, credenciales);

                        if (!respuestaDocumentoConFirma.ok)
                        {
                            JObject error = new()
                            {
                                { "new_firmaelectronica", "Error al crear documento con firma electronica." }
                            };

                            if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                            {
                                await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                            }
                            else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                            {
                                await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                            }

                            return BadRequest("Error al crear documento con firma electronica.");
                        }

                        if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        {
                            await ActualizarDescripcionEstadoFirmaElectronica(api,
                                "El documento se genero con éxito.", document.DocumentacionPorCuenta_id, credenciales);
                        }
                        else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        {
                            JObject Docuxcuenta = new()
                            {
                                { "new_firmaelectronica", "El documento se genero con éxito." }
                            };

                            await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, Docuxcuenta, credenciales);
                        }

                        JObject annotation = new()
                        {
                            { "subject", documento.title },
                            { "isdocument", true },
                            { "mimetype", "application/pdf" },
                            { "documentbody", documento.file_content },
                            { "filename", documento.title }
                        };

                        if (respuestaDocumentoConFirma.ok)
                            annotation.Add("objectid_new_documentosconfirmaelectronica@odata.bind", "/new_documentosconfirmaelectronicas(" + respuestaDocumentoConFirma.descripcion + ")");

                        ResponseAPI respuestaCreacionNota = await api.CreateRecord("annotations", annotation, credenciales);

                        if (!respuestaCreacionNota.ok)
                        {
                            JObject error = new()
                            {
                                { "new_firmaelectronica", "Error al crear nota." }
                            };

                            if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                            {
                                await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                            }
                            else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                            {
                                await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                            }

                            return BadRequest("Error al crear nota.");
                        }

                        //Eliminar nota docu x cuenta
                        string borrar_nota = apiV.DeleteRecord("annotations", annotation_id, credenciales);

                        if (signature.signatures != null && signature.signatures.Length > 0)
                        {
                            foreach (var firma in signature.signatures)
                            {
                                string estadoFirma = string.Empty;
                                string correoFirma = string.Empty;
                                string contact_id = string.Empty;
                                string nombreContacto = string.Empty;

                                estadoFirma = ObtenerEstadoFirmaDocumento(firma.status);

                                string detalleFirma = await apiSignatura.GetSignatureDetail(firma.id, credenciales);

                                if (detalleFirma != string.Empty && detalleFirma != null)
                                {
                                    Signatura detalle = JsonConvert.DeserializeObject<Signatura>(detalleFirma);

                                    if (detalle.validations.EM != null) //Extraemos el correo de la validacion
                                    {
                                        correoFirma = detalle.validations.EM.value;
                                    }

                                    if (correoFirma != string.Empty) //Buscamos el id del contacto por el correo
                                    {
                                        contact_id = firmantes.FirstOrDefault(x => x.correo.Equals(correoFirma)).contactid;

                                        nombreContacto = firmantes.FirstOrDefault(x => x.correo.Equals(correoFirma)).nombre;
                                    }
                                }

                                JObject Firma = new()
                                {
                                    { "new_id", firma.id },
                                    { "new_name", nombreContacto + " - " + socio + " - " + nombreDocumento },
                                    { "new_url", firma.url },
                                    { "new_DocumentoconFirmaElectronica@odata.bind", "/new_documentosconfirmaelectronicas(" + respuestaDocumentoConFirma.descripcion + ")" },
                                    { "new_Firmante@odata.bind", "/contacts(" + contact_id + ")" },
                                    { "new_UnidaddeNegocio@odata.bind", "/businessunits(" + document.UnidadDeNegocio + ")" }
                                };
                                if (!string.IsNullOrEmpty(estadoFirma))
                                    Firma.Add("statuscode", Convert.ToInt32(estadoFirma));
                                if (firma.validations != null)
                                    Firma.Add("new_validaciones", GenerarDescripcionValidacion(firma.validations));

                                ResponseAPI respuestaFirma = await api.CreateRecord("new_firmantesdedocumentoses", Firma, credenciales);

                                if (!respuestaFirma.ok)
                                {
                                    JObject error = new()
                                    {
                                        { "new_firmaelectronica", "Error al crear firmante de documento." }
                                    };

                                    if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                                    {
                                        await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                                    }
                                    else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                                    {
                                        await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                                    }

                                    return BadRequest("Error al crear firmante de documento.");
                                }
                            }
                        }
                    }
                    //else
                    //{
                        //JObject error = new()
                        //{
                        //    { "new_firmaelectronica", "Hubo un error al enviar el documento a signatura." }
                        //};

                        //if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                        //{
                        //    await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                        //}
                        //else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                        //{
                        //    await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                        //}

                        //throw new Exception("Error al generar");
                    //}
                }
                else
                {
                    JObject error = new()
                    {
                        { "new_firmaelectronica", "No se encontro el adjunto en la documentacion." }
                    };

                    if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                    {
                        await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                    }
                    else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                    {
                        await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                    }
                    return BadRequest("No se encontro el adjunto en la documentacion.");
                }

                return Ok("Proceso de signatura finalizado");
            }
            catch (Exception ex)
            {
                new Excepciones(credenciales.cliente, "Exepción en controlador Documentos signatura" + ex.Message);
                JObject error = new()
                {
                    { "new_firmaelectronica", "Error al generar firma." }
                };
                if (!string.IsNullOrWhiteSpace(document.DocumentacionPorCuenta_id))
                {
                    await api.UpdateRecord("new_documentacionporcuentas", document.DocumentacionPorCuenta_id, error, credenciales);
                }
                else if (!string.IsNullOrWhiteSpace(document.DocumentacionPorOperacion_id))
                {
                    await api.UpdateRecord("new_documentacionporoperacions", document.DocumentacionPorOperacion_id, error, credenciales);
                }

                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/documentos/certificado")]
        [HttpPost]
        public async Task<ActionResult> Certificado(Certificado certificado)
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
                ApiSignatura apiSignatura = new()
                {
                    apiKey = certificado.ApiKey
                };

                string response = await apiSignatura.GetCertificate(certificado.Documentoid, credenciales.cliente);

                if (response != "Not Found")
                {
                    JObject annotation = new()
                    {
                        { "subject", "signatura" },
                        { "isdocument", true },
                        { "mimetype", "application/pdf" },
                        { "documentbody", response },
                        { "filename", "certificado-" + certificado.NombreDocumento },
                        { "objectid_new_documentosconfirmaelectronica@odata.bind", "/new_documentosconfirmaelectronicas(" + certificado.Documentofirmaelectronicaid + ")" }
                    };

                    ResponseAPI respuestaNota = await api.CreateRecord("annotations", annotation, credenciales);
                    if (!respuestaNota.ok)
                        return BadRequest("Error al crear certificado.");
                }
                else
                {
                    return BadRequest(response);
                }

                return Ok("Certificado generado.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/documentos/descargar")]
        [HttpPost]
        public async Task<ActionResult> Descargar(Descargar descarga)
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
                ApiSignatura apiSignatura = new()
                {
                    apiKey = descarga.ApiKey
                };

                string response = await apiSignatura.DownloadDocument(descarga.Documentoid, credenciales.cliente);

                if (response != "Not Found")
                {
                    JObject annotation = new()
                    {
                        { "subject", "signatura" },
                        { "isdocument", true },
                        { "mimetype", "application/pdf" },
                        { "documentbody", response },
                        { "filename", descarga.NombreDocumento },
                        { "objectid_new_documentosconfirmaelectronica@odata.bind", "/new_documentosconfirmaelectronicas(" + descarga.Documentofirmaelectronicaid + ")" }
                    };

                    ResponseAPI respuestaNota = await api.CreateRecord("annotations", annotation, credenciales);
                    if (!respuestaNota.ok)
                    {
                        return BadRequest("Error al crear nota.");
                    }
                }
                else
                {
                    return BadRequest(response);
                }

                return Ok("Descarga con exito.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/documentos/cancelardocumento")]
        [HttpPost]
        public async Task<ActionResult> CancelarDocumento(Cancelacion cancelacion)
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
                CancelacionResponse cancelacionResponse = new();
                ApiDynamicsV2 api = new();
                string resultado = string.Empty;
                ApiSignatura apiSignatura = new()
                {
                    apiKey = cancelacion.ApiKey
                };

                //string response = await apiSignatura.CancelDocument(cancelacion.Cancel_reason, cancelacion.Id, credenciales.cliente);

                string response = await apiSignatura.CancelDocument(cancelacion.Id, credenciales.cliente);

                if (response != null)
                {
                    cancelacionResponse = JsonConvert.DeserializeObject<CancelacionResponse>(response);
                }

                string estado = ObtenerEstadoDocumento(cancelacionResponse.status);

                JObject documento = new()
                {
                    { "statuscode", Convert.ToInt32(estado) }
                };

                ResponseAPI documentoResponse = await api.UpdateRecord("new_documentosconfirmaelectronicas", cancelacion.Documentoid, documento, credenciales);
                if (!documentoResponse.ok)
                    return BadRequest("Error al actualizar cancelacion.");

                return Ok("Cancelacion generada con exito.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/documentos/completardocumento")]
        [HttpPost]
        public async Task<string> CompletarDocumento(Completar completar)
        {
            try
            {
                ApiDynamics api = new ApiDynamics();
                string resultado = string.Empty;
                ApiSignatura apiSignatura = new ApiSignatura();
                apiSignatura.apiKey = completar.apiKey;

                string response = await apiSignatura.completeDocument(completar.id, completar.usuarioApi);

                return resultado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Firmas
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/firmas")]
        [HttpPut]
        public async Task<ActionResult<Signatura>> ActualizarDetalleFirma([FromBody] Signature firma)
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
                Signatura signatures = new();
                ApiDynamicsV2 api = new();
                ApiSignatura apiSignatura = new()
                {
                    apiKey = firma.ApiKey
                };

                string respuesta = await apiSignatura.GetSignatureDetail(firma.Firma_id, credenciales);

                if (respuesta != null)
                {
                    signatures = JsonConvert.DeserializeObject<Signatura>(respuesta);

                    if (signatures.id != null)
                    {
                        int estadoFirma = ObtenerEstadoFirma(signatures.status);

                        JObject Firma = new();

                        if (signatures.invalidation_reason != null)
                            Firma.Add("new_razondeinvalidacion", signatures.invalidation_reason);
                        if (signatures.signed_date != null)
                            Firma.Add("new_fechadefirmado", signatures.signed_date);
                        if (signatures.certificate != null)
                            Firma.Add("new_certificado", signatures.certificate);
                        if (signatures.signature_content != null)
                            Firma.Add("new_contenidodefirma", signatures.signature_content);
                        if (signatures.validations != null)
                            Firma.Add("new_validaciones", GenerarDescripcionValidacionFirma(signatures.validations));

                        ResponseAPI respuestaFirmaActualizada = await api.UpdateRecord("new_firmantesdedocumentoses", firma.Firmante_id, Firma, credenciales);

                        if (!respuestaFirmaActualizada.ok)
                        {
                            return BadRequest("Error al actualizar detalle firmante");
                        }
                    }
                }
                else
                {
                    return BadRequest("Error al consultar detalle signatura");
                }

                return signatures;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/firmas/invalidar")]
        [HttpPost]
        public async Task<ActionResult<Signatura>> InvalidarFirma([FromBody] Invalidar firma)
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
                Signatura signatures = new();
                ApiSignatura apiSignatura = new();
                ApiDynamics api = new();
                apiSignatura.apiKey = firma.apiKey;
                string respuesta = await apiSignatura.invalidateSignature(firma, firma.usuarioApi);

                if (respuesta != null)
                {
                    signatures = JsonConvert.DeserializeObject<Signatura>(respuesta);

                    if (signatures != null && signatures.id != null)
                    {
                        int estadoFirma = ObtenerEstadoFirma(signatures.status);

                        JObject Firma = new()
                        {
                            { "statuscode", estadoFirma }
                        };

                        string firmaActualizada = api.UpdateRecord("new_firmantesdedocumentoses", firma.firmaid, Firma, credenciales);
                    }
                }

                return signatures;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/signatura/firmas/reenviar")]
        [HttpPost]
        public async Task<string> Reenviar([FromBody] Reenviar firma)
        {
            try
            {
                string resultado = string.Empty;
                ApiSignatura apiSignatura = new();
                ApiDynamics api = new();
                apiSignatura.apiKey = firma.apiKey;
                string respuesta = await apiSignatura.resendSignature(firma, firma.usuarioApi);

                resultado = respuesta;

                return resultado;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Notificaciones
        [HttpPost]
        [Route("api/signatura/notificaciones")]
        public async Task<IActionResult> Notificaciones(string cliente, [FromBody] Notificaciones notificacion)
        {
            try
            {
                #region Credenciales
                Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente);
                if (credenciales == null)
                {
                    throw new Exception("No existen credenciales para ese cliente.");
                }
                #endregion
                string prueba = string.Empty;
                ApiDynamicsV2 api = new();
                JArray documento;
                JArray firmaDelDocumento;
                string firma_id = string.Empty;
                string documento_id = string.Empty;
                string estadoDocumento = string.Empty;

                if (notificacion.notification_action != null)
                {
                    switch (notificacion.notification_action)
                    {
                        case "DS":
                            firmaDelDocumento = obtenerFirmaPorID(notificacion.signature_id, credenciales);
                            if (firmaDelDocumento.Count == 0)
                                return BadRequest("Firmante no encontrado.");

                            foreach (var item in firmaDelDocumento.Children())
                            {
                                firma_id = item["new_firmantesdedocumentosid"].ToString();
                            }

                            if (!string.IsNullOrEmpty(firma_id))
                            {
                                JObject firma = new()
                                {
                                    { "statuscode", 100000001 }, //Completa
                                    { "new_fechadefirmado", DateTime.Now.ToString("yyyy-MM-dd") }
                                };

                                ResponseAPI respuestaFirma = await api.UpdateRecord("new_firmantesdedocumentoses", firma_id, firma, credenciales);
                                if (!respuestaFirma.ok)
                                {
                                    return BadRequest("Error al actualizar estado de firma.");
                                }
                            }
                            break;
                        case "DC":
                            documento = await ObtenerDocumentoPorID(notificacion.document_id, api, credenciales);
                            if (documento.Count == 0)
                                return BadRequest("Documento no encontrado.");

                            foreach (var item in documento.Children())
                            {
                                documento_id = item["new_documentosconfirmaelectronicaid"].ToString();
                                estadoDocumento = item["statuscode"].ToString();
                            }

                            if (documento_id != string.Empty)
                            {
                                JObject Documento = new();

                                switch (notificacion.new_status)
                                {
                                    case "PE":
                                        Documento.Add("statuscode", 1);
                                        break;
                                    case "CO":
                                        Documento.Add("statuscode", 100000000);
                                        break;
                                    case "CA":
                                        Documento.Add("statuscode", 100000001);
                                        break;
                                    default:
                                        Documento.Add("statuscode", 1);
                                        break;
                                }

                                ResponseAPI respuestaFirma = await api.UpdateRecord("new_documentosconfirmaelectronicas", documento_id, Documento, credenciales);
                                if (!respuestaFirma.ok)
                                {
                                    return BadRequest("Error al actualizar estado de firma");
                                }
                            }
                            break;
                        case "TU":
                            documento = await ObtenerDocumentoPorID(notificacion.document_id, api, credenciales);
                            if (documento.Count == 0)
                                return BadRequest("Documento no encontrado.");
                            
                            foreach (var item in documento.Children())
                            {
                                firma_id = item["firma.new_firmantesdedocumentosid"].ToString();
                            }

                            if (firma_id != string.Empty)
                            {
                                JObject Firma = new();

                                switch (notificacion.new_status)
                                {
                                    case "IN":
                                        Firma.Add("new_timestamp", 100000000);
                                        break;
                                    case "PE":
                                        Firma.Add("new_timestamp", 100000001);
                                        break;
                                    case "CO":
                                        Firma.Add("new_timestamp", 100000002);
                                        break;
                                    default:
                                        Firma.Add("new_timestamp", 100000000);
                                        break; 
                                }

                                Firma.Add("new_timestampid", notificacion.timestamp_id);

                                ResponseAPI timestampActualizado = await api.UpdateRecord("new_firmantesdedocumentoses", firma_id, Firma, credenciales);
                                if (!timestampActualizado.ok)
                                {
                                    return BadRequest("Error al actualizar timestamp en firma");
                                }
                            }
                            break;
                        default:
                            return BadRequest("No se encontro paridad en la opcion del campo notification_action.");
                    }
                }

                return Ok("Notificacion exitosa.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        public static string ObtenerEstadoDocumento(string estadoDocumento)
        {
            string estado = string.Empty;

            switch (estadoDocumento)
            {
                case "PE":
                    estado = "1";
                    break;
                case "CO":
                    estado = "100000000";
                    break;
                case "CA":
                    estado = "100000001";
                    break;
                default:
                    return "1";
            }

            return estado;
        }
        public static string ObtenerEstadoFirmaDocumento(string estadoDocumento)
        {
            string estado;
            switch (estadoDocumento)
            {
                case "IN":
                    estado = "1";
                    break;
                case "PE":
                    estado = "100000000";
                    break;
                case "CO":
                    estado = "100000001";
                    break;
                case "CA":
                    estado = "100000002";
                    break;
                default:
                    return "1";
            }
            return estado;
        }
        public static string obtenerValidacion(string nombreValidacion)
        {
            string validacion = string.Empty;

            switch (nombreValidacion)
            {
                case "Email":
                    validacion = "EM";
                    break;
                case "Facebook":
                    validacion = "FA";
                    break;
                case "Twitter":
                    validacion = "TW";
                    break;
                case "AFIP":
                    validacion = "AF";
                    break;
                case "Teléfono":
                    validacion = "PH";
                    break;
                default:
                    return "EM";
            }

            return validacion;
        }
        public static string GenerarDescripcionValidacion(Validation validation)
        {
            string descripcion = string.Empty;

            if (validation.AF != null)
            {
                descripcion = $"AFIP: Estado: {resultadoValidacion(false)}";
            }

            if (validation.EM != null)
            {
                if (descripcion != string.Empty)
                {
                    descripcion += $" | Correo: {validation.EM.value} Estado: {resultadoValidacion(validation.EM.validated)}";
                }
                else
                {
                    descripcion = $"Correo: {validation.EM.value} Estado: {resultadoValidacion(validation.EM.validated)}";
                }
            }

            if (validation.FA != null)
            {
                if (descripcion != string.Empty)
                {
                    descripcion += $" | Facebook: {validation.FA.value} Estado: {resultadoValidacion(validation.FA.validated)}";
                }
                else
                {
                    descripcion = $"Correo: {validation.FA.value} Estado: {resultadoValidacion(validation.FA.validated)}";
                }
            }

            if (validation.PH != null)
            {
                if (descripcion != string.Empty)
                {
                    descripcion += $" | Teléfono: {validation.PH.value} Estado: {resultadoValidacion(validation.PH.validated)}";
                }
                else
                {
                    descripcion = $"Teléfono: {validation.PH.value} Estado: {resultadoValidacion(validation.PH.validated)}";
                }
            }

            return descripcion;
        }
        public static async Task<ResponseAPI> ActualizarDescripcionEstadoFirmaElectronica(ApiDynamicsV2 api, string mensaje, string documentacionPorCuenta_id, Credenciales credenciales)
        {
            ResponseAPI resultado;
            try
            {
                JObject Docuxcuenta = new()
                {
                    { "new_firmaelectronica", mensaje }
                };

                resultado = await api.UpdateRecord("new_documentacionporcuentas", documentacionPorCuenta_id, Docuxcuenta, credenciales);
            }
            catch (Exception)
            {
                throw;
            }
            return resultado;
        }
        public static string resultadoValidacion(bool validacion)
        {
            string resultado = string.Empty;

            if (validacion == true)
            {
                resultado = "Validado";
            }
            else
            {
                resultado = "Pendiente de validación";
            }

            return resultado;
        }
        public static string ObtenerTipoFirma(string tipoFirma)
        {
            string firma;
            switch (tipoFirma)
            {
                case "Correo Electrónico":
                    firma = "SE";
                    break;
                case "Cantidad":
                    firma = "CA";
                    break;
                case "Irrestricta":
                    firma = "UN";
                    break;
                case "Firmas personalizadas":
                    firma = "CS";
                    break;
                case "Teléfonos seleccionados (Cuenta Premium)":
                    firma = "SP";
                    break;
                default:
                    return "SE";
            }

            return firma;
        }
        public static async Task<JArray> ObtenerSocio(string socio_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            JArray respuesta = null;
            api.EntityName = "accounts";
            string fetchXML = "<entity name='account'>" +
                    "<attribute name='name'/>" +
                    "<filter type='and'>" +
                        $"<condition attribute='accountid' operator='eq' value='{socio_id}' />" +
                    "</filter>" +
                "</entity>";

            if (api.EntityName != string.Empty)
            {

                if (fetchXML != string.Empty)
                {
                    api.FetchXML = fetchXML;
                }

                respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
            }

            return respuesta;
        }
        public static async Task<JArray> ObtenerDocumentoPorDocumentoID(string documento_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            JArray respuesta = null;
            api.EntityName = "annotations";
            string fetchXML = "<entity name='annotation'>" +
                    "<attribute name='filename'/>" +
                    "<attribute name='mimetype'/> " +
                    "<attribute name='subject'/> " +
                    "<attribute name='annotationid'/> " +
                    //"<attribute name='documentbody'/> " +
                    "<link-entity name='new_documentacionporcuenta' from='new_documentacionporcuentaid' to='objectid' link-type='inner' alias='documento'>" +
                            "<attribute name='new_cuentaid'/> " +
                            "<attribute name='new_documentoid'/> " +
                            "<attribute name='new_tipodefirma'/> " +
                            "<attribute name='new_tipodevalidacion'/> " +
                            "<attribute name='new_requierefirmanteunidaddenegocio'/> " +
                            "<filter type='and'>" +
                            $"<condition attribute='new_documentacionporcuentaid' operator='eq' value='{documento_id}' />" +
                            "</filter>" +
                    "</link-entity>" +
                "</entity>";

            if (api.EntityName != string.Empty)
            {

                if (fetchXML != string.Empty)
                {
                    api.FetchXML = fetchXML;
                }

                respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
            }

            return respuesta;
        }
        public static async Task<JArray> ObtenerDocumentoOPPorDocumentoID(string documento_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            JArray respuesta = null;
            api.EntityName = "annotations";
            string fetchXML = "<entity name='annotation'>" +
                    "<attribute name='filename'/>" +
                    "<attribute name='mimetype'/> " +
                    "<attribute name='subject'/> " +
                    "<attribute name='annotationid'/> " +
                    //"<attribute name='documentbody'/> " +
                    "<link-entity name='new_documentacionporoperacion' from='new_documentacionporoperacionid' to='objectid' link-type='inner' alias='documento'>" +
                            //"<attribute name='new_cuentaid'/> " +
                            "<attribute name='new_documento'/> " +
                            "<attribute name='new_tipodefirma'/> " +
                            "<attribute name='new_tipodevalidacion'/> " +
                            "<attribute name='new_requierefirmanteunidaddenegocio'/> " +
                            "<filter type='and'>" +
                            $"<condition attribute='new_documentacionporoperacionid' operator='eq' value='{documento_id}' />" +
                            "</filter>" +
                    "</link-entity>" +
                "</entity>";

            if (api.EntityName != string.Empty)
            {

                if (fetchXML != string.Empty)
                {
                    api.FetchXML = fetchXML;
                }

                respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
            }

            return respuesta;
        }
        public static async Task<JArray> ObtenerDocumentoPorDocumentoIDSignatura(string documento_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            JArray respuesta = null;
            api.EntityName = "annotations";
            string fetchXML = "<entity name='annotation'>" +
                    "<attribute name='filename'/>" +
                    "<attribute name='mimetype'/> " +
                    "<attribute name='subject'/> " +
                    "<attribute name='annotationid'/> " +
                    //"<attribute name='documentbody'/> " +
                    "<link-entity name='new_documentacionporcuenta' from='new_documentacionporcuentaid' to='objectid' link-type='inner' alias='documento'>" +
                            "<attribute name='new_cuentaid'/> " +
                            "<attribute name='new_documentoid'/> " +
                            "<attribute name='new_tipodefirma'/> " +
                            "<attribute name='new_tipodevalidacion'/> " +
                            "<attribute name='new_requierefirmanteunidaddenegocio'/> " +
                            "<attribute name='new_cantidadfirmasrequeridas'/> " +
                            "<filter type='and'>" +
                            $"<condition attribute='new_documentacionporcuentaid' operator='eq' value='{documento_id}' />" +
                            "</filter>" +
                    "</link-entity>" +
                "</entity>";

            if (api.EntityName != string.Empty)
            {

                if (fetchXML != string.Empty)
                {
                    api.FetchXML = fetchXML;
                }

                respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
            }

            return respuesta;
        }
        public static async Task<JArray> BuscarDocumentBody(string annotationid, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXml = String.Empty;
                api.EntityName = "annotations";

                fetchXml = string.Format(@"<entity name='annotation' >
                                <attribute name='documentbody' />
                                <order attribute='createdon' descending='true' />
                                <filter type='and' >
                                    <condition attribute='annotationid' operator='eq' value='{0}'/>
                                </filter >
                            </entity>", annotationid);


                if (api.EntityName != string.Empty)
                {

                    if (fetchXml != string.Empty)
                    {
                        api.FetchXML = fetchXml;
                    }

                    //respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<JArray> ObtenerDocumentoOPPorDocumentoIDSignatura(string documento_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            JArray respuesta = null;
            api.EntityName = "annotations";
            string fetchXML = "<entity name='annotation'>" +
                    "<attribute name='filename'/>" +
                    "<attribute name='mimetype'/> " +
                    "<attribute name='subject'/> " +
                    "<attribute name='annotationid'/> " +
                    //"<attribute name='documentbody'/> " +
                    "<link-entity name='new_documentacionporoperacion' from='new_documentacionporoperacionid' to='objectid' link-type='inner' alias='documento'>" +
                            //"<attribute name='new_cuentaid'/> " +
                            "<attribute name='new_documento'/> " +
                            "<attribute name='new_tipodefirma'/> " +
                            "<attribute name='new_tipodevalidacion'/> " +
                            "<attribute name='new_requierefirmanteunidaddenegocio'/> " +
                            "<attribute name='new_cantidadfirmasrequeridas'/> " +
                            "<filter type='and'>" +
                            $"<condition attribute='new_documentacionporoperacionid' operator='eq' value='{documento_id}' />" +
                            "</filter>" +
                    "</link-entity>" +
                "</entity>";

            if (api.EntityName != string.Empty)
            {

                if (fetchXML != string.Empty)
                {
                    api.FetchXML = fetchXML;
                }

                respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
            }

            return respuesta;
        }
        public static async Task<JArray> ObtenerDocumentoPorID(string documento_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;

                api.EntityName = "new_documentosconfirmaelectronicas";
                string fetchXML = "<entity name='new_documentosconfirmaelectronica'>" +
                                        "<attribute name='new_documentosconfirmaelectronicaid'/>" +
                                        "<attribute name='new_id'/> " +
                                        "<attribute name='new_name'/> " +
                                        "<attribute name='statuscode'/> " +
                                        "<filter type='and'>" +
                                                $"<condition attribute='new_id' operator='eq' value='{documento_id}' />" +
                                        "</filter>" +
                                    "</entity>";

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public static int ObtenerEstadoFirma(string estado)
        {
            int estadoFirma = int.MinValue;
            switch (estado)
            {
                case "IN":
                    break;
                case "PE":
                    estadoFirma = 100000000;
                    break;
                case "CO":
                    estadoFirma = 100000001;
                    break;
                case "CA":
                    estadoFirma = 100000002;
                    break;
                default:
                    estadoFirma = 1;
                    break;
            }
            return estadoFirma;
        }
        public static string GenerarDescripcionValidacionFirma(Validations validation)
        {
            string descripcion = string.Empty;

            if (validation.AF.value != null)
            {
                descripcion = $"AFIP: {validation.AF.value.full_name} - {validation.AF.value.CUIT} - {validation.AF.value.tipo_persona} Estado: {resultadoValidacion(validation.AF.validated)}";
            }

            if (validation.EM != null)
            {
                if (descripcion != string.Empty)
                {
                    descripcion += $" | Correo: {validation.EM.value} Estado: {resultadoValidacion(validation.EM.validated)}";
                }
                else
                {
                    descripcion = $"Correo: {validation.EM.value} Estado: {resultadoValidacion(validation.EM.validated)}";
                }
            }

            if (validation.FA != null)
            {
                if (descripcion != string.Empty)
                {
                    descripcion += $" | Facebook: {validation.FA.value} Estado: {resultadoValidacion(validation.FA.validated)}";
                }
                else
                {
                    descripcion = $"Correo: {validation.FA.value} Estado: {resultadoValidacion(validation.FA.validated)}";
                }
            }

            if (validation.PH != null)
            {
                if (descripcion != string.Empty)
                {
                    descripcion += $" | Teléfono: {validation.PH.value} Estado: {resultadoValidacion(validation.PH.validated)}";
                }
                else
                {
                    descripcion = $"Teléfono: {validation.PH.value} Estado: {resultadoValidacion(validation.PH.validated)}";
                }
            }

            return descripcion;
        }
        public static JArray obtenerFirmaPorID(string firma_id, Credenciales credenciales)
        {
            string estado = string.Empty;
            JArray respuesta = null;
            string fetchXML = string.Empty;
            ApiDynamics api = new ApiDynamics();
            List<dynamic> listaEntidad = new List<dynamic>();

            api.EntityName = "new_firmantesdedocumentoses";
            fetchXML = "<fetch mapping='logical'>" +
                                                   "<entity name='new_firmantesdedocumentos'>" +
                                                       "<attribute name='new_firmantesdedocumentosid'/>" +
                                                       "<attribute name='new_name'/>" +
                                                       "<attribute name='createdon'/> " +
                                                       "<attribute name='statuscode'/> " +
                                                       "<attribute name='new_id'/> " +
                                                       "<filter type='and'>" +
                                                        $"<condition attribute='new_id' operator='eq' value='{firma_id}' />" +
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
        public static async Task<JArray> BuscarRolesFirmantes(string documnento_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;
                api.EntityName = "new_documentacions";

                fetchXML = string.Format(@"<entity name='new_documentacion' >
                                <attribute name='new_documentacionid' />
                                <attribute name='new_rolesfirmantes' />
                                <attribute name='new_cantidadfirmasrequeridas' />
                                <order attribute='createdon' descending='true' />
                                <filter type='and' >
                                    <condition attribute='new_documentacionid' operator='eq' value='{0}' />
                                </filter >
                            </entity>", documnento_id);

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarFirmante(string cuenta_id, string[] tipos, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;
                string filtroTipo = string.Empty;
                api.EntityName = "new_participacionaccionarias";
                
                if (tipos.Length > 0)
                {
                    filtroTipo = "<condition attribute='new_tipoderelacion' operator='in'>";
                    foreach (var item in tipos)
                    {
                        filtroTipo += $"<value>{item}</value>";
                    }
                    filtroTipo += " </condition>";
                }

                fetchXML = string.Format(@"<entity name='new_participacionaccionaria' >
                                <attribute name='new_participacionaccionariaid' />
                                <attribute name='new_name' />
                                <order attribute='createdon' descending='true' />
                                <filter type='and' >
                                    {1}
                                    <condition attribute='new_cuentaid' operator='eq' value='{0}' />
                                    <condition attribute='


' operator='eq' value='0' />
                                </filter >
                                <link-entity name='contact' from='contactid' to='new_cuentacontactovinculado' link-type='outer' alias='contacto'>
                                    <attribute name='contactid' />   
                                    <attribute name='emailaddress1' />
                                    <attribute name='firstname'/>
                                    <attribute name='lastname'/>
                                </link-entity >
                            </entity>", cuenta_id, filtroTipo);

                //< link - entity name = 'account' from = 'accountid' to = 'new_cuentacontactovinculado' link - type = 'outer' alias = 'cuenta' >
                //                    < attribute name = 'emailaddress1' />
                //                    < attribute name = 'accountid' />
                //                    < attribute name = 'name' />
                //                </ link - entity >

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<JArray> BuscarFirmanteEntity(string documentacionXcuenta, string documentacionXoperacion, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;
                string filtroTipo = string.Empty;
                api.EntityName = "new_firmantes";

                if(documentacionXcuenta != null)
                {
                    filtroTipo = $"<condition attribute='new_documentacionporcuenta' operator='eq' value='{documentacionXcuenta}'/>";
                }
                else if (documentacionXoperacion != null)
                {
                    filtroTipo = $"<condition attribute='new_documentacionporoperacion' operator='eq' value='{documentacionXoperacion}'/>";
                }

                fetchXML = string.Format(@"<entity name='new_firmante' >
                                <attribute name='new_firmanteid' />
                                <attribute name='new_cuenta' />
                                <attribute name='new_contactofirmante' />
                                <order attribute='createdon' descending='true' />
                                <filter type='and' >
                                    {0}
                                    <condition attribute='statecode' operator='eq' value='0' />
                                </filter >
                                <link-entity name='contact' from='contactid' to='new_contactofirmante' link-type='outer' alias='contacto'>
                                    <attribute name='contactid' />   
                                    <attribute name='emailaddress1' />
                                    <attribute name='firstname'/>
                                    <attribute name='lastname'/>
                                </link-entity >
                            </entity>", filtroTipo);

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task<FirmantesYFirmas> BuscarYArmarFirmantes(string documentacion_id, string cuenta_id, ApiDynamicsV2 api, Credenciales credenciales,
            string documentacionPorCuenta_id = null, string documentacionPorOp_id = null)
        {
            try
            {
                FirmantesYFirmas firmantesYFirmas = new();
                JArray rolesFirmantes = await BuscarRolesFirmantes(documentacion_id, api, credenciales);
                if (rolesFirmantes.Count > 0)
                {
                    DocumentacionSignatura documentoS = JsonConvert.DeserializeObject<DocumentacionSignatura>(rolesFirmantes.First.ToString());

                    if (documentoS.new_cantidadfirmasrequeridas > 0)
                        firmantesYFirmas.firmasRequeridas = documentoS.new_cantidadfirmasrequeridas;

                    JArray contactosFirmantes = await BuscarFirmanteEntity(documentacionPorCuenta_id, documentacionPorOp_id, api, credenciales);

                    if (contactosFirmantes.Count > 0)
                        firmantesYFirmas.firmantes = JsonConvert.DeserializeObject<List<FirmanteSignatura>>(contactosFirmantes.ToString());
                }

                return firmantesYFirmas;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public static string ObtenerSocioName(JToken socioJT)
        {
            string socioName = string.Empty;

            SocioOperacion socioOP = JsonConvert.DeserializeObject<SocioOperacion>(socioJT.First().ToString());
            if (socioOP.name != null)
                socioName = socioOP.name;

            return socioName;
        }
        public static async Task<JArray> BuscarFirmanteUnidadDeNegocio(string unidadDeNegocio_id, ApiDynamicsV2 api, Credenciales credenciales)
        {
            try
            {
                JArray respuesta = null;
                string fetchXML = string.Empty;
                api.EntityName = "businessunits";

                fetchXML = string.Format(@"<entity name='businessunit' >
                                <attribute name='businessunitid'/>
                                <attribute name='new_contactofirmante'/>
                                <filter type='and'>
                                    <condition attribute='businessunitid' operator='eq' value='{0}'/>
                                </filter>
                                <link-entity name='contact' from='contactid' to='new_contactofirmante' link-type='outer' alias='contacto'>
                                    <attribute name='contactid'/>   
                                    <attribute name='emailaddress1'/>
                                    <attribute name='firstname'/>
                                    <attribute name='lastname'/>
                                </link-entity>
                            </entity>", unidadDeNegocio_id);

                if (api.EntityName != string.Empty)
                {

                    if (fetchXML != string.Empty)
                    {
                        api.FetchXML = fetchXML;
                    }

                    respuesta = await api.RetrieveMultipleWithFetch(api, credenciales);
                }

                return respuesta;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DocumentBodyTemplate ArmarDocumentBodyTemplate(JToken notaJT)
        {
            return JsonConvert.DeserializeObject<DocumentBodyTemplate>(notaJT.First().ToString());
        }
    }
}
