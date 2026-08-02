using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChronoLog.SqlDatabase.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmployeeEntryUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Employees_ObjectId",
                table: "Employees",
                column: "ObjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_ObjectId",
                table: "Employees");
        }
    }
}
