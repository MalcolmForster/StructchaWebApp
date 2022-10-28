using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;

namespace StructchaWebApp.Pages.Shared
{
    public abstract class PostTaskAbstract
    {
        protected char postType { get;}
        private string table { get; set; }
        public string postId { get; set; }        
        public string UserName { get; set; }
        public string ProjectName { get; set; }
        public string CompanyName { get; set; }
        public bool UserViewedBody { get; set; }
        public bool UserViewedReplies { get; set; }
        protected string IdCompany { get; set; }
        protected string UserId { get; set; }
        public Reply[] replies { get; set; }
        public string? ProjectId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public DateTime TimeOfPost { get; set; }
        public DateTime? TimeOfEdit { get; set; }
        protected SqlConnection _connection { get; set; }
        protected UserManager<ApplicationUser> um { get; set; }

        public PostTaskAbstract(char type, string postID, UserManager<ApplicationUser> userManager, string userId, SqlConnection conn)
        {
            findConnection(conn);
            //postId = postID;
            postType = type; 
            setSQLInputs();

            postId = postID;
            um = userManager;
            UserId = userId;

            //string query = String.Format("SELECT * FROM [dbo].{0} WHERE [Id] = @id", table);

            //var cmd = new SqlCommand(query, _connection);
            //cmd.Parameters.AddWithValue("@id", postId);

            //using (var reader = cmd.ExecuteReader())
            //{
            //    while (reader.Read())
            //    {
            //        UserId = reader.GetString(1);
            //        if (reader.GetValue(2) != DBNull.Value)
            //        {
            //            ProjectId = reader.GetString(2);
            //        }
            //        else
            //        {
            //            ProjectId = null;
            //        }

            //        Body = reader.GetString(6);
            //        TimeOfPost = reader.GetDateTime(7);

            //        IdCompany = reader.GetString(3);

            //        // IdRoles = reader.GetString(4);
            //        Title = reader.GetString(5);
            //        bool edited = reader.IsDBNull(8);
            //        if (!edited)
            //        {
            //            TimeOfEdit = reader.GetDateTime(8);
            //        }
            //        else
            //        {
            //            TimeOfEdit = null;
            //        }

            //        if (!reader.IsDBNull(12))
            //        {
            //            UserViewedBody = Viewed(UserId, reader.GetString(12));
            //        }
            //        else
            //        {
            //            UserViewedBody = false;
            //        }
            //        if (!reader.IsDBNull(13))
            //        {
            //            UserViewedReplies = Viewed(UserId, reader.GetString(13));
            //        }
            //        else
            //        {
            //            UserViewedReplies = false;
            //        }
            //    }
            //    reader.Close();
            //}

            // CompanyName = Company.NameOfCompany(IdCompany, _connection);

            //if (ProjectId != null)
            //{
            //    Project pro = new Project(ProjectId, _connection);

            //    if (pro.Title != null)
            //    {
            //        ProjectName = pro.Title;
            //    }
            //    else
            //    {
            //        ProjectName = pro.Location;
            //    }
            //}
            //else
            //{
            //    ProjectName = CompanyName;
            //}

            
            UserName = um.FindByIdAsync(UserId).Result.UserName;
            findReplies(um);
        }

        private void setSQLInputs()
        {
            if (postType == 'p')
            {
                table = "[Posts]";
            } else if (postType == 't')
            {
                table = "[Tasks]";
            }
        }

        public void findConnection(SqlConnection? conn)
        {
            if (conn == null)
            {
                _connection = _Common.connDB();
            }
            else
            {
                _connection = conn;
            }
        }

        protected bool Viewed(string userId, string viewedList)
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

        private async Task findReplies(UserManager<ApplicationUser> um)
        {
            //string query = String.Format("SELECT Id FROM [dbo].[Posts] WHERE [ReplyTo] = @id ORDER BY [TimeOfPost] ASC";
            string query = String.Format("SELECT Id FROM [dbo].{0} WHERE [ReplyTo] = @id ORDER BY [TimeOfPost] ASC", table);
            var connection = _Common.connDB();
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", postId);            

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    List<string> repliesList = new List<string>();
                    while (reader.Read())
                    {
                        repliesList.Add(reader.GetInt32(0).ToString());
                    }
                    string[] repliesArray = repliesList.ToArray();
                    reader.Close();

                    var replyTasks = new List<Task<Reply>>();
                    replies = new Reply[repliesArray.Length];

                    for (int i = 0; i < repliesArray.Length; i++)
                    {
                        string s = repliesArray[i];
                        Task<Reply> task = Task.Run(() => new Reply(s, table, um));
                        replyTasks.Add(task);
                    }
                    replies = await Task.WhenAll(replyTasks);
                }
                else
                {
                    replies = new Reply[0];
                }
            }
            
            connection.Close();
        }
        public void addUserViewed(string userId)
        {
            string query = String.Format(
                "IF ((SELECT [SeenBody] FROM {0} WHERE Id = @postId) IS NULL) " +
                "BEGIN " +
                "UPDATE [dbo].{0} SET [SeenBody] = @user WHERE Id = @postId " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "UPDATE [dbo].{0} SET [SeenBody] = CONCAT(SeenBody,', ', @user) WHERE Id = @postId " +
                "END; " +
                "IF ((SELECT [SeenReplies] FROM {0} WHERE Id = @postId) IS NULL)  " +
                "BEGIN " +
                "UPDATE [dbo].{0} SET [SeenReplies] = @user WHERE Id = @postId " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "UPDATE [dbo].{0} SET [SeenReplies] = CONCAT(SeenReplies,', ', @user) WHERE Id = @postId " +
                "END;", table);

            _connection = _Common.connDB();
            SqlCommand cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@user", userId);
            cmd.Parameters.AddWithValue("@postId", postId);
            cmd.ExecuteNonQuery();
            _connection.Close();
        }



    }
}
