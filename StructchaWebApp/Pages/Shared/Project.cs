using Microsoft.Build.Evaluation;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;

namespace StructchaWebApp.Pages.Shared
{
    public class Project
    {
        public string ProjectCode { get; set; }
        public string? Title { get; set; }
        public string Location { get; set; }
        public string Companies { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeFinished { get; set; }
        private SqlConnection? _connection { get; set; }

        public Project(string projectCode, SqlConnection? conn)
        {
            _connection = conn;
            ProjectCode = projectCode;
            Title = null;
            EndDate = null;
            TimeFinished = null;
            getProjectInfo();
        }

        private SqlConnection connectToDB()
        {
            return _Common.connDB();
        }

        private string? nullCheck(SqlDataReader reader, int i)
        {
            string? dbString = null;
            if (!reader.IsDBNull(i))
            {
                reader.GetString(i);
            }
            return dbString;
        }

        private void getProjectInfo()
        {
            if(_connection == null)
            {
                _connection = connectToDB();
            }
            string query = "SELECT [Title], [Location], [Companies], [StartDate],[EndDate],[TimeCreated], [TimeFinished] FROM [dbo].[Projects] WHERE [ProjectCode] = @code";
            var cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@code", ProjectCode);

            using var rdr = cmd.ExecuteReader();

            rdr.Read();
            if (!rdr.IsDBNull(0)) { 
                Title = rdr.GetString(0);
            };
            Location = rdr.GetString(1);
            Companies = rdr.GetString(2);
            StartDate = DateOnly.FromDateTime(rdr.GetDateTime(3));
            if (!rdr.IsDBNull(4)) { EndDate = DateOnly.FromDateTime(rdr.GetDateTime(4)); };
            TimeCreated = rdr.GetDateTime(5);
            if (!rdr.IsDBNull(6)) { TimeFinished = rdr.GetDateTime(6); };
            rdr.Close();

        }

    }
}
