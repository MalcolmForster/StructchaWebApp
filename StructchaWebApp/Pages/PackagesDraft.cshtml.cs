using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Pages
{
    public class DraftModel : PageModel
    {
        AppManager app { get; set; }

        private readonly ILogger<IndexModel> _logger;

        public DraftModel(ILogger<IndexModel> logger)
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
                app = new AppManager(User.Claims.First().Value, "JointDraw");
            }
            if(Request.Form.ContainsKey("jointDrawClose") == true)
            {
                _Common.closeSoftware(User.Claims.First().Value, "JointDraw");
            }            
        }
    }
}