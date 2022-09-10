using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;

namespace StructchaWebApp.Pages.Shared
{
    public class AdminDash
    {
        private RoleManager<IdentityRole> roleManager { get; set; }
        public AdminDash(RoleManager<IdentityRole> rm)
        {
            roleManager = rm;
        }

        public List<IdentityRole> AllUserRoles()
        {
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

        public List<string> DeactivatedCompanies()
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

        public async Task addRole(string role)
        {
            role = role.Replace(" ", "_");
            List<string> allRoles = new List<string>();                
            AllUserRoles().ForEach(s => allRoles.Add(s.Name));

            bool dup = false;
            foreach(string roleName in allRoles)
            {
                if(roleName == role) { dup = true; break; }
            }

            if (!dup)
            {
                IdentityRole identityRole = new IdentityRole();
                identityRole.Name = role;

                await roleManager.CreateAsync(identityRole);

                Console.WriteLine(role + "created");
            }

            //method to report if the role was added or not (as it was a duplicate)
        }

        public async Task deleteRole(string role)
        {
            string _remove = "_remove";
            int len = role.Length - _remove.Length;
            role = role.Substring(0,len);
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

        public void addCompany(string company, string adminUser)
        {
            Console.WriteLine("Adding company");
            List<string> companies = AllCompanies();
            string lwrComp = company.ToLower();
            string query = "";
            SqlConnection conn = _Common.connDB();
            SqlCommand comm = new SqlCommand();
            foreach (string s in companies)
            {
                if (s.ToLower() == lwrComp)
                {
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

            //insert into the companies register 
            query = "INSERT INTO [dbo].[CompanyRegister] ([Company], [Activated], [AdminEmail]) VALUES (@comp,1,@adm)";
            comm = new SqlCommand(query, conn);
            comm.Parameters.AddWithValue("@comp", company);
            comm.Parameters.AddWithValue("@adm", adminUser);
            comm.ExecuteNonQuery();

            Console.WriteLine(company + " added to companies");
        }

        public void deleteCompany(string company) //doesn't delete, just deactivates it
        {
            string _remove = "_remove";
            int len = company.Length - _remove.Length;
            company = company.Substring(0, len);

            SqlConnection conn = _Common.connDB();
            string query = "UPDATE [dbo].[CompanyRegister] SET [Activated] = 0 WHERE [Company] = @comp";
            SqlCommand comm = new SqlCommand(query, conn);
            comm.Parameters.AddWithValue("@comp",company);
            comm.ExecuteNonQuery();
            conn.Close();
        }

        public void assignRole()
        {

        }

        public void unassignRole()
        {

        }
    }
}
