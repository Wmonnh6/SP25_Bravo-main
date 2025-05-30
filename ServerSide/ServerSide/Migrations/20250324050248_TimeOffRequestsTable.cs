using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerSide.Migrations
{
    /// <inheritdoc />
    public partial class TimeOffRequestsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeOffRequestId",
                table: "TimeEntries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TimeOffRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeOffRequests", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_TimeOffRequests_TimeOffRequestId",
                table: "TimeEntries",
                column: "TimeOffRequestId",
                principalTable: "TimeOffRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_TimeOffRequests_TimeOffRequestId",
                table: "TimeEntries");

            migrationBuilder.DropTable(
                name: "TimeOffRequests");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_TimeOffRequestId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "TimeOffRequestId",
                table: "TimeEntries");
        }
    }
}
