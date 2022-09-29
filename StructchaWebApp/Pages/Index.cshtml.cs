using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;
using System.Data;

namespace StructchaWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private AppManager app { get; set; }
        public ApplicationUser? user { get; set; }
        public UserHomePage userHomePage { get; set; }
        private SqlConnection _connection { get; set; }
        private UserManager<ApplicationUser> userManager { get; set; }

        public IndexModel(UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm, IHttpContextAccessor httpContextAccessor)
        {
            _connection = _Common.connDB();
            string userName = httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if(userName != null)
            {
                user = um.FindByNameAsync(userName).Result;
                userHomePage = new UserHomePage(user, um, rm, _connection);
                userManager = um;
            }
        }

        public void OnGet()
        {
            
        }

        public void OnPost()
        {
            if (Request.Form.ContainsKey("jointDrawSubmit") == true)
            {
                //Console.WriteLine("Post success");
                app = new AppManager(user.Id, "JointDraw");
            }
            if (Request.Form.ContainsKey("jointDrawClose") == true)
            {
                _Common.closeSoftware(user.Id, "JointDraw");
            }

            if (Request.Form.ContainsKey("StructFEASubmit") == true)
            {
                //Console.WriteLine("Post success");
                app = new AppManager(user.Id, "Structcha_FEA");
            }
            if (Request.Form.ContainsKey("StructFEAClose") == true)
            {
                _Common.closeSoftware(user.Id, "Structcha_FEA");
            }


        }

        public void OnPostNewProjectPost()
        {
            string projectCode = Request.Form["PostProjectCode"];
            string postTitle = Request.Form["PostTitle"];
            string postBody = Request.Form["PostBody"];

            userHomePage.createPost(projectCode,postTitle,postBody);
        }

        public void OnPostNewProjectTask()
        {
            string projectCode = Request.Form["TaskProjectCode"];
            string taskPriority = Request.Form["SetTaskPriority"];
            string[] taskRoles = { Request.Form["TaskAssignRole"] };
            string[] taskUsers = { Request.Form["TaskAssignUser"] };
            string taskTitle = Request.Form["TaskTitle"];
            string taskBody = Request.Form["TaskBody"];

            if(projectCode != "" && taskTitle != "" && taskBody != "")
            {
                userHomePage.createTask(projectCode, taskPriority, taskTitle, taskBody, taskRoles, taskUsers);
            }
        }

        //public ActionResult OnPostNewReply(string type, string id)
        //{
        //    string body = Request.Form["ReplyBody"];

        //    if(type == "task" || type == "post")
        //    {
        //        userHomePage.createReply(type, id, body);
        //        if (type == "task")
        //        {
        //            return OnGetTaskPartial(id);
        //        }
        //        else if (type == "post")
        //        {
        //            return OnGetPostPartial(id);
        //        }
        //    }
        //    return (new PartialViewResult() { ViewName="_Error"});            
        //}
        public void NewReply(string type)
        {
            string body = Request.Form["ReplyBody"];
            string id = Request.Form["ReplyTo"];

            if (type == "task" || type == "post")
            {
                userHomePage.createReply(type, id, body);
            }
        }

        public ActionResult OnGetSendSelected(string code)
        {
            userHomePage.setTaskAccessSelectLists(code);

            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_DynamicTaskSelector",
                ViewData = new ViewDataDictionary<IndexModel>(ViewData, this)
            };
            return result;
        }

        public ActionResult OnGetNewProjectPost()
        {
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_NewProjectPost",
                ViewData = new ViewDataDictionary<UserHomePage>(ViewData, userHomePage)
            };
            
            return result;
        }

        public ActionResult OnGetNewProjectTask()
        {
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_NewProjectTask",
                ViewData = new ViewDataDictionary<IndexModel>(ViewData, this)
            };
            return result;
        }

        public ActionResult OnGetTaskPartial(string taskId)
        {
            if(_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }            
            ProjectTask projectTask = new ProjectTask(taskId, userManager, _connection);
            _connection.Close();
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_TaskPartial",
                ViewData = new ViewDataDictionary<ProjectTask>(ViewData, projectTask)
            };
            return result;
        }

        public ActionResult OnGetPostPartial(string postId)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
            ProjectPost projectPost = new ProjectPost(postId, userManager, _connection);
            _connection.Close();
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_PostPartial",
                ViewData = new ViewDataDictionary<ProjectPost>(ViewData, projectPost)
            };
            return result;
        }
        public void OnPostTaskReply()
        {
            NewReply("task"); 
        }

        public void OnPostPostReply()
        {
            NewReply("post");
        }
        
        public void OnPostTaskComplete()
        {
            string taskId = Request.Form["CompletionButton"];
            string i = Request.Form["SetTo"];

            ProjectTask task = new ProjectTask(taskId,userManager,_connection);
            task.SetComplete(i);
        }
    }
}