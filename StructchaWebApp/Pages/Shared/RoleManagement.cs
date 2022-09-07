using Microsoft.AspNetCore.Identity;

namespace StructchaWebApp.Pages.Shared
{
    public class RoleManagement
    {

        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManagement(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public List<IdentityRole>AllUserRoles()
        {
            return _roleManager.Roles.ToList();
        }

        public void addRole()
        {

        }

        public void removeRole()
        {

        }
    }
}
