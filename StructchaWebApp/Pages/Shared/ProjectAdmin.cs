using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;
using System.Security.Claims;

namespace StructchaWebApp.Pages.Shared
{
    public class ProjectAdmin
    {
        private string? companyCode { get; set; }
        private string? companyName { get; set; }
        private RoleManager<IdentityRole> roleManager { get; set; }
        private UserManager<ApplicationUser> userManager { get; set; }
        public IEnumerable<SelectListItem> userRoles { get; set; }
        public IEnumerable<SelectListItem> companyUsers { get; set; }

        public ProjectAdmin(string company, RoleManager<IdentityRole> rm, UserManager<ApplicationUser> um)
        {
            roleManager = rm;
            userManager = um;
            companyName = company;
            companyCode = getCompCode(company);
            userRoles = AllUserRoles();
            companyUsers = getCompanyUsers();
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
            string intCompJson = "{\"Lead\":{\"Code\":\"" + companyCode + "\",\"RoleAccess\":[],\"UserAccess\":[],\"UserBlocks\":[]}}";

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
                    insertCommand.Parameters.AddWithValue("@companies", intCompJson);
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

        //returns the users in the company
        private IEnumerable<SelectListItem> getCompanyUsers()
        {
            List<ApplicationUser> companyUsers = userManager.Users.ToList();
            List<SelectListItem> list = new List<SelectListItem>();
            foreach(ApplicationUser user in companyUsers)
            {
                if(user.Company == companyName)
                {
                    list.Add(new SelectListItem { Text = user.UserName, Value = user.UserName });
                }                
            }
            return list;
        }

        //Returns users roles which can be added to a project
        private IEnumerable<SelectListItem> AllUserRoles()
        {
            var roles = roleManager.Roles.OrderBy(x => x.Name).ToList();
            List<SelectListItem>  selectList = new List<SelectListItem>();
            foreach (IdentityRole role in roles)
            {
                if (role.Name != "admin")
                {
                    SelectListItem item = new SelectListItem
                    {
                        Value = role.Name,
                        Text = role.Name
                    };
                    selectList.Add(item);
                }
            }
            return selectList;
        }

        //editProject can perform serveral things, edit the roles/groups/individuals on the project, sign it off as completed etc
        public void editProject(string projectCode, string company, string editOperation, string parameters)
        {
            var conn = _Common.connDB();
            var editedProject = new Project(projectCode,conn);
            int num = -1;
            string accessType = "";

            switch (editOperation)
            {
                case "addRoles":
                    num = 0;
                    accessType = "RoleAccess";

                    break;
                case "removeRoles":
                    num = 1;
                    accessType = "RoleAccess";

                    break;
                case "addIndividual":
                    num = 0;
                    accessType = "UserAccess";

                    break;
                case "removeIndividual":
                    num = 1;
                    accessType = "UserAccess";

                    break;
                case "blockIndividual":
                    num = 0;
                    accessType = "UserBlocks";

                    break;
                case "unblockIndividual":
                    num = 1;
                    accessType = "UserBlocks";

                    break;
                case "addCompany":
                    editedProject.addCompany(company, parameters);

                    break;
                case "removeCompany":
                    editedProject.removeCompany(company, parameters);

                    break;
                case "editStart":
                    editedProject.editStart(company, parameters);

                    break;
                case "editFinish":
                    editedProject.editFinish(company, parameters);

                    break;
            }

            if(num != -1)
            {
                editedProject.editProjectAccess(num, company, accessType, parameters);
            }








            //switch (editOperation)
            //{
            //    case "addRoles":
            //        editedProject.addRoles(company, parameters);

            //        break;
            //    case "removeRoles":
            //        editedProject.removeRoles(company, parameters);

            //        break;
            //    case "addCompany":
            //        editedProject.addCompany(company, parameters);

            //        break;
            //    case "removeCompany":
            //        editedProject.removeCompany(company, parameters);

            //        break;
            //    case "addIndividual":
            //        editedProject.addIndividual(company, parameters);

            //        break;
            //    case "removeIndividual":
            //        editedProject.removeIndividual(company, parameters);

            //        break;
            //    case "editStart":
            //        editedProject.editStart(company, parameters);

            //        break;
            //    case "editFinish":
            //        editedProject.editFinish(company, parameters);

            //        break;
            //}
            conn.Close();
        }
    }
}
