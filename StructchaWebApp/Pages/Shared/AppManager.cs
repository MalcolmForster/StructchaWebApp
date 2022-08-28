using Microsoft.Data.SqlClient;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Identity;

namespace StructchaWebApp.Pages.Shared
{
    public class AppManager
    {
        public string session { get; set; }
        public string user { get; set; }

        public string software { get; set; }

        //private string connectString = "Server=192.168.86.24,1433;Database=Structcha;user=StructchaGUI;password=StructchaGUI12#;";
        //private SqlConnection conn;
        public AppManager(string UserID, string software) //what would happen if they are working for mulitple companies? i.e. a contractor. Maybe they can have multiple logins, one for each company they are part of
        {
            session = sessionMaker();
            user = UserID;
            this.software = software;
            insertIntoDB();            
        }
        private string sessionMaker()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[128];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }

        private void insertIntoDB()
        {
            string company = _Common.userCompany(user); //Retrived users company

            SqlConnection conn = _Common.connDB();
            string query = "UPDATE TOP (1) [dbo].[ActiveSoftware] SET CurrentUserId = @userid, SessionKey = @sessionKey, StartTime = SYSDATETIME() WHERE CurrentUserId IS NULL AND Company = @usercompany AND Software = @software"; 
            SqlCommand cmd = new SqlCommand(query, conn);


            cmd.Parameters.AddWithValue("@userid", user);
            cmd.Parameters.AddWithValue("@sessionKey", session);
            cmd.Parameters.AddWithValue("@usercompany", company);
            cmd.Parameters.AddWithValue("@software", software);
            cmd.ExecuteNonQuery();

            query = "INSERT INTO [dbo].[ActiveSoftwareBackup] (Company,Software,CurrentUserId,SessionKey,StartTime) VALUES (@usercompany, @software, @userid, @sessionKey, SYSDATETIME())";
            cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userid", user);
            cmd.Parameters.AddWithValue("@sessionKey", session);
            cmd.Parameters.AddWithValue("@usercompany", company);
            cmd.Parameters.AddWithValue("@software", software);
            cmd.ExecuteNonQuery();

            conn.Close();
        }
    }
}
