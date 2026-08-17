using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoulderingRecordAPI.Migrations.SqlServer
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

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SessionGradeRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SessionGradeRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

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

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SessionGradeRecords");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "SessionGradeRecords",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SessionGradeRecords",
                table: "SessionGradeRecords",
                columns: new[] { "SessionId", "Id" });
        }
    }
}
