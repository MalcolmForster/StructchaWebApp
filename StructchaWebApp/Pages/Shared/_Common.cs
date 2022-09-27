using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Data.SqlClient;

namespace StructchaWebApp.Pages.Shared
{
    public class _Common
    {

        public static SqlConnection connDB()
        {
            string connectString = "Server=192.168.86.24,1433;Database=Structcha;user=StructchaGUI;password=StructchaGUI12#;";
            SqlConnection conn = new SqlConnection(connectString);
            conn.Open();

            return conn;
        }

        public static void connDB(SqlConnection sql)
        {
            if (sql.State != System.Data.ConnectionState.Open)
            {
                sql.Open();
            }
        }

        public static string? findUserID(string input)
        {
            string? userID = null;
            if(input.Contains('@'))
            {
                SqlConnection conn = connDB();
                string query = "Select Id FROM [dbo].[AspNetUsers] WHERE [NormalizedEmail] = @email";
                SqlCommand comm = new SqlCommand(query, conn);
                comm.Parameters.AddWithValue("@email", input.ToUpper());
                userID = comm.ExecuteScalar().ToString();

                conn.Close();
            }

            return userID;
        }

        public static string userSoftwareActive(string uId, string software)
        {
            SqlConnection conn = connDB();
            string query = "Select SessionKey FROM [dbo].[ActiveSoftware] WHERE Software = @software AND CurrentUserId = @userID";
            SqlCommand comm = new SqlCommand(query, conn);
            comm.Parameters.AddWithValue("@software", software);
            comm.Parameters.AddWithValue("@userId", uId);

            string softwareKey = comm.ExecuteScalar()?.ToString();
            conn.Close();

            if(softwareKey == null)
            {
                softwareKey = "";
            }
            return softwareKey;

        }

        public static int numberOfSeat(string software)
        {
            SqlConnection conn = connDB();

            string query = "Select COUNT(*) FROM [dbo].[ActiveSoftware] WHERE Software = @software AND CurrentUserId IS NULL";
            SqlCommand comm = new SqlCommand(query, conn);
            comm.Parameters.AddWithValue("@software", software);           

            int count = (int)comm.ExecuteScalar();
            conn.Close();

            return count;
        }

        public static void closeSoftware(string uId, string software)
        {
            SqlConnection conn = connDB();
            string query = "UPDATE [dbo].[ActiveSoftware] SET CurrentUserId = NULL, SessionKey = NULL, StartTime = NULL WHERE Software = @software AND CurrentUserId = @uId";
            // May change this to use the sessionkey instead
            SqlCommand comm = new SqlCommand(query, conn);
            comm.Parameters.AddWithValue("@software", software);
            comm.Parameters.AddWithValue("@uId", uId);
            comm.ExecuteNonQuery();

            query = "UPDATE [dbo].[ActiveSoftwareBackup] SET EndTime = SYSDATETIME() WHERE Software = @software AND CurrentUserId = @uId AND EndTime IS NULL";
            comm = new SqlCommand(query, conn);
            comm.Parameters.AddWithValue("@software", software);
            comm.Parameters.AddWithValue("@uId", uId);
            comm.ExecuteNonQuery();

            conn.Close();            
        }

        public static string userCompany(string uId)
        {
            //string company = "This user was not found";
            SqlConnection conn = connDB();
            string query = "SELECT Company FROM [dbo].[AspNetUsers] WHERE Id = @uId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uId", uId);

            string? company = cmd.ExecuteScalar()?.ToString();
            conn.Close();

            if (company == null)
            {
                company = "User not found";
            }
            return company;
        }
    }
}
