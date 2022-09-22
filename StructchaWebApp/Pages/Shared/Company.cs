using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StructchaWebApp.Pages.Shared
{
    public class Company
    {
        private SqlConnection _connection;
        string CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public Company(string code, SqlConnection? conn)
        {
            if (conn == null)
            {
                _connection = _Common.connDB();
            }
            else
            {
                _connection = conn;
            }
            CompanyCode = code;
            CompanyName = NameOfCompany(code);
        }

        private string? NameOfCompany(string code)
        {
            string query = "SELECT [Company] FROM [dbo].[CompanyRegister] WHERE [Code] = @code";
            SqlConnection conn = _connection;
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@code", code);
            string? company = cmd.ExecuteScalar()?.ToString();
            conn.Close();

            return company;
        }
    }
}
