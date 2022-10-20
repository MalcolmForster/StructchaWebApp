using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        public ApplicationUser user { get; set; }
        public List<Project>? currentProjects { get; set; }
        public List<Project>? finishedProjects { get; set; }

        public DashboardModel(RoleManager<IdentityRole> rm, UserManager<ApplicationUser> um, IHttpContextAccessor httpContextAccessor)
        {
            //Need to add methods to only set the dashBoard sections which are accessable by the users roles
            user = um.FindByNameAsync(httpContextAccessor.HttpContext?.User.Identity?.Name).Result;
            projectAdmin = new ProjectAdmin(user.Company, rm,um);
            adminDash = new AdminDash(rm);
            companyDash = new CompanyDash(rm, um, user);
            checkingUser = "";
        }

        public void OnGet()
        {
            
        }
        public async Task OnPostNewRoleSubmit()     //This is the oringal method before bugging out? Same with RoleRemove()
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

        public ActionResult OnPostFindUser(string user)
        {
            checkingUser = user;
            if (checkingUser != null)
            {
                companyDash.selectedUserRoles(checkingUser);
            }
            return PartialView("~/Pages/Shared/_CompanyAdminSettings.cshtml");
        }

        private PartialViewResult PartialView(string partialName)
        {
            PartialViewResult result = new PartialViewResult();
            result.ViewName = partialName;
            result.ViewData = new ViewDataDictionary<DashboardModel>(ViewData, this);
            
            return result;
        }

        public ActionResult OnPostUserRoleAdd(string userBeingAltered, string newUserRole)
        {
            //var userBeingAltered = Request.Form["userBeingAltered"];
            //var newUserRole = Request.Form["userNewRole"];
            companyDash.assignRole(userBeingAltered, newUserRole);
            return OnPostFindUser(userBeingAltered);
        }

        public ActionResult OnPostUserRoleRemove(string userBeingAltered, string newUserRole)
        {
            //var userBeingAltered = Request.Form["userBeingAltered"];
            //string roleToDel = Request.Form["deleteUserRole"];

            string _remove = "_remove";
            int len = newUserRole.Length - _remove.Length;
            newUserRole = newUserRole.Substring(0, len);
            newUserRole = newUserRole.Replace("_", " ");
            companyDash.unAssignRole(userBeingAltered, newUserRole);
            return OnPostFindUser(userBeingAltered);
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

        public void OnPostAlterAppAccess()
        {
            string command = Request.Form["submit"];
            int l = command.Length - 8;
            string app = command.Substring(0,l);
            string role = Request.Form[command];            

            if (command.Contains("_AddRole"))
            {
                companyDash.addAppRole(app, role);
            }
            else if (command.Contains("_RmvRole"))
            {
                companyDash.rmvAppRole(app, role);
            }
        }


        public void OnPostCreateNewProject()
        {
            projectAdmin.createProject(Request.Form["newProjectName"], Request.Form["newProjectLocation"], DateOnly.Parse(Request.Form["newProjectStartDate"]));
        }

        public void setProjects()
        {
            currentProjects = projectAdmin.getProjects(0);
            finishedProjects = projectAdmin.getProjects(1);
        }

        public ActionResult OnPostEditCurrentProject(string pressedButton)
        {
            var request = pressedButton.ToString().Split('_');
            string operation = request[0];
            string projectCode = request[1];
            var parameters = Request.Form[operation].ToString();
            projectAdmin.editProject(projectCode, user.Company, operation, parameters);
            currentProjects = projectAdmin.getProjects(0);
            finishedProjects = projectAdmin.getProjects(1);

            return PartialView("~/Pages/Shared/_ProjectAdmin.cshtml");
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

        //public void OnPostAddAppRole()
        //{

        //}

        //public void OnPostRemoveAppRole()
        //{

        //}

        public void OnPostRemoveAppRole(string app, string role)
        {
            if(role!= "")
            {
                companyDash.addAppRole(app, role);
            }
            
            
            //PartialViewResult result = new PartialViewResult()
            //{
            //    ViewName = "_CompanyAdminSettings",
            //    ViewData = new ViewDataDictionary<DashboardModel>(ViewData, this)
            //};
            //return result;
        }

        public void OnPostAddAppRole(string app, string role)
        {
            if (role != "")
            {
                companyDash.rmvAppRole(app, role);
            }
            //PartialViewResult result = new PartialViewResult()
            //{
            //    ViewName = "_CompanyAdminSettings",
            //    ViewData = new ViewDataDictionary<DashboardModel>(ViewData, this)
            //};
            //return result;
        }
    }
}
