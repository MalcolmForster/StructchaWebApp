using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using StructchaWebApp.Data;
using System.IO;

namespace StructchaWebApp.Pages.Shared
{
    public class ImageManager
    {

        public List<UserImage> UserImages { get; set; }
        private ApplicationUser user { get; set; }

        public ImageManager(ApplicationUser user)
        {
            this.user = user;
            UserImages = new List<UserImage>();
        }

        private string createImageCode()
        {
            // Ideas: use time, username, company, and random code?
            string comp = Company.CompanyID(user.Company, null);
            string imageCode = comp+_Common.generateKey(20);
            if (File.Exists(Connections.testingUserImageLocation + imageCode))
            {
                imageCode = createImageCode();
            }
            return imageCode;
        }

        public void UploadImage(IFormFile[] files) // When the user selects an image from the file manager, this function automatically uploads it to the server even before the post is accepted
        {
            //Security check, see if jpeg or png etc
            //List<IFormFile> checkedFiles = new List<IFormFile>();
            foreach(IFormFile file in files)
            {
                string? fileExtension = null;
                if (file.ContentType == "image/jpeg")
                {
                    fileExtension = ".jpg";

                } else if(file.ContentType == "image/png")
                {
                    fileExtension = ".png";
                }

                if (fileExtension != null)
                {
                    // Rename file to Structcha format implement laters
                    string imageName = createImageCode();
                    //file.FileName = imageName;
                    FileStream stream = new FileStream(Connections.testingUserImageLocation + imageName + fileExtension, FileMode.Create);
                    file.CopyTo(stream);
                    stream.Close();
                    UserImages.Add(new UserImage(imageName + fileExtension));
                    //checkedFiles.Add(file);
                }
            }
        }

        public FileResult setFileResult(string id)
        {
            string path = Connections.testingUserImageLocation;
            var fs = new FileStream(path + id, FileMode.Open);

            var fsr = new FileStreamResult(fs, "image/jpeg");
            fs.Close();

            return fsr;
        }


        //public UrlActionContext setFileResult(string id)
        //{
        //    string path = Connections.testingUserImageLocation;
        //    var fs = new FileStream(path + id, FileMode.Open);

        //    var fsr = new FileStreamResult(fs, "image/jpeg");
        //    fs.Close();

        //    var uac = new UrlActionContext();
        //    uac.Action(setFileResult(id));

        //    return fsr;
        //}

        public void RemoveImage() // removes the image from the current post and server. This function is only used BEFORE the post is posted
        {

        }

        public void ImageAccept() // Used when the post is accepted by the user. Accepts the image, does NOT reupload it. Adds it's link to the posts table row
        {

        }

    }
}
