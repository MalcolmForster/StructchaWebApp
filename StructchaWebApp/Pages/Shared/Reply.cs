using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;

namespace StructchaWebApp.Pages.Shared
{
    public class Reply
    {
        public string postedBy { get; set; }
        public string body { get; set; }
        public DateTime timePosted { get; set; }
        private SqlConnection _connection { get; set; }
        private UserManager<ApplicationUser> userManager { get; set; }

        public Reply(string id, string table, UserManager<ApplicationUser> um)
        {
            _connection = _Common.connDB();
            userManager = um;
            string OP = "";
            if(table == "Posts")
            {
                OP = "[IdUserOP]";
            } else if(table == "Tasks")
            {
                OP = "[IdAssigner]";
            }
            string query = String.Format("SELECT {0},[PostBody],[TimeOfPost] FROM [dbo].[{1}] WHERE [Id] = @id",OP, table);
            SqlCommand cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@id", id);

            SetInfo(cmd);

            _connection.Close();
        }

        private void SetInfo(SqlCommand cmd)
        {
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    body = reader.GetString(1);
                    timePosted = reader.GetDateTime(2);
                    postedBy = userManager.FindByIdAsync(reader.GetString(0)).Result.UserName;
                }
                reader.Close();
            }
        }

        private async Task<string> getUserName(string userId)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userId);
            string userName = user.UserName;

            return userName;
        }
    }
}
