using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Pages.Shared
{
    public class ProjectPost
    {
        public string Id { get; set; }        
        public string UserName { get; set; }
        public string ProjectName { get; set; }
        private string UserId { get; set; }
        public Reply[] replies { get; set; }
        public string? ProjectId { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public DateTime TimeOfPost { get; set; }
        public DateTime? TimeOfEdit { get; set; }
        private SqlConnection _connection { get; set; }

        public ProjectPost(string postID, UserManager<ApplicationUser> userManager, SqlConnection conn)
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

                    // IdCompany = reader.GetString(3);
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
                }
            }

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
                ProjectName = "No Project";
            }

            UserName = userManager.FindByIdAsync(UserId).Result.UserName;
            findReplies(userManager);
        }

        private void findReplies(UserManager<ApplicationUser> um)
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

                    reader.Close();

                    replies = new Reply[repliesList.Count];

                    for (int i = 0; i < repliesList.Count; i++)
                    {
                        replies[i] = new Reply(repliesList[i], "Posts", _connection);
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
