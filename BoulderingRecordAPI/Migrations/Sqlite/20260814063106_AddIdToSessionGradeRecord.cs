using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoulderingRecordAPI.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddIdToSessionGradeRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SessionGradeRecords",
                table: "SessionGradeRecords");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SessionGradeRecords",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SessionGradeRecords",
                table: "SessionGradeRecords",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SessionGradeRecords_SessionId",
                table: "SessionGradeRecords",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SessionGradeRecords",
                table: "SessionGradeRecords");

            migrationBuilder.DropIndex(
                name: "IX_SessionGradeRecords_SessionId",
                table: "SessionGradeRecords");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "SessionGradeRecords",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SessionGradeRecords",
                table: "SessionGradeRecords",
                columns: new[] { "SessionId", "Id" });
        }
    }
}
