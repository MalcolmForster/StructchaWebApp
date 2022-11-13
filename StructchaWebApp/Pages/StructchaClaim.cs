namespace StructchaWebApp.Pages
{
    public class StructchaClaim
    {
        public string key { get; set; }
        public string description { get; set; }        
        public StructchaClaim(string key, string description)
        {
            this.key = key;
            this.description = description;      
        }
    }
}
