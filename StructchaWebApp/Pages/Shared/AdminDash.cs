using Microsoft.AspNetCore.Identity;
using System.Data;

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
            foreach(IdentityRole role in roles)
            {
                if(role.Name == "admin")
                {
                    roles.Remove(role);
                    break;
                }                
            }            
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

        public async Task deleteRole(string role)
        {
            string _remove = "_remove";
            int len = role.Length - _remove.Length;
            role = role.Substring(0,len);
            IdentityRole identityRole;

            foreach (IdentityRole ir in AllUserRoles())
            {
                if (ir.Name == role)
                {
                    identityRole = ir;
                    await roleManager.DeleteAsync(identityRole);
                    break;
                }
            }            
        }

        public void assignRole()
        {

        }

        public void unassignRole()
        {

        }
    }
}
