using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace StructchaWebApp.Pages.Shared
{
    public class AdminDash
    {
        private RoleManager<IdentityRole> roleManager { get; set; }
        
        public AdminDash(RoleManager<IdentityRole> rm)
        {
            roleManager = rm;
        }

        public List<IdentityRole> AllUserRoles() // moved to company dash
        {
            //Perhaps add ability to order alphabetically 
            var roles = roleManager.Roles.ToList();
            foreach(IdentityRole role in roles)
            {
                if(role.Name == "admin")
                {
                    roles.Remove(role);
                    break;
                }                
            }            
            return roles;
        }

        //All companies ----------- looking to consolidate all company finding lists to one method
        public List<string> AllCompanies()
        {
            List<string> companyList = new List<string>();

            SqlConnection conn =  _Common.connDB();
            string query = "SELECT [Company] FROM [dbo].[CompanyRegister] WHERE NOT [Company] = 'Structcha'";
            SqlCommand comm = new SqlCommand(query, conn);

            using (SqlDataReader reader = comm.ExecuteReader())
            {
                while(reader.Read())
                {
                    companyList.Add(reader.GetString(0));
                }
            }
            conn.Close();
            return companyList;
        }

        //Companies that are activated, [Activated] = 0
        public List<string> ActivatedCompanies()
        {
            List<string> companyList = new List<string>();

            SqlConnection conn = _Common.connDB();
            string query = "SELECT [Company] FROM [dbo].[CompanyRegister] WHERE [Activated] = 1 AND NOT [Company] = 'Structcha'";
            SqlCommand comm = new SqlCommand(query, conn);

            using (SqlDataReader reader = comm.ExecuteReader())
            {
                while (reader.Read())
                {
                    companyList.Add(reader.GetString(0));
                }
            }
            conn.Close();
            return companyList;
        }


        //Companies that are deactived, [Activated] = 0, for example they have stopped paying to access Structcha
        public List<string> DeactivatedCompanies()
        {
            List<string> companyList = new List<string>();

            SqlConnection conn = _Common.connDB();
            string query = "SELECT [Company] FROM [dbo].[CompanyRegister] WHERE [Activated] = 0 AND NOT [Company] = 'Structcha'";
            SqlCommand comm = new SqlCommand(query, conn);

            using (SqlDataReader reader = comm.ExecuteReader())
            {
                while (reader.Read())
                {
                    companyList.Add(reader.GetString(0));
                }
            }
            conn.Close();
            return companyList;
        }

        //Creates a new role which can be assigned to users --------- need to add edit ability to change role accesses/permissions
        public async Task addRole(string role)
        {
            List<string> allRoles = new List<string>();                
            AllUserRoles().ForEach(s => allRoles.Add(s.Name));

            bool dup = false;
            foreach(string roleName in allRoles)
            {                
                if (roleName == role) { Console.WriteLine(role + " already exists"); dup = true; break; }
            }

            if (!dup)
            {
                IdentityRole identityRole = new IdentityRole();
                identityRole.Name = role;

                await roleManager.CreateAsync(identityRole);

                Console.WriteLine(role + " created");
            }
            //method to report if the role was added or not (as it was a duplicate)
        }

        //Delete a role from the database
        public async Task deleteRole(string role)
        {
            string _remove = "_remove";
            int len = role.Length - _remove.Length;
            role = role.Substring(0,len);
            role = role.Replace("_"," ");
            IdentityRole identityRole;

            foreach (IdentityRole ir in AllUserRoles())
            {
                if (ir.Name == role)
                {
                    identityRole = ir;
                    await roleManager.DeleteAsync(identityRole);
                    break;
                }
            }            
        }

        //Creates a new company and assigns a registered user as it's admin
        public void addCompany(string company, string adminUser)
        {
            if(company == "" || adminUser == "")
            {
                Console.WriteLine("Please enter valid company name and email");
            } else
            {
                Console.WriteLine("Adding company");
                List<string> companies = AllCompanies();
                string lwrComp = company.ToLower();
                string query = "";
                SqlConnection conn = _Common.connDB();
                SqlCommand comm = new SqlCommand();
                bool compExists = false;

                foreach (string s in companies)
                {
                    if (s.ToLower() == lwrComp)
                    {
                        compExists = true;
                        conn = _Common.connDB();
                        query = "UPDATE [dbo].[CompanyRegister] SET [Activated] = 1 WHERE [Company] = @comp";
                        comm.Connection = conn;
                        comm.CommandText = query;
                        comm.Parameters.AddWithValue("@comp", s);
                        comm.ExecuteNonQuery();
                        conn.Close();
                        Console.WriteLine(company + " reactivated");
                    }
                }
                if (!compExists)
                {
                    //insert into the companies register 
                    string? userID = _Common.findUserID(adminUser);
                    if (userID != null)
                    {
                        query = "INSERT INTO [dbo].[CompanyRegister] ([Company], [Activated], [AdminUserID], [AdminEmail]) VALUES (@comp,1,@uID,@adm)";
                        comm = new SqlCommand(query, conn);
                        comm.Parameters.AddWithValue("@comp", company);
                        comm.Parameters.AddWithValue("@uID", userID);
                        comm.Parameters.AddWithValue("@adm", adminUser);
                        comm.ExecuteNonQuery();

                        Console.WriteLine(company + " added to companies");
                    }
                    else
                    {
                        Console.WriteLine("Email " + adminUser + " not found");
                    }

                }
            }
        }

        //deactivates a company, ie they stopped paying changes [Activated] to a 0, not deleted from database
        public void deleteCompany(string company) //doesn't delete, just deactivates it
        {
            string _remove = "_remove";
            int len = company.Length - _remove.Length;
            company = company.Substring(0, len);
            company = company.Replace("_", " ");
            SqlConnection conn = _Common.connDB();
            string query = "UPDATE [dbo].[CompanyRegister] SET [Activated] = 0 WHERE [Company] = @comp";
            SqlCommand comm = new SqlCommand(query, conn);
            comm.Parameters.AddWithValue("@comp",company);
            comm.ExecuteNonQuery();
            conn.Close();
        }
    }
}
