using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChronoLog.SqlDatabase.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "char(36)", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    ResponseObject = table.Column<string>(type: "longtext", nullable: false),
                    DefaultResponseText = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Workdays",
                columns: table => new
                {
                    WorkdayId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workdays", x => x.WorkdayId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Projecttimes",
                columns: table => new
                {
                    ProjecttimeId = table.Column<Guid>(type: "char(36)", nullable: false),
                    WorkdayId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProjectId = table.Column<Guid>(type: "char(36)", nullable: false),
                    TimeSpent = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    ResponseText = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projecttimes", x => x.ProjecttimeId);
                    table.ForeignKey(
                        name: "FK_Projecttimes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projecttimes_Workdays_WorkdayId",
                        column: x => x.WorkdayId,
                        principalTable: "Workdays",
                        principalColumn: "WorkdayId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Worktimes",
                columns: table => new
                {
                    WorktimeId = table.Column<Guid>(type: "char(36)", nullable: false),
                    WorkdayId = table.Column<Guid>(type: "char(36)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    BreakTime = table.Column<TimeSpan>(type: "time(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worktimes", x => x.WorktimeId);
                    table.ForeignKey(
                        name: "FK_Worktimes_Workdays_WorkdayId",
                        column: x => x.WorkdayId,
                        principalTable: "Workdays",
                        principalColumn: "WorkdayId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsDefault",
                table: "Projects",
                column: "IsDefault",
                unique: true,
                filter: "IsDefault = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Projecttimes_ProjectId",
                table: "Projecttimes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projecttimes_WorkdayId",
                table: "Projecttimes",
                column: "WorkdayId");

            migrationBuilder.CreateIndex(
                name: "IX_Worktimes_WorkdayId",
                table: "Worktimes",
                column: "WorkdayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Projecttimes");

            migrationBuilder.DropTable(
                name: "Worktimes");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Workdays");
        }
    }
}
