using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using StructchaWebApp.Data;
using System.Reflection;

namespace StructchaWebApp.Pages.Shared
{
    public class UserHomePage
    {
        private UserManager<ApplicationUser> um { get; set; }
        private ApplicationUser user { get; set; }
        private IList<string> usersRoles { get; set; }
        private SqlConnection conn { get; set; }
        public List<Project> projectList { get; set; }
        public IEnumerable<SelectListItem> projectSelectList { get; set; }
        public IEnumerable<SelectListItem> userSelectList { get; set; }
        public IEnumerable<SelectListItem> roleSelectList { get; set; }
        public List<Project> finishedProjectList { get; set; }
        public List<ProjectPost> projectPostList { get; set; }
        public List<ProjectTask> taskList { get; set; }
        public List<ProjectTask> ownTaskList { get; set; }
        public UserHomePage(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            um = userManager;
            this.user = user;
            usersRoles = userManager.GetRolesAsync(user).Result;
            conn = _Common.connDB();
            userSelectList = new List<SelectListItem>();
            roleSelectList = new List<SelectListItem>();
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
                    var value = (rdr.GetValue(0));
                    list.Add(value.ToString());
                }
            }            
            return list;
        }

        private IEnumerable<SelectListItem> listToSelectList(List<Project> list)
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (Project project in list)
            {
                string? text = project.Title;
                if (project.Title == null)
                {
                    text = project.Location;
                }
                SelectListItem selectListItem = new SelectListItem()
                {
                    Text=text,
                    Value=project.ProjectCode
                };
                selectList.Add(selectListItem);
            }
            return selectList;
        }

        public void setProjectList()
        {
            List<Project> projects = new List<Project>();
            string query = "SELECT [ProjectCode] FROM [dbo].[Projects] WHERE [EndDate] IS NULL";
            string userCompany = user.Company;
            List<string> projectIds = idListRetrieve(query, null);

            foreach (string id in projectIds)
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
            projectSelectList = listToSelectList(projects);
        }

        private void setProjectPostList() //-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_Going to test when posting is implemented
        {
            int projectCount = projectList.Count;
            SqlParameter[] sqlParameters = new SqlParameter[projectCount];
            string queryBuilder = "SELECT [Id] FROM [dbo].[Posts] WHERE [PostTitle] IS NOT NULL AND [IdProject] IN ({0}) ORDER BY TimeOfPost ASC";
            string formattedIn = "";

            for (int i = 0; i < projectCount; i++)
            {
                string projectCode = projectList[i].ProjectCode;
                string parameterCode = "@project" + (i.ToString())+",";
                formattedIn = String.Concat(formattedIn, parameterCode);
                sqlParameters[i] = new SqlParameter(parameterCode.Remove(parameterCode.Length - 1), projectCode);
            }
            
            string query = string.Format(queryBuilder, formattedIn.Remove(formattedIn.Length-1));
            var projectPostIds = idListRetrieve(query, sqlParameters);

            List<ProjectPost> projectPosts = new List<ProjectPost>();
            foreach (string s in projectPostIds)
            {
                projectPosts.Add(new ProjectPost(s,um,conn));
            }
            projectPostList = projectPosts;
        }

        private void setTaskList()
        {
            List<ProjectTask> allTaskList = new List<ProjectTask>();
            List<ProjectTask> selfTaskList = new List<ProjectTask>();





            taskList = allTaskList;
            ownTaskList = selfTaskList;
        }

        public void createPost(string projectCode, string postTitle, string postBody)
        {
            conn.Open();
            string query = "INSERT INTO [dbo].[Posts] (IDUserOP,IdProject,PostTitle,PostBody,TimeOfPost) VALUES (@user, @project, @title, @body, GETDATE())";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user", user.Id);
            cmd.Parameters.AddWithValue("@project", projectCode);
            cmd.Parameters.AddWithValue("@title", postTitle);
            cmd.Parameters.AddWithValue("@body", postBody);

            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void createTask(string projectCode, string taskTitle, string taskBody, string[] taskRoles, string[] taskUsers)
        {
            conn.Open();
            string query = "INSERT INTO [dbo].[Tasks] (IDUserOP,IdProject,PostTitle,PostBody,TimeOfPost) VALUES (@user, @project, @title, @body, GETDATE())";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user", user.Id);
            cmd.Parameters.AddWithValue("@project", projectCode);
            cmd.Parameters.AddWithValue("@title", taskTitle);
            cmd.Parameters.AddWithValue("@body", taskBody);

            cmd.ExecuteNonQuery();
            conn.Close();
        }

    }
}
