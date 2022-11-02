using Microsoft.AspNetCore.Mvc;

namespace StructchaWebApp.Pages.Shared
{
    public class UserImage
    {
        public string id { get; set; }
        public string? name { get; set; }
        //public string path  { get; set; }

        public UserImage(string id)
        {
            this.id = id;
        }

        public string getImage()
        {
            string path = Connections.testingUserImageLocation;
            //var fs = new FileStream(path + id, FileMode.Open);
            //var fsr = new FileStreamResult(fs, "image/jpeg");
            //fs.Close();

            return Convert.ToBase64String(File.ReadAllBytes(path+id));
        }

    }
}
 