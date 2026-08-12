using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoulderingRecordAPI.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddHasEditPermissionToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasEditPermission",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasEditPermission",
                table: "Users");
        }
    }
}
