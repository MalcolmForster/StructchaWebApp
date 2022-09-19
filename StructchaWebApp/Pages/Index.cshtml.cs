using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private AppManager app { get; set; }

        public ApplicationUser user { get; set; }
        private UserHomePage userHomePage { get; set; }


        public IndexModel(UserManager<ApplicationUser> um, IHttpContextAccessor httpContextAccessor)
        {
            user = um.FindByNameAsync(httpContextAccessor.HttpContext?.User.Identity?.Name).Result;
            userHomePage = new UserHomePage(user, um);
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

        }

        public void OnPostNewTask()
        {

        }
    }
}