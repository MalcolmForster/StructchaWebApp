using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using StructchaWebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace StructchaWebApp.Pages.Shared
{
    public class CompanyDash
    {
        private RoleManager<IdentityRole> roleManager { get; set; }
        private UserManager<ApplicationUser> userManager { get; set; }
        private ApplicationUser user { get; set; }

        public string checkingUserName { get; set; }

        public IList<string>? usersRoles { get; set; }

        public List<ApplicationUser> unRegUsers { get; set; }

        public CompanyDash(RoleManager<IdentityRole> rm, UserManager<ApplicationUser> um, ApplicationUser applicationUser)
        {
            roleManager = rm;
            userManager = um;
            unRegUsers = um.Users.Where(u=>u.Company == "").ToListAsync().Result;
            user = applicationUser;
            usersRoles = new List<string>();
            checkingUserName = "";
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

        public void acceptUserToCompany(string uId)
        {
            ApplicationUser userToAccept = userManager.FindByIdAsync(uId).Result;
            userToAccept.Company = user.Company;
            IdentityResult deleteTask = userManager.UpdateAsync(userToAccept).Result;
        }

        public void deleteUserFromCompany(string uId)
        {
            ApplicationUser userToDelete = userManager.FindByIdAsync(uId).Result;
            IdentityResult deleteTask = userManager.DeleteAsync(userToDelete).Result;
        }

        public void selectedUserRoles(string user)
        {
            var appUser = userManager.FindByIdAsync(user).Result;
            var roles = userManager.GetRolesAsync(appUser).Result;

            checkingUserName = appUser.UserName;
            //foreach (string role in roles)
            //{
            //    if (role == "admin")
            //    {
            //        roles.Remove(role);
            //        break;
            //    }
            //}
            usersRoles = roles;
        }

        public List<ApplicationUser> UnassignedCompanyUsers()
        {
            List<ApplicationUser> users = new List<ApplicationUser>();
            string compWeb = user.Email.Split('@')[1];
            
            foreach (ApplicationUser aUser in unRegUsers)
            {
                string aUserEmail = aUser.Email.Split('@')[1];
                
                if (aUserEmail == compWeb)
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

        public void assignRole(string userID, string roleID)
        {
            var user = userManager.FindByIdAsync(userID).Result;
            var role = roleManager.FindByIdAsync(roleID).Result;
            userManager.AddToRoleAsync(user, role.Name).Wait();
        }

        public void unAssignRole(string userID, string roleName)
        {
            var user = userManager.FindByIdAsync(userID).Result;
            userManager.RemoveFromRoleAsync(user, roleName).Wait();
        }
    }
}
