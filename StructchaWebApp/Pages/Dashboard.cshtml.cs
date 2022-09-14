using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Xml.Linq;

namespace StructchaWebApp.Pages
{
    public class DashboardModel : PageModel
    {
        public AdminDash adminDash { get; set; }
        public CompanyDash companyDash { get; set; }
        public ProjectAdmin projectAdmin { get; set; }
        public string checkingUser { get; set; }

        public DashboardModel(RoleManager<IdentityRole> rm, UserManager<ApplicationUser> um, IHttpContextAccessor httpContextAccessor)
        {
            projectAdmin = new ProjectAdmin((um.FindByNameAsync(httpContextAccessor.HttpContext?.User.Identity?.Name).Result).Company);
            adminDash = new AdminDash(rm);
            companyDash = new CompanyDash(rm, um, um.FindByNameAsync(httpContextAccessor.HttpContext?.User.Identity?.Name).Result);
            checkingUser = "";
        }

        public void OnGet()
        {
            
        }
        public async Task OnPostNewRoleSubmit()
        {
            string s = Request.Form["roleName"];
            //adminDash.addRole(s);
            await adminDash.addRole(s);
        }

        public void OnPostNewCompanySubmit()
        {
            //check if company already exists - if doesn't create and activate, if does just activate
            string company = Request.Form["companyName"];
            string adminUser = Request.Form["companyAdmin"];
            adminDash.addCompany(company, adminUser);
        }

        public async Task OnPostRoleRemove()
        {
            string s = Request.Form["deleteRoleButton"];
            await adminDash.deleteRole(s);
        }

        public void OnPostCompanyRemove()
        {
            string s = Request.Form["deleteCompanyButton"];
            adminDash.deleteCompany(s);
        }

        public void OnPostFindUser()
        {
            checkingUser = Request.Form["userSelect"];
            if(checkingUser != null)
            {
                companyDash.selectedUserRoles(checkingUser);
            }            
        }

        public void OnPostUserRoleAdd()
        {
            var userBeingAltered = Request.Form["userBeingAltered"];
            var newUserRole = Request.Form["userNewRole"];
            companyDash.assignRole(userBeingAltered, newUserRole);
        }

        public void OnPostUserRoleRemove()
        {
            var userBeingAltered = Request.Form["userBeingAltered"];
            string roleToDel = Request.Form["deleteUserRole"];

            string _remove = "_remove";
            int len = roleToDel.Length - _remove.Length;
            roleToDel = roleToDel.Substring(0, len);
            roleToDel = roleToDel.Replace("_", " ");
            companyDash.unAssignRole(userBeingAltered, roleToDel);
        }

        public void OnPostUnassignedCompanyUsers()
        {
            string accepted = Request.Form["acceptUser"].ToString();
            string deleted = Request.Form["deleteUser"].ToString();
            if (accepted != "")
            {
                companyDash.acceptUserToCompany(accepted);
            } else if (Request.Form["deleteUser"].ToString() != "")
            {
                companyDash.deleteUserFromCompany(deleted);
            }
        }

        public void OnPostCreateNewProject()
        {
            projectAdmin.createProject(Request.Form["newProjectName"], Request.Form["newProjectLocation"], DateOnly.Parse(Request.Form["newProjectStartDate"]));
        }

        public void OnPostEditCurrentProject()
        {

        }

        public void OnPostEditCompletedProject()
        {

        }

        public void OnPostEditContractors()
        {

        }

        private static bool adminCheck(string query, string userID)
        {
            SqlConnection conn = _Common.connDB();
            var comm = new SqlCommand(query, conn);
            comm.Parameters.AddWithValue("@userID", userID);

            if (comm.ExecuteScalar() != null)
            {
                conn.Close();
                return true;
            }
            else
            {
                conn.Close();
                return false;
            }
        }

        public static bool superAdmin(string userID)
        {            
            string query = "SELECT Activated FROM [dbo].[CompanyRegister] WHERE [AdminUserID] = @userID AND [Company] = 'Structcha'";
            return adminCheck(query, userID);
        }

        public static bool companyOwner(string userID)
        {
            string query = "SELECT Activated FROM [dbo].[CompanyRegister] WHERE [AdminUserID] = @userID";
            return adminCheck(query, userID);
        }
    }
}
