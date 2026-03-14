using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChronoLog.SqlDatabase.Migrations
{
    /// <inheritdoc />
    public partial class RenameProjecttimeToTimeEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Projecttimes",
                newName: "TimeEntries");

            migrationBuilder.RenameColumn(
                name: "ProjecttimeId",
                table: "TimeEntries",
                newName: "TimeEntryId");

            migrationBuilder.Sql(
                "ALTER TABLE `TimeEntries` CHANGE `TimeSpent` `Duration` time(6) NOT NULL DEFAULT '00:00:00';");

            migrationBuilder.RenameIndex(
                name: "IX_Projecttimes_ProjectId",
                table: "TimeEntries",
                newName: "IX_TimeEntries_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Projecttimes_WorkdayId",
                table: "TimeEntries",
                newName: "IX_TimeEntries_WorkdayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TimeEntries",
                newName: "Projecttimes");

            migrationBuilder.RenameColumn(
                name: "TimeEntryId",
                table: "Projecttimes",
                newName: "ProjecttimeId");

            migrationBuilder.Sql(
                "ALTER TABLE `Projecttimes` CHANGE `Duration` `TimeSpent` time(6) NOT NULL DEFAULT '00:00:00';");

            migrationBuilder.RenameIndex(
                name: "IX_TimeEntries_ProjectId",
                table: "Projecttimes",
                newName: "IX_Projecttimes_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeEntries_WorkdayId",
                table: "Projecttimes",
                newName: "IX_Projecttimes_WorkdayId");
        }
    }
}
