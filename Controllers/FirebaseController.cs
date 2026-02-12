using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using static Api.Web.Dynamics365.Models.PortalCASFOG;

namespace Api.Web.Dynamics365.Controllers
{
    [ApiController]
    public class FirebaseController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext context;

        public FirebaseController(IConfiguration _configuration,
           UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            configuration = _configuration;
            this.userManager = userManager;
            this.context = context;
        }

        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[HttpPut]
        //[Route("api/portalcasfog/garantia")]
        //public async Task<IActionResult> ActualizaGarantia([FromBody] GarantiaCasfog garantia)
        //{
        //    try
        //    {
        //        #region Credenciales
        //        var clienteClaim = HttpContext.User.Claims.Where(claim => claim.Type == "cliente").FirstOrDefault();
        //        if (clienteClaim == null)
        //        {
        //            return BadRequest("El usuario no contiene un cliente asociado para operar.");
        //        }
        //        var cliente_db = clienteClaim.Value;
        //        Credenciales credenciales = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == cliente_db);
        //        if (credenciales == null)
        //        {
        //            return BadRequest("No existen credenciales para ese cliente.");
        //        }
        //        #endregion
        //        ApiDynamics apiDynamics = new ApiDynamics();

        //        JObject Garantia = new();
        //        if (garantia.new_socioparticipe != null && garantia.new_socioparticipe != string.Empty)
        //            Garantia.Add("new_SocioParticipe@odata.bind", "/accounts(" + garantia.new_socioparticipe + ")");

        //        if (garantia.new_tipodeoperacion > 0)
        //            Garantia.Add("new_tipodeoperacion", garantia.new_tipodeoperacion);

        //        if (garantia.new_fechadeorigen != null && garantia.new_fechadeorigen != string.Empty)
        //            Garantia.Add("new_fechadeorigen", DateTime.Parse(garantia.new_fechadeorigen).ToString("yyyy-MM-dd"));

        //        if (garantia.new_acreedor != null && garantia.new_acreedor != string.Empty)
        //            Garantia.Add("new_Acreedor@odata.bind", "/new_acreedors(" + garantia.new_acreedor + ")");

        //        if (garantia.new_nmerodeserie != null && garantia.new_nmerodeserie != string.Empty)
        //            Garantia.Add("new_NmerodeSerie@odata.bind", "/new_seriedeoperacinsindicadas(" + garantia.new_nmerodeserie + ")");

        //        if (garantia.statuscode > 0)
        //            Garantia.Add("statuscode", garantia.statuscode);

        //        if (garantia.new_referido != null && garantia.new_referido != string.Empty)
        //            Garantia.Add("new_Referido@odata.bind", "/accounts(" + garantia.new_referido + ")");

        //        if (garantia.new_fechaemisindelcheque != null && garantia.new_fechaemisindelcheque != string.Empty)
        //            Garantia.Add("new_fechaemisindelcheque", DateTime.Parse(garantia.new_fechaemisindelcheque).ToString("yyyy-MM-dd"));

        //        if (garantia.new_numerodeprestamo > 0)
        //            Garantia.Add("new_numerodeprestamo", garantia.new_numerodeprestamo);

        //        if (garantia.new_oficialdecuentas != null && garantia.new_oficialdecuentas != string.Empty)
        //            Garantia.Add("new_Oficialdecuentas@odata.bind", "/contacts(" + garantia.new_oficialdecuentas + ")");

        //        if (garantia.new_fechadenegociacion != null && garantia.new_fechadenegociacion != string.Empty)
        //            Garantia.Add("new_fechadenegociacion", DateTime.Parse(garantia.new_fechadenegociacion).ToString("yyyy-MM-dd"));

        //        if (garantia.new_sistemadeamortizacion > 0)
        //            Garantia.Add("new_sistemadeamortizacion", garantia.new_sistemadeamortizacion);

        //        if (garantia.new_tasa > 0)
        //            Garantia.Add("new_tasa", garantia.new_tasa);

        //        if (garantia.new_puntosporcentuales > 0)
        //            Garantia.Add("new_puntosporcentuales", garantia.new_puntosporcentuales);

        //        if (garantia.new_periodicidadpagos > 0)
        //            Garantia.Add("new_periodicidadpagos", garantia.new_periodicidadpagos);

        //        if (garantia.new_dictamendelaval > 0)
        //            Garantia.Add("new_dictamendelaval", garantia.new_dictamendelaval);

        //        if (garantia.new_nroexpedientetad != null && garantia.new_nroexpedientetad != string.Empty)
        //            Garantia.Add("new_nroexpedientetad", garantia.new_nroexpedientetad);

        //        if (garantia.new_creditoaprobado != null && garantia.new_creditoaprobado != string.Empty)
        //            Garantia.Add("new_creditoaprobado", garantia.new_creditoaprobado);

        //        if (garantia.new_codigo != null && garantia.new_codigo != string.Empty)
        //            Garantia.Add("new_codigo", garantia.new_codigo);

        //        if (garantia.new_tipodegarantias > 0)
        //            Garantia.Add("new_tipodegarantias", garantia.new_tipodegarantias);

        //        if (garantia.new_nroexpedientetad != null && garantia.new_nroexpedientetad != string.Empty)
        //            Garantia.Add("new_nroexpedientetad", garantia.new_nroexpedientetad);

        //        if (garantia.new_plazodias > 0)
        //            Garantia.Add("new_plazodias", garantia.new_plazodias);

        //        if (garantia.new_fechadevencimiento != null && garantia.new_fechadevencimiento != string.Empty)
        //            Garantia.Add("new_fechadevencimiento", DateTime.Parse(garantia.new_fechadevencimiento).ToString("yyyy-MM-dd"));

        //        if (garantia.new_montocomprometidodelaval > 0)
        //            Garantia.Add("new_montocomprometidodelaval", garantia.new_montocomprometidodelaval);

        //        if (garantia.new_determinadaenasamblea != null && garantia.new_determinadaenasamblea != string.Empty)
        //            Garantia.Add("new_determinadaenasamblea", garantia.new_determinadaenasamblea);

        //        if (garantia.new_monto > 0)
        //            Garantia.Add("new_monto", garantia.new_monto);

        //        string resultadoActualizacion = apiDynamics.UpdateRecord("new_garantias", garantia.new_garantiaid, Garantia, credenciales);

        //        return Ok(resultadoActualizacion);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}
