namespace StructchaWebApp.Pages.Shared
{
    public class Project
    {
        public string ProjectCode { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? Companies { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateTime? TimeCreated { get; set; }
        public DateTime? TimeFinished { get; set; }

        public Project(string projectCode)
        {
            ProjectCode = projectCode;
        }


    }
}
