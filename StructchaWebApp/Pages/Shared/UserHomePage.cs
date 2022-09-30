using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using StructchaWebApp.Data;
using System.Data;
using System.Reflection;
using System.Security.Claims;

namespace StructchaWebApp.Pages.Shared
{
    public class UserHomePage
    {
        private UserManager<ApplicationUser> um { get; set; }
        private RoleManager<IdentityRole> rm { get; set; }
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
        public string selectedProject { get; set; }
        public ImageManager imageManager { get; set; }

        public bool JointDrawAccess { get; set; }
        public bool StructchaFEAAccess { get; set; }

        public UserHomePage(ApplicationUser user, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SqlConnection sqlConnection)
        {
            um = userManager;
            rm = roleManager;
            this.user = user;
            usersRoles = userManager.GetRolesAsync(user).Result;
            conn = sqlConnection;
            _Common.connDB(conn);
            imageManager = new ImageManager();           
            //userSelectList = new List<SelectListItem>();
            //roleSelectList = new List<SelectListItem>();
            setProjectList();
            setProjectPostList();
            setTaskList();
            conn.Close();
            userAppAccess();
            selectedProject = "";
        }

        private List<string> idListRetrieve(string query, SqlParameter[]? sqlParameters)
        {
            _Common.connDB(conn);
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
                if (project.BlockIndividual.Keys.Contains(userCompany) && !project.BlockIndividual[userCompany].Contains(user.Id))
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
            if(projectCount > 0)
            {
                for (int i = 0; i < projectCount; i++)
                {
                    string projectCode = projectList[i].ProjectCode;
                    string parameterCode = "@project" + (i.ToString()) + ",";
                    formattedIn = String.Concat(formattedIn, parameterCode);
                    sqlParameters[i] = new SqlParameter(parameterCode.Remove(parameterCode.Length - 1), projectCode);
                }

                string query = string.Format(queryBuilder, formattedIn.Remove(formattedIn.Length - 1));
                var projectPostIds = idListRetrieve(query, sqlParameters);

                List<ProjectPost> projectPosts = new List<ProjectPost>();
                foreach (string s in projectPostIds)
                {
                    projectPosts.Add(new ProjectPost(s, um, conn));
                }

                if (projectPosts.Count > 0)
                {
                    projectPostList = projectPosts;
                }
            }
          
        }

        private List<ProjectTask> findTasks(int t) //coming back to this, going to make methods that can create task first
        {
            string option = "";

            SqlParameter[] sqlParameters = { new SqlParameter("@uId", user.Id) };



            if (t == 0)
            {
                sqlParameters = new SqlParameter[1 + usersRoles.Count];
                sqlParameters[0] = new SqlParameter("@uId", user.Id);
                string rolesIds = "";

                for(int i=0; i < usersRoles.Count;i++)
                {
                    string role = usersRoles[i];
                    string roleId = rm.FindByNameAsync(role).Result.Id;
                    rolesIds = String.Concat(rolesIds," OR [IdRoles] = @role"+i.ToString());
                    sqlParameters[i+1] = new SqlParameter("@role"+i.ToString(), roleId);
                }

                option = String.Format("([IdUsers] = @uId{0})",rolesIds);
                //option = "IdUsers";
            }
            else if (t == 1)
            {
                option = "[IdAssigner] = @uId";
            }

            string query = String.Format("SELECT [Id] FROM [dbo].[Tasks] WHERE [PostTitle] IS NOT NULL AND {0} ORDER BY [Priority] DESC, TimeOfPost ASC",option);
            List<string> taskIds =  idListRetrieve(query, sqlParameters);
            var taskList = new List<ProjectTask>();

            foreach(string s in taskIds)
            {
                taskList.Add(new ProjectTask(s,um, conn));
            }

            return taskList;
        }

        private void setTaskList()
        {
            taskList = findTasks(0);
            ownTaskList = findTasks(1);
        }

        private IEnumerable<SelectListItem> stringArrayToSelectList(string[] strings)
        {
            var list = new List<SelectListItem>();
            
            foreach (string s in strings)
            {
                list.Add(new SelectListItem
                {
                    Text = s,
                    Value = s
                }
                    );
            }

            return list;
        }

        public void setTaskAccessSelectLists(string selection)
        {
            conn.Open();
            Project project = new Project(selection,conn);
            conn.Close();

            string[] indiv = project.AccessIndividual[user.Company];
            string[] roles = project.AccessRoles[user.Company];

            userSelectList = stringArrayToSelectList(indiv);
            roleSelectList = stringArrayToSelectList(roles);
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

        private string[]? getIdentityID(string[] input, int t) //gets the id of roles and users, 0 for user, 1 for roles
        {
            if(input[0] != "")
            {
                string[]? id = new string[input.Length];

                for (int i = 0; i < id.Length; i++)
                {
                    if (t == 0)
                    {
                        id[i] = (um.FindByNameAsync(input[i]).Result).Id;
                    }
                    else if (t == 1)
                    {
                        id[i] = (rm.FindByNameAsync(input[i]).Result).Id;
                    }
                }
                return id;
            } else
            {
                return null;
            }
        }

        public void createReply(string type, string replyTo, string body)
        {
            string table = "";
            string userCol="";
            if(type == "task")
            {
                table = "[Tasks]";
                userCol = "[IdAssigner]";
            } else if (type == "post")
            {
                table = "[Posts]";
                userCol = "[IdUserOp]";
            }
            string query = String.Format("INSERT INTO [dbo].{0} ({1},[PostBody],[TimeOfPost],[ReplyTo]) VALUES (@user,@body,GETDATE(),@replyTo)", table, userCol);
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user",user.Id);
            cmd.Parameters.AddWithValue("@body",body);
            cmd.Parameters.AddWithValue("@replyTo",replyTo);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();            
        }

        public void createTask(string projectCode, string taskPriority, string taskTitle, string taskBody, string[] taskRoles, string[] taskUsers)
        {
            _Common.connDB(conn);
            string query = "INSERT INTO [dbo].[Tasks] (IdAssigner,IdUsers,IdProject,IdCompany,IdRoles,Priority,PostTitle,PostBody,TimeOfPost) VALUES (@user, @assignUsers, @project, @company, @assignRoles,@priority,@title, @body, GETDATE())";
            var cmd = new SqlCommand(query, conn);
            string[]? userIds = getIdentityID(taskUsers, 0);
            string[]? roleIds = getIdentityID(taskRoles, 1);

            if (userIds != null)
            {
                cmd.Parameters.AddWithValue("@assignUsers", String.Join(',', userIds));
            }
            else
            {
                cmd.Parameters.AddWithValue("@assignUsers", DBNull.Value);
            }

            if (roleIds != null)
            {
                cmd.Parameters.AddWithValue("@assignRoles", String.Join(',', roleIds));
            } else
            {
                cmd.Parameters.AddWithValue("@assignRoles", DBNull.Value);
            }

            cmd.Parameters.AddWithValue("@user", user.Id);
            cmd.Parameters.AddWithValue("@company", user.Company);
            cmd.Parameters.AddWithValue("@priority", taskPriority);
            cmd.Parameters.AddWithValue("@project", projectCode);
            cmd.Parameters.AddWithValue("@title", taskTitle);
            cmd.Parameters.AddWithValue("@body", taskBody);

            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void userAppAccess()
        {
            IList<string> usersRoles =  um.GetRolesAsync(user).Result;
            
            bool JD = false;
            bool SF = false;

            foreach (string role in usersRoles)
            {
                IdentityRole roleIdentity = rm.FindByNameAsync(role).Result;
                IList<Claim> usersClaims = rm.GetClaimsAsync(roleIdentity).Result;

                foreach (Claim claim in usersClaims)
                {
                    if (JD == false && claim.Type == "JointDraw" && claim.Value == user.Company)
                    {
                        JD = true;
                        JointDrawAccess = true;
                    }
                    else if (SF == false && claim.Type == "Structcha_FEA" && claim.Value == user.Company)
                    {
                        SF = true;
                        StructchaFEAAccess = true;
                    }
                    if(JD && SF)
                    {
                        break;
                    }
                }
                if (JD && SF)
                {
                    break;
                }
            }
        }
    }
}
