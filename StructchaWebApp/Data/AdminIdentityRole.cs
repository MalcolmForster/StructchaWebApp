using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using StructchaWebApp.Pages.Shared;

namespace StructchaWebApp.Data
{
    public class AdminIdentityRole : IdentityRole // maybe make another sub-class for CompanyIdentityRole which is seen and editable by the CompanyOwners etc
    {
        public string[]? DefaultClaims { get; set; }
        public string[]? Claims { get; set; }
        public bool CompanyAdminEdit { get; set; }
        public bool CompanyOwnerEdit { get; set; }

        public IdentityRole BaseRole { get; set; }

        public AdminIdentityRole(IdentityRole role) {
            BaseRole = role;
            SetAdminRoleOptions();
        }

        private void SetAdminRoleOptions()
        {
            var conn = _Common.connDB();
            var query = "SELECT TOP 1 DefaultClaims, CompanyAdminEdit, CompanyOwnerEdit FROM [dbo].[AspNetRoles] WHERE Id = @roleId";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@roleId", BaseRole.Id);
            string? claimString = null;

            using(var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    claimString = reader.IsDBNull(0) ? null: reader.GetString(0);
                    CompanyAdminEdit = reader.GetBoolean(1);
                    CompanyOwnerEdit = reader.GetBoolean(2);
                }
            }
            conn.Close();

            if (claimString!= null)
            {
                DefaultClaims = claimString.Split(',');
                Claims = DefaultClaims; //change to pull the claims from the role claims database table
                
            }
            Claims = new string[] { "PCWP" }; // for testing only
        }

        public void SetDefaultClaims()
        {

        }
    }

    
}
