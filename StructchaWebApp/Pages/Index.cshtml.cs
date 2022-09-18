using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StructchaWebApp.Data;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Pages
{
    public class IndexModel : PageModel
    {
        AppManager app { get; set; }
        UserHomePage userHomePage { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            userHomePage = new UserHomePage(User.Claims.First().Value);
        }

        public void OnGet()
        {
            
        }

        public void OnPost()
        {
            if (Request.Form.ContainsKey("jointDrawSubmit") == true)
            {
                //Console.WriteLine("Post success");
                app = new AppManager(User.Claims.First().Value, "JointDraw");
            }
            if (Request.Form.ContainsKey("jointDrawClose") == true)
            {
                _Common.closeSoftware(User.Claims.First().Value, "JointDraw");
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