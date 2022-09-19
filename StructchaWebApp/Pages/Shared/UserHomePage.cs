using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using StructchaWebApp.Data;


namespace StructchaWebApp.Pages.Shared
{
    public class UserHomePage
    {
        private ApplicationUser user { get; set; }
        private IList<string> usersRoles { get; set; }
        private SqlConnection conn { get; set; }
        public List<Project> projectList { get; set; }
        public List<Project> finishedProjectList { get; set; }
        public List<ProjectPosts> projectPostList { get; set; }
        public List<ProjectTasks> taskList { get; set; }
        public List<ProjectTasks> ownTaskList { get; set; }
        public UserHomePage(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            this.user = user;
            usersRoles = userManager.GetRolesAsync(user).Result;
            conn = _Common.connDB();
            setProjectList();
            setProjectPostList();
            setTaskList();
            conn.Close();
        }

        private List<string> idListRetrieve(string query, SqlParameter[]? sqlParameters)
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            if(sqlParameters != null)
            {
                cmd.Parameters.AddRange(sqlParameters);
            }            
            List<string> list = new List<string>();

            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while(rdr.Read())
                {
                    list.Add(rdr.GetString(0));
                }
            }            
            return list;
        }

        private void setProjectList()
        {
            List<Project> projects = new List<Project>();
            string query = "SELECT [ProjectCode] FROM [dbo].[Projects] WHERE [EndDate] IS NULL";
            string userCompany = user.Company;
            foreach (string id in idListRetrieve(query, null))
            {
                Project project = new Project(id,conn);
                bool userCanAccess = false;

                //Checking if the project is meant to be accessed by the user
                //First check if the user is blocked
                if (!project.BlockIndividual[userCompany].Contains(user.Id))
                {
                    if (project.Companies.Contains(user.Company) || project.LeadCompany.CompanyName == user.Company)
                    {
                        foreach(string role in usersRoles)
                        {
                            if (project.AccessRoles[user.Company].Contains(role))
                            {
                                userCanAccess = true;
                            }
                        }
                        if(!userCanAccess && project.AccessIndividual[user.Company].Contains(user.Email))
                        {
                            userCanAccess = true;
                        }
                    }
                }
                if (userCanAccess == true)
                {
                    projects.Add(project);
                }
            }
            projectList = projects;
        }

        private void setProjectPostList() //-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_Going to test when posting is implemented
        {
            int projectCount = projectList.Count;
            SqlParameter[] sqlParameters = new SqlParameter[projectCount];
            string queryBuilder = "SELECT [Id] FROM [dbo].[Posts] WHERE [PostTitle] IS NOT NULL AND [IdProject] IN ({0})";

            string formattedIn = "";

            for (int i = 0; i < projectCount; i++)
            {
                string projectCode = projectList[i].ProjectCode;
                string parameterCode = "@project" + (i.ToString())+",";
                formattedIn = String.Concat(formattedIn, parameterCode);
                sqlParameters[i] = new SqlParameter(parameterCode.Remove(parameterCode.Length - 1), projectCode);
            }
            char something = formattedIn[formattedIn.Length - 1];
            string query = string.Format(queryBuilder, formattedIn.Remove(formattedIn.Length-1));
            var projectPostIds = idListRetrieve(query, sqlParameters);

            List<ProjectPosts> projectPosts = new List<ProjectPosts>();
            foreach (string s in projectPostIds)
            {
                projectPosts.Add(new ProjectPosts(s));
            }
            

            projectPostList = projectPosts;
        }

        private void setTaskList()
        {
            List<ProjectTasks> allTaskList = new List<ProjectTasks>();
            List<ProjectTasks> selfTaskList = new List<ProjectTasks>();





            taskList = allTaskList;
            ownTaskList = selfTaskList;
        }

    }
}
