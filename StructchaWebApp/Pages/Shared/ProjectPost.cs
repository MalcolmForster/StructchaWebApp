﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Pages.Shared
{
    public class ProjectPost
    {
        private string Id { get; set; }
        
        public string UserName { get; set; }
        private string UserId { get; set; }
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

            var cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@id", postID);

            using(var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    UserId = reader.GetString(1);
                    ProjectId = reader.GetString(2);
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
            
            UserName = userManager.FindByIdAsync(UserId).Result.UserName;

        }
    }
}