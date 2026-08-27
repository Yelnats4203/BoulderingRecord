using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoulderingRecordAPI.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class FixSessionGradeRecordIdValueGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @constraintName nvarchar(200);
                SELECT @constraintName = dc.name
                FROM sys.default_constraints dc
                JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
                WHERE dc.parent_object_id = OBJECT_ID('SessionGradeRecords') AND c.name = 'Id';
                IF @constraintName IS NOT NULL
                    EXEC('ALTER TABLE [SessionGradeRecords] DROP CONSTRAINT [' + @constraintName + ']');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE [SessionGradeRecords] ADD DEFAULT (NEWID()) FOR [Id];");
        }
    }
}
