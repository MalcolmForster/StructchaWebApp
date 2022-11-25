using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Pages.Shared
{
    public class ProjectPost : PostTaskAbstract
    {
        public ProjectPost(string postID, UserManager<ApplicationUser> userManager, string userId, SqlConnection conn) : base('p', postID, userManager, userId,conn)
        {
            //The main getting info from
            string query = "SELECT * FROM [dbo].[Posts] WHERE [Id] = @id";
            var connection = _Common.connDB();
            var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", postId);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    UserId = reader.GetString(1);
                    if (reader.GetValue(2) != DBNull.Value)
                    {
                        ProjectId = reader.GetString(2);
                    }
                    else
                    {
                        ProjectId = null;
                    }

                    Body = reader.GetString(6);
                    TimeOfPost = reader.GetDateTime(7);
                    IdCompany = reader.GetString(3);

                    // IdRoles = reader.GetString(4);
                    Title = reader.GetString(5);
                    bool edited = reader.IsDBNull(8);
                    if (!edited)
                    {
                        TimeOfEdit = reader.GetDateTime(8);
                    }
                    else
                    {
                        TimeOfEdit = null;
                    }

                    if (!reader.IsDBNull(12))
                    {
                        UserViewedBody = Viewed(UserId, reader.GetString(12));
                    }
                    else
                    {
                        UserViewedBody = false;
                    }
                    if (!reader.IsDBNull(13))
                    {
                        UserViewedReplies = Viewed(UserId, reader.GetString(13));
                    }
                    else
                    {
                        UserViewedReplies = false;
                    }
                    if (!reader.IsDBNull(14))
                    {
                        imageJson = reader.GetString(14);
                        setImages();
                    } else
                    {
                        imageJson = null;
                    }
                }
                reader.Close();
            }

            CompanyName = Company.NameOfCompany(IdCompany, connection);
            if (ProjectId != null)
            {
                Project pro = new Project(ProjectId);

                if (pro.Title != null)
                {
                    ProjectName = pro.Title;
                }
                else
                {
                    ProjectName = pro.Location;
                }
            }
            else
            {
                ProjectName = CompanyName;
            }
            connection.Close();
        }               
    }
}
