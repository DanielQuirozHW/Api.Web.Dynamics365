using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using DinkToPdf;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
//using OpenXmlPowerTools;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Drawing.Imaging;
using System.Text;
using System.Xml.Linq;
using static Api.Web.Dynamics365.Models.ConvertirDocumento;
using ColorMode = DinkToPdf.ColorMode;
//using ColorMode = 
namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class ConvertirdocumentoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ConvertirdocumentoController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("api/convertirdocumento/htmltopdf")]
        public async Task<IActionResult> HtmlToPdf([FromBody] HtmlToPdf htmlToPdf)
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

            ApiDynamics api = new ApiDynamics();

            try
            {
                //var converter = new SynchronizedConverter(new PdfTools());

                //var htmlBytes = System.Convert.FromBase64String(htmlToPdf.htmlB64);
                //string html = string.Empty;

                //html = ParseDOCX(htmlBytes, "ACUERDO MARCO - ALIANZA SGR.docx");

                ////string html = Encoding.UTF8.GetString(htmlBytes);

                //var doc = new HtmlToPdfDocument()
                //{
                //    GlobalSettings =
                //    {
                //        ColorMode = ColorMode.Color,
                //        Orientation = Orientation.Portrait,
                //        PaperSize = PaperKind.A4,
                //        Margins = new MarginSettings() { Top = 10, Right = 10 , Bottom = 10, Left = 10 },
                //        DPI = 500,
                //    },
                //    Objects =
                //    {
                //        new ObjectSettings()
                //        {
                //            HtmlContent = html,
                //            WebSettings = { DefaultEncoding = "utf-8", MinimumFontSize = 12 },
                //            HeaderSettings = { FontSize = 9, HtmUrl =  "" }
                //            //HeaderSettings = {FontSize =  9, Right = "Page [page] of [toPage]", Line = false},
                //        }
                //     }
                //};

                //byte[] pdf = converter.Convert(doc);
                ////byte[] pdf = converter.Convert(pdfContent);
                //string b64 = Convert.ToBase64String(pdf);

                //JObject annotation = new JObject();
                //annotation.Add("subject", "nombre");
                //annotation.Add("isdocument", true);
                //annotation.Add("mimetype", "application/pdf");
                //annotation.Add("documentbody", b64);
                //annotation.Add("filename", "nombre" + ".pdf");

                //string nota_id = api.CreateRecord("annotations", annotation, credenciales);

                return Ok("asd");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public static Uri FixUri(string brokenUri)
        {
            string newURI = string.Empty;
            if (brokenUri.Contains("mailto:"))
            {
                int mailToCount = "mailto:".Length;
                brokenUri = brokenUri.Remove(0, mailToCount);
                newURI = brokenUri;
            }
            else
            {
                newURI = " ";
            }
            return new Uri(newURI);
        }

        //public static string ParseDOCX(byte[] byteArray, string nombreArchivo)
        //{
        //    try
        //    {
        //        //byte[] byteArray = File.OpenRead(fileInfo.FullName));
        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            memoryStream.Write(byteArray, 0, byteArray.Length);
        //            using (WordprocessingDocument wDoc =
        //                                        WordprocessingDocument.Open(memoryStream, true))
        //            {
        //                int imageCounter = 0;
        //                var pageTitle = nombreArchivo;
        //                var part = wDoc.CoreFilePropertiesPart;
        //                if (part != null)
        //                    pageTitle = (string)part.GetXDocument()
        //                                            .Descendants(DC.title)
        //                                            .FirstOrDefault() ?? nombreArchivo;

        //                WmlToHtmlConverterSettings settings = new WmlToHtmlConverterSettings()
        //                {
        //                    AdditionalCss = "body { margin: 1cm auto; max-width: 20cm; padding: 0; }",
        //                    PageTitle = pageTitle,
        //                    FabricateCssClasses = true,
        //                    CssClassPrefix = "pt-",
        //                    RestrictToSupportedLanguages = false,
        //                    RestrictToSupportedNumberingFormats = false,
        //                    ImageHandler = imageInfo =>
        //                    {
        //                        ++imageCounter;
        //                        string extension = imageInfo.ContentType.Split('/')[1].ToLower();
        //                        ImageFormat imageFormat = null;
        //                        if (extension == "png") imageFormat = ImageFormat.Png;
        //                        else if (extension == "gif") imageFormat = ImageFormat.Gif;
        //                        else if (extension == "bmp") imageFormat = ImageFormat.Bmp;
        //                        else if (extension == "jpeg") imageFormat = ImageFormat.Jpeg;
        //                        else if (extension == "tiff")
        //                        {
        //                            extension = "gif";
        //                            imageFormat = ImageFormat.Gif;
        //                        }
        //                        else if (extension == "x-wmf")
        //                        {
        //                            extension = "wmf";
        //                            imageFormat = ImageFormat.Wmf;
        //                        }

        //                        if (imageFormat == null) return null;

        //                        string base64 = null;
        //                        try
        //                        {
        //                            using (MemoryStream ms = new MemoryStream())
        //                            {
        //                                imageInfo.Bitmap.Save(ms, imageFormat);
        //                                var ba = ms.ToArray();
        //                                base64 = System.Convert.ToBase64String(ba);
        //                            }
        //                        }
        //                        catch (System.Runtime.InteropServices.ExternalException)
        //                        { return null; }

        //                        ImageFormat format = imageInfo.Bitmap.RawFormat;
        //                        ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders()
        //                                                    .First(c => c.FormatID == format.Guid);
        //                        string mimeType = codec.MimeType;

        //                        string imageSource =
        //                                string.Format("data:{0};base64,{1}", mimeType, base64);

        //                        XElement img = new XElement(Xhtml.img,
        //                                new XAttribute(NoNamespace.src, imageSource),
        //                                imageInfo.ImgStyleAttribute,
        //                                imageInfo.AltText != null ?
        //                                    new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
        //                        return img;
        //                    }
        //                };

        //                XElement htmlElement = WmlToHtmlConverter.ConvertToHtml(wDoc, settings);
        //                var html = new XDocument(new XDocumentType("html", null, null, null),
        //                                                                            htmlElement);
        //                var htmlString = html.ToString(SaveOptions.DisableFormatting);
        //                return htmlString;
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return "The file is either open, please close it or contains corrupt data";
        //    }
        //}
    }
}
