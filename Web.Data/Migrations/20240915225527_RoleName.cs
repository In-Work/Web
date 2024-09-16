using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RoleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleUser_Roles_RolesId",
                table: "RoleUser");

            migrationBuilder.RenameColumn(
                name: "RolesId",
                table: "RoleUser",
                newName: "UserRolesId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Roles",
                newName: "RoleName");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleUser_Roles_UserRolesId",
                table: "RoleUser",
                column: "UserRolesId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleUser_Roles_UserRolesId",
                table: "RoleUser");

            migrationBuilder.RenameColumn(
                name: "UserRolesId",
                table: "RoleUser",
                newName: "RolesId");

            migrationBuilder.RenameColumn(
                name: "RoleName",
                table: "Roles",
                newName: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleUser_Roles_RolesId",
                table: "RoleUser",
                column: "RolesId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
