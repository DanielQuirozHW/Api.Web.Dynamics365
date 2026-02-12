using Api.Web.Dynamics365.Clases;
using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Api.Web.Dynamics365.Models.Credenciales;

namespace Api.Web.Dynamics365.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CredencialesentornosController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public CredencialesentornosController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize(Policy = "EsAdmin")]
        [HttpPost]
        public async Task<IActionResult> CrearCredenciales([FromBody] Credenciales credencial)
        {
            var existeCredencial = await context.Credenciales.AnyAsync(x => x.cliente == credencial.cliente);

            if (existeCredencial)
            {
                return BadRequest($"Ya existe credenciales para el cliente {credencial.cliente}");
            }

            context.Credenciales.Add(credencial);
            await context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Policy = "EsAdmin")]
        [HttpPut]
        public async Task<IActionResult> EditarCredenciales(ActualizarCredenciales credencial)
        {
            var credencialDB = await context.Credenciales.FirstOrDefaultAsync(x => x.cliente == credencial.cliente);

            if (credencialDB == null)
            {
                return NotFound();
            }

            credencialDB.clientsecret  = credencial.clientsecret;
            credencialDB.clientid = credencial.clientid;
            credencialDB.url = credencial.url;
            credencialDB.tenantid = credencial.tenantid;

            context.Update(credencialDB);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
