using Microsoft.EntityFrameworkCore.Migrations;

namespace UniSync.Migrations
{
    public partial class AddRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { "1", "Student", "STUDENT", Guid.NewGuid().ToString() },
                    { "2", "Moderator", "MODERATOR", Guid.NewGuid().ToString() },
                    { "3", "CourseManager", "COURSEMANAGER", Guid.NewGuid().ToString() },
                    { "4", "NewsEditor", "NEWSEDITOR", Guid.NewGuid().ToString() },
                    { "5", "Admin", "ADMIN", Guid.NewGuid().ToString() },
                    { "6", "SuperAdmin", "SUPERADMIN", Guid.NewGuid().ToString() }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
                migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValues: new object[] { "1", "2", "3", "4", "5", "6" });
        }
    }
}