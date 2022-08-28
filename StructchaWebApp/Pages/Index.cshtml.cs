using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public void OnPost()
        {

            if (Request.Form.ContainsKey("jointDrawSubmit") == true)
            {
                //Console.WriteLine("Post success");
                AppManager app = new AppManager(User.Claims.First().Value, "JointDraw");
            }
            if(Request.Form.ContainsKey("jointDrawClose") == true)
            {
                _Common.closeSoftware(User.Claims.First().Value, "JointDraw");
            }
            
        }
    }
}