using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StructchaWebApp.Data.Migrations
{
    public partial class AddDefaultRoleClaimSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "DefaultClaims",
                table: "AspNetRoles",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "CompanyAdminEdit",
                table: "AspNetRoles",
                defaultValue: false,
                nullable: false);
            migrationBuilder.AddColumn<bool>(
                name: "CompanyOwnerEdit",
                table: "AspNetRoles",
                defaultValue: false,
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DefaultClaims", table: "AspNetRoles");
            migrationBuilder.DropColumn(name: "CompanyAdminEdit", table: "AspNetRoles");
            migrationBuilder.DropColumn(name: "CompanyOwnerEdit", table: "AspNetRoles");
        }
    }
}
