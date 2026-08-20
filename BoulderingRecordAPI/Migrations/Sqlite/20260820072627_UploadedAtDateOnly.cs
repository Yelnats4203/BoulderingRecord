using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoulderingRecordAPI.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class UploadedAtDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sqlite 的 UploadedAt 欄位型別維持 TEXT，但既有資料是 DateTimeOffset 格式字串（例如
            // "2026-08-06 08:42:00+00:00"），改讀為 DateOnly 後需先截斷為純日期字串，否則解析會失敗。
            migrationBuilder.Sql("UPDATE Sends SET UploadedAt = substr(UploadedAt, 1, 10);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
