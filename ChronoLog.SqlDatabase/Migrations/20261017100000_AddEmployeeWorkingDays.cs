using ChronoLog.SqlDatabase.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChronoLog.SqlDatabase.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SqlDbContext))]
    [Migration("20261017100000_AddEmployeeWorkingDays")]
    public partial class AddEmployeeWorkingDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkingDays",
                table: "Employees",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "1,2,3,4,5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkingDays",
                table: "Employees");
        }
    }
}

