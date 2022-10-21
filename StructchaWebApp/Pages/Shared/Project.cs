using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace StructchaWebApp.Pages.Shared
{
    public class Project
    {
        public string ProjectCode { get; set; }
        public string? Title { get; set; }
        public string Location { get; set; }
        public List<string> Companies { get; set; }
        public Dictionary<string, string[]> AccessRoles { get; set; }
        public Dictionary<string, string[]> AccessIndividual { get; set; }
        public Dictionary<string, string[]> BlockIndividual { get; set; }

        public IEnumerable<SelectListItem> SelectListCompanies { get; set; }
        public Dictionary<string, IEnumerable<SelectListItem>> SelectListRoles { get; set; }
        public Dictionary<string, IEnumerable<SelectListItem>> SelectListIndividuals { get; set; }
        public Dictionary<string, IEnumerable<SelectListItem>> SelectListBlocks { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeFinished { get; set; }
        private SqlConnection? _connection { get; set; }
        public Company LeadCompany { get; set; }
        private JsonObject AccessJson { get; set; }

        public Project(string projectCode, SqlConnection conn)
        {
            _connection = conn;
            _Common.connDB(_connection);
            ProjectCode = projectCode;
            Title = null;
            EndDate = null;
            TimeFinished = null;
            Companies =  new List<string>();

            getProjectInfo();
            FindLeadCompany();
            SetAccess();
            SetSelectLists();            
        }

        private SqlConnection connectToDB()
        {
            return _Common.connDB();
        }

        private string? nullCheck(SqlDataReader reader, int i)
        {
            string? dbString = null;
            if (!reader.IsDBNull(i))
            {
                reader.GetString(i);
            }
            return dbString;
        }

        private void getProjectInfo()
        {
            if(_connection == null)
            {
                _connection = connectToDB();
            }

            _Common.connDB(_connection);
            string query = "SELECT [Title], [Location], [Companies], [StartDate], [EndDate], [TimeCreated], [TimeFinished] FROM [dbo].[Projects] WHERE [ProjectCode] = @code";
            var cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@code", ProjectCode);
            using var rdr = cmd.ExecuteReader();

            rdr.Read();
            if (!rdr.IsDBNull(0)) { Title = rdr.GetString(0);};
            Location = rdr.GetString(1);
            AccessJson = JsonObject.Parse(rdr.GetString(2)).AsObject();
            getCompanies();
            StartDate = DateOnly.FromDateTime(rdr.GetDateTime(3));
            if (!rdr.IsDBNull(4)) { EndDate = DateOnly.FromDateTime(rdr.GetDateTime(4)); };
            TimeCreated = rdr.GetDateTime(5);
            if (!rdr.IsDBNull(6)) { TimeFinished = rdr.GetDateTime(6); };
            rdr.Close();
        }
        
        private void FindLeadCompany()
        {
            //This will change to use the data pulled from the database column [Companies], but for now will use the change code to project code
            LeadCompany = new Company(ProjectCode.Substring(0,6),_connection);
        }

        private void AlterProjectJson()
        {

        }

        public List<string> CurrentProjectRoles() //will be used for adding what roles can be assigned on the ProjectAdmin.cs and what roles can be removed from a project
        {
            List<string> roles = new List<string>();
            return roles;
        }

        private SelectListItem emptyListItem()
        {
            return new SelectListItem{
                Text = "No Items Found",
                Disabled = true
            };
        }

        private void getCompanies()
        {
            foreach (KeyValuePair<string, JsonNode?> kvp in AccessJson)
            {
                if (kvp.Key != "Lead")
                {
                    Company company = new Company((string)(kvp.Value["Code"]), _connection);
                    Companies.Add(company.CompanyName);
                }
            }     
        }

        private string[] arrayJsonToString(JsonArray j)
        {
            string[] s = new string[j.Count];
            for(int i = 0; i < j.Count; i++)
            {
                s[i] = j[i].ToString();
            }
            return s;
        }

        private void SetAccess()
        {
            AccessRoles = new Dictionary<string, string[]>();
            AccessIndividual = new Dictionary<string, string[]>();
            BlockIndividual = new Dictionary<string, string[]>();


            foreach (KeyValuePair<string, JsonNode?> kvp in AccessJson)
            {
                string key = kvp.Key;
                if (key == "Lead")
                {
                    key = LeadCompany.CompanyName;
                }
                AccessRoles.Add(key, arrayJsonToString(kvp.Value["RoleAccess"].AsArray()));
                AccessIndividual.Add(key, arrayJsonToString(kvp.Value["UserAccess"].AsArray()));
                BlockIndividual.Add(key, arrayJsonToString(kvp.Value["UserBlocks"].AsArray()));
            }
        }

        private SelectListItem[] convertToSelectList(string[] sa)
        {
            int length = sa.Length;
            SelectListItem[] slia = new SelectListItem[length];
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    slia[i] = (new SelectListItem
                    {
                        Text = sa[i],
                        Value = sa[i]
                    }
                    );
                }
            }
            else
            {
                slia = new SelectListItem[1];
                slia[0] = emptyListItem();
            }

            return slia;
        }

        private void SetSelectLists()
        {
            SelectListRoles = new Dictionary<string, IEnumerable<SelectListItem>>();
            SelectListIndividuals = new Dictionary<string, IEnumerable<SelectListItem>>();
            SelectListBlocks = new Dictionary<string, IEnumerable<SelectListItem>>();

            SelectListCompanies = convertToSelectList(Companies.ToArray());
            Dictionary<string, string[]>[] jsonInfoDictonary = { AccessRoles, AccessIndividual, BlockIndividual };
            Dictionary<string, IEnumerable<SelectListItem>>[] selectListDictonary = { SelectListRoles, SelectListIndividuals, SelectListBlocks };

            for (int i = 0; i < jsonInfoDictonary.Length; i++)
            {
                string[] keys = jsonInfoDictonary[i].Keys.ToArray();
                string[][] values = jsonInfoDictonary[i].Values.ToArray();
                for (int j = 0; j < jsonInfoDictonary[i].Values.Count; j++)
                {
                    selectListDictonary[i].Add(keys[j], convertToSelectList(values[j]));
                }
            }
        }

        private void alterAccessJson(string jsonString)
        {
            _Common.connDB(_connection);
            string query = "UPDATE [dbo].[Projects] SET [Companies] = @json WHERE [ProjectCode] = @projectCode";
            var cmd = new SqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@json", jsonString);
            cmd.Parameters.AddWithValue("@projectCode", ProjectCode);
            cmd.ExecuteNonQuery();
            //AccessJson = JsonObject.Parse(jsonString).AsObject();
        }

        public void editProjectAccess(int num,string company, string accessType, string parameters)
        {
            if(num == 0)
            {
                addProjectAccess(company,accessType,parameters);
            } else if (num == 1)
            {
                rmvProjectAccess(company, accessType, parameters);
            }

            SetAccess();
            SetSelectLists();
        }

        private void addProjectAccess(string company, string accessType,string parameters)
        {
            JsonObject newJson = AccessJson;
            if (company == LeadCompany.CompanyName)
            {
                company = "Lead";
            }

            JsonArray jsonArray = newJson[company][accessType].AsArray();
            bool add = true;
            foreach (JsonNode node in jsonArray)
            {
                if (node.AsValue().ToString() == parameters)
                {
                    add = false;
                }
            }

            if (add == true)
            {
                newJson[company][accessType].AsArray().Add(parameters);
                alterAccessJson(newJson.ToString());
            }
            else
            {
                Console.WriteLine(parameters + " already allowed access on this project");
            }

        }

        private void rmvProjectAccess(string company, string accessType, string parameters)
        {
            JsonObject newJson = AccessJson;
            if (company == LeadCompany.CompanyName)
            {
                company = "Lead";
            }

            JsonArray jsonArray = newJson[company][accessType].AsArray();
            foreach (JsonNode node in jsonArray)
            {
                if (node.AsValue().ToString() == parameters)
                {
                    newJson[company][accessType].AsArray().Remove(node);
                    alterAccessJson(newJson.ToString());
                    break;
                }
            }
        }

        public void addCompany(string companyCode)
        {
            JsonObject newJson = AccessJson;
            bool getNum = false;
            bool add = true;
            int count = 0;
            string compNum = "";


            while (getNum == false)
            {
                compNum = "Company" + count;
                if (!newJson.ContainsKey(compNum))
                {
                    break;
                } else
                {
                    count++;
                }                
            }

            foreach (KeyValuePair<string, JsonNode?> kvp in AccessJson)
            {
                if (kvp.Key != "Lead")
                {
                    if (kvp.Value["Code"].ToString() == companyCode)
                    {
                        add = false;
                        break;
                    }
                }
            }

            //Check if company exists
            if (add == true)
            {
                string intCompJson = ",\""+ compNum+ "\":{\"Code\":\"" + companyCode + "\",\"RoleAccess\":[],\"UserAccess\":[],\"UserBlocks\":[]}}";
                string oldJsonString = newJson.ToString();
                int length = oldJsonString.Length;
                string newJsonString = String.Concat(oldJsonString.Substring(0,length - 1), intCompJson);
                alterAccessJson(newJsonString);
            }
        }

        public void removeCompany(string companyCode)
        {
            JsonObject newJson = AccessJson;

            foreach (KeyValuePair<string, JsonNode?> kvp in newJson)
            {
                if (kvp.Key != "Lead")
                {
                    if (kvp.Value["Code"].ToString() == companyCode)
                    {
                        newJson.Remove(kvp.Key);
                        alterAccessJson(newJson.ToString());
                    }
                }
            }            
        }

        public void editStart(string company, string parameters)
        {

        }
        public void editFinish(string company, string parameters)
        {

        }
    }
}
