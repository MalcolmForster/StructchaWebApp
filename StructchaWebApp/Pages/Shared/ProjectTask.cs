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
        public string ProjectLocation { get; set; }
        public bool Completed { get; set; }
        public bool UserViewedBody { get; set; }
        public bool UserViewedReplies { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public Reply[] replies { get; set; }
        public DateTime TimeOfPost { get; set; }
        public DateTime? TimeOfEdit { get; set; }
        public int Priority { get; set; }
        private SqlConnection _connection { get; set; }
        public ProjectTask(string id, UserManager<ApplicationUser> userManager, string userId, SqlConnection conn)
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

            _Common.connDB(_connection);

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
                    Completed = reader.GetBoolean(13);
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
                    if (!reader.IsDBNull(14))
                    {
                        UserViewedBody = Viewed(userId, reader.GetString(14));
                    } else
                    {
                        UserViewedBody = false;
                    }
                    if (!reader.IsDBNull(15))
                    {
                        UserViewedReplies = Viewed(userId, reader.GetString(15));
                    }
                    else
                    {
                        UserViewedReplies = false;
                    }                    
                }
                reader.Close();
                reader.Dispose();
            }

            UserName = userManager.FindByIdAsync(assignerId).Result.UserName;

            Project pro = new Project(ProjectCode, conn);

            findReplies(userManager);

            ProjectLocation = pro.Location;

            if (pro.Title != null)
            {
                ProjectName = pro.Title;
            }
            else
            {
                ProjectName = pro.Location;
            }
           
        }

        private bool Viewed(string userId, string viewedList)
        {
            if(viewedList.Contains(userId))
            {
                return true;
            } else
            {
                return false;
            }
        }

        private async void findReplies(UserManager<ApplicationUser> um) //turn into a new method/class that can be accessed by both tasks and posts
        {
            string query = "SELECT Id FROM [dbo].[Tasks] WHERE [ReplyTo] = @id ORDER BY [TimeOfPost] ASC";
            SqlCommand cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@id", Id);

            _Common.connDB(_connection);

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
                    ApplicationUser[] replyUsers = new ApplicationUser[repliesList.Count];

                    for(int i = 0; i < repliesList.Count; i++)
                    {
                        replies[i] = new Reply(repliesList[i], "Tasks", um);
                    }
                }
                else
                {
                    replies = new Reply[0];
                }
            }
        }

        public void SetComplete(string i)
        {
            string query = "UPDATE [dbo].[Tasks] SET [Completed] = @int WHERE [Id] = @taskId";
            var cmd = new SqlCommand(query, _connection);

            cmd.Parameters.AddWithValue("@int",i);
            cmd.Parameters.AddWithValue("@taskId",Id);

            _Common.connDB(_connection);

            cmd.ExecuteNonQuery();
            _connection.Close();
        }

        public void addUserViewed(string userId)
        {
            string query =
                "IF ((SELECT [SeenBody] FROM [Tasks] WHERE Id = @replyId) IS NULL) " +
                "BEGIN " +
                "UPDATE [dbo].[Tasks] SET [SeenBody] = @user WHERE Id = @replyId " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "UPDATE [dbo].[Tasks] SET [SeenBody] = CONCAT(SeenBody,', ', @user) WHERE Id = @replyId " +
                "END; " +
                "IF ((SELECT [SeenReplies] FROM [Tasks] WHERE Id = @replyId) IS NULL)  " +
                "BEGIN " +
                "UPDATE [dbo].[Tasks] SET [SeenReplies] = @user WHERE Id = @replyId " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "UPDATE [dbo].[Tasks] SET [SeenReplies] = CONCAT(SeenReplies,', ', @user) WHERE Id = @replyId " +
                "END;";

            SqlCommand cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@user", userId);
            cmd.Parameters.AddWithValue("@replyId", Id);
            _Common.connDB(_connection);
            cmd.ExecuteNonQuery();
            _connection.Close();
        }
    }

}
