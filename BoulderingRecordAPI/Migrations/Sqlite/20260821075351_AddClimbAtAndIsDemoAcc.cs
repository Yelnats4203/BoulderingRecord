using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoulderingRecordAPI.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddClimbAtAndIsDemoAcc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDemoAcc",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ClimbAt",
                table: "Sends",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            // 既有紀錄的攀爬日期回填為原本的上傳日期，因為 ClimbAt 承接了 UploadedAt 過去在前端代表的日期語意。
            migrationBuilder.Sql("UPDATE Sends SET ClimbAt = UploadedAt;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDemoAcc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClimbAt",
                table: "Sends");
        }
    }
}
