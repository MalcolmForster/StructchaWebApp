using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using StructchaWebApp.Pages.Shared;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace StructchaWebApp.Pages
{
    public class DashboardModel : PageModel
    {
        public AdminDash adminDash { get; set; }

        public DashboardModel(RoleManager<IdentityRole> rm)
        {

            adminDash = new AdminDash(rm);
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

        public static bool superAdmin(string userID)
        {
            SqlConnection conn = _Common.connDB();
            string query = "SELECT Activated FROM [dbo].[CompanyRegister] WHERE [AdminUserID] = @userID AND [Company] = 'Structcha'";
            var comm = new SqlCommand(query, conn);            
            comm.Parameters.AddWithValue("@userID", userID);

            if(comm.ExecuteScalar() != null)
            {
                conn.Close();
                return true;
            } else
            {
                conn.Close();
                return false;
            }
        }
    }
}
