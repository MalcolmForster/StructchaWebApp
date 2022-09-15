using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StructchaWebApp.Pages.Shared
{
    public class Company
    {
        string CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public Company(string code)
        {
            CompanyCode = code;
            CompanyName = NameOfCompany(code);
        }

        private string? NameOfCompany(string code)
        {
            var con = _Common.connDB();

            string query = "SELECT [Company] FROM [dbo].[CompanyRegister] WHERE [Code] = @code";
            SqlConnection conn = _Common.connDB();
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@code", code);
            string? company = cmd.ExecuteScalar()?.ToString();
            conn.Close();

            return company;
        }
    }
}
