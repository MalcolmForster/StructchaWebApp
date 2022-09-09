using Microsoft.AspNetCore.Identity;

namespace StructchaWebApp.Pages.Shared
{
    public class AdminDash
    {
        private RoleManager<IdentityRole> roleManager { get; set; }
        public AdminDash(RoleManager<IdentityRole> rm)
        {
            roleManager = rm;
        }

        public List<IdentityRole> AllUserRoles()
        {
            var roles = roleManager.Roles.ToList();
            return roles;
        }


        public async Task addRole(string role)
        {
            List<string> allRoles = new List<string>();                
            AllUserRoles().ForEach(s => allRoles.Add(s.Name));

            bool dup = false;
            foreach(string roleName in allRoles)
            {
                if(roleName == role) { dup = true; break; }
            }

            if (!dup)
            {

                IdentityRole identityRole = new IdentityRole();
                identityRole.Name = role;

                await roleManager.CreateAsync(identityRole);

                Console.WriteLine(role + "created");
            }

            //method to report if the role was added or not (as it was a duplicate)
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
