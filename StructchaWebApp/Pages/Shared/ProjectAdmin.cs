using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StructchaWebApp.Pages.Shared
{
    public class ProjectAdmin
    {
        private string? companyCode { get; set; }
        private string? companyName { get; set; }

        public ProjectAdmin(string company)
        {
            companyName = company;
            companyCode = getCompCode(company);
        }

        private string? getCompCode(string company)
        {
            //this is risky, giving access to the companyregister
            string query = "SELECT [Code] FROM [dbo].[CompanyRegister] WHERE [Company] = @comp";
            SqlConnection conn =  _Common.connDB();
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@comp", company);

            string? code = cmd.ExecuteScalar()?.ToString();
            conn.Close();
            return code;
        }

        private string addToJson(string roleType, string company)
        {

            string newJson = "";

            return newJson;
        }

        //Method to create a project, name can be null is wanted but requires a location and start date
        public void createProject(string name, string location, DateOnly date)
        {
            string? projectName = null;
            if (name != "")
            {
                projectName = name;
            }

            string locationTrimmed = "CODE";
            //Making of a code for the project mixing the company code with the location and date
            foreach (string s in location.Split(' '))
            {
                if (s.All(char.IsLetter) && s.Length > 2)
                {
                    locationTrimmed = (s+locationTrimmed).Substring(0,4).ToUpper();
                    break;
                }
            }
            string projectCode = companyCode + locationTrimmed + date.ToString("MMyy");

            bool codeOpen = false;
            string query = "SELECT [ProjectCode] FROM [dbo].[Projects] WHERE [ProjectCode] = @projectCode";
            SqlConnection conn = _Common.connDB();
            SqlCommand selectCommand = new SqlCommand(query, conn);
            int i = 0;

            while (codeOpen == false)
            {
                string projectCodeTry = projectCode + i.ToString();
                selectCommand.Parameters.Clear();
                selectCommand.Parameters.AddWithValue("@projectCode", projectCodeTry);
                if(selectCommand.ExecuteScalar() == null)
                {
                    codeOpen = true;
                    query = "INSERT INTO [dbo].[Projects] (ProjectCode, Title, Location, Companies, StartDate, TimeCreated) VALUES (@projectCode, @title, @location, @companies, @startdate, @timeCreated)";
                    SqlCommand insertCommand = new SqlCommand(query, conn);
                    insertCommand.Parameters.AddWithValue("@projectcode", projectCodeTry);
                    if(projectName != null)
                    {
                        insertCommand.Parameters.AddWithValue("@title", projectName);
                    } else
                    {
                        insertCommand.Parameters.AddWithValue("@title", DBNull.Value);
                    }                    
                    insertCommand.Parameters.AddWithValue("@location", location);
                    insertCommand.Parameters.AddWithValue("@companies", addToJson("lead",companyName));
                    insertCommand.Parameters.AddWithValue("@startdate", date.ToDateTime(TimeOnly.MinValue));
                    insertCommand.Parameters.AddWithValue("@timeCreated", DateTime.Now);
                    insertCommand.ExecuteNonQuery();
                } else
                {
                    i++;
                }
            }
            conn.Close();

        }

        //Returns a list of the projects for the company
        public List<Project> getProjects(int i) //int i designates if the project is completed or not, lets say 0 is not completed, 1 is completed
        {
            List<Project> projectList = new List<Project>();
            string not = "";
            if(i == 1)
            {
                not = " NOT";
            }
            //technique of using LIKE as seen below is apparently slow, so will need to look at making faster in the future
            string query = String.Format("SELECT [ProjectCode] FROM [dbo].[Projects] WHERE [ProjectCode] LIKE @companyCode AND [TimeFinished] IS{0} NULL", not);
            SqlConnection conn = _Common.connDB();
            SqlCommand selectCommand = new SqlCommand(query, conn);
            selectCommand.Parameters.AddWithValue("@companyCode", companyCode+'%');
            using SqlDataReader rdr = selectCommand.ExecuteReader();
            List<string> projectCodes = new List<string>();

            while (rdr.Read())
            {
                projectCodes.Add(rdr.GetString(0));
            }

            rdr.Close();

            foreach(string code in projectCodes)
            {
                projectList.Add(new Project(code, conn));
            }

            conn.Close();
            return projectList;
        }

        //editProject can perform serveral things, edit the roles/groups/individuals on the project, sign it off as completed etc
        public void editProject()
        {

        }
    }
}
