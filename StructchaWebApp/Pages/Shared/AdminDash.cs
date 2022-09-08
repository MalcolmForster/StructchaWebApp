using Microsoft.AspNetCore.Identity;

namespace StructchaWebApp.Pages.Shared
{
    public class AdminDash
    {

        private RoleManager<IdentityRole> roleManager { get; }
        public AdminDash(RoleManager<IdentityRole> rm)
        {
            roleManager = rm;
        }

        public List<IdentityRole> AllUserRoles()
        {
            var roles = roleManager.Roles.ToList();
            return roles;
        }

        public void addRole()
        {
            
        }

        public void deleteRole()
        {

        }

        public void assignRole()
        {

        }

        public void unassignRole()
        {

        }
    }
}
