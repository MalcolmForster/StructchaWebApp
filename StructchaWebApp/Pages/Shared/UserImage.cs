using Microsoft.AspNetCore.Mvc;
using System.Buffers.Text;

namespace StructchaWebApp.Pages.Shared
{
    public class UserImage
    {
        public string id { get; set; }
        public string? name { get; set; }
        public string format { get; set; }
        public string label { get; set; }
        public string description { get; set; }
        //public string path  { get; set; }

        public UserImage(string id, string format)
        {
            this.id = id;
            this.format = format;
            this.label = "";
            this.description = "";
        }

        public UserImage(string id, string format, string label, string description)
        {
            this.id = id;
            this.format = format;
            this.label = label;
            this.description = description;
        }

        public string getImage()
        {
            string path = Connections.testingUserImageLocation;
            //var fs = new FileStream(path + id, FileMode.Open);
            //var fsr = new FileStreamResult(fs, "image/jpeg");
            //fs.Close();
            string source = "data:"+format+"; base64, "+ Convert.ToBase64String(File.ReadAllBytes(path + id));
            return source;
        }

    }
}
 