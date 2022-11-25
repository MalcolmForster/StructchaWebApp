using Microsoft.AspNetCore.Identity;

namespace StructchaWebApp.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
    }
}

