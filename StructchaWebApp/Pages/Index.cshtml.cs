using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private AppManager app { get; set; }
        public ApplicationUser user { get; set; }
        public UserHomePage userHomePage { get; set; }
        private UserManager<ApplicationUser> userManager { get; set; }

        public IndexModel(UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm, IHttpContextAccessor httpContextAccessor)
        {
            user = um.FindByNameAsync(httpContextAccessor.HttpContext?.User.Identity?.Name).Result;
            userHomePage = new UserHomePage(user, um,rm);
            userManager = um;
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

            userHomePage.createTask(projectCode,taskPriority, taskTitle, taskBody,taskRoles, taskUsers);

        }

        public void OnPostNewTask()
        {

        }

        public void OnPostChangePartial()
        {

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
            var conn = _Common.connDB();
            ProjectTask projectTask = new ProjectTask(taskId, userManager, conn);
            conn.Close();

            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_TaskPartial",
                ViewData = new ViewDataDictionary<ProjectTask>(ViewData, projectTask)
            };
            return result;
        }
    }
}