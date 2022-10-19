using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using StructchaWebApp.Areas.Identity.Pages.Account;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Xml.Linq;
using System.Web;
using System.Data.SqlTypes;
using System.Runtime.Intrinsics.Arm;

namespace StructchaWebApp.Pages
{
    public class LinkCentreNode : PageModel
    {
        public ApplicationUser? user { get; set; }
        //public LoginModel loginModel { get; set; }
        public string machineName { get; set; }
        public bool validHashParameter { get; set; }
        public string osVersion { get; set; }
        public string osUser { get; set; }
        public List<CentreNodeSessions> currentLinks { get; set; }
        public DateTime timeAdded { get; set; }
        public string hash { get; set; }
        public LinkCentreNode(UserManager<ApplicationUser> um, IHttpContextAccessor httpContextAccessor, SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            hash = httpContextAccessor.HttpContext.Request.Query["Id"];
            //Need to add methods to only set the dashBoard sections which are accessable by the users roles
            string? userName = httpContextAccessor.HttpContext?.User.Identity?.Name;
            if(userName != null)
            {
                user = um.FindByNameAsync(userName).Result;

                validHash();
                            
                if (validHashParameter)
                {                    
                    linkInfo();
                }
                validHash();
                getCurrentLinks();
            }
            //else
            //{
            //    loginModel = new LoginModel(signInManager, logger);
            //}
        }



        private void getCurrentLinks()
        {
            currentLinks = new List<CentreNodeSessions>();
            SqlConnection conn = _Common.connDB();
            string query = "SELECT [CNHash],[MachineName], [OSVersion], [WindowsUser], [TimeAdded],[TimeResult] FROM [CenterNode] WHERE [UserId] = @user AND [CNActive] = 1 AND [TimeResult] IS NOT NULL ORDER BY TimeAdded DESC";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user", user.Id);

            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    CentreNodeSessions cns = new CentreNodeSessions();
                    cns.hash = rdr.GetString(0);
                    cns.MachineName = rdr.GetString(1);
                    cns.osVersion= rdr.GetString(2);
                    cns.osUser = rdr.GetString(3);
                    cns.TimeAdded = rdr.GetDateTime(4);
                    cns.TimeResult = rdr.GetDateTime(5);
                    currentLinks.Add(cns);
                }
                rdr.Close();
            }            
            conn.Close();
        }

        public void validHash()
        {
            bool result = false;
            if (hash != null)
            {

                SqlConnection conn = _Common.connDB();
                string query = "SELECT * FROM [CenterNode] WHERE [CNHash] = @hash AND [UserId] IS NULL AND [CNActive] = 0 AND [TimeResult] IS NULL ORDER BY TimeAdded DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@hash", hash);

                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.HasRows)
                {
                    result = true;
                }
                conn.Close();
            }
            validHashParameter = result;
        }

        private void linkInfo()
        {
            SqlConnection conn = _Common.connDB();
            string query = "SELECT [MachineName], [OSVersion], [WindowsUser], [TimeAdded] FROM [CenterNode] WHERE [CNHash] = @hash AND [UserId] IS NULL AND [CNActive] = 0 AND [TimeResult] IS NULL ORDER BY TimeAdded DESC";
            SqlCommand cmd = new SqlCommand(query,conn);
            cmd.Parameters.AddWithValue("@hash",hash);

            SqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.HasRows)
            {
                int i = 0;
                while (rdr.Read() && i == 0)
                {
                    machineName = rdr.GetString(0);
                    osVersion = rdr.GetString(1);
                    osUser = rdr.GetString(2);
                    timeAdded = rdr.GetDateTime(3);
                    i++;
                }
            }
            rdr.Close();
            conn.Close();
        }

        private void setActive(int b) //b is true or false for the CNActive in CenterNode tables
        {
            SqlConnection conn = _Common.connDB();
            string query = "UPDATE [CenterNode] SET [CNActive] = @active, [UserId] = @userID, [TimeResult] = GETDATE() WHERE [CNHash] = @hash AND [UserId] IS NULL AND [CNActive] = 0 AND [TimeResult] IS NULL AND [TimeAdded] = @timeAdded";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@active", b);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@userID", user.Id);
            cmd.Parameters.AddWithValue("@timeAdded", new SqlDateTime(timeAdded));

            cmd.ExecuteNonQuery();
            conn.Close();
        }

        private void reloadPage()
        {

        }

        public void OnPost()
        {
            string response = Request.Form["linkCenterNode"];
            if (response == "Accept")
            {
                setActive(1);   
            } else if (response == "Reject")
            {
                setActive(0);
            }

        }

        private void deactivateDevice(string d)
        {
            SqlConnection conn = _Common.connDB();
            string query = "UPDATE [CenterNode] SET [CNActive] = 0 WHERE [CNHash] = @hash AND [UserId] = @userID AND [CNActive] = 1 AND [TimeResult] IS NOT NULL AND [TimeAdded] IS NOT NULL";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@hash", d);
            cmd.Parameters.AddWithValue("@userID", user.Id);

            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void OnPostRemoveLinked()
        {
            string device = Request.Form["RemoveNodeDevice"];
            deactivateDevice(device);
        }

        public class CentreNodeSessions {
            public string? hash { get; set; }
            public string? MachineName { get; set; }
            public string? osVersion { get; set; }
            public string? osUser { get; set; }
            public DateTime? TimeAdded { get; set; }
            public DateTime? TimeResult { get; set; }
        }

        //public async void OnPostLoginViaLink()
        //{
        //    IActionResult actionResult = loginModel.OnPostAsync().Result;
        //}
    }
}
