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
            string query = String.Format("SELECT [IdAssigner],[PostBody],[TimeOfPost] FROM [dbo].[{0}] WHERE [Id] = @id ORDER BY [TimeOfPost] DESC", table);
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
