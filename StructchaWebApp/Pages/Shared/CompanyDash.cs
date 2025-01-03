﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using StructchaWebApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Options;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Hosting;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        public StructchaClaim[] roleClaims = {
            new StructchaClaim("PCWP","Post company wide posts"),
            new StructchaClaim("PPWP","Post project wide posts"),
            new StructchaClaim("RCWP","Reply to company posts"),
            new StructchaClaim("RPP","Reply to project posts"),
            new StructchaClaim("CrPro","Create Project"),
            new StructchaClaim("AUR2P","Assign users and roles to project"),
            new StructchaClaim("CrPT","Create Project Task"),
            new StructchaClaim("CoPT","Complete project Task"),
            new StructchaClaim("ChPT","Check project Task")
        };

        public CompanyDash(RoleManager<IdentityRole> rm, UserManager<ApplicationUser> um, ApplicationUser applicationUser)
        {
            roleManager = rm;
            userManager = um;
            unRegUsers = um.Users.Where(u=>u.Company == "").ToListAsync().Result;
            user = applicationUser;
            usersRoles = new List<string>();
            checkingUserName = "";
        }

        //Gets all roles that can be assigned by the company director/manager etc.
        public List<IdentityRole> AllUserRoles()
        {
            var roles = roleManager.Roles.OrderBy(x => x.Name).ToList();
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

        //Changes the accepted user's company value from "" to the company name, allowing them access to that companies items within their assigned roles.
        public void acceptUserToCompany(string uId)
        {
            ApplicationUser userToAccept = userManager.FindByEmailAsync(uId).Result;
            userToAccept.Company = user.Company;
            IdentityResult deleteTask = userManager.UpdateAsync(userToAccept).Result;
        }

        //Deletes entire user from database
        public void deleteUserFromCompany(string uId)
        {
            ApplicationUser userToDelete = userManager.FindByEmailAsync(uId).Result;
            IdentityResult deleteTask = userManager.DeleteAsync(userToDelete).Result;
        }

        //Returns all the roles of the specific user
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

        //Finds and returns all users which have the same email domain as the admin which have a value of "" for company
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

        //Gets all users from database with matching company
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

            return users.OrderBy(x => x.Email).ToList();
        }

        public IEnumerable<SelectListItem> CompanyUsersSL()
        {
            List<SelectListItem> users = new List<SelectListItem>();
            foreach (ApplicationUser compUser in CompanyUsers())
            {

                users.Add(new SelectListItem
                {
                    Text = compUser.Email,
                    Value = compUser.Id                    
                });
            }

            return users;
        }

        public IEnumerable<SelectListItem> AllUserRolesSL()
        {
            List<SelectListItem> roles = new List<SelectListItem>();
            foreach (IdentityRole role in AllUserRoles())
            {

                roles.Add(new SelectListItem
                {
                    Text = role.Name,
                    Value = role.Name
                });
            }

            return roles;
        }

        //assigns selected role to the selected user
        public void assignRole(string userID, string role)
        {
            var user = userManager.FindByIdAsync(userID).Result;
            var assignRole = userManager.AddToRoleAsync(user, role).Result;
        }
        //deletes the selected role from the selected user
        public void unAssignRole(string userID, string roleName)
        {
            var user = userManager.FindByIdAsync(userID).Result;
            userManager.RemoveFromRoleAsync(user, roleName).Wait();
        }

        public IEnumerable<SelectListItem> RoleAppAccessSL(string software)
        {
            List<SelectListItem> roles = new List<SelectListItem>();
            foreach (IdentityRole role in AllUserRoles())
            {
                IList<Claim> claims = roleManager.GetClaimsAsync(role).Result;
                Claim chk = new Claim(software, user.Company);
                foreach(Claim claim in claims)
                {
                    if (claim.Type == software && claim.Value == user.Company)
                    {
                        roles.Add(new SelectListItem
                        {
                            Text = role.Name,
                            Value = role.Name
                        });
                    }
                }
            }
            return roles;
        }

        public void addAppRole(string app, string role)
        {
            if (role != "")
            {
                IdentityResult result = roleManager.AddClaimAsync(roleManager.FindByNameAsync(role).Result, new Claim(app, user.Company)).Result;
            }
        }

        public void rmvAppRole(string app, string role)
        {
            if(role != "")
            {
                IdentityResult result = roleManager.RemoveClaimAsync(roleManager.FindByNameAsync(role).Result, new Claim(app, user.Company)).Result;
            }           

        }
    }
}
