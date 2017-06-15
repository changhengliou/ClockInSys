using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ReactSpa.Migrations
{
    public partial class fix3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "CheckRecord",
                newName: "StatusOfApprovalForOvertime");

            migrationBuilder.AddColumn<DateTime>(
                name: "OffApplyDate",
                table: "CheckRecord",
                type: "Date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OvertimeEndTime",
                table: "CheckRecord",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffApplyDate",
                table: "CheckRecord");

            migrationBuilder.DropColumn(
                name: "OvertimeEndTime",
                table: "CheckRecord");

            migrationBuilder.RenameColumn(
                name: "StatusOfApprovalForOvertime",
                table: "CheckRecord",
                newName: "Comment");
        }
    }
}
