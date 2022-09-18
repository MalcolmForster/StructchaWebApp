using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;

namespace StructchaWebApp.Pages.Shared
{
    public class UserHomePage
    {
        private string user { get; set; }
        private SqlConnection conn { get; set; }
        public List<Project> projectList { get; set; }
        public List<Project> finishedProjectList { get; set; }
        public List<ProjectPosts> projectPostList { get; set; }
        public List<ProjectTasks> taskList { get; set; }
        public List<ProjectTasks> ownTaskList { get; set; }
        public UserHomePage(string user)
        {
            this.user = user;
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

            foreach(string id in idListRetrieve(query, null))
            {
                Project project = new Project(id,conn);
                //if(project.Companies.Contains())
                //projects.Add(project);
            }

            projectList = projects;
        }

        private void setProjectPostList()
        {
            List<ProjectPosts> projectPosts = new List<ProjectPosts>();



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
