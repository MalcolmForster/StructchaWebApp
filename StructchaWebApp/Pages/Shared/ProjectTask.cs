using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;

namespace StructchaWebApp.Pages.Shared
{
    public class ProjectTask
    {
        public string type = "reply";
        public string Id { get; set; }
        public string UserName { get; set; }
        private string assignerId { get; set; }
        private string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public Reply[] replies { get; set; }
        public DateTime TimeOfPost { get; set; }
        public DateTime? TimeOfEdit { get; set; }
        public int Priority { get; set; }
        private SqlConnection _connection { get; set; }
        public ProjectTask(string id, UserManager<ApplicationUser> userManager, SqlConnection conn)
        {
            Id = id;
            string query = "SELECT * FROM [dbo].[Tasks] WHERE [Id] = @id";

            if (conn == null)
            {
                _connection = _Common.connDB();
            }
            else
            {
                _connection = conn;
            }

            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }

            var cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@id", Id);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    assignerId = reader.GetString(1);
                    ProjectCode = reader.GetString(3);
                    Body = reader.GetString(8);
                    TimeOfPost = reader.GetDateTime(9);
                    Priority = reader.GetInt32(6);
                    //values which can be null;

                    // IdCompany = reader.GetString(3);
                    // IdRoles = reader.GetString(4);
                    Title = reader.GetString(7);
                    bool edited = reader.IsDBNull(10);
                    if (!edited)
                    {
                        TimeOfEdit = reader.GetDateTime(10);
                    }
                    else
                    {
                        TimeOfEdit = null;
                    }
                }
                reader.Close();
                reader.Dispose();
            }

            Project pro = new Project(ProjectCode, conn);

            findReplies(userManager);

            if (pro.Title != null)
            {
                ProjectName = pro.Title;
            }
            else
            {
                ProjectName = pro.Location;
            }
            
            UserName = userManager.FindByIdAsync(assignerId).Result.UserName;
        }

        private void findReplies(UserManager<ApplicationUser> um)
        {
            string query = "SELECT Id FROM [dbo].[Tasks] WHERE [ReplyTo] = @id ORDER BY [TimeOfPost] DESC";
            SqlCommand cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@id", Id);


            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                _connection.Open();
            }


            using (var reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    List<string> repliesList = new List<string>();
                    while (reader.Read())
                    {
                        repliesList.Add(reader.GetInt32(0).ToString());
                    }

                    reader.Close();

                    replies = new Reply[repliesList.Count];

                    for (int i = 0; i < repliesList.Count; i++)
                    {
                        replies[i] = new Reply(repliesList[i], "Tasks", _connection);
                        replies[i].postedBy = um.FindByIdAsync(replies[i].postedBy).Result.UserName;
                    }
                }
                else
                {
                    replies = new Reply[0];
                }
            }
        }
    }

}
