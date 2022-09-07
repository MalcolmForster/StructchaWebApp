using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Pages.Shared;
using System.Runtime.CompilerServices;

namespace StructchaWebApp.Pages
{
    public class DashboardModel : PageModel
    {               
        public void OnGet()
        {
        }

        public void OnPostNewRoleSubmit()
        {
            Console.WriteLine("Test");
        }

        public static bool superAdmin(string userID)
        {
            SqlConnection conn = _Common.connDB();
            string query = "SELECT Activated FROM [dbo].[CompanyRegister] WHERE [AdminUserID] = @userID";
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
