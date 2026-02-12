using Microsoft.AspNetCore.Identity;

namespace Api.Web.Dynamics365.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Cliente { get; set; }
    }
}
