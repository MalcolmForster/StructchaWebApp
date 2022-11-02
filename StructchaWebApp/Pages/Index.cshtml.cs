using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StructchaWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private AppManager app { get; set; }
        public ApplicationUser user { get; set; }
        public UserHomePage userHomePage { get; set; }
        private SqlConnection _connection { get; set; }
        private UserManager<ApplicationUser> userManager { get; set; }

        public IndexModel(UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm, IHttpContextAccessor httpContextAccessor)
        {
            _connection = _Common.connDB();
            string? userName = httpContextAccessor.HttpContext?.User?.Identity?.Name;
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
            string? projectCode = Request.Form["PostProjectCode"];
            string postTitle = Request.Form["PostTitle"];
            string postBody = Request.Form["PostBody"];
            if(projectCode == "No Project")
            {
                projectCode = null;
            }
            userHomePage.createPost(projectCode,user.Company,postTitle,postBody);
        }

        public void OnPostNewProjectTask()
        {
            string projectCode = Request.Form["TaskProjectCode"];
            string taskPriority = Request.Form["SetTaskPriority"];
            string[] taskRoles = Request.Form["roleAssigned"];
            string[] taskUsers = Request.Form["userAssigned"];
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
        public void NewReply(string type, string body, string id)
        {
            //string body = Request.Form["ReplyBody"];
            //string id = Request.Form["ReplyTo"];

            if (type == "task" || type == "post")
            {
                userHomePage.createReply(type, id, body);
            }
        }

        public ActionResult OnGetTaskBarRefresh()
        {
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_TaskBar",
                ViewData = new ViewDataDictionary<IndexModel>(ViewData, this)
            };
            return result;
        }

        private ActionResult TaskBarTasks(List<ProjectTask> list)
        {
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_TaskBarTaskPartial",
                ViewData = new ViewDataDictionary<List<ProjectTask>>(ViewData, list)
            };
            return result;
        }

        public ActionResult OnGetTaskBarSectionTasks()
        {
            return TaskBarTasks(userHomePage.taskList);
        }

        public ActionResult OnGetTaskBarSectionSelf()
        {
            return TaskBarTasks(userHomePage.ownTaskList);
        }

        public ActionResult OnGetTaskBarSectionPosts()
        {
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_TaskBarPostPartial",
                ViewData = new ViewDataDictionary<List<ProjectPost>>(ViewData, userHomePage.projectPostList)
            };
            return result;
        }

        public ActionResult OnGetSendSelected(string code)
        {
            userHomePage.setTaskAccessSelectLists(code);

            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_DynamicTaskSelector",
                ViewData = new ViewDataDictionary<UserHomePage>(ViewData, userHomePage)
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

        public void OnPostNewTaskAddUser(string test)
        {
            userHomePage.usersSelected.Add(test);
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
            _connection = _Common.connDB();         
            ProjectTask projectTask = new ProjectTask(taskId, userManager, user.Id, _connection);
            _connection.Close();
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_TaskPartial",
                ViewData = new ViewDataDictionary<ProjectTask>(ViewData, projectTask)
            };
            projectTask.addUserViewed(user.Id);
            return result;
        }

        public ActionResult OnGetPostPartial(string postId)
        {
            _connection = _Common.connDB();
            ProjectPost projectPost = new ProjectPost(postId, userManager, user.Id, _connection);
            _connection.Close();
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_PostPartial",
                ViewData = new ViewDataDictionary<ProjectPost>(ViewData, projectPost)
            };
            projectPost.addUserViewed(user.Id);
            return result;
        }

        public ActionResult OnPostTaskReply(string replyBody, string Id)
        {
            NewReply("task", replyBody, Id);
            return OnGetTaskPartial(Id);
        }

        public ActionResult OnPostPostReply(string replyBody, string Id)
        {
            NewReply("post", replyBody, Id);
            return OnGetPostPartial(Id);
        }        

        public ActionResult OnPostTaskComplete(string setTo,string taskId)
        {
            //string taskId = Request.Form["CompletionButton"];
            //string i = Request.Form["SetTo"];

            ProjectTask task = new ProjectTask(taskId,userManager,user.Id,_connection);
            task.SetComplete(setTo);

            return OnGetTaskPartial(taskId);
        }



        public ActionResult OnPostImageUpload()
        {
            var currentImages = Request.Form["CurrentImages"];
            var files = Request.Form.Files.ToArray();
            userHomePage.imageManager.UploadImage(currentImages, files);
            
            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_UploadImage",
                ViewData = new ViewDataDictionary<UserHomePage>(ViewData, userHomePage)
            };
            return (result);

        }
    }
}