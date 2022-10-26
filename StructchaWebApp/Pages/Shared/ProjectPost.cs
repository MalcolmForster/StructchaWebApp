using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Pages.Shared
{
    public class ProjectPost : ProjectAbstractClass
    {
        public string Id { get; set; }        
        public string UserName { get; set; }
        public string ProjectName { get; set; }
        public string CompanyName { get; set; }
        public bool UserViewedBody { get; set; }
        public bool UserViewedReplies { get; set; }
        private string IdCompany { get; set; }
        private string UserId { get; set; }
        public Reply[] replies { get; set; }
        public string? ProjectId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public DateTime TimeOfPost { get; set; }
        public DateTime? TimeOfEdit { get; set; }
        private SqlConnection _connection { get; set; }

        public ProjectPost(string postID, UserManager<ApplicationUser> userManager, string userId, SqlConnection conn)
        {
            Id = postID;
            string query = "SELECT * FROM [dbo].[Posts] WHERE [Id] = @id";

            if(conn == null)
            {
                _connection = _Common.connDB();
            } else
            {
                _connection = conn;
            }
            _Common.connDB(_connection);

            var cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@id", postID);

            using(var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    UserId = reader.GetString(1);
                    if(reader.GetValue(2) != DBNull.Value)
                    {
                        ProjectId = reader.GetString(2);
                    } else
                    {
                        ProjectId = null;
                    }
                    
                    Body = reader.GetString(6);
                    TimeOfPost = reader.GetDateTime(7);

                    //values which can be null;

                    IdCompany = reader.GetString(3);
                    
                    // IdRoles = reader.GetString(4);
                    Title = reader.GetString(5);
                    bool edited = reader.IsDBNull(8);
                    if (!edited)
                    {
                        TimeOfEdit = reader.GetDateTime(8);
                    } else
                    {
                        TimeOfEdit = null;
                    }

                    if (!reader.IsDBNull(12))
                    {
                        UserViewedBody = Viewed(userId, reader.GetString(12));
                    }
                    else
                    {
                        UserViewedBody = false;
                    }
                    if (!reader.IsDBNull(13))
                    {
                        UserViewedReplies = Viewed(userId, reader.GetString(13));
                    }
                    else
                    {
                        UserViewedReplies = false;
                    }
                }
                reader.Close();
            }

            CompanyName = Company.NameOfCompany(IdCompany, _connection);

            if (ProjectId != null)
            {
                Project pro = new Project(ProjectId, conn);

                if (pro.Title != null)
                {
                    ProjectName = pro.Title;
                }
                else
                {
                    ProjectName = pro.Location;
                }
            } else
            {
                ProjectName = CompanyName;
            }

            UserName = userManager.FindByIdAsync(UserId).Result.UserName;
            findReplies(userManager);
        }

        private bool Viewed(string userId, string viewedList)
        {
            if (viewedList.Contains(userId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void findReplies(UserManager<ApplicationUser> um)
        {
            string query = "SELECT Id FROM [dbo].[Posts] WHERE [ReplyTo] = @id ORDER BY [TimeOfPost] ASC";
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
                    string[] repliesArray= repliesList.ToArray();
                    reader.Close();

                    var replyTasks = new List<Task<Reply>>();
                    replies = new Reply[repliesArray.Length];

                    for (int i = 0; i < repliesArray.Length; i++)
                    {
                        string s = repliesArray[i];
                        Task<Reply> task = Task.Run(() => new Reply(s, "Posts", um));
                        replyTasks.Add(task);
                    }
                    replies = await Task.WhenAll(replyTasks);
                }
                else
                {
                    replies = new Reply[0];
                }
            }
        }
        public void addUserViewed(string userId)
        {
            string query =
                "IF ((SELECT [SeenBody] FROM [Posts] WHERE Id = @postId) IS NULL) " +
                "BEGIN " +
                "UPDATE [dbo].[Posts] SET [SeenBody] = @user WHERE Id = @postId " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "UPDATE [dbo].[Posts] SET [SeenBody] = CONCAT(SeenBody,', ', @user) WHERE Id = @postId " +
                "END; " +
                "IF ((SELECT [SeenReplies] FROM [Posts] WHERE Id = @postId) IS NULL)  " +
                "BEGIN " +
                "UPDATE [dbo].[Posts] SET [SeenReplies] = @user WHERE Id = @postId " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "UPDATE [dbo].[Posts] SET [SeenReplies] = CONCAT(SeenReplies,', ', @user) WHERE Id = @postId " +
                "END;";

            SqlCommand cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@user",userId);
            cmd.Parameters.AddWithValue("@postId", Id);
            _Common.connDB(_connection);
            cmd.ExecuteNonQuery();
            _connection.Close();
        }
    }
}
