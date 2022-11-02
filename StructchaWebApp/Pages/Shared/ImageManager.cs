using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StructchaWebApp.Data;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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

        public Bitmap ResizeImage(Image image, int width, int height) // not my code, taken from mpen's answer on stackoverflow
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public void UploadImage(IFormFile[] files) // When the user selects an image from the file manager, this function automatically uploads it to the server even before the post is accepted
        {
            //Security check, see if jpeg or png etc
            //List<IFormFile> checkedFiles = new List<IFormFile>();
            foreach(IFormFile file in files)
            {
                string? fileExtension = null;
                string format = "";
                if (file.ContentType == "image/jpeg")
                {
                    fileExtension = ".jpg";
                    format = "image/jpeg";

                } else if(file.ContentType == "image/png")
                {
                    fileExtension = ".png";
                    format = "image/png";
                }

                if (fileExtension != null)
                {
                    // Rename file to Structcha format implement laters
                    string imageName = createImageCode();
                    //file.FileName = imageName;
                    FileStream stream = new FileStream(Connections.testingUserImageLocation + imageName + fileExtension, FileMode.Create);
                    //file.
                    using(var memoryStream = new MemoryStream())
                    {
                        file.CopyTo(memoryStream);
                        using(var image = Image.FromStream(memoryStream))
                        {
                            
                            int imageWidth = image.Width;
                            int imageHeight = image.Height;                            
                            int maxDim = 1000;

                            int x = 0;
                            int y = 0;

                            if(imageWidth < maxDim && imageHeight < maxDim)
                            {
                                x = imageWidth;
                                y = imageHeight;
                            } else
                            {
                                double ratio = ((double)imageWidth / (double)imageHeight);
                                if(ratio >= 1)
                                {
                                    x = maxDim;
                                    y = (int)(x / ratio);
                                } else
                                {
                                    y = maxDim;
                                    x = (int)(y * ratio);
                                }
                            }


                            var resizedImage = (Image)ResizeImage(image, x, y);
                            resizedImage.Save(stream, ImageFormat.Jpeg);
                        }
                    }                   
                    
                    //resizedImage.Save(stream);
                    //resizedImage.CopyTo(stream);
                    stream.Close();
                    UserImages.Add(new UserImage(imageName + fileExtension, format));
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
