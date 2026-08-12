using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoulderingRecordAPI.Migrations.Sqlite
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
                type: "INTEGER",
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
