using Api.Web.Dynamics365.Models;
using Api.Web.Dynamics365.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Api.Web.Dynamics365.Models.CredencialesUsuario;

namespace Api.Web.Dynamics365.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly HashService hashService;

        public UsuariosController(UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            SignInManager<ApplicationUser> signInManager,
            HashService hashService)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashService = hashService;
        }

        [Authorize(Policy = "EsAdmin")]
        [HttpPost("registrar")]
        public async Task<ActionResult<Autenticacion>> Registrar(CredencialesUsuario credencialesUsuario)
        {
            var usuario = new ApplicationUser { UserName = credencialesUsuario.Email, 
                Email = credencialesUsuario.Email, 
                Cliente = credencialesUsuario.Cliente
            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password);

            if (resultado.Succeeded)
            {
                var usuario_db = await userManager.FindByEmailAsync(credencialesUsuario.Email);
                await userManager.AddClaimAsync(usuario_db, new Claim("cliente", credencialesUsuario.Cliente));
                return await ConstruirToken(credencialesUsuario.Email);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<Autenticacion>> Login(CredencialesLogin credencialesUsuario)
        {
            try
            {
                var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email,
                credencialesUsuario.Password, isPersistent: false, lockoutOnFailure: false);

                if (resultado.Succeeded)
                {
                    return await ConstruirToken(credencialesUsuario.Email);
                }
                else
                {
                    return BadRequest("Login incorrecto");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al autenticar - {ex.Message}");
            }
        }

        [HttpGet("RenovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Autenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").FirstOrDefault();
            var email = emailClaim.Value;

            var credencialesUsuario = new CredencialesUsuario()
            {
                Email = email
            };

            return await ConstruirToken(email);
        }

        private async Task<Autenticacion> ConstruirToken(string correo)
        {
            var usuario = await userManager.FindByEmailAsync(correo);
            var claimsDB = await userManager.GetClaimsAsync(usuario);

            var claims = new List<Claim>
            {
                new Claim("email", correo)
            }; //Esta info viaja dentro del token, puede ser visible para el usuario. Se recomienda no mandar informacion sensible

            claims.AddRange(claimsDB);//Fusiona el claim con el usuario

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"])); //Key del jwt en Appsettings
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddMinutes(90); //Tiempo que dura el token

            var securityToken = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion, signingCredentials: creds); //Genera el token

            return new Autenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            }; //Devuelve un objeto de Autenticacion
        }

        [Authorize(Policy = "EsAdmin")]
        [HttpPost("GenerarAdmin")]
        public async Task<ActionResult> HacerAdmin(Admin admin)
        {
            var usuario = await userManager.FindByEmailAsync(admin.Email);
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "true"));
            return NoContent();
        }

        [HttpPost("RemoverAdmin")]
        [Authorize(Policy = "EsAdmin")]
        public async Task<ActionResult> RemoverAdmin(Admin admin)
        {
            var usuario = await userManager.FindByEmailAsync(admin.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "true"));
            return NoContent();
        }
        //Utilizar SAL en HASH: valor aleatorio que se anexa al texto plano al cual le queremos aplicar la funcion hash
    }
}
