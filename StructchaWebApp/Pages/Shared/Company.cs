using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StructchaWebApp.Pages.Shared
{
    public class Company
    {
        private SqlConnection _connection;
        public string CompanyCode { get; set; }
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
            CompanyName = NameOfCompany(code,_connection);
            _connection.Close();
        }
        public static string? CompanyID(string name, SqlConnection? conn)
        {
            bool closeConn = false;
            if (conn == null)
            {
                closeConn = true;
                conn = _Common.connDB();
            }
            string query = "SELECT [Code] FROM [dbo].[CompanyRegister] WHERE [Company] = @company";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@company", name);
            string? company = cmd.ExecuteScalar()?.ToString();
            if (closeConn)
            {
                conn.Close();
            }
            return company;
        }

        public static string? NameOfCompany(string code, SqlConnection? conn)
        {
            bool closeConn = false;
            var connection = _Common.connDB();            

            string query = "SELECT [Company] FROM [dbo].[CompanyRegister] WHERE [Code] = @code";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@code", code);
            string? company = cmd.ExecuteScalar()?.ToString();

            connection.Close();
            return company;
        }

    }
}
