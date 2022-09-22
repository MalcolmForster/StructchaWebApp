using Microsoft.Data.SqlClient;

namespace StructchaWebApp.Pages.Shared
{
    public class Reply
    {
        public string postedBy { get; set; }
        public string body { get; set; }
        public DateTime timePosted { get; set; }

        public Reply(string id, string table, SqlConnection conn)
        {
            string OP = "";
            if(table == "Posts")
            {
                OP = "[IdUserOP]";
            } else if(table == "Tasks")
            {
                OP = "[IdAssigner]";
            }
            string query = String.Format("SELECT {0},[PostBody],[TimeOfPost] FROM [dbo].[{1}] WHERE [Id] = @id",OP, table);
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using(var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    postedBy = reader.GetString(0);
                    body = reader.GetString(1);
                    timePosted = reader.GetDateTime(2);
                }
                reader.Close();
            }
        }
    }
}
