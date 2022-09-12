using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using StructchaWebApp.Data;

namespace StructchaWebApp.Pages.Shared
{
    public class CompanyDash
    {
        private RoleManager<IdentityRole> roleManager { get; set; }
        private UserManager<ApplicationUser> userManager { get; set; }
        private ApplicationUser user { get; set; }

        public CompanyDash(RoleManager<IdentityRole> rm, UserManager<ApplicationUser> um, ApplicationUser applicationUser)
        {
            roleManager = rm;
            userManager = um;
            user = applicationUser;
        }

        public List<IdentityRole> AllUserRoles()
        {
            //Perhaps add ability to order alphabetically 
            var roles = roleManager.Roles.ToList();
            foreach (IdentityRole role in roles)
            {
                if (role.Name == "admin")
                {
                    roles.Remove(role);
                    break;
                }
            }
            return roles;
        }

        public List<ApplicationUser> UnassignedCompanyUsers()
        {
            List<ApplicationUser> users = new List<ApplicationUser>();
            string compWeb = user.Email.Split('@')[1];

            foreach (var aUser in userManager.Users)
            {
                string aUserEmail = aUser.Email.Split('@')[1];
                
                if ((aUser.Company == "" || aUser.Company == null) && (aUserEmail == compWeb || user.Company == "Structcha"))
                {
                    users.Add(aUser);
                }
            }
            return users;
        }

        public List<ApplicationUser> CompanyUsers()
        {
            List<ApplicationUser> users = new List<ApplicationUser>();

            foreach (var aUser in userManager.Users)
            {
                if (aUser.Company == user.Company)
                {
                    users.Add(aUser);
                }
            }

            return users;
        }

        public void assignRole()
        {

        }

        public void unassignRole()
        {

        }
    }
}
