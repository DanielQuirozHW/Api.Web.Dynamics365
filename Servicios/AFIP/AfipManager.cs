using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Text;
using System.Xml;
using ServiceReference1;
using System.ServiceModel;
using Api.Web.Dynamics365.Models;
using Wsaa;
using Microsoft.AspNetCore.Server.Kestrel;
using static Api.Web.Dynamics365.Models.Afip;
using System.Globalization;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;

namespace Api.Web.Dynamics365.Servicios.AFIP
{
    public class AfipManager
    {
        #region Parametros
        const string DEFAULT_URLWSAAWSDL = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms?WSDL";
        const string DEFAULT_SERVICIO = "wsfe";
        const string DEFAULT_CERTSIGNER = "C:\\Desarrollos Clientes\\HWalias.p12";

        const bool DEFAULT_VERBOSE = true;
        private readonly CrmManager crm;
        public uint UniqueId; // Entero de 32 bits sin signo que identifica el requerimiento
        public DateTime GenerationTime; // Momento en que fue generado el requerimiento
        public DateTime ExpirationTime; // Momento en el que expira la solicitud
        public string Service; // Identificacion del WSN para el cual se solicita el TA
        public string Sign; // Firma de seguridad recibida en la respuesta
        public string Cuit;
        public string Token; // Token de seguridad recibido en la respuesta

        public XmlDocument XmlLoginTicketRequest = null;
        public XmlDocument XmlLoginTicketResponse = null;
        public string RutaDelCertificadoFirmante;
        public string XmlStrLoginTicketRequestTemplate = "<loginTicketRequest><header><uniqueId></uniqueId><generationTime></generationTime><expirationTime></expirationTime></header><service></service></loginTicketRequest>";
        private bool _verboseMode = true;
        private static uint _globalUniqueID = 0; // OJO! NO ES THREAD-SAFE
        public ServiceSoapClient afipService;
        #endregion
        public AfipManager(CrmManager crm)
        {
            this.crm = crm;
        }
        public async Task<FEAuthRequest> ObtenerLoginTicket(string argServicio, string argUrlWsaa, byte[] argRutaCertX509Firmante,
           SecureString argPassword, string tokenCrm, string signCrm, string CuitRepresentado)
        {

            Cuit = CuitRepresentado;

            const string ID_FNC = "[ObtenerLoginTicketResponse]";
            //this.RutaDelCertificadoFirmante = argRutaCertX509Firmante;
            CertificadosX509Lib.VerboseMode = true;
            string cmsFirmadoBase64 = null;
            string loginTicketResponse = null;
            XmlNode xmlNodoUniqueId = default;
            XmlNode xmlNodoGenerationTime = default;
            XmlNode xmlNodoExpirationTime = default;
            XmlNode xmlNodoService = default;

            // PASO 1: Genero el Login Ticket Request
            try
            {
                CultureInfo ci = CultureInfo.GetCultureInfo("es-AR");

                _globalUniqueID += 1;

                XmlLoginTicketRequest = new XmlDocument();
                XmlLoginTicketRequest.LoadXml(XmlStrLoginTicketRequestTemplate);

                xmlNodoUniqueId = XmlLoginTicketRequest.SelectSingleNode("//uniqueId");
                xmlNodoGenerationTime = XmlLoginTicketRequest.SelectSingleNode("//generationTime");
                xmlNodoExpirationTime = XmlLoginTicketRequest.SelectSingleNode("//expirationTime");
                xmlNodoService = XmlLoginTicketRequest.SelectSingleNode("//service");
                xmlNodoGenerationTime.InnerText = DateTime.UtcNow.AddHours(-3).AddMinutes(-10).ToString("s", ci);
                xmlNodoExpirationTime.InnerText = DateTime.Now.AddHours(-3).AddMinutes(10).ToString("s", ci);
                //xmlNodoGenerationTime.InnerText = DateTime.Now.AddMinutes(-10).ToString("s", ci); //LOCALHOST
                //xmlNodoExpirationTime.InnerText = DateTime.Now.AddMinutes(10).ToString("s", ci);//LOCALHOST
                xmlNodoUniqueId.InnerText = Convert.ToString(_globalUniqueID);
                xmlNodoService.InnerText = argServicio;
                Service = argServicio;

                //DateTime.Now.AddMinutes(-10).ToString("s");
                //DateTime.Now.AddMinutes(10).ToString("s");
            }
            catch (Exception excepcionAlGenerarLoginTicketRequest)
            {
                throw new Exception("***Error GENERANDO el LoginTicketRequest : " + excepcionAlGenerarLoginTicketRequest.Message + excepcionAlGenerarLoginTicketRequest.StackTrace);
            }

            // PASO 2: Firmo el Login Ticket Request
            try
            {
                //X509Certificate2 certFirmante = CertificadosX509Lib.ObtieneCertificadoDesdeArchivo(argRutaCertX509Firmante, argPassword);
                //X509Certificate2 certFirmante = CertificadosX509Lib.ObtieneCertificadoDesdeArchivo(RutaDelCertificadoFirmante, argPassword);
                X509Certificate2 certFirmante = new (argRutaCertX509Firmante, argPassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                             | X509KeyStorageFlags.Exportable);
                // Convierto el Login Ticket Request a bytes, firmo el msg y lo convierto a Base64
                Encoding EncodedMsg = Encoding.UTF8;
                byte[] msgBytes = EncodedMsg.GetBytes(XmlLoginTicketRequest.OuterXml);
                byte[] encodedSignedCms = CertificadosX509Lib.FirmaBytesMensaje(msgBytes, certFirmante);
                cmsFirmadoBase64 = Convert.ToBase64String(encodedSignedCms);
            }
            catch (Exception excepcionAlFirmar)
            {
                throw new Exception(ID_FNC + "***Error FIRMANDO el LoginTicketRequest : " + excepcionAlFirmar.Message);
            }

            // PASO 3: Invoco al WSAA para obtener el Login Ticket Response
            try
            {
                loginCmsRequestBody loginBody = new(cmsFirmadoBase64);
                loginCmsRequest loginRequest = new(loginBody);
                LoginCMSClient loginClient = new(LoginCMSClient.EndpointConfiguration.LoginCms, argUrlWsaa);
                //loginClient.Endpoint = argUrlWsaa;
                loginCmsResponse loginResponse = await loginClient.loginCmsAsync(loginRequest);
                loginTicketResponse = loginResponse.Body.loginCmsReturn;
            }
            catch (Exception excepcionAlInvocarWsaa)
            {
                if (excepcionAlInvocarWsaa.Message.Equals("El CEE ya posee un TA valido para el acceso al WSN solicitado"))
                {
                    Sign = signCrm;  //"pZx8cr+4UXC3H2re11GVPrXTHPRVHdJJ8eZrpvch1sdoMN7H485cFPcQz8EH/wnfZtfgZKD0XDvaOlMFKxeac76WLxb5atKmRiBzCttOxJtJDMjVDexl551KqqVW9T8riD/yGowChnkL2tuAdTnU2F3vNCaXg5tpqI2jxSnZLX0=";
                    Token = tokenCrm; //"PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9InllcyI/Pgo8c3NvIHZlcnNpb249IjIuMCI+CiAgICA8aWQgc3JjPSJDTj13c2FhaG9tbywgTz1BRklQLCBDPUFSLCBTRVJJQUxOVU1CRVI9Q1VJVCAzMzY5MzQ1MDIzOSIgdW5pcXVlX2lkPSI2MzExMjgyMDgiIGdlbl90aW1lPSIxNTU0MzA3ODUyIiBleHBfdGltZT0iMTU1NDM1MTExMiIvPgogICAgPG9wZXJhdGlvbiB0eXBlPSJsb2dpbiIgdmFsdWU9ImdyYW50ZWQiPgogICAgICAgIDxsb2dpbiBlbnRpdHk9IjMzNjkzNDUwMjM5IiBzZXJ2aWNlPSJ3c210eGNhIiB1aWQ9IlNFUklBTE5VTUJFUj1DVUlUIDI3MjY5Mjg3ODA4LCBDTj1zZ3JvbmVjbGljayIgYXV0aG1ldGhvZD0iY21zIiByZWdtZXRob2Q9IjIyIj4KICAgICAgICAgICAgPHJlbGF0aW9ucz4KICAgICAgICAgICAgICAgIDxyZWxhdGlvbiBrZXk9IjI3MjY5Mjg3ODA4IiByZWx0eXBlPSI0Ii8+CiAgICAgICAgICAgIDwvcmVsYXRpb25zPgogICAgICAgIDwvbG9naW4+CiAgICA8L29wZXJhdGlvbj4KPC9zc28+Cg==";
                }
                else
                {
                    throw new Exception(ID_FNC + "***Error INVOCANDO al servicio WSAA : " + excepcionAlInvocarWsaa.Message + xmlNodoGenerationTime.InnerText + '/' + ExpirationTime);
                }
            }

            // PASO 4: Analizo el Login Ticket Response recibido del WSAA
            try
            {
                if (loginTicketResponse != null)
                {
                    XmlLoginTicketResponse = new XmlDocument();
                    XmlLoginTicketResponse.LoadXml(loginTicketResponse);

                    UniqueId = uint.Parse(XmlLoginTicketResponse.SelectSingleNode("//uniqueId").InnerText);
                    GenerationTime = DateTime.Parse(XmlLoginTicketResponse.SelectSingleNode("//generationTime").InnerText);
                    ExpirationTime = DateTime.Parse(XmlLoginTicketResponse.SelectSingleNode("//expirationTime").InnerText);
                    Sign = XmlLoginTicketResponse.SelectSingleNode("//sign").InnerText;
                    Token = XmlLoginTicketResponse.SelectSingleNode("//token").InnerText;
                }
            }
            catch (Exception excepcionAlAnalizarLoginTicketResponse)
            {
                throw new Exception(ID_FNC + "***Error ANALIZANDO el LoginTicketResponse : " + excepcionAlAnalizarLoginTicketResponse.Message);
            }

            return GetAuth();
        }
        public FEAuthRequest GetAuth()
        {
            FEAuthRequest auth = new FEAuthRequest();

            auth.Cuit = long.Parse(Cuit);
            auth.Sign = Sign;
            auth.Token = Token;
            return auth;
        }
        public async Task ConsultarNrosAutorizados(FEAuthRequest auth, int PuntoVenta, int TipoComprobante, string IdParametroWSAfip, Credenciales credenciales)
        {
            if (afipService == null) InvokeWs();

            FECompUltimoAutorizadoRequestBody requestUltimoBody = new(auth, PuntoVenta, TipoComprobante);

            FECompUltimoAutorizadoRequest requestUltimo = new FECompUltimoAutorizadoRequest(requestUltimoBody);

            FECompUltimoAutorizadoResponse ultimo = await afipService.FECompUltimoAutorizadoAsync(requestUltimo);

            crm.UpdateTokenYSign(Token, Sign, IdParametroWSAfip, ultimo.Body.FECompUltimoAutorizadoResult.CbteNro, TipoComprobante, credenciales);
        }
        public void InvokeWs()
        {
            BasicHttpsBinding myBinding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
            myBinding.Name = "BasicHttpBinding_MTXCAService";
            myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            // Endpoint Address defining the asmx Service to be called.https://wsaa.afip.gov.ar/ws/services.asmx
            //EndpointAddress endPointAddress = new EndpointAddress(@"https://wswhomo.afip.gov.ar/wsfev1/service.asmx");
            EndpointAddress endPointAddress = new EndpointAddress(@"https://servicios1.afip.gov.ar/wsfev1/service.asmx");
            myBinding.SendTimeout = new TimeSpan(0, 3, 0);
            myBinding.OpenTimeout = new TimeSpan(0, 3, 0);
            myBinding.ReceiveTimeout = new TimeSpan(0, 3, 0);
            myBinding.MaxReceivedMessageSize = 2147483647;
            myBinding.MaxBufferSize = 2147483647;
            myBinding.MaxBufferPoolSize = 2147483647;

            afipService = new ServiceSoapClient(myBinding, endPointAddress);
        }
        //public async Task<FECompConsultaResponse> ConsultarComprobante(FEAuthRequest auth, FECompConsultaReq comprobante)
        //{
        //    try
        //    {
        //        FECompConsultarRequestBody requestBody = new(auth, comprobante);
        //        FECompConsultarRequest request = new(requestBody);
        //        FECompConsultarResponse consulta = await afipService.FECompConsultarAsync(request);
        //        return consulta;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
            
        //}

        public async Task<FECompConsultarResponse> ConsultarComprobante2(FEAuthRequest auth, FECompConsultaReq comprobante)
        {
            FECompConsultarRequestBody requestBody = new(auth, comprobante);
            FECompConsultarRequest request = new(requestBody);
            FECompConsultarResponse consultar = await afipService.FECompConsultarAsync(request);
            return consultar;
        }
        public async Task<ResultadoAFIP> AutorizarComprobante(List<DTOFeDetReq> comprobantes, FEAuthRequest auth, int PuntoVenta, int TipoComprobante, Credenciales credenciales)
        {
            ResultadoAFIP resultado = new();
            FECAERequest Compte = new()
            {
                FeCabReq = new FECAECabRequest()
            };

            Compte.FeCabReq.CantReg = comprobantes.Count;
            Compte.FeCabReq.CbteTipo = TipoComprobante;
            Compte.FeCabReq.PtoVta = PuntoVenta;
            List<FECAEDetRequest> detalles = new();

            try
            {
                InvokeWs();
            }
            catch (Exception ex)
            {
                //System.Console.Write(ex.Message);
                //System.Console.ReadLine();
                throw new Exception(ex.Message);
            }

            foreach (DTOFeDetReq cabecera in comprobantes)
            {
                //Verifico los rechazados antes de enviar un nuevo request
                if (cabecera.new_estadoafip > 0)
                {

                    if (cabecera.new_estadoafip.Equals(100000001)) //1- statuscode 
                    {
                        FECompConsultaReq comprobante = new()
                        {
                            CbteNro = cabecera.NroComprobante,
                            CbteTipo = cabecera.TipoComprobante,
                            PtoVta = cabecera.PuntoVenta
                        };

                        FECompConsultarResponse existe = await ConsultarComprobante2(auth, comprobante);
                        if (existe.Body.FECompConsultarResult.ResultGet != null)
                        {
                            await crm.UpdateComprobante(existe.Body.FECompConsultarResult.ResultGet.CodAutorizacion, DateTime.ParseExact(existe.Body.FECompConsultarResult.ResultGet.FchVto, "yyyyMMdd", CultureInfo.InvariantCulture),
                                existe.Body.FECompConsultarResult.ResultGet.CbteDesde, existe.Body.FECompConsultarResult.ResultGet.PtoVta, 
                                existe.Body.FECompConsultarResult.ResultGet.CbteTipo, existe.Body.FECompConsultarResult.ResultGet.Observaciones, credenciales);
                        }
                    }
                }

                FECAEDetRequest detalle = new()
                {
                    Concepto = cabecera.Concepto, //2;
                    DocTipo = cabecera.TipoDocumento, // 80;
                    DocNro = cabecera.NroDocumento, //30710362218;
                    CbteFch = cabecera.FechaEmision, // DateTime.Now.Date.ToString("yyyyMMdd");
                                                     // detalle.CbteFch =  "20200101";
                    ImpTotal = cabecera.ImporteTotal, // 24200;
                    ImpNeto = cabecera.ImporteNeto, // 20000;
                    ImpIVA = cabecera.ImporteIva, // 4200;
                    ImpTotConc = cabecera.ImporteNetoNoGravado, // 0;
                    ImpOpEx = cabecera.ImporteExcento, // 0;
                    ImpTrib = cabecera.ImporteTributos, // 0;

                    FchServDesde = cabecera.FechaDesde, //new DateTime(2019, 4, 1).Date.ToString("yyyyMMdd");
                    FchServHasta = cabecera.FechaHasta, // new DateTime(2019, 4, 30).Date.ToString("yyyyMMdd");
                    FchVtoPago = cabecera.FechaVtoPago, // new DateTime(2019, 5, 1).Date.ToString("yyyyMMdd");
                    MonId = cabecera.MonedaId, // "PES";
                    MonCotiz = cabecera.Cotizacion, // 1;
                    CbteDesde = cabecera.NroComprobante,
                    CbteHasta = cabecera.NroComprobante
                };
                //detalle.CbteDesde = 1;
                //detalle.CbteHasta = 1;

                List<AlicIva> listIva = new();

                foreach (DTOAlicIva item in cabecera.detalles)
                {
                    AlicIva iva = new()
                    {
                        BaseImp = item.BaseImponible, //20000;
                        Importe = item.Total // 4200;
                    };

                    if (iva.Importe == 0)
                        iva.Id = 3;
                    else
                        iva.Id = item.TipoIva; // 5;

                    listIva.Add(iva);
                }

                if (listIva.Count > 0)
                {
                    detalle.Iva = listIva.ToArray();
                }

                if (cabecera.percepciones != null && cabecera.percepciones.Count > 0)
                {
                    List<Tributo> listTributos = new();

                    foreach (DTOTributos percepcion in cabecera.percepciones)
                    {
                        Tributo item = new()
                        {
                            Alic = percepcion.Alicuota,
                            BaseImp = percepcion.BaseImponible,
                            Id = short.Parse(percepcion.Tipo.ToString()),
                            Importe = percepcion.Total
                        };
                        listTributos.Add(item);
                    }

                    detalle.Tributos = listTributos.ToArray();
                }

                //Si es NC o ND en todas sus letras tiene que informar Comprobante Asociado
                if (TipoComprobante.Equals(2) || TipoComprobante.Equals(3) || TipoComprobante.Equals(7) || TipoComprobante.Equals(8))
                {
                    if (cabecera.CbteAsociado != null)
                    {
                        List<CbteAsoc> listCbteAsoc = new();

                        CbteAsoc cbtAso = new()
                        {
                            PtoVta = cabecera.CbteAsociado.PtoVenta,
                            Tipo = cabecera.CbteAsociado.TipoComprobante,
                            Nro = cabecera.CbteAsociado.NroComprobante
                        };

                        listCbteAsoc.Add(cbtAso);

                        detalle.CbtesAsoc = listCbteAsoc.ToArray();
                    }
                }

                detalles.Add(detalle);
            }

            Compte.FeDetReq = detalles.ToArray();

            try
            {
                try
                {
                    FECAESolicitarRequestBody requestBody = new(auth, Compte);
                    FECAESolicitarRequest request = new(requestBody);
                    FECAESolicitarResponse respuesta = await afipService.FECAESolicitarAsync(request);

                    switch (respuesta.Body.FECAESolicitarResult.FeCabResp.Resultado)
                    {
                        case "A":
                            foreach (FECAEDetResponse item in respuesta.Body.FECAESolicitarResult.FeDetResp)
                            {
                                await crm.UpdateComprobante(item.CAE, new DateTime(int.Parse(item.CAEFchVto.Substring(0, 4)), int.Parse(item.CAEFchVto.Substring(4, 2)),
                                    int.Parse(item.CAEFchVto.Substring(6, 2))), item.CbteDesde, respuesta.Body.FECAESolicitarResult.FeCabResp.PtoVta,
                                    respuesta.Body.FECAESolicitarResult.FeCabResp.CbteTipo, item.Observaciones, credenciales);
                                resultado.codigo = 200;
                                resultado.resultado = "Comprobante informado";
                            }
                            break;

                        case "R":

                            string errores = string.Concat("| Codigo Error |", " Descripcion |", "\r\n");

                            if (respuesta.Body.FECAESolicitarResult.Errors != null)
                            {
                                foreach (Err item in respuesta.Body.FECAESolicitarResult.Errors)
                                {
                                    errores += string.Concat("| ", item.Code, " | ", item.Msg, " | ", "\r\n");
                                }

                                if (respuesta.Body.FECAESolicitarResult.FeDetResp != null)
                                {
                                    foreach (FECAEDetResponse item in respuesta.Body.FECAESolicitarResult.FeDetResp)
                                    {
                                        await crm.UpdateComprobante(errores, item.CbteDesde.ToString(), respuesta.Body.FECAESolicitarResult.FeCabResp.CbteTipo,
                                            respuesta.Body.FECAESolicitarResult.FeCabResp.PtoVta, credenciales);
                                        resultado.codigo = 400;
                                        resultado.resultado = $"Error comprobante - {errores}";
                                    }
                                }
                                else
                                {
                                    foreach (FECAEDetRequest item in Compte.FeDetReq)
                                    {
                                        await crm.UpdateComprobante(errores, item.CbteDesde.ToString(), respuesta.Body.FECAESolicitarResult.FeCabResp.CbteTipo,
                                            respuesta.Body.FECAESolicitarResult.FeCabResp.PtoVta, credenciales);
                                        resultado.codigo = 400;
                                        resultado.resultado = $"Error comprobante - {errores}";
                                    }
                                }
                            }
                            else
                            {
                                if (respuesta.Body.FECAESolicitarResult.FeDetResp != null)
                                {
                                    foreach (FECAEDetResponse item in respuesta.Body.FECAESolicitarResult.FeDetResp)
                                    {
                                        await crm.UpdateComprobante(item.Observaciones, item.CbteDesde.ToString(), respuesta.Body.FECAESolicitarResult.FeCabResp.PtoVta,
                                            respuesta.Body.FECAESolicitarResult.FeCabResp.CbteTipo, credenciales);
                                        string observaciones = "";
                                        foreach (var itemObs in item.Observaciones)
                                        {
                                            observaciones += string.Concat("| ", itemObs.Code, " | ", itemObs.Msg, " | ", "\r\n");
                                        }
                                        resultado.codigo = 400;
                                        resultado.resultado = $"Error comprobante - {observaciones}";
                                    }
                                }
                            }

                            break;

                        case "P":
                            foreach (FECAEDetResponse ErrDet in respuesta.Body.FECAESolicitarResult.FeDetResp)
                            {
                                switch (ErrDet.Resultado)
                                {
                                    case "R":
                                        await crm.UpdateComprobante(ErrDet.Observaciones, ErrDet.CbteDesde.ToString(), respuesta.Body.FECAESolicitarResult.FeCabResp.PtoVta,
                                            respuesta.Body.FECAESolicitarResult.FeCabResp.CbteTipo, credenciales);
                                        string observaciones = "";
                                        foreach (var itemObs in ErrDet.Observaciones)
                                        {
                                            observaciones += string.Concat("| ", itemObs.Code, " | ", itemObs.Msg, " | ", "\r\n");
                                        }
                                        resultado.codigo = 400;
                                        resultado.resultado = $"Error comprobante - {observaciones}";
                                        break;
                                    case "A":
                                        await crm.UpdateComprobante(ErrDet.CAE, new DateTime(int.Parse(ErrDet.CAEFchVto.Substring(0, 4)), int.Parse(ErrDet.CAEFchVto.Substring(4, 2)),
                                            int.Parse(ErrDet.CAEFchVto.Substring(6, 2))), ErrDet.CbteDesde, respuesta.Body.FECAESolicitarResult.FeCabResp.PtoVta,
                                            respuesta.Body.FECAESolicitarResult.FeCabResp.CbteTipo, ErrDet.Observaciones, credenciales);
                                        resultado.codigo = 200;
                                        resultado.resultado = "Comprobante informado";
                                        break;
                                    default:
                                        break;
                                }
                            }

                            break;
                    }
                }
                catch (Exception e)
                {
                    await crm.UpdateComprobantes(e.Message, comprobantes, credenciales);
                    return resultado;
                }
                return resultado;
            }
            catch (Exception ex)
            {
                //System.Console.Write(ex.Message);
                //System.Console.ReadLine();
                throw;
            }
            finally
            {
                FECompUltimoAutorizadoRequestBody requestBody = new(auth, PuntoVenta, TipoComprobante);
                FECompUltimoAutorizadoRequest request = new(requestBody);
                FECompUltimoAutorizadoResponse ultimo = await afipService.FECompUltimoAutorizadoAsync(request);
                crm.UpdateTokenYSign(Token, Sign, comprobantes[0].IdParametroWSAfip.ToString(), ultimo.Body.FECompUltimoAutorizadoResult.CbteNro, TipoComprobante, credenciales);
            }
        }
    }
}
