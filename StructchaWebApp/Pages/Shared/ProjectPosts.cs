namespace StructchaWebApp.Pages.Shared
{
    public class ProjectPosts
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public ProjectPosts(string postID)
        {
            Id = postID;


            //methods to get other variables from database


        }
    }
}
