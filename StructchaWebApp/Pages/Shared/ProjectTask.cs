using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;

namespace StructchaWebApp.Pages.Shared
{
    public class ProjectTask
    {
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
            }

            Project pro = new Project(ProjectCode, conn);

            replies = new Reply[0];

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
    }
}
